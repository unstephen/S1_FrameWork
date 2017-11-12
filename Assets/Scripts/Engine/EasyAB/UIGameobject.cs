using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Model;

/// <summary>
/// 用于UIGameObject的驱动
/// </summary>
public class LoopKit : MonoBehaviour 
{
    private ZRender.IRenderObject obj;
    public void Set(ZRender.IRenderObject obj) 
    {
        this.obj = obj;
    }

    void Update() 
    {
        if (this.obj != null)
            this.obj.Update();
    }

    void LateUpdate() 
    {
        if (this.obj != null)
            this.obj.LateUpdate();
    }
}



