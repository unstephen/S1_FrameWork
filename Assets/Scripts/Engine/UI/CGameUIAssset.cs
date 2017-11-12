using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleSpritePacker;
using UnityEngine.UI;
using System.Linq;
using System.Text;

public class CGameUIAsset : ZRender.IRenderObject
{
    private Map<string, CSimpleSpriteObject> spriteDic = new Map<string, CSimpleSpriteObject>();
    private List<CRawImageObject> imageDic = new List<CRawImageObject>();

    public delegate void Callback(CGameUIAsset entity);
    protected Callback mCachedCallback;

    private bool _dontDestoryOnLoad = false;
    public bool dontDestoryOnLoad
    {
        set
        {
            _dontDestoryOnLoad = value;
            if (this.gameObject != null && value)
                UnityEngine.Object.DontDestroyOnLoad(this.gameObject);
        }
        get { return _dontDestoryOnLoad; }
    }

    public void RegisterCompleteCallback(Callback cb)
    {
        this.mCachedCallback = cb;
    }

    protected override void OnDestroy()
    {
        CClientCommon.DestroyImmediate(this.gameObject);

        foreach (var sp in spriteDic.Values)
        {
            if (sp != null)
                sp.Destroy();
        }
        spriteDic.Clear();

        foreach (var tex in imageDic)
        {
            if (tex != null)
                tex.Destroy();
        }
        imageDic.Clear();
    }

    protected override void OnCreate()
    {
        this.gameObject = UnityEngine.Object.Instantiate(this.GetOwner().GetAsset()) as GameObject;
        this.gameObject.SetActive(false);
        if (_dontDestoryOnLoad)
            UnityEngine.Object.DontDestroyOnLoad(this.gameObject);
        LoadAsset(this.gameObject);
    }

    protected void LoadAsset(GameObject go)
    {
        CText[] texts = go.GetComponentsInChildren<CText>(true);
        for (int i = 0; i < texts.Length; ++i)
        {
            CText t = texts[i];
            if (t.FontName == Global.uifont.name)
                t.font = Global.uifont;
            else
                t.font = Global.uifont_title;
        }

        List<CImage> imagelist = new List<CImage>();
        CImage[] images = go.GetComponentsInChildren<CImage>(true);
        for (int i = 0; i < images.Length; ++i) 
        {
            CImage ci = images[i];
            if (string.IsNullOrEmpty(ci.AtlasName))
                continue;
            imagelist.Add(ci);
            CreateSPInstance(ci.AtlasName);
        }
        AddTimer(OnAllSpritesCompleteEvent, imagelist, 0, 0);
    }

    private void CreateSPInstance(string AtlasName)
    {
        AtlasName = AtlasName.ToLower();
        if (spriteDic.ContainsKey(AtlasName))
            return;
        spriteDic[AtlasName] = CResourceFactory.CreateInstance<CSimpleSpriteObject>(string.Format("res/ui/sprite/{0}.sp", AtlasName), null);
    }

    private bool OnAllSpritesCompleteEvent(object obj, int p1, int p2)
    {
        foreach (var sp in spriteDic.Values)
        {
            if (!sp.complete)
                return true;
        }
        List<CImage> images = obj as List<CImage>;
        for (int i = 0; i < images.Count; ++i)
        {
            CImage image = images[i];
            if(spriteDic.ContainsKey(image.AtlasName))
            {
                CSimpleSpriteObject Spriteobj = spriteDic[image.AtlasName];
                image.sprite = Spriteobj.GetSPInstance().GetSprite(image.SpriteName);
            }
        }
        if (mCachedCallback != null)
        {
            mCachedCallback(this);
            mCachedCallback = null;
        }
        return false;
    }


    /// <summary>
    /// 生成并替换sprite中的atlas
    /// </summary>
    /// <param name="image"></param>
    /// <param name="spname"></param>
    /// <returns></returns>
    public CSimpleSpriteObject CreateSPInstance(CImage image, string spname)
    {
        if (string.IsNullOrEmpty(spname))
            return null;
        spname = spname.ToLower();
        CSimpleSpriteObject Spriteobj;
        if (!this.spriteDic.TryGetValue(spname, out Spriteobj))
        {
            Spriteobj = CResourceFactory.CreateInstance<CSimpleSpriteObject>(string.Format("res/ui/sprite/{0}.sp", spname), null);
            spriteDic[spname] = Spriteobj;
        }
        Spriteobj.SetImage(image);
        return Spriteobj;
    }

    public CRawImageObject CreateImage(RawImage image, string name)
    {
        name = name.ToLower();
        CRawImageObject imageobj = CResourceFactory.CreateInstance<CRawImageObject>(string.Format("res/ui/tex/{0}.tex", name), null);
        imageobj.SetTexture(image);
        imageDic.Add(imageobj);
        return imageobj;
    }

    // 生成特效对象
    public CEffectObject CreateEffect(string filename, Transform parent, Vector3 position, Vector3 scale, Vector3 euler_angle)
    {
        filename = filename.ToLower();
        CEffectObject obj = CResourceFactory.CreateInstance<CEffectObject>(filename, this);
        obj.SetParentTransform(parent);
        obj.SetPosition(position);
        obj.SetScale(scale);
        obj.SetRotation(euler_angle);
        return obj;
    }

    public SPInstance GetSPInstance(string spname)
    {
        spname = spname.ToLower();
        if (spriteDic.ContainsKey(spname))
        {
            CSimpleSpriteObject so = spriteDic[spname];
            if (so != null)
                return so.GetSPInstance();
            return null;
        }
        return null;
    }

    public Sprite GetSprite(string sname, string spname)
    {
        spname = spname.ToLower();
        SPInstance SPInstance = GetSPInstance(sname);
        if (SPInstance != null)
            return SPInstance.GetSprite(spname);
        return null;
    }
}


public class CRawImageObject : ZRender.IRenderObject
{
    public Texture texture { get; private set; }
    protected RawImage rawImage;
    protected override void OnCreate() 
    {
        this.texture = this.GetOwner().GetAsset() as Texture;
        if (this.rawImage != null)
        {
            SetTexture(this.rawImage);
            this.rawImage = null;
        }
    }

    protected override void OnDestroy()
    {
        this.texture = null;
    }

    public virtual void SetTexture(RawImage image) 
    {
        if (image && this.texture)
        {
            image.enabled = true;
            image.texture = this.texture;
            if (image.material)
                image.material.SetTexture("_MainTex", texture);
            return;
        }
        this.rawImage = image;
    }
}


public class CSimpleSpriteObject : ZRender.IRenderObject 
{
    private SPInstance instance = null;
    private List<CImage> imagelist = new List<CImage>();

    protected override void OnDestroy()
    {
        this.instance = null;
        this.imagelist.Clear();
        this.imagelist = null;
    }

    protected override void OnCreate() 
    {
        this.instance = this.GetOwner().GetAsset() as SPInstance;
        for (int i = 0; i < this.imagelist.Count; i++)
            SetImage(this.imagelist[i]);
        this.imagelist.Clear();
    }

    public void SetImage(CImage image) 
    {
        if (!image)
            return;

        if (instance != null) 
        {
            image.sprite = instance.GetSprite(image.SpriteName);
            image.enabled = true;         
        }
        else
            this.imagelist.Add(image);
    }


    public SPInstance GetSPInstance()
    {
        return instance;
    }
}
