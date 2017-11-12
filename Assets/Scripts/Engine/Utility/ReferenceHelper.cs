using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using LitJson;
using UniRx;
using System;

public partial class PropDefine
{
    public int Id;
    public string Name;

    public virtual void LoadConfig() { }
}

[System.AttributeUsage(System.AttributeTargets.Class)]
public partial class RefAttribute : System.Attribute
{
    public string config;
    public string view;

    public RefAttribute(string config, string view)
    {
        this.config = config;
        this.view = view;
    }
}
internal static class RefDescribe<T>
{
    private static readonly Dictionary<int, RefAttribute> dict_ = new Dictionary<int, RefAttribute>();
    static RefDescribe()
    {
        foreach (FieldInfo fi in typeof(T).GetFields())
        {
            if (fi.FieldType == typeof(int) || fi.FieldType == typeof(short))
            {
                int v = Convert.ToInt32(fi.GetValue(null));
                var attr = Attribute.GetCustomAttribute(fi, typeof(RefAttribute)) as RefAttribute;
                if (attr != null)
                    dict_[v] = attr;
            }
        }
    }

    public static string GetPath(int v)
    {
        RefAttribute attr;
        if (dict_.TryGetValue(v, out attr))
            return attr.config;
        else
            return "";
    }
}

