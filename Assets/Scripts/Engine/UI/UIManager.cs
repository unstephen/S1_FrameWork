using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SimpleSpritePacker;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

/* UI Camera层取值范围
1.主界面、头顶文字、战斗飘字	0~19
2.系统面板(一级面板)			20~39			
3.系统子面板（例如tips）(二级面板)	40~59
4.压屏框（例如任务剧情对话）	60~79
5.弹出框（模态框）		80~99
6.跑马灯、系统公告		100~119
*/
public class CUILayer 
{
    /// <summary>
    /// 主界面(或者能和主界面共存的界面)
    /// </summary>
    public const int MainFace = 0;

    /// <summary>
    /// 全屏界面（系统界面 压屏....）
    /// </summary>
    public const int FullWindow = 1;

    /// <summary>
    /// 不受任何界面影响
    /// </summary>
    public const int Free = 2;
}

public class UISpriteName 
{

}

/// <summary>
///  UI管理器
/// </summary>
public class CUIManager : MonoSingleton<CUIManager>
{
    public const int MAX_UI_COUNT = 100;
    private UniqueIndex index_pool = new UniqueIndex(MAX_UI_COUNT);
    private CGameUI[] uis = new CGameUI[MAX_UI_COUNT];
    private Map<string, CGameUI> names = new Map<string, CGameUI>();
    private bool disposing = false;
    public const int DISPOS_TIME = 30;

    // 正在加载的UI列表
    private Map<string, CGameUIAsset> loading = new Map<string, CGameUIAsset>();

    private void AddLoading(string name, CGameUIAsset asset)
    {
        loading.Add(name, asset);
    }

    public bool IsLoading(string name)
    {
        return loading.ContainsKey(name);
    }

    public void RemoveLoading(string name)
    {
        loading.Remove(name);
    }


    public void StartTimer()
    {
        GameTimer.StartTimer(DISPOS_TIME, false, true, ClearByTime, "UIClearTimer");
    }

    private void ClearByTime(object sender, EventArgs e)
    {
        if (disposing)
            return;
        for (int i = 0; i < uis.Length; i++)
        {
            CGameUI ui = uis[i];
            if (null == ui)
                continue;
            if (ui.dontDestoryOnLoad)
                continue;
            if (ui.IsShow() || GameTimer.Within(ui.showTime, DISPOS_TIME)  || ui.Layer == CUILayer.Free)//|| ui.IsMainFace
                continue;
            Remove(ui);
            ui.Dispose();
        }
    }

    void OnDestroy()
    {
        this.UIDispose();
        this.disposing = true;
    }

    public void Add(CGameUI ui)
    {
        if (!index_pool.CanAlloc())
        {
            LOG.TraceRed("ERROR: ui数量超出上限。ui name:{0}", ui.Name);
            return;
        }
        int index = index_pool.Alloc();

        if (uis[index] != null)
            throw new Exception(string.Format("[CUIManager] ui index:{0} is already in use", index));
        if (names.ContainsKey(ui.Name))
            throw new Exception(string.Format("[CUIManager] ui name:{0} is already exist", ui.Name));
        uis[index] = ui;
        names[ui.Name] = ui;
        ui.index = index;
        ui.SetPosition(new Vector3((ui.index + 1) * 100, 0, 0));
        // 最后显示该ui
        ui.Show();
    }

    public static void OpenUI(string ui_type, object context = null)
    {
        Type uitype = Type.GetType(ui_type);
        if (uitype == null)
        {
            LOG.Erro("ui_type ERRO   " + ui_type);
            return;
        }
        if (Global.ui_mgr != null)
            Global.ui_mgr.LoadUI(uitype, context);
    }

    public CGameUI Get(string name)
    {
        CGameUI ui;
        if (names.TryGetValue(name, out ui))
            return ui;
        else
            return null;
    }

    public T Get<T>() where T : CGameUI
    {
        Type ui_type = typeof(T);
        string ui_name = rg.Match(ui_type.Name).Value;
        return Get(ui_name) as T;
    }

    public string GetNameByType(Type t)
    {
        string ui_name = rg.Match(t.Name).Value;
        return ui_name;
    }

    public void UIDispose()
    {
        for (int i = 0; i < uis.Length; ++i)
        {
            if (uis[i] != null)
                Remove(uis[i]);
        }
        names.Clear();
        foreach (var e in loading.Values)
        {
            CGameUIAsset asset = e as CGameUIAsset;
            CClientCommon.DestroyImmediate(ref asset);
        }
        loading.Clear();
    }

    public void Clear(bool reconnectToServer = false)
    {
        for (int i = 0; i < uis.Length; ++i)
        {
            if (uis[i] != null)
            {
                if (!uis[i].dontDestoryOnLoad)
                    Remove(uis[i]);
                else
                {
                    if (!reconnectToServer)
                        uis[i].OnLoadLevelBegin();
                }
            }
        }

        ArrayList keys = new ArrayList(loading.Keys);
        foreach (var k in keys)
        {
            string key = k as string;
            CGameUIAsset asset = loading[key];
            if (asset != null && !asset.dontDestoryOnLoad)
                CClientCommon.DestroyImmediate(ref asset);

            loading.Remove(key);
        }
    }


    public void Remove(CGameUI ui)
    {
        if (!ui || ui.disposed)
            return;
        if (ui.index < 0)
            return;
        uis[ui.index] = null; 
        ui.Close();
        names.Remove(ui.Name);
        index_pool.Free(ui.index);
        ui.index = -1;

        if (loading.ContainsKey(ui.Name))
        {
            CGameUIAsset asset = loading[ui.Name] as CGameUIAsset;
            CClientCommon.DestroyImmediate(ref asset);
            loading.Remove(ui.Name);
        }
        ui.Dispose();
        ui = null;
    }


    public void CloseActiveUIs(CGameUI other)
    {
        if (other.Layer == CUILayer.FullWindow)
        {
            for (int i = 0; i < uis.Length; i++)
            {
                CGameUI ui = uis[i];
                if (null == ui || !ui.IsShow() || other == ui)
                    continue;
                if (other.Layer >= ui.Layer)
                    other.AddCloseUI(ui);
            }
        }
    }


    void Update()
    {
        if (loading.Count > 0)
        {
            ArrayList keys = new ArrayList(loading.Keys);
            foreach (var k in keys)
            {
                string key = k as string;
                CGameUIAsset asset = loading[key] as CGameUIAsset;
                if (asset != null)
                    asset.Update();
            }

        }

        for (int i = 0; i < uis.Length; ++i)
        {
            if (uis[i] == null)
                continue;
            if (uis[i].asset != null)
                uis[i].asset.Update();
        }
    }

    /*public override void OnLevelWasLoaded()
    {
        base.OnLevelWasLoaded();
        Clear();
        string loadedLevelName = SceneManager.GetActiveScene().name;
        if (loadedLevelName == SceneName.ASYNC_LOADER_SCENE)
            CAsyncLevelLoaderUI.Create();
        else if (loadedLevelName == SceneName.LOGIN_SCENE)
            CLoginUI.Create();
        else if (loadedLevelName == SceneName.ROLE_SELECT_SCENE)
            CRoleSelectUI.Create();

        FireEvent(new CEvent.LevelWasLoaded(SceneManager.GetActiveScene().name));
    }*/


    //获取图标信息//
    public static string[] GetSpriteInfo(string res)
    {
        string[] icons = null;
        if (string.IsNullOrEmpty(res))
        {
            icons = new string[2] { "", "" };
            return icons;
        }
        icons = res.Split('/');
        return icons;
    }

    /// <summary>
    /// 检查是否点击到UI上了
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public static bool IsPointerOverUI(Vector2 point)
    {
        PointerEventData currentEventData = new PointerEventData(EventSystem.current);
        currentEventData.position = point;
        List<RaycastResult> result = new List<RaycastResult>();
        EventSystem.current.RaycastAll(currentEventData, result);
        return result.Count > 0;
    }



    #region UI加载相关
    //>--------------------------------------------------------------------
    // UI加载相关

    public Regex rg = new Regex("(?<=(" + "C" + "))[.\\s\\S]*?(?=(" + "UI" + "))", RegexOptions.Singleline);

    public void LoadUI<T>(object context = null, bool needwait = false) where T : CGameUI
    {
        LoadUI(typeof(T), context);
    }

    public void LoadUI(Type ui_type,object context = null, bool needwait = false)
    {
        if (ui_type == null || !ui_type.IsSubclassOf(typeof(CGameUI)))
            return;
        string ui_name = rg.Match(ui_type.Name).Value;
        CGameUI exists = Global.ui_mgr.Get(ui_name) as CGameUI;
        if (exists != null)
        {
            exists.context = context;//在界面还没有show前赋值所传的参数列表
            if (!exists.IsShow())
                exists.Show();
            exists.LoadUICallback();
            return;
        }
        // 防止重复加载
        if (IsLoading(ui_name))
            return;
        CGameUIAsset asset = CResourceFactory.CreateInstance<CGameUIAsset>(string.Format("res/ui/uiprefab/{0}.ui", ui_name).ToLower(), null);
        AddLoading(ui_name, asset);

        asset.RegisterCompleteCallback(delegate(CGameUIAsset e)
        {
            CGameUI ui = e.gameObject.AddComponent(ui_type) as CGameUI;
            ui.SetName(ui_name);
            ui.SetAsset(e);
            ui.context = context;
            Global.ui_mgr.Add(ui);
            ui.LoadUICallback();
            RemoveLoading(ui_name);
        });
    }
}
#endregion