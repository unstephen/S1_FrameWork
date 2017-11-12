#define USEWWW
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

namespace ZRender {

    /*! IResourceFactory
     \brief
       资源管理器/资源对象工厂
     \author    figo
     \date      August,2016
     */
    public class IResourceFactory {
        private IEmptyObject empty_parent = null;
        private IEmptyResource empty = null;
        private Map<string, IRenderResource> loading = new Map<string, IRenderResource>();
        private Map<string, IRenderResource> complete = new Map<string, IRenderResource>();
        private Map<string, IRenderResource> idle = new Map<string, IRenderResource>();
        public float linger_time = 0;

        public T CreateInstance<T>(string filename, IRenderObject parent, bool unload, params object[] args)
            where T : IRenderObject {
            IRenderResource resource;
            filename = filename.ToLower();
            if (CMisc.isLegalNumber(filename))
                LOG.LogError(Localization.Format("INVALID_LOAD_PATH", filename));
            if ( parent != null && parent.GetOwner() != null ) {
                if ( parent.GetOwner().unload_asset )
                    unload = parent.GetOwner().unload_asset;
            }

            if ( idle.TryGetValue( filename, out resource ) ) {
                idle.Remove( filename );
                if ( resource.complete )
                    complete.Add( filename, resource );
                else
                    loading.Add( filename, resource );
                //LOG.Debug( "***** get resource {0} from idle, linger {1}s", filename, Time.realtimeSinceStartup -  resource.idle_time );
            } else if ( !loading.TryGetValue( filename, out resource ) ) {
                if ( !complete.TryGetValue( filename, out resource ) ) {
                    resource = new IRenderResource(filename, this);
                    resource.unload_asset = unload;
                    loading.Add(resource.name, resource);

                    Cookie cookie = GetCookie(filename);
                    ++cookie.create;
                }
            }
            if (resource == null)
                return null;
            return resource.CreateInstance<T>(parent, args);
        }

        public T CreateEmptyInstance<T>(IRenderObject parent, params object[] args)
            where T : IRenderObject {
            if (empty == null) {
                empty = new IEmptyResource(this);
                Cookie cookie = GetCookie(empty.name);
                ++cookie.create;
                empty_parent = empty.CreateInstance<IEmptyObject>(null);
                loading.Add("empty", empty);
            }
            return empty.CreateInstance<T>(parent, args);
        }

        public IEmptyObject CreateEmptyInstance() {
            if (empty == null) {
                empty = new IEmptyResource(this);
                Cookie cookie = GetCookie(empty.name);
                ++cookie.create;
                empty_parent = empty.CreateInstance<IEmptyObject>(null);
                loading.Add("empty", empty);
            }
            return empty.CreateInstance<IEmptyObject>(empty_parent);
        }

        public void DestroyResource(IRenderResource resource) {
            loading.Remove(resource.name);
            complete.Remove(resource.name);

            if ( idle.ContainsKey( resource.name ) ) {
                // 因为报了一个IRenderResource已经complete了但GetAsset()为空的错误，
                // 现在没时间去复现该问题，暂时这样容错!!!
                idle[resource.name].Destroy();
                idle.Remove( resource.name );
                resource.Destroy();
            } else
                idle.Add( resource.name, resource );
        }

        private List<string> complete_list = new List<string>();

#if USEWWW
        public void Update() {
            if ( loading.Count > 0 ) {
                int n = 0;
                ArrayList list = new ArrayList( loading.Values );
                foreach ( IRenderResource resource in list ) {
                    if ( !resource.loading ) {
                        ++n;
                        resource.Load();
                    }
                    if ( resource.WWW == null || resource.WWW.isDone ) {
                        if ( complete.ContainsKey( resource.name ) ) {
                            LOG.LogError( "【IResourceFactory】加载资源出错 " + resource.name );
                            break;
                        }
                        complete_list.Add( resource.name );
                        complete[resource.name] = resource;
                        resource.Create();
                        break;
                    }

                    if ( n == 3 )
                        break;
                }

                for ( int i = 0; i < complete_list.Count; ++i )
                    loading.Remove( complete_list[i] );
                complete_list.Clear();
            }

            float time = Time.realtimeSinceStartup;
            // 处理掉逗留时间过长的资源
            foreach ( IRenderResource resource in idle.Values ) {
                if ( time < resource.idle_time + linger_time )
                    continue;
                resource.Destroy();
                complete_list.Add( resource.name );
            }
            for ( int i = 0; i < complete_list.Count; ++i )
                idle.Remove( complete_list[i] );
            complete_list.Clear();
        }
#else
        public void Update()
        {
            if (loading.Count == 0)
                return;
            int n = 0;
            for (loading.Begin(); loading.Next(); )
            {
                if (!loading.Value.loading)
                {
                    ++n;
                    loading.Value.Load();
                }
                if (loading.Value.complete)
                {
                    complete.Add(loading.Value.name, loading.Value);
                    loading.Value.Create();
                    complete_list.Add(loading.Value.name);
                    break;
                }

                if (n == 3)
                    break;
            }

            for (int i = 0; i < complete_list.Count; ++i)
                loading.Remove(complete_list[i]);
            complete_list.Clear();
        }
#endif

        public void UnloadUnusedAssets() {
            foreach ( IRenderResource resource in idle.Values ) {
                //RemoveCookie( resource.name );
                resource.Destroy();
            }
            idle.Clear();
        }

        #region Cookie and Dumper
        private Map<string, Cookie> cookies = new Map<string, Cookie>();

        public class Cookie {
            public string name;
            public int create;
            public int total;
            public int count;
            public float create_time;
            public long memory;

            private bool isReproducible = false;
            private float mbseed = 1.0f / (1024 * 1024);
            public string ToLogString() {
                return string.Format("{4}\t{0}\t{1}\t{2}\t{3}\t{5}\t{6}"
                    , name, create, total, count,DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    , GetMemory(), Time.realtimeSinceStartup - create_time);
            }

            public string ToEditorString() {
                return string.Format("memory:{4}M name:{0} create:{1} total:{2} count:{3} survival_time:{5}"
                    , name, create, total, count
                    , GetMemory(), Time.realtimeSinceStartup - create_time);
            }

            public void RecordMemory(long memory, bool isReproducible)
            {
                this.isReproducible = isReproducible;
                this.memory = memory;
            }

            public float GetMemory() {
                return isReproducible ? memory * count * mbseed : memory * mbseed;
            }
        }

        public Cookie GetCookie(string name) {
            Cookie cookie;
            if (!cookies.TryGetValue(name, out cookie)) {
                cookie = new Cookie();
                cookie.name = name;
                cookie.create_time = Time.realtimeSinceStartup;
                cookies.Add(name, cookie);
            }
            return cookie;
        }

        public void RemoveCookie(string name) {
            if (cookies.ContainsKey(name))
                cookies.Remove(name);
        }
#if false
        class Dumper : MonoBehaviour {
            [SerializeField]
            private float memory = 0;

            private IResourceFactory factory;
            private const int INTERVAL = 60;
            private const int EditorINTERVAL = 1;
            private float dump_time = 0;
            private float editor_dump_time = 0;
            public List<string> all = new List<string>();
            private string filePath = string.Empty;
            private float mbseed = 1.0f / (1024 * 1024);

            public void SetFactory( IResourceFactory factory ) {
                this.factory = factory;
                dump_time = Time.realtimeSinceStartup + INTERVAL;
                editor_dump_time = Time.realtimeSinceStartup + EditorINTERVAL;
            }

            public void Update() {
                if ( Time.realtimeSinceStartup > dump_time ) {
                    DumpToFile();
                    dump_time = Time.realtimeSinceStartup + INTERVAL;
                }

                if (Application.isEditor && Time.realtimeSinceStartup > editor_dump_time) {
                    DumpEditor();
                    editor_dump_time = Time.realtimeSinceStartup + EditorINTERVAL;
                }
            }

            private void DumpToFile() {
                if (string.IsNullOrEmpty(filePath)) {
                    if (!Directory.Exists(CDirectory.MakeCachePath("resdump")))
                        Directory.CreateDirectory(CDirectory.MakeCachePath("resdump"));
                    filePath = CDirectory.MakeCachePath(string.Format("resdump/{0}.txt", DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")));
                }

                StreamWriter writer = File.AppendText(filePath);
                foreach (var c in factory.cookies.Values) {
                    if (c.count == 0) {
                        c.create_time = Time.realtimeSinceStartup;
                        continue;
                    }
                    writer.WriteLine(c.ToLogString());
                }
                writer.Close();
            }

            List<Cookie> cookies = new List<Cookie>();
            private void DumpEditor() {
                all.Clear();
                cookies.Clear();
                memory = 0;
                cookies = new List<Cookie>(factory.cookies.Values);
               
                cookies.Sort(delegate(Cookie c1, Cookie c2) {
                    int m = c2.memory.CompareTo(c1.memory);
                    if(m != 0)
                        return m;
                    return 0;
                });
                for (int i = 0; i < cookies.Count; i++) {
                    //if (cookies[i].count == 0)
                    //    continue;
                    all.Add(cookies[i].ToEditorString());
                    memory += cookies[i].GetMemory();
                }
            }
        }
#endif
        #endregion
    }

    /*! IEmptyResource
     \brief
       空资源，实例化出来的对象约定为IEmptyObject。
     　在游戏中会用到实例化GameObject，不从任何资源实例化。
     \author    figo
     \date      August, 2016
     */
    public class IEmptyResource : IRenderResource {
        public IEmptyResource(IResourceFactory factory)
            : base("empty", factory) { }
    }

    /*! IEmptyObject
     \brief
      空资源对象，应该使用IResourceFactory.CreateEmptyInstance创建
     \author    figo
     \date      August, 2016
     */
    public class IEmptyObject : IRenderObject {
        protected override void OnCreate() {
            this.gameObject = new GameObject("empty");
        }

        protected override void OnDestroy() {
            if (this.gameObject != null) {
                UnityEngine.Object.DestroyImmediate(this.gameObject, true);
                this.gameObject = null;
            }
        }
    }
}
