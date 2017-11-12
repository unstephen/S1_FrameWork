using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using LitJson;
using UniRx;
using System.Net; 
public partial class ReferenceManager<T> : IDisposable where T : PropDefine , new() 
{
    protected Dictionary<string, T> dict_ = new Dictionary<string, T>();
    public void Dispose() { }

    public T GetReference(long refid)
    {
        return GetReference(refid.ToString());
    }

    public T GetReference(string key)
    {
        T obj;
        this.dict_.TryGetValue(key, out obj);
        if (obj == null)
        {
            if(obj!=null) //找到了才缓存
                this.dict_[key] = obj;
        }
        if (obj != null)
            obj.LoadConfig();
        return obj;
    }

    public List<T> GetReferences(params object[] args)
    {
        List<T> objs = new List<T>();
        for (int i = 0; i < args.Length; i++)
            objs.Add(GetReference(args[i].ToString()));
        for (int i = 0; i < objs.Count; i++)
        {
            if (objs[i] != null)
                objs[i].LoadConfig();
        }
        return objs;
    }


    public void ForEach( Action<T> action ) 
    {
        if ( action == null ) 
            return;

        foreach (var obj in this.dict_.Values)
            action(obj);
        return;
    }

    public void Reset( )
    {
        this.dict_.Clear();
    }

    public IEnumerable<T> ForEach()
    {
        foreach (var obj in this.dict_.Values)
        {
            yield return obj;
        }
    }

    public void AddReference( long refid, T obj ) 
    {
        this.dict_[refid.ToString()] = obj;
    }
}