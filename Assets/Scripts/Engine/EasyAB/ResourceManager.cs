using UnityEngine.SceneManagement;

public class CResourceFactory : MonoSingleton<CResourceFactory> {
    private static ZRender.IResourceFactory factory;
    private static ZRender.IRenderObject cache_root;

    public CResourceFactory() 
    {
        factory = new ZRender.IResourceFactory();
        factory.linger_time = 20F;
        SceneManager.sceneLoaded += OnLevelWasLoaded;
    }

    public static T CreateCacheInstance<T>( string filename, ZRender.IRenderObject parent, params object[] args )
        where T : ZRender.IRenderObject {
        return factory.CreateInstance<T>( filename, parent, true, args );
    }

    public static T CreateInstance<T>(string filename, ZRender.IRenderObject parent, params object[] args)
        where T : ZRender.IRenderObject {
        return factory.CreateInstance<T>( filename, parent, false, args );
    }

    public static T CreateEmptyInstance<T>(ZRender.IRenderObject parent, params object[] args) 
        where T : ZRender.IRenderObject {
        return factory.CreateEmptyInstance<T>( parent, args );
    }

    void Update() {
        factory.Update();
    }

    void OnLevelWasLoaded(Scene scene, LoadSceneMode mode) 
    {
        factory.UnloadUnusedAssets();
    }
}
