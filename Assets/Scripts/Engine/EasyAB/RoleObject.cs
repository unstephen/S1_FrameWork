using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Linq;
using System.Collections;
using UnityEngine.Rendering;
using LitJson;

public class CRoleObject : CBaseObject {

  
}
/*
public static class CRoleObjectDict
{
    private static Map<long, CRoleObject> Sn2Ro = new Map<long, CRoleObject>();
    public static void Add(CRoleObject ro) 
    {
        if (Sn2Ro.ContainsKey(ro.cr.SN))
        {
            LOG.Erro("CRoleObjectDict Contains same RoleObject SN = " + ro.cr.SN);
            CRoleObject old = Sn2Ro[ro.cr.SN];
            LOG.Debug(old.Name+"    "+  old.cr.Name );
            old.Dispose();
        }
        Sn2Ro[ro.cr.SN] = ro;
    }

    /// <summary>
    /// 禁止外部调用
    /// </summary>
    /// <param name="cr"></param>
    public static void Remove(CClientRole cr) 
    {
        if (cr == null)
            return;
        if (Sn2Ro.ContainsKey(cr.SN))
            Sn2Ro.Remove(cr.SN);
    }

    //保留该接口
    public static CRoleObject FindByClientRole(CClientRole cr) 
    {
        if (cr == null)
            return null;
        return FindBySN(cr.SN);
    }

    public static CRoleObject FindBySN(long sn) 
    {
        CRoleObject ro = null;
        Sn2Ro.TryGetValue(sn, out ro);
        return ro; ;
    }

    public static IEnumerable<CRoleObject> GetAll() 
    {
        return Sn2Ro.Values;
    }

    public static void ClearAll() 
    {
        Sn2Ro.Clear();
    }


    /// <summary>
    /// 按距离排序
    /// </summary>
    public static void DictonarySort()
    {
        if (!OperateCD.IsOperate("DictonarySort", 3000f))
            return;
        var dicSort = from d in Sn2Ro

                      orderby CMisc.GetDimDistance(d.Value.cr.position, Global.MainPlayer.position)

                      ascending

                      select d;

        Sn2Ro = new Map<long, CRoleObject>(dicSort.ToDictionary(p => p.Key, o => o.Value));
    }
}
*/