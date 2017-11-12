using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor(typeof(NGUILink))]
public class NGUILinkEditor : Editor
{
    [MenuItem("GameObject/AddGameObjectToLink &n", false, 10000)]
    private static void AddGameObjectToLink()
    {
        UnityEngine.GameObject select = Selection.activeGameObject;
        if (select == null)
            return;
        if (!select.activeInHierarchy)
        {
            Debug.LogError(string.Format("【NGUILink】自动添加对象失败：请先激活需要添加的对象和NGUILink对象", select.name), select);
            return;
        }

        NGUILink link = FindInParents<NGUILink>(select.transform);
        if (link == null)
        {
            Debug.LogError(string.Format("【NGUILink】自动添加对象失败：{0}父节点无NGUILink组件", select.name), select);
            return;
        }

        if (link.gameObject == select && select.transform.parent != null)
            link = FindInParents<NGUILink>(select.transform.parent);
        if (link == null)
        {
            Debug.LogError(string.Format("【NGUILink】自动添加对象失败：{0}父节点无NGUILink组件", select.name), select);
            return;
        }
        link.ReBuildLinkMap();
        if (link.Get(select.name) == null)
        {
            NGUILink.UILink item = new NGUILink.UILink();
            item.Name = select.name;
            item.LinkObj = select;
            link.Links.Add(item);
            Debug.Log(string.Format("【NGUILink】自动添加对象成功：{0}  NGUILink:{1}", select.name, link.name), link);
        }
        else
            Debug.LogError(string.Format("【NGUILink】自动添加对象失败：已经存在重复名字{0}的对象  NGUILink:{1}", select.name, link.name), link);
    }

    string errostr = "Erro！！！Link丢失物件，检查";
    public override void OnInspectorGUI()
    {
        NGUILink link = target as NGUILink;
        GUI.changed = false;
        if (link.Links != null)
        {
            RegisterUndo("NGUILink Change", link);
            for (int i = 0; i < link.Links.Count; i++)
            {
                NGUILink.UILink uilink = link.Links[i];
                GameObject linkobj = uilink.LinkObj;

                if (!linkobj)
                {
                    uilink.Name = errostr;
                    continue;
                }
                if (linkobj)
                {
                    if (string.IsNullOrEmpty(uilink.Name) || uilink.Name == errostr)
                        uilink.Name = linkobj.name;
                    if (!uilink.component || uilink.component.gameObject != linkobj.gameObject)
                        uilink.component = linkobj.gameObject.GetComponent<MonoBehaviour>();
                }
            }
            EditorUtility.SetDirty(link);
        }
        base.OnInspectorGUI();
    }

    static public void RegisterUndo(string name, params Object[] objects)
    {
        if (objects != null && objects.Length > 0)
        {
            UnityEditor.Undo.RecordObjects(objects, name);

            foreach (Object obj in objects)
            {
                if (obj == null) continue;
                EditorUtility.SetDirty(obj);
            }
        }
    }

    static public T FindInParents<T>(Transform trans) where T : Component
    {
        if (trans == null) 
            return null;
        return trans.GetComponentInParent<T>();
    }
}