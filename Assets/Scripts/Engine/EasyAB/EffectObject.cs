using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// 特效对象
public class CEffectObject : ZRender.IRenderObject 
{
    private float start_time, duration_time = 0;
    private bool delayShow = false;
    private bool isscaled;
    private float ParticleScale = 1;
    public bool isDone { private set; get; }
    protected override void OnCreate() {
        this.gameObject = UnityEngine.Object.Instantiate(this.GetOwner().GetAsset()) as GameObject;
       
        DestoryByTime timeDesCompoment = gameObject.GetComponent<DestoryByTime>();
        if (timeDesCompoment != null) {
            start_time = GameTimer.time;
            duration_time = timeDesCompoment.time;
            CClientCommon.DestroyImmediate(timeDesCompoment);
        }

        if(delayShow)
            DelayShow();

        SetParticleScale(ParticleScale);

        isDone = true;
    }

    protected override void OnDestroy() 
    {
        if (this.gameObject != null) 
        {
            UnityEngine.Object.DestroyImmediate(this.gameObject, true);
            this.gameObject = null;
        }
    }

    public override void SetScale(Vector3 scale)
    {
        base.SetScale(Vector3.one);
        SetParticleScale(scale.x);
    }

    public void SetParticleScale(float scale)
    {
        ParticleScale = scale;
        if (isscaled)
            return;
        if (!Mathf.Approximately(ParticleScale, 1) && this.gameObject)
            isscaled = true;
        ApplyParticleScale(ParticleScale);
    }

    protected override void OnUpdate() {
        base.OnUpdate();

        if (start_time + duration_time <= GameTimer.time && start_time != 0) 
        {
            CClientCommon.DestroyImmediate(this);
            start_time = duration_time = 0;
        }
    }

    public void DelayShow()
    {
        delayShow = true;
        if(this.gameObject)
        {
            ParticleSystem[] psarray = this.gameObject.GetComponentsInChildren<ParticleSystem>();
            if (psarray!=null)
            {
                for (int i = 0; i < psarray.Length; i++)
                {
                    ParticleSystem ps = psarray[i];
                    if (ps)
                    {
                        ParticleSystem.MainModule MainModule = ps.main;
                        float size = MainModule.startSize.constant;
                        MainModule.startDelay = 1;
                    }
                }
            }
        }
    }
}

//玩家点击地面的特效
public class CPlayerTouchEffect : CEffectObject {
    private Vector3 effectPos = Vector3.zero;
    public CPlayerTouchEffect(Vector3 pos) {
        effectPos = pos;
    }

    protected override void OnCreate() {
        base.OnCreate();
        CategorySettings.Attach(gameObject.transform, "_effect/");
        ShowTouchEffect(effectPos);
    }

    public void ShowTouchEffect(Vector3 pos) {
        pos.y += 0.1f;
        SetPosition(pos);
        SetScale(Vector3.one);
        SetLayer(CDefines.Layer.Terrain);
    }
}

public class CSelectEffect : CEffectObject
{
    public static CSelectEffect Selectgo { private set; get; }
    public static CBaseObject SelectBO { private set; get; }
    public static void Create(CBaseObject bo)
    {
        if (Selectgo == null)
            Selectgo = CResourceFactory.CreateInstance<CSelectEffect>(ConstFilePathDefine.SelectEffect, null);
        Selectgo.SetOwner(bo);
        SelectBO = bo;
    }

    protected override void OnCreate()
    {
        base.OnCreate();
        LoopKit kit = this.gameObject.AddComponent<LoopKit>();
        kit.Set(this);
    }

    public static void DestroyGO()
    {
         if (Selectgo != null)
         {
             Selectgo.Destroy();
             Selectgo = null;
         }
    }

    private CBaseObject owner;

    public void SetOwner(CBaseObject bo)
    {
        owner = bo;
        if (this.gameObject)
            this.gameObject.SetActive(true);
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        if (this.owner && owner.IsVisible())
        {
            Vector3 pos = this.owner.GetPosition();
            pos.y += 0.2f;
            SetPosition(pos);
            if (this.gameObject)
                this.gameObject.SetActive(true);
        }
        else if (this.gameObject)
            this.gameObject.SetActive(false);
    }
}