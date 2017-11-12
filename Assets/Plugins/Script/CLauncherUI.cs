using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CLauncherUI : MonoBehaviour
{
    private CLoaderUI Loaderbar;
    private System.IDisposable pValueDisposable;

    void Awake()
    {
        NGUILink link = this.gameObject.GetComponent(typeof(NGUILink)) as NGUILink;
        if (link == null)
            return;
        Loaderbar = this.gameObject.AddComponent(typeof(CLoaderUI)) as CLoaderUI;

        pValueDisposable = Progress.Instance.ObserveEveryValueChanged(p => p.progress).Subscribe(p => Loaderbar.SetProgress(p));
    }

    void OnDestroy()
    {
        pValueDisposable.Dispose();
    }


    //public static void LoadLevel(int targetLevelId,string targetLevel)
    //{
    //    LastLevel = TargetLevel;
    //    TargetLevel = targetLevel;
    //    TargetLevelID = targetLevelId;
    //    Resources.UnloadUnusedAssets().AsAsyncOperationObservable().Where(x => x.isDone).Subscribe(_ => SceneManager.LoadSceneAsync("AsyncLevelLoader"));
    //}
}
