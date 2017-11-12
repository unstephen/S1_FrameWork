using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;
using System;
using Object = UnityEngine.Object;
public interface IGame
{
    void StartGame();
}

public class Launcher : MonoBehaviour {
    const string _DllPath = "res/Game.dll";
    const string _MDBPath = "res/Game.dll.mdb";
    const string language = "res/language/language.txt";
    protected Progress.AssetProgress.ItemProgress progress;
    IGame game;
    void Awake()
    {
        InitEventSystem();
        StartCoroutine(WWWLoadLanguage());
    }

    private void LoadDll()
    {
        progress = Progress.Instance.CreateItem();
        progress.Done();
        if (Application.isEditor)
            LoadAssembly();
        else
            StartCoroutine(WWWLoadAssembly());

        //加载UI
        GameObject launcherui = UnityEngine.Object.Instantiate(Resources.Load("UI/Login/UIPrefab/Launcher")) as GameObject;
        launcherui.AddComponent<CLauncherUI>();
    }

    IEnumerator WWWLoadLanguage()
    {
        yield return null;
        string path = CDirectory.MakeCachePath(language);
        if (!File.Exists(path) || Application.isEditor)
        {
            using (var www = new WWW(CDirectory.MakeWWWStreamPath(language)))
            {
                yield return www;
                if (!string.IsNullOrEmpty(www.error))
                {
                    throw new Exception(string.Format("WWW download:" + www.error + "  path :  " + www.url));
                }
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, www.bytes);
            }
        }

        byte[] bytes = File.ReadAllBytes(path);
        Localization.language = "language";
        Localization.LoadCSV(bytes);

        LoadDll();
    }

    private void InitEventSystem()
    {
        EventSystem es = FindObjectOfType<EventSystem>();
        if (es == null)
        {
            GameObject eventsystem = new GameObject("EventSystem");
            Object.DontDestroyOnLoad(eventsystem);
            es = eventsystem.AddComponent<EventSystem>();
            StandaloneInputModule sim = eventsystem.AddComponent<StandaloneInputModule>();
            sim.forceModuleActive = true;
        }
    }

    #region PC环境下 加载DLL
    //private void LoadLanguage()
    //{
    //    byte[] bytes = File.ReadAllBytes(CDirectory.MakeFilePath(language));
    //    Localization.language = "language";
    //    Localization.LoadCSV(bytes);
    //}

    void LoadAssembly()
    {
        string mdb_path;
        string path = GetAssemblyPath(out mdb_path);
        System.Reflection.Assembly assembly = null;
        if (path.IndexOf(_DllPath) == -1)
        {
            FileStream stream = File.Open(path, FileMode.Open);
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);
            stream.Close();
            if (string.IsNullOrEmpty(mdb_path) && File.Exists(mdb_path))
            {
                FileStream mdb_stream = File.Open(mdb_path, FileMode.Open);
                byte[] mdb_buffer = new byte[mdb_stream.Length];
                mdb_stream.Read(mdb_buffer, 0, (int)mdb_stream.Length);
                mdb_stream.Close();
                assembly = System.Reflection.Assembly.Load(buffer, mdb_buffer);
            }
            else
            {
                assembly = System.Reflection.Assembly.Load(buffer);
            }
        }
        else
        {
#if ZTK
            assembly = ZToolKit.LoadAssembly(path);
#endif
        }
        this.game = (IGame)assembly.CreateInstance("CGame");
        this.game.StartGame();
    }

    //---------------------------------------------------
    static string GetAssemblyPath(out string mdb_path)
    {
        mdb_path = "";
        string path = CDirectory.MakeFilePath(_DllPath);
        if (!Application.isMobilePlatform)
        {
            path = Application.dataPath + "/../Library/ScriptAssemblies/" + "Assembly-CSharp.dll";
            if (File.Exists(path))
                mdb_path = Application.dataPath + "/../Library/ScriptAssemblies/" + "Assembly-CSharp.dll.mdb";
            else//client_build的路径
            {
                path = CDirectory.MakeFilePath(_DllPath);
                mdb_path = CDirectory.MakeFilePath(_MDBPath);
            }
        }
        return path;
    }
    #endregion

    #region 手机平台加载DLL
    IEnumerator WWWLoadAssembly()
    {
        yield return null;
        string path = CDirectory.MakeCachePath(_DllPath);
        if (!File.Exists(path))
        {
            using (var www = new WWW(CDirectory.MakeWWWStreamPath(_DllPath)))
            {
                yield return www;
                if (!string.IsNullOrEmpty(www.error))
                {
                    throw new Exception(string.Format("WWW download:" + www.error + "  path :  " + www.url));
                }
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, www.bytes);
            }
        }
        System.Reflection.Assembly assembly = null;
#if ZTK
            assembly = ZToolKit.LoadAssembly(path);
#else
        FileStream stream = File.Open(path, FileMode.Open);
        byte[] buffer = new byte[stream.Length];
        stream.Read(buffer, 0, (int)stream.Length);
        stream.Close();
        assembly = System.Reflection.Assembly.Load(buffer);
#endif

        this.game = (IGame)assembly.CreateInstance("CGame");
        this.game.StartGame();
    }
    #endregion
}
