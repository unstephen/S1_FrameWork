#define USEWWW
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZRender {
    /*! IRenderResource
     \brief
       资源对象类
     \author    figo
     \date      August,2016
     */
    public class IRenderResource {
        public readonly string name;
        private AssetBundle asset_bundle;
        protected UnityEngine.Object asset;
        protected StringBuilder text;
       // protected MovieTexture movie;
        private List<IRenderObject> insts = new List<IRenderObject>();
        public bool loading;
        public bool complete;
        private IResourceFactory factory;
        private List<Renderer> renders = new List<Renderer>();
        public bool unload_asset;
#if USEWWW
        private WWW www = null;
        public WWW WWW { get { return this.www; } }
#endif
        public float idle_time { get; private set; }

        public IRenderResource(string name, IResourceFactory factory) {
            this.name = name.ToLower();
            this.factory = factory;
        }

        public T CreateInstance<T>(IRenderObject parent, params object[] args)
            where T : IRenderObject {
            T inst = AllocInstance<T>(args);
            inst.SetOwner(this);
            inst.SetParent(parent);
            if (this.complete) {
                inst.Create();
            }

            IResourceFactory.Cookie cookie = factory.GetCookie(name);
            ++cookie.total;
            ++cookie.count;

            idle_time = 0;
            this.insts.Add(inst);
            return inst;
        }

        public void RemoveInstance(IRenderObject obj) {
            if (!this.insts.Remove(obj))
                return;

            IResourceFactory.Cookie cookie = factory.GetCookie(name);
            --cookie.count;
            if (cookie.count <= 0) {
                cookie.create_time = Time.realtimeSinceStartup;
            }

            /* 由factory去决定什么时候Destroy之~ */
            if ( this.insts.Count == 0 ) {
                idle_time = Time.realtimeSinceStartup;
                if ( factory != null )
                    factory.DestroyResource( this );
                else
                    Destroy();
            }
        }

        public int ReferenceCount {
            get { return this.insts.Count; }
        }

#if USEWWW
        public void Create() {
            this.complete = true;
            if (this.www != null) {
                try {
                    if (this.www.bytesDownloaded > 0 && string.IsNullOrEmpty(www.error)) {
                        if (this.name.EndsWith( ".txt") && !string.IsNullOrEmpty(this.www.text))
                            this.text = new StringBuilder(this.www.text);
                      //  this.movie = this.www.movie;
                        this.asset_bundle = this.www.assetBundle;
                    }
                } catch (Exception e) {
                    LOG.Debug(name);
                    LOG.Debug(e.ToString());
                }
                this.www.Dispose();
                this.www = null;
            }

            if (CMisc.isLegalNumber(name))
                return;

            OnCreate(this.asset_bundle);

            for (int i = 0; i < insts.Count; ++i) {
                insts[i].Create();
            }

            if ( this.unload_asset && this.asset_bundle != null ) {
                this.asset_bundle.Unload( false );
                //LOG.Debug( "=> unload asset {0} after create", name );
            }
        }

        public void Load() {
            loading = true;
            this.www = null;
            if (CMisc.isLegalNumber(name) && Global.publishConfig.isDebugOpen)
            {
                //CGameHintUI.Show(ErrorCodes.GetTextFormat(ErrorCodes.INVALID_LOAD_PATH, name));
                return;
            }
            if (!string.IsNullOrEmpty(name) && name != "empty")
                this.www = new WWW(CDirectory.MakeFullWWWPath(name));
        }

#else
        public void Load()
        {
            loading = true;
            if (!string.IsNullOrEmpty(name) && name != "empty")
            {
                string path = string.Empty;
                path = CDirectory.MakeCachePath(name);
                if (!File.Exists(path))
                    path = CDirectory.MakeOtherStreamingPath(name);
                if (this.name.EndsWith(".txt"))
                    this.text = new StringBuilder(File.ReadAllText(path));
                else
                    this.asset_bundle = AssetBundle.CreateFromFile(path);
                //this.complete = true;
            }
            if (!string.IsNullOrEmpty(name))
                this.complete = true;
        }
        public void Create()
        {
            this.complete = true;
            OnCreate(this.asset_bundle);
            for (int i = 0; i < insts.Count; ++i)
                insts[i].Create();

            if (this.asset_bundle != null)
                this.asset_bundle.Unload(false);
        }
#endif

        public void Destroy() {
            OnDestroy();

            //bool unload = false;
            if (this.asset_bundle != null) {
                this.asset_bundle.Unload(true);
                this.asset_bundle = null;
                //unload = true;
            }
            // 因为报了一个IRenderResource已经complete了但GetAsset()为空的错误，
            // 现在没时间去复现该问题，暂时这样容错!!!
            this.complete = false;
            this.loading = false;
            //LOG.Debug( "**** Destroy {0}, {1}, linger {2}", this.name, unload, Time.realtimeSinceStartup  - this.idle_time);
        }

        // 加载资源
       
        protected virtual void OnCreate(AssetBundle asset_bundle) {
            if (asset_bundle != null) {
                Object[] objs = asset_bundle.LoadAllAssets();
                if (objs != null && objs.Length > 0)
                    this.asset = objs[0];
                renders = CClientCommon.ReplaceShader(this.asset, string.Empty);
                IResourceFactory.Cookie cookie = factory.GetCookie(name);
                cookie.RecordMemory(UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(asset), asset is GameObject);
            }
        }

        protected virtual void OnDestroy() {
            if (this.asset != null) {
                UnityEngine.Object.DestroyImmediate(this.asset, true);
                this.asset = null;
            }
            //if (movie != null){
            //    UnityEngine.Object.DestroyImmediate(this.movie, true);
            //    this.movie = null;
            //}
        }

        public UnityEngine.Object GetAsset() { return asset; }
        public StringBuilder GetText() { return text; }

        //public MovieTexture GetMovie() { return movie; }

        protected  T AllocInstance<T>( params object[] args)
            where T : IRenderObject {
            if (args == null)
                return Activator.CreateInstance(typeof(T)) as T;
            else
                return Activator.CreateInstance(typeof(T), args) as T;
       }
    }
}
