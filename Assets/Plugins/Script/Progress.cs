using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 场景进度条
/// </summary>
public class Progress : IDisposable
{
    public void Dispose()
    {
        if (asset != null)
        {
            asset.Dispose();
            asset = null;
        }

        if (www != null)
        {
            www.Dispose();
            www = null;
        }

        if (async != null)
        {
            async.Dispose();
            async = null;
        }
        asset = new AssetProgress();
    }
    public int progress
    {
        get
        {
            int value = 0;

            if (asset != null)
                value += asset.GetValue();

            if (www != null)
                value += www.GetValue();

            if (async != null)
                value += async.GetValue();

            return value;
        }
    }
    public string Tips = string.Empty;
    public static Progress Instance = new Progress();
    private AssetProgress asset;
    private WWWSceneProgress www;
    private AsyncSceneProgress async;

    public Progress()
    {
        Tips = Localization.Get("LOAD_ASSET_TIPS");
        asset = new AssetProgress();
    }

    public WWWSceneProgress CreateWWW()
    {
        asset.Done();
        www = new WWWSceneProgress();
        return www;
    }

    public AsyncSceneProgress CreateAsync()
    {
        asset.Done();
        async = new AsyncSceneProgress();
        return async;
    }

    public AssetProgress.ItemProgress CreateItem()
    {
        return asset.CreateItem();
    }

    public class Base : IDisposable
    {
        public virtual void Dispose() { }

        public bool isDone;
        public virtual void Done()
        {
            isDone = true;
            this.percent = 1;
        }

        public virtual int GetValue()
        {
            return (int)(progress * this.percent);
        }

        public void SetPercent(float t)
        {
            this.percent = t;
        }

        public float progress { protected set; get; }
        public virtual float percent { protected set; get; }
    }

    public class AssetProgress : Base
    {
        public override void Dispose()
        {
            itemdic.Clear();
            itemdic = null;
        }

        public AssetProgress()
        {
            progress = 70;
        }

        public class ItemProgress : Base
        {
            public ItemProgress()
            {
                progress = 1;
            }

            public override int GetValue()
            {
                if (isDone)
                    return (int)progress;
                return 0;
            }
        }

        private List<ItemProgress> itemdic = new List<ItemProgress>();

        public ItemProgress CreateItem()
        {
            ItemProgress item = new ItemProgress();
            itemdic.Add(item);
            return item;
        }

        public override int GetValue()
        {
            if(isDone)
                return (int)this.progress;

            int value = 0;
            float avg = this.progress / itemdic.Count;
            for (int i = 0; i < itemdic.Count; i++)
                value += (int)(itemdic[i].GetValue() * avg);

            if (value >= this.progress)
                value = (int)this.progress;
            return value;
        }

    }

    public class WWWSceneProgress : Base
    {
        public WWWSceneProgress()
        {
            progress = 20;
        }
    }

    public class AsyncSceneProgress : Base
    {
        public override void Dispose()
        {
            async = null;
        }

        public AsyncSceneProgress()
        {
            progress = 10;
        }

        public AsyncOperation async;

        public override float percent
        {
            get
            {
                if (async != null)
                {
                    if (async.progress >= 0.9f)
                        return 1;
                    return async.progress;
                }

                return 0;
            }
        }
    }
}