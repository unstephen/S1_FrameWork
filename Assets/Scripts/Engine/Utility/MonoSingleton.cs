using UnityEngine;
using System.Collections;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{

    private static string MonoSingletonName = "MonoSingletonRoot";
    private static GameObject MonoSingletonRoot;
    private static T instance;

    public static T Instance
    {
        get
        {
            if (MonoSingletonRoot == null)//如果是第一次调用单例类型就查找所有单例类的总结点  
            {
                MonoSingletonRoot = GameObject.Find(MonoSingletonName);
                if (MonoSingletonRoot == null)//如果没有找到则创建一个所有继承MonoBehaviour单例类的节点  
                {
                    MonoSingletonRoot = new GameObject();
                    MonoSingletonRoot.name = MonoSingletonName;
                    DontDestroyOnLoad(MonoSingletonRoot);//防止被销毁  
                }
            }
            if (instance == null)//为空表示第一次获取当前单例类  
            {
                instance = MonoSingletonRoot.GetComponent<T>();
                if (instance == null)//如果当前要调用的单例类不存在则添加一个  
                {
                    instance = MonoSingletonRoot.AddComponent<T>();
                }
            }
            return instance;
        }
    }
}
