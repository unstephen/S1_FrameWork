using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 进度条
/// </summary>
public class CLoaderUI : CUIBehaviour
{
    private Slider Bar;
    private Text ProgressLabel;
    private Text Tips;

    public override void Initialize()
    {
        NGUILink link = this.gameObject.GetComponent(typeof(NGUILink)) as NGUILink;
        Bar = link.Get<Slider>("ProgressSlider");
        ProgressLabel = link.Get<Text>("ProgressLabel");
        Tips = link.Get<Text>("Tips");
    }

    public void SetProgress(float progress)
    {
        Bar.value = (float)Progress.Instance.progress / 100;
        LOG.Debug("progress :" + Bar.value);
        ProgressLabel.text = Progress.Instance.progress + "%";
        Tips.text = Progress.Instance.Tips;
    }
}
