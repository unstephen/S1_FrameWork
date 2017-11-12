using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Collections;
using UnityEngine.Rendering;
public class CBaseObject : ZRender.IRenderObject, IDisposable 
{
    #region IDisposable 接口
    public bool disposed;
    public virtual void Dispose()
    {
        if (disposed)
            return;
        disposed = true;
        CClientCommon.DestroyImmediate(this);
    }

    protected override void OnDestroy()
    {
        DestroyComponent();

        if (this.gameObject)
            CClientCommon.DestroyImmediate(gameObject);


        if (!Application.isEditor)
            GC.SuppressFinalize(this);
    }

    public static implicit operator bool(CBaseObject exists) 
    {
        return exists != null && exists.GameObject;
    }

    #endregion

    //>--------------------------------------------------------------------
    #region 一系列虚接口，供派生类重载
    public virtual void OnSelectBound() { }
    protected virtual void InitializeComponent() { }
    protected virtual void DestroyComponent() { }

    protected virtual void OnAllModelsLoadDone(){}
    #endregion
    //>--------------------------------------------------------------------
    public bool IsUI_obj { get; protected set; }
    public bool IsLoadOver;
    public string Name { get; protected set; }
    private float scale = 1.0f;
    public GameObject GameObject { get { return this.gameObject; } } 
    private Transform transform;
    public Transform Transform 
    {
        get 
        {
            if (transform != null)
                return transform;
            if (GameObject)
                transform = GameObject.transform;
            return transform;
        }
    }

    public GameObject RoleGameObject { get; private set; }

    public void SetScale(float scale)
    {
        if (scale < 0.001f || scale > 10f)
            return;
        this.scale = scale;
        SetScale(Vector3.one * this.scale);
    }

    public float GetScale() { return scale; }

    //>--------------------------------------------------------------------
    // 动画
    private Animator animator;
    public Animator Animator
    {
        get
        {
            if (!RoleGameObject)
                return null;
            if (animator)
                return animator;
            if (!animator)
            {
                animator = RoleGameObject.GetComponent<Animator>();
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }
            return animator;
        }
    }

    public void PlayAni(string statename)
    {
        if (Animator)
            Animator.CrossFade(statename, 0.2f, 0);
    }

    public CBaseObject()
    {
        this.gameObject = new GameObject();
    }

    protected override void OnCreate() 
    {
        if (disposed)
            return;
        if (!string.IsNullOrEmpty(Name))
            this.gameObject.name = Name;

        Object asset = this.GetOwner().GetAsset();
        this.RoleGameObject = UnityEngine.Object.Instantiate(asset) as GameObject;
        this.RoleGameObject.transform.parent = this.Transform;
        this.RoleGameObject.name = asset.name;
        InitializeComponent();
    }

    #region 特殊效果
    /// <summary>
    /// 死亡溶剂效果
    /// </summary>
    /// <param name="dissolvetex"></param>
    public void DoDeadEffect(Texture dissolvetex) {
        if (disposed)
            return;
     /*   DissolveBurn db = DissolveBurn.Begin(this.GameObject, 0.2f, 0, 1.2f);
        for (int i = 0; i < materials.Count; i++) {
            Material mat = materials[i];
            db.SetMats(mat, dissolvetex, Color.red);
        }*/
    }

    public void ChangeColor(Color color)
    {
    }

    /// <summary>
    /// 受伤效果
    /// </summary>
    public void EnterWoundColor()
    {
        ChangeColor(Color.white);
        AddTimer(0.15F, OnTimerLevelWoundColor, null, 0, 0);
    }

    private bool OnTimerLevelWoundColor(object obj, int p1, int p2)
    {
        LeaveWoundColor();
        return false;
    }

    private void LeaveWoundColor()
    {
        ChangeColor(Color.black);
    }

    #endregion
}