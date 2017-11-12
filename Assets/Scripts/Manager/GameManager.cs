using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CGameManager : MonoSingleton<CGameManager> {
    public CGameManager() { }
    private IDisposable breathDisposable;
    private int HeartCount;
    private Map<int, float> HeartMap = new Map<int, float>();
    private GameTimer Timer;
    private int DisConnetTime;
    public const int PingPongTime = 5;
    void Start()
    {

    }

    public void LogCallback(GameFramework.LogLevel l, object message)
    {
        Debug.LogError(message.ToString());
    }

    private void HandlerLog( string condition, string stackTrace, LogType type ) {
        if ( type == LogType.Warning || type == LogType.Log )
            return;
       // CMessageBoxUI.Show( string.Format( "{0}\n{1}", condition, stackTrace ), type.ToString(), CMessageBoxUI.Buttons.OKCancel );
    }

    public static bool IsLoadGameScene()
    {
        string loadedLevelName = SceneManager.GetActiveScene().name;
        if (loadedLevelName == SceneName.ASYNC_LOADER_SCENE)
            return false;
        else if (loadedLevelName == SceneName.LOGIN_SCENE)
            return false;
        else if (loadedLevelName == SceneName.ROLE_SELECT_SCENE)
            return false;
        return true;
    }


    private void ClearMainCamera()
    {
        if (Application.isEditor)
            return;
        Camera[] cobjs = Resources.FindObjectsOfTypeAll<Camera>();
        if (cobjs != null)
        {
            for (int i = 0; i < cobjs.Length; i++)
                cobjs[i].enabled = false;
        }
        Camera[] cameras = Camera.allCameras;
        if (cameras != null)
        {
            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i])
                    cameras[i].enabled = false;
            }
        }
    }
    //>--------------------------------------------------------------------
}
