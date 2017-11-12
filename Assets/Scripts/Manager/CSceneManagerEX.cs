using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UniRx;

public class SceneAssetBundle : IDisposable
{
    IDisposable disa;
    public void Dispose()
    {
        if (SceneAB)
            SceneAB.Unload(false);

        if (disa != null)
            disa.Dispose();
    }
    public Coroutine coroutine { private get; set; }
    public AssetBundle SceneAB { get;private set; }

    public string SceneName { get; private set; }

    public bool isDone { get; private set; }

    private Progress.WWWSceneProgress wwwProgress;
    public float progress
    {
        get
        {
            return progress_;
        }
        private set
        {
            progress_ = value;
            wwwProgress.SetPercent(progress_);
        }
    }

    private float progress_;

    private bool AutoLoadScene;

    public SceneAssetBundle(string scene)
    {
        if (CSceneManager.LoadedLevelName == scene)
            return;
        wwwProgress = Progress.Instance.CreateWWW();
        this.SceneName = scene;
        disa = Observable.FromCoroutine(() => LoadLevelBundle(scene)).Subscribe();
    }

    private IEnumerator LoadLevelBundle(string scene)
    {
        WWW www = new WWW(CDirectory.MakeFullWWWPath(string.Format("res/scenes/{0}.scene", scene)));
        while (!www.isDone)
        {
            progress = www.progress;
            yield return null;
        }
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            LOG.LogError("WWW download:" + www.error + "  path :  " + www.url);
            yield break;
        }
        if (www.isDone)
        {
            wwwProgress.Done();
            isDone = true;
            SceneAB = www.assetBundle;
            www.Dispose();
            if (AutoLoadScene)
                CSceneManager.LoadSceneAsync(this.SceneName);
        }
    }

    public bool LoadLevel(string scene)
    {
        if (SceneName == scene)
        {
            if (isDone)
                CSceneManager.LoadSceneAsync(this.SceneName);
            else
                AutoLoadScene = true;
            return true;
        }
        return false;
    }
}

public class CSceneManager : MonoSingleton<CSceneManager>
{

    public SceneAssetBundle SceneAB;
    public float progress;
    public float wwwprogress
    {
        get
        {
            if (SceneAB != null)
                return SceneAB.progress;
            return 0;
        }
    }
    /// <summary>
    /// 目标场景（过图-------》目标场景）
    /// </summary>
    public string targetLevel;

    /// <summary>
    /// 当前应该加载的场景
    /// </summary>
    public string loadLevel;
    void Start()
    {
        SceneManager.sceneLoaded += OnLevelWasLoaded;
    }

    private void OnLevelWasLoaded(Scene scene,  LoadSceneMode mode)
    {
        if (scene.name == targetLevel)
            DestroyScene();
        else if (scene.name == SceneName.ASYNC_LOADER_SCENE)
            LoadLevel(targetLevel);

        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    public void LoadLevel(string scene)
    {
        if (string.IsNullOrEmpty(scene))
            return;
        loadLevel = scene;
        targetLevel = scene;
        if (SceneAB != null)
        {
            if (SceneAB.LoadLevel(targetLevel))
                return;
            DestroyScene();
        }
        if (LoadedLevelName == SceneName.ASYNC_LOADER_SCENE)
        {
            if (SceneName.InUnity(targetLevel))
                CSceneManager.LoadScene(targetLevel);
            else
                LoadLevelAssetBundle(targetLevel, true);
            return;
        }
        CSceneManager.LoadScene(SceneName.ASYNC_LOADER_SCENE);

        Progress.Instance.Dispose();
    }

    public bool LoadLevelAssetBundle(string scene, bool autoload = false)
    {
        if (LoadedLevelName == scene)
            return false;
        loadLevel = scene;
        SceneAB = new SceneAssetBundle(scene);
        if (autoload)
            SceneAB.LoadLevel(scene);
        return true;
    }

    public void DestroyScene()
    {
        if (SceneAB != null)
        {
            SceneAB.Dispose();
            SceneAB = null;
        }
    }

    #region 禁止外部使用
    public static void LoadScene(string name)
    {
        if (LoadedLevelName == name)
            return;
        Global.scene_mgr.loadLevel = name;
        //这里因为事件 所以loadLevel可能会发生变化
        //Global.scene_mgr.FireEvent(new CEvent.LoadLevelBegin(name));
        if (Global.scene_mgr.loadLevel == name)
            SceneManager.LoadScene(name);
    }

    public static void LoadSceneAsync(string name)
    {
        if (LoadedLevelName == name)
            return;
        Global.scene_mgr.loadLevel = name;
        //这里因为事件 所以loadLevel可能会发生变化
        //Global.scene_mgr.FireEvent(new CEvent.LoadLevelBegin(name));
        if (Global.scene_mgr.loadLevel == name)
            Progress.Instance.CreateAsync().async = SceneManager.LoadSceneAsync(name);
    }
    #endregion

    
    public static string LoadedLevelName
    {
        get
        {
            return SceneManager.GetActiveScene().name;
        }
    }
}
