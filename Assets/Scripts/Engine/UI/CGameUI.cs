#if true
using System;
using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine.UI;
[System.AttributeUsage( System.AttributeTargets.Class )]
public class CGameUIViewportAttribute : System.Attribute 
{
    public CGameUIViewportAttribute(string name)
        : this(name, string.Empty) 
    {
    }

    public CGameUIViewportAttribute(string name, string content) 
    {
        this.name = name;
        this.content = content;
    }

    public string Content { get { return content; } }
    public string Name { get { return name; }}
    private string name;
    private string content;
}


/// <summary>
///  游戏UI基类，控制UI相关逻辑处理
/// </summary>
public abstract class CGameUI : CGameUIBase, IDisposable
{
    public abstract bool isFullScreen { get; }
    public abstract int Layer { get; }
    /// <summary>
    /// 当顶替该界面的界面被关闭后是否需要自动加载该界面
    /// </summary>
    public abstract bool autoLoad { get; }

    public string Name { get; protected set; }
    public CGameUIAsset asset { get; private set; }

    public Map<Type, bool> ClosedDic = new Map<Type, bool>();

    public void AddCloseUI(CGameUI ui)
    {
        ui.UIClose();
        ClosedDic[ui.GetType()] = ui.autoLoad;
    }

    public void SetName(string name) 
    {
        this.Name = name;
        this.gameObject.name = name;
    }

    public void SetAsset(CGameUIAsset asset) 
    {
        this.asset = asset;
        if (this.asset != null)
            this.asset.dontDestoryOnLoad = _dontDestoryOnLoad;
    }

    public int index = -1;
    public object context; //加载时传入的数据

    public float showTime;
    //public List<string> Textures = new List<string>();
    protected CGameUI() { } // disable public call
    public void SetPosition(Vector3 pos) 
    {
        this.transform.position = pos;
    }

#if false
    public virtual void ProLoadTextures() { }
#endif

    private NGUILink _nguiLink = null;
    public NGUILink nguiLink
    {
        get 
        {
            if(_nguiLink == null)
                _nguiLink = gameObject.GetComponent<NGUILink>();

            return _nguiLink;
        }
    }

    private bool _dontDestoryOnLoad = false;
    public bool dontDestoryOnLoad 
    {
        get 
        {
            return _dontDestoryOnLoad;
        }
        set 
        {
            _dontDestoryOnLoad = value;
            if (this.asset != null)
                this.asset.dontDestoryOnLoad = _dontDestoryOnLoad;
        }
    }

    public void Show() 
    {
       if(UIShow())
           Global.ui_mgr.CloseActiveUIs(this);
    }


    private bool UIShow()
    {
        if (disposed)
            return false;
        if (!IsShow())
        {
            if (gameObject)
                gameObject.SetActive(true);
            return true;
        }
        return false;
    }

    public virtual void Close()
    {
        if (UIClose())
        {
            foreach(var kvp in ClosedDic)
            {
                string ui_name = Global.ui_mgr.GetNameByType(kvp.Key);
                CGameUI ui = Global.ui_mgr.Get(ui_name);
                if (ui)
                    ui.UIShow();
                else if(kvp.Value)
                    Global.ui_mgr.LoadUI(kvp.Key);
            }
            ClosedDic.Clear();
        }
    }


    private bool UIClose()
    {
        if (disposed)
            return false;
        if (IsShow())
        {
            if (gameObject)
                gameObject.SetActive(false);
            return true;
        }
        return false;
    }

    public bool IsShow() 
    {
        if (disposed)
            return false;
        if (gameObject)
            return gameObject.activeInHierarchy;
        else
            return false;
    }

    public virtual void Awake() { }
    public virtual void Update() { }
    public virtual void LateUpdate() { }

    public virtual void LoadUICallback() { }

    //注：为了防止派生类忘记调用基类的OnEnable与OnDisable，不让派生类重写这两个接口，而是分别重写OnUIEnable与OnUIDisable接口
    protected void OnEnable() 
    {
        showTime = GameTimer.time;
        OnUIEnable();
        if (IsShow())
        {
            if (needCloseCamera)
                RenderSettings.fog = false;
            StartCoroutine(DeLayDo());
        }
    }

    public virtual bool needCloseCamera
    { 
        get 
        {
            if (!Global.IsInGame)
                return false;
            if (Layer == CUILayer.Free)
                return false;
            if (!isFullScreen)
                return false;
            return true;
        }
    }

    IEnumerator DeLayDo() 
    {
        yield return new WaitForSeconds(0.2f);
        if (!IsShow())
            yield break;
    }

    protected void OnDisable()
    {
        OnUIDisable();
    }

    public virtual void OnUIEnable() { }

    public virtual void OnUIDisable() { }

    public virtual void OnDestroy() 
    {
        Dispose();
    }

    public bool disposed { private set; get; }
    public bool isDispose()
    {
        return disposed;
    }
    // 
    public void Dispose() 
    {
        context = null;
        if (disposed)
            return;
        Dispose(true);
        //LOG.Debug("{0} Dispose", GetType().Name);
        // 暂时这样
        if (Global.ui_mgr != null)
            Global.ui_mgr.Remove(this);

        if (this.gameObject != null)
            this.gameObject.SetActive(false);

        //删除UI对象，必须放最后
        if (this.asset != null) {
            this.asset.Destroy();
            this.asset = null;
        } else {
            UnityEngine.Object.Destroy(this.gameObject);
        } 
        disposed = true;
    }
    protected virtual void Dispose(bool whatever) { }


    public virtual void OnLoadLevelBegin()
    {
        if (gameObject)
            gameObject.SetActive(false);
    }


    public static void Create<T>(object cb_ud = null) where T : CGameUI 
    {
      //  Global.ui_mgr.LoadUI<T>(cb_ud);
    }


    //>-------------------------------------------------------------------------------------------

    /// <summary>
    /// 初始化/更新sprite
    /// </summary>
    /// <param name="sprite"></param>
    /// <param name="spname"></param>
    /// <param name="spritename"></param>
    public void CreateSprite(CImage sprite, string spname, string spritename) 
    {
        if (this.asset == null)
            return;
        sprite.enabled = false;           
        sprite.SpriteName = spritename;
        this.asset.CreateSPInstance(sprite, spname);
    }

    /// <summary>
    /// 生成并初始化texture
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="filename"></param>
    /// <returns></returns>
    public CRawImageObject CreateImage(RawImage image, string filename)
    {
        if (this.asset == null) 
            return null;
        image.enabled = false;
        return this.asset.CreateImage(image, filename);
    }

    // 生成特效对象
    public CEffectObject CreateEffect(string filename, Transform parent, Vector3 position, Vector3 scale, Vector3 euler_angle)
    {
        if (this.asset == null) return null;
        return this.asset.CreateEffect(filename, parent, position, scale, euler_angle);
    }
}

public static class CUIBehaviour_Extension 
{
    public static void CreateSprite(this CUIBehaviour self, CImage sprite, string spname, string spritename) 
    {
        CGameUI root = self.GetRoot<CGameUI>();
        if (root == null) 
            return;
        root.CreateSprite(sprite, spname, spritename);
    }

    public static void CreateImage(this CUIBehaviour self, RawImage texture, string name)
    {
        CGameUI root = self.GetRoot<CGameUI>();
        if (root == null)
            return;
        root.CreateImage(texture, name);
    }

    public static CEffectObject CreateEffect(this CUIBehaviour self, string filename, Transform parent, Vector3 position, Vector3 scale, Vector3 euler_angle)
    {
        CGameUI root = self.GetRoot<CGameUI>();
        if (root == null)
            return null;
        return root.CreateEffect(filename, parent, position, scale, euler_angle);
    }
}
#endif