using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Object = UnityEngine.Object;
using LitJson;
public class CMapObject : CBaseObject
{
    public int apprid_;
    public int refid { get; private set; }
    public Vector3 position { get; private set; }

    /// <summary>
    /// Dispose的时候，把自己从MapObjectList里去掉
    /// </summary>
    public override void Dispose()
    {
        base.Dispose();
    }

    public static void DestroyAll()
    {
        MapObjectList.Clear();
    }

    /*public static CMapObject Create(JsonData data)
    {
        int RefID = Convert.ToInt32(data["templet_id"]);
        MapObjectReference mapgoref = Global.mapobj_mgr.GetReference(RefID);
        if (mapgoref == null)
        {
            Debug.LogError(string.Format("The map object not exist：{0}", RefID));
            return null;
        }
        CMapObject mo = CResourceFactory.CreateInstance<CMapObject>(string.Format("res/role/{0}.role", mapgoref.Appearance), null, data, mapgoref);
        return mo;
    }*/

    protected override void OnCreate()
    {
        base.OnCreate();
    }

    /*public CMapObject(JsonData data, MapObjectReference mapgoref)
    {
        this.ActorData = new PlayActor();
        this.ActorData.SN = Convert.ToInt64(data["SN"]);
        this.reference = mapgoref;
        if (reference == null)
        {
            Debug.LogError(string.Format("The map object not exist：{0},please check 'MapObjectReference' config.May be the 'MapObjectReference' config in server and client are not synchronized,please synchronized.", refid));
            Dispose();
            return;
        }
        this.refid = mapgoref.Id;
        this.ActorData.Movedata.X = Convert.ToInt32(data["pos"]["x"]);
        this.ActorData.Movedata.Z = Convert.ToInt32(data["pos"]["y"]);
        this.ActorData.Movedata.Dir_X = Convert.ToSingle(data["forward"]["x"]);
        this.ActorData.Movedata.Dir_Z = Convert.ToSingle(data["forward"]["y"]);

        Vector3 navpos;
        CClientCommon.GetNavMeshPostion(out navpos, new Vector3(this.ActorData.Movedata.X, 0f, this.ActorData.Movedata.Z), Global.world.NavLayerMask);
        position = navpos;
        SetPosition(navpos);

        float x = this.ActorData.Movedata.Dir_X;
        float z = this.ActorData.Movedata.Dir_Z;
        if (x != 0 || z != 0)
            SetForward(new Vector3(x, 0, z).normalized);

        this.Name = string.Format("XYMapObject{0}_{1}", this.ActorData.SN, this.refid);

        MapObjectList.Add(this.ActorData.SN, this);
        SetLayer(CDefines.Layer.Player);
    }*/

    protected override void OnAllModelsLoadDone()
    {
        base.OnAllModelsLoadDone();
        CategorySettings.Attach(base.Transform, "_roles/_map_objects/", false);
    }

    protected override void InitializeComponent()
    {
        base.InitializeComponent();

     /*   if (this.reference == null)
            return;
        if (reference.ShowName)
            CWorld._FireEvent(new CEvent.MapObjectAppear(ActorData.SN));*/
    }
}

public static class MapObjectList
{
    private static Dictionary<long, CMapObject> dict_ = new Dictionary<long, CMapObject>();
    static MapObjectList() { }
    public static void Add(long sn, CMapObject mo)
    {
        if (dict_.ContainsKey(sn))
        {
            CMapObject old = dict_[sn];
            old.Dispose();
            dict_.Remove(sn);
            LOG.Erro("MapObjectDict Contains same MapObject SN = " + sn);
        }
        dict_[sn] = mo;
    }

    /// <summary>
    /// 按距离排序
    /// </summary>
    public static void DictonarySort()
    {
       /* if (!OperateCD.IsOperate("DictonaryMapSort", 3000f))
            return;
        var dicSort = from d in dict_

                      orderby CMisc.GetDimDistance(d.Value.position, Global.MainPlayer.position)

                      ascending

                      select d;

        dict_ = dicSort.ToDictionary(p => p.Key, o => o.Value);*/
    }

    public static IEnumerable<CMapObject> GetMapObjectEnumerator()
    {
        foreach (var kvp in dict_)
        {
            if (null == kvp.Value)
                continue;
            yield return kvp.Value;
        }
    }

    public static IEnumerable<CMapObject> GetAllByRefId(int refId)
    {
        if (dict_.Count == 0 || refId <= 0)
        {
            return null;
        }
        var list = new List<CMapObject>(dict_.Count);
        foreach (CMapObject mo in dict_.Values)
        {
            if (mo.refid == refId)
            {
                list.Add(mo);
            }
        }
        return list;
    }

    /// <summary>
    ///  删除一个CMapObject
    /// </summary>
    /// <param name="sn"></param>
    public static void Remove(long sn)
    {
        CMapObject mo;
        if (dict_.TryGetValue(sn, out mo))
        {
            dict_.Remove(sn);
            mo.Dispose();
        }
    }

    public static void Clear()
    {
        if (dict_.Count > 0)
        {
            var moList = new CMapObject[dict_.Count];
            dict_.Values.CopyTo(moList, 0);
            dict_.Clear();
            foreach (CMapObject mo in moList)
            {
                mo.Dispose();
            }
        }
    }

    public static CMapObject FindBySN(long sn)
    {
        CMapObject mo;
        if (dict_.TryGetValue(sn, out mo))
        {
            return mo;
        }
        return null;
    }

    public static CMapObject FindByReferenceID(int vID)
    {
        foreach (var kv in dict_)
        {
            if (kv.Value.refid == vID)
                return kv.Value;
        }
        return null;
    }

    public static CMapObject Find(Predicate<CMapObject> pred)
    {
        foreach (var kvp in dict_)
        {
            if (pred(kvp.Value))
            {
                return kvp.Value;
            }
        }
        return null;
    }

    public static void Update()
    {
        foreach (CMapObject m in dict_.Values)
        {
            if (m != null)
                m.Update();
        }
    }
}
