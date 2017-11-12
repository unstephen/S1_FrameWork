using System;
using UnityEngine;
using Object = UnityEngine.Object;


public class GameTimer : MonoBehaviour {
    //当前时间，统一接口用
    public static float time { get { return Time.realtimeSinceStartup; } }
    //当前时间(毫秒，从启动开始算)
    public static int mtime { get { return ( int )(Time.realtimeSinceStartup * 1000); } }

    #region 计时器用到的数据
    private bool destroyOnLoad_ = true;
    private float elapse_;
    private string identification_ = string.Empty;
    private bool once_;
    private float timeLeft_;

    public float timeLeft { get { return timeLeft_; } }
    public string identification { get { return identification_; } }
    public EventHandler OnTimer;
    #endregion

    // 检测给予时间和当前时间是否在一定范围内
    public static bool Within( float time, float amount ) {
        return GameTimer.time - time < amount;
    }

    // 检测给予时间和当前时间是否在一定范围内
    public static bool WithinMtime(float mtime, float amount)
    {
        return GameTimer.mtime - mtime < amount;
    }

    #region 设置Timer的静态接口

    public static GameTimer StartTimer(float time, bool once, bool destroyOnLoad, string identification, EventHandler receiver, string tname = "timer")
    {
        GameTimer gt = StartTimer(time, once, destroyOnLoad, receiver, tname);
        gt.identification_ = identification;
        return gt;
    }

    public static GameTimer StartTimer(float time, bool once, bool destroyOnLoad, EventHandler receiver, string tname = "timer")
    {
        var go = new GameObject(tname);
        var com = go.AddComponent( typeof( GameTimer ) ) as GameTimer;
        com.OnTimer = receiver;
        com.timeLeft_ = time;
        com.elapse_ = time;
        com.once_ = once;
        com.destroyOnLoad_ = destroyOnLoad;

        //CategorySettings
        CategorySettings.Attach(go.transform, "_timers/", false);
        return com;
    }

    public void RestTimer(float time, bool once, bool destroyOnLoad, EventHandler receiver)
    {
        OnTimer = receiver;
        timeLeft_ = time;
        elapse_ = time;
        once_ = once;
        destroyOnLoad_ = destroyOnLoad;
    }

    public static GameTimer StartTimer(float time, float repeat, bool once, bool destroyOnLoad, string name, EventHandler receiver, string tname = "timer")
    {
        var go = new GameObject(tname);
        var com = go.AddComponent( typeof( GameTimer ) ) as GameTimer;
        com.timeLeft_ = time;
        com.elapse_ = repeat;
        com.once_ = once;
        com.identification_ = name;
        com.destroyOnLoad_ = destroyOnLoad;
        com.OnTimer = receiver;
        return com;
    }

    #endregion

    #region Timer处理

    private void Start() {
        if ( !destroyOnLoad_ ) {
            DontDestroyOnLoad( gameObject );
        }
    }

    private void Update() {
        timeLeft_ -= Time.deltaTime;
        if ( timeLeft_ <= 0 ) {
            if ( OnTimer != null ) {
                OnTimer( this, null );
            }
            if ( once_ ) {
                enabled = false;
                Destroy( gameObject );
                OnTimer = null;
            } else {
                timeLeft_ = elapse_;
            }
        }
    }

    public void Stop() {
        enabled = false;
        Destroy( gameObject );
        OnTimer = null;
    }

    #endregion

}