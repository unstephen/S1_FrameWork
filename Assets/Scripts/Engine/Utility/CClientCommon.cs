#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using System.Xml;
using LitJson;
using UnityEngine.Rendering;
using UniRx;

public static class CClientCommon {
    public const int StandingLayer = (CDefines.Layer.Mask.Terrain);
    public static readonly float DOUBLE_PI = Mathf.PI * 2;
    public static readonly float HALF_PI = Mathf.PI * 0.5f;
    public static readonly float QUARTER_PI = Mathf.PI * 0.25f;

    // 改变一个GameObject的Layer
    public static void ChangeLayer( GameObject obj, int layer, bool includeChildren, int ignorelayer = CDefines.Layer.Default ) {
        if ( obj ) {
            ChangeLayer( obj.transform, layer, includeChildren, ignorelayer );
        }
    }

    // 改变一个Transform所附着的GameObject的Layer
    public static void ChangeLayer( Transform trans, int layer, bool includeChildren, int ignorelayer ) {
        if ( trans ) {
            if ( (trans.gameObject.layer & ignorelayer) == 0 )
                trans.gameObject.layer = layer;
            if ( includeChildren ) {
                for ( int i = 0; i < trans.childCount; i++ ) {
                    Transform child = trans.GetChild( i );
                    if ( child )
                        ChangeLayer( child, layer, true, ignorelayer );
                }
            }
        }
    }

    /// <summary>
    /// 改变Transform包含child的render
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="render"></param>
    /// <param name="includeChildren"></param>
    public static void EnableRender( Transform trans, bool render, bool includeChildren )
    {
        if ( !trans ) 
            return;
        Renderer renderer = trans.gameObject.GetComponent<Renderer>();
        if (renderer)
            renderer.enabled = render;
        if ( !includeChildren ) 
            return;
        foreach ( Transform child in trans ) 
            EnableRender( child, render, true );
    }


    public static T GetComponent<T>( GameObject target ) where T : UnityEngine.Component {
        if ( target ) {
            T result = target.GetComponent(typeof(T)) as T;
            return result;
        } else
            return null;
    }

    public static T AddComponent<T>( GameObject target ) where T : UnityEngine.Component {
        if ( target ) {
            T result = target.GetComponent(typeof(T)) as T;
            if ( result == null )
                result = target.AddComponent(typeof(T)) as T;
            return result;
        } else {
            return null;
        }
    }

    public static void RemoveComponent<T>( GameObject go ) where T : Component {
        if ( go ) {
            Component c = go.GetComponent(typeof(T)) as T;
            if ( c ) 
                Object.DestroyImmediate(c, true);
        }
    }

    public static GameObject FindChild( GameObject go, Predicate<GameObject> pred ) {
        if ( go ) {
            foreach ( Transform ts in go.transform ) {
                if ( pred( ts.gameObject ) ) {
                    return ts.gameObject;
                }
                GameObject result = FindChild( ts.gameObject, pred );
                if ( result ) {
                    return result;
                }
            }
        }
        return null;
    }

    public static GameObject FindChild( GameObject go, Predicate<GameObject> pred, bool include_root ) {
        if ( go ) {
            if ( include_root ) {
                if ( pred( go ) )
                    return go;
            }
            foreach ( Transform ts in go.transform ) {
                GameObject cgo = FindChild( ts.gameObject, pred, true );
                if ( cgo )
                    return cgo;
            }
        }
        return null;
    }

    public static void FindChildren( GameObject go, Predicate<GameObject> pred, List<GameObject> list ) {
        if ( go ) {
            foreach ( Transform ts in go.transform ) {
                if ( pred( ts.gameObject ) ) {
                    list.Add( ts.gameObject );
                }
                FindChildren( ts.gameObject, pred, list );
            }
        }
    }

    public static void ForEachChildren( GameObject go, Action<GameObject> action, bool includeRoot ) {
        if ( go ) {
            if ( includeRoot ) {
                action( go );
            }
            foreach ( Transform ts in go.transform ) {
                ForEachChildren( ts.gameObject, action, true );
            }
        }
    }

    public static void ForEachChildren( GameObject go, string url, Action<GameObject, string> action, bool includeRoot ) {
        if ( go ) {
            if ( includeRoot ) {
                action( go, url );
            }
            foreach ( Transform ts in go.transform ) {
                ForEachChildren( ts.gameObject, url, action, true );
            }
        }
    }

    public static float AdjustAngleRad( float angleRad ) {
        if ( angleRad <= -DOUBLE_PI ) {
            angleRad += DOUBLE_PI;
        } else if ( angleRad >= DOUBLE_PI ) {
            angleRad -= DOUBLE_PI;
        }
        return angleRad;
    }

    public static float YawFromXZRad( Vector2 v ) {
        return YawFromXZRad( v.x, v.y );
    }

    public static float YawFromXZRad( Vector3 v ) {
        return YawFromXZRad( v.x, v.z );
    }

    public static float YawFromXZRad( float x, float z ) {
        float r = Mathf.Sqrt( x * x + z * z );
        float yaw = Mathf.Asin( x / r );
        if ( z < 0 ) {
            yaw = Mathf.PI - yaw;
        }
        return AdjustAngleRad( yaw );
    }

    /// <summary>
    ///     从给定平面坐标向下做射线
    /// </summary>
    /// <param name="pos">水平坐标</param>
    /// <param name="ignoreAirWall">忽略空气墙吗？</param>
    /// <param name="hit">返回信息</param>
    /// <returns>是否射到东西？</returns>
    public static bool RaycastDown(Vector3 pos, out RaycastHit hit, int ray = StandingLayer)
    {
        pos.y = 2000f;
        return Physics.Raycast(pos, Vector3.down, out hit, 5000f, ray);
    }

    public static void AdjustToNavMesh(UnityEngine.AI.NavMeshAgent nav, int allowmask, int ray = StandingLayer)
    {
        if (!nav)
            return;
        Vector3 navpos;
        GetNavMeshPostion(out navpos, nav.transform.position, allowmask, ray, 100);
        nav.transform.position = navpos;
    }

    public static void AdjustToNavMesh(Vector3 tpos, UnityEngine.AI.NavMeshAgent nav, int allowmask, int ray = StandingLayer)
    {
        if (!nav)
            return;
        Vector3 navpos;
        GetNavMeshPostion(out navpos, tpos, allowmask, ray, 100);
        nav.transform.position = navpos;
    }

    /// <summary>
    /// 返回在navmesh上面的坐标
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="allowmask"></param>
    /// <param name="succes"></param>
    /// <param name="radius"></param>
    /// <param name="layer"></param>
    /// <returns></returns>
    public static bool GetNavMeshPostion(out Vector3 navpos, Vector3 pos, int navmask, int ray = StandingLayer, int radius = 100)
    {
        navpos = pos;
        RaycastHit hit;
        if (RaycastDown(pos, out hit, ray))
            navpos = hit.point;
        else
            LOG.LogWarning(string.Format("坐标异常;{0}", pos));
        
        UnityEngine.AI.NavMeshHit mesh_hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(navpos, out mesh_hit, radius, navmask))
        {
            navpos = mesh_hit.position;
            return true;
        }
        return false;
    }


    // 将Layer加到layerMask中
    public static int AddToLayerMask( int layerMask, int layer ) {
        return layerMask | (1 << layer);
    }

    public static int AddToLayerMask( int layerMask, params int[] layers ) {
        foreach ( int l in layers ) {
            layerMask = AddToLayerMask( layerMask, l );
        }
        return layerMask;
    }

    // 从layerMask中去掉layer
    public static int RemoveFromLayerMask( int layerMask, int layer ) {
        return layerMask & (~(1 << layer));
    }

    public static int RemoveFromLayerMask( int layerMask, params int[] layers ) {
        foreach ( int l in layers ) {
            layerMask = RemoveFromLayerMask( layerMask, l );
        }
        return layerMask;
    }

    public static bool WithinDistance( float x1, float y1, float x2, float y2, float dist ) {
        return ((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2)) <= (dist * dist);
    }

    public static int CalculateRate( int standard, int rate ) {
        return ( int )(standard * (rate / 10000.0f));
    }

    public static int CalculateMoney( int price, int count, int rate ) {
        int money = CalculateRate( price * count, rate );
        if ( money < count ) return count;
        return money;
    }

    /// <summary>
    /// 误差为上下0.1f以内，返回true；反之为false
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <returns></returns>
    public static bool IsSimilar( Vector3 p1, Vector3 p2 ) 
    {
        return Approximately(p1.x, p2.x) && Approximately(p1.y, p2.y) && Approximately(p1.z, p2.z);
    }

    public static bool IsSimilar(float x, float y, float x1, float y1)
    {
        return Approximately(x, x1) && Approximately(y, y1);
    }

    /// <summary>
    /// 误差为上下0.1f以内，返回true；反之为false
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool Approximately(float a,float b)
    {
        return Mathf.RoundToInt(a * 10) == Mathf.RoundToInt(b * 10);
    }

    /// <summary>
    /// 计算夹角的角度 0~360(注意是Vector2)
    /// </summary>
    /// <param name="from_"></param>
    /// <param name="to_"></param>
    /// <returns></returns>
    public static float VectorAngle(Vector2 from_, Vector2 to_)
    {
        Vector3 v3 = Vector3.Cross(from_, to_);
        if (v3.z > 0)
            return Vector3.Angle(from_, to_);
        else
            return 360 - Vector3.Angle(from_, to_);
    }

    //获取一个向量朝向另一个向量的角度
    public static Vector3 LookAtEuler(Vector3 from, Vector3 to) {
        Vector3 forwardDir = to - from;
        Quaternion lookAtRot = Quaternion.LookRotation(forwardDir);
        Vector3 resultEuler = lookAtRot.eulerAngles;
        return resultEuler;
    }

    static public void DestroyImmediate(UnityEngine.Object obj)
    {
        if (obj)
        {
            UnityEngine.Object.Destroy(obj);
            obj = null;
        }
    }

    static public void DestroyImmediate(ZRender.IRenderObject obj)
    {
        if (obj != null)
        {
            obj.Destroy();
            obj = null;
        }
    }

    static public void DestroyImmediate<T>(ref T obj) where T : ZRender.IRenderObject
    {
        if (obj != null)
        {
            obj.Destroy();
            obj = null;
        }
    }


    static public bool isEnlarge(Vector2 oP1, Vector2 oP2, Vector2 nP1, Vector2 nP2)
    {
        var leng1 = Mathf.Sqrt((oP1.x - oP2.x) * (oP1.x - oP2.x) + (oP1.y - oP2.y) * (oP1.y - oP2.y));
        var leng2 = Mathf.Sqrt((nP1.x - nP2.x) * (nP1.x - nP2.x) + (nP1.y - nP2.y) * (nP1.y - nP2.y));
        if (leng1 < leng2) //放大
            return true;
        else //缩小
            return false;
    }

    public static bool isSlow;

    public static void EnterSlow(float time = 0.6f, System.Action cb = null)
    {
        if (isSlow)
            return;
        if (!Global.IsInGame)
            return;
        isSlow = true;
        Time.timeScale = 0.3F;
        Observable.Timer(TimeSpan.FromSeconds(time)).Subscribe(x=>
        {
            LeaveSlow();
            if (cb != null)
                cb();
        });
    }


    public static bool isCameraVisisble(Vector3 pos)
    {
        if (!Camera.main)
            return false;
        pos = Camera.main.WorldToScreenPoint(pos);
        return isCameraWithinScreen(pos);
    }

    public static bool isCameraWithinScreen(Vector3 pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x <= Screen.width && pos.y <= Screen.height && pos.z > 0;
    }

    public static void ZeroTransform(Transform tf)
    {
        if (!tf)
            return;
        tf.localEulerAngles = Vector3.zero;
        tf.localPosition = Vector3.zero;
        tf.localScale = Vector3.one;
    }

    public static void LeaveSlow()
    {
        isSlow = false;
        Time.timeScale = 1;
    }

    public static float GetRange(float min, float max, float value)
    {
        if (value < min)
            return min;
        else if (value > max)
            return max;
        return value;
    }

    public static Transform FindChild(string path, Transform parent)
    {
        string[] paths = path.Split('/');
        for (int i = 0; i < paths.Length; i++)
        {
            string name = paths[i];
            for (int j = 0; j < parent.childCount; j++)
            {
                Transform child = parent.GetChild(j);
                if (child.name == name)
                {
                    parent = child;
                    break;
                }
            }
        }
        if (parent.name != paths[paths.Length - 1])
            return null;
        return parent;
    }

    static public GameObject AddChild(GameObject parent)
    {
        GameObject go = new GameObject();
        if (parent != null)
        {
            Transform t = go.transform;
            t.parent = parent.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            go.layer = parent.layer;
        }
        return go;
    }

    static public GameObject AddChild(GameObject parent,GameObject child)
    {
        if (!child)
            return null;
        GameObject go = Object.Instantiate(child);
        if (parent != null)
        {
            Transform t = go.transform;
            t.SetParent(parent.transform);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            go.layer = parent.layer;
        }
        return go;
    }

    //---------------------------------------------------------------------
    public static void NormalizeTransform(GameObject gameObject)
    {
        NormalizeTransform(gameObject.transform);
    }

    //---------------------------------------------------------------------
    public static void NormalizeTransform(Transform transform)
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }


    //---------------------------------------------------------------------
    private static Vector3 ms_KeepPosition = Vector3.zero;
    private static Quaternion ms_KeepRotation = Quaternion.identity;
    private static bool ms_KeepTransform = false;
    private static Vector3 ms_KeepLocalScale = Vector3.one;
    //---------------------------------------------------------------------
    public static void SaveTransform(Transform transform)
    {
        ms_KeepTransform = true;
        ms_KeepPosition = transform.position;
        ms_KeepRotation = transform.rotation;
        ms_KeepLocalScale = transform.localScale;
    }

    //---------------------------------------------------------------------
    public static void RevertTransform(Transform transform)
    {
        if (!ms_KeepTransform)
        {
            Debug.LogWarning("You must call BeginTransform first.");
        }

        transform.position = ms_KeepPosition;
        transform.rotation = ms_KeepRotation;
        transform.localScale = ms_KeepLocalScale;
    }

    //---------------------------------------------------------------------
    public static void AttachChild(Transform parent, Transform child)
    {
        AttachChild(parent, child, false);
    }

    //---------------------------------------------------------------------
    public static void AttachChild(Transform parent,
        Transform child, bool inheritLayer)
    {
        if (child == null)
        {
            return;
        }

        child.parent = parent;

        if (parent == null)
        {
            return;
        }

        if (inheritLayer && parent.gameObject.layer != child.gameObject.layer)
        {
            ForEach(parent, HanleModifyLayer,false);
        }
    }

    //---------------------------------------------------------------------
    private static bool HanleModifyLayer(Transform root, Transform child)
    {
        child.gameObject.layer = root.gameObject.layer;
        return false;
    }

    //---------------------------------------------------------------------
    public static bool ForEach(Transform transform,
        Func<Transform, bool> functor, bool includeRoot) {
        if (transform == null) {
            return false;
        }
        if (includeRoot)
            functor(transform);
        for (int i = 0; i < transform.childCount; ++i) {
            if (ForEach(transform.GetChild(i), functor, true)) {
                return true;
            }
        }

        return false;
    }

    //---------------------------------------------------------------------
    public static bool ForEach(Transform transform,
        Func<Transform, Transform, bool> functor, bool includeRoot) {
            return ForEach(transform, transform, functor, includeRoot);
    }

    //---------------------------------------------------------------------
    private static bool ForEach(Transform rootTrans, Transform child,
        Func<Transform, Transform, bool> functor, bool includeRoot) {
        if (child == null) {
            return false;
        }

        if (includeRoot)
            functor(rootTrans, child);

        for (int i = 0; i < child.childCount; ++i) {
            if (ForEach(rootTrans, child.GetChild(i), functor,true)) {
                return true;
            }
        }

        return false;
    }

    public static string GetChildPath(Transform root, Transform child) {
        string path = string.Empty;
        List<string> nodes = new List<string>();

        nodes.Add(child.name);
        Transform temp = child.parent;
        while (temp != root && temp != null) {
            nodes.Add(temp.name);
            temp = temp.parent;
            if (temp == root)
                nodes.Add(temp.name);
        }

        for (int i = nodes.Count - 1; i >= 0; i--) {
            if (i == nodes.Count - 1)
                path = nodes[i];
            else
                path += "/" + nodes[i];
        }
        return path;
    }

    public static void ApplyParticleScale(Transform go, float scale)
    {
        scale = Mathf.Abs(scale);
        if (!go || Mathf.Approximately(scale, 1))
            return;
        for (int i = 0; i < go.childCount; i++)
            ApplyParticleScale(go.GetChild(i), scale);

        ParticleSystem ps = go.GetComponent<ParticleSystem>();
        if (ps) 
        {
            ParticleSystem.MainModule MainModule = ps.main;
            float size = MainModule.startSize.constant;
            MainModule.startSize = new ParticleSystem.MinMaxCurve(size * scale);
            go.transform.localScale = Vector3.one;
        }
    }

    #region replace shader
    public static void FindRenderers(ref List<Renderer> Renderers, Transform parent, bool includeInactive = true)
    {
        if (Renderers == null || !parent)
            return;
        Renderer[] renderers = parent.GetComponentsInChildren<Renderer>(includeInactive);
        for (int i = 0; i < renderers.Length; i++)
            Renderers.Add(renderers[i]);

        //ParticleSystem[] ps = parent.GetComponentsInChildren<ParticleSystem>(includeInactive);
        //for (int i = 0; i < renderers.Length; i++)
        //    Renderers.Add(renderers[i]);
        //else {
        //    ParticleSystem ps = parent.GetComponent<ParticleSystem>();
        //    if (ps && ps.renderer)
        //        Renderers.Add(ps.renderer);
        //    else {
        //        ParticleEmitter pe = parent.particleEmitter;
        //        if (pe && pe.renderer)
        //            Renderers.Add(pe.renderer);
        //    }
        //}
    }

    public static List<Renderer> ReplaceShader(UnityEngine.Object obj, string url) {
        List<Renderer> render_list = new List<Renderer>();
        if (!obj)
            return render_list;
        if (obj is GameObject) {
            //FindRenderers(render_list, (obj as GameObject).transform);
            CClientCommon.FindRenderers(ref render_list, (obj as GameObject).transform);
            if (render_list.Count == 0)
                return render_list;
            for (int i = 0; i < render_list.Count; i++)
                ReplaceRendererShader(render_list[i], url);
        } else if (obj is Material)
            ReplaceMaterialShader(obj as Material, url);

        return render_list;
    }

    public static void ReplaceMaterialShader(Material mat, string url) {
        if (!mat || !mat.shader)
        {
            LOG.TraceRed("error: mat or shader of mat is null");
            return;
        }

        if (mat.shader) {
            int renderQueue = mat.renderQueue;
            string shadername = mat.shader.name;
            Shader  shader = Shader.Find(shadername);
            if (shader) {
                mat.shader = null;
                mat.shader = shader;
                mat.renderQueue = renderQueue;
            } else
                LOG.Debug(string.Format("error: shader {0} {1} {2} can't find in local", url, mat.name, mat.shader.name));


        }
    }

    public static void ReplaceRendererShader(Renderer renderer, string url) {
        //替换SHADER
        if (renderer) {
            Material[] sharedMaterials = renderer.sharedMaterials;
            for (int i = 0; i < sharedMaterials.Length; i++) {
                Material mat = sharedMaterials[i];
                if (mat) {
                    ReplaceMaterialShader(mat, url);
                    renderer.lightProbeUsage = LightProbeUsage.Off;
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                }
            }
        }
    }
    #endregion

    /// <summary>
    /// 将十六进制的颜色值转为颜色
    /// </summary>
    /// <param color="00fa00" or "#00fa00" or "[00fa00]"></param>
    /// <returns></returns>
    public static Color GetColorFromHex(string color)
    {
        string clr = color;

        var r = "";
        var g = "";
        var b = "";
        var ch = clr[0];

        var k = 0;
        if (ch == '#'||ch=='[')
            k++;
        if (k + 5 >= clr.Length) {
            return Color.white;
        }
        r += clr[k++];
        r += clr[k++];
        g += clr[k++];
        g += clr[k++];
        b += clr[k++];
        b += clr[k++];
        try
        {
            var rr = int.Parse(r, System.Globalization.NumberStyles.HexNumber);
            var gg = int.Parse(g, System.Globalization.NumberStyles.HexNumber);
            var bb = int.Parse(b, System.Globalization.NumberStyles.HexNumber);
            return new Color(rr / 255f, gg / 255f, bb / 255f);
        }
        catch (FormatException e)
        {
            LOG.Debug(e.StackTrace);
            return Color.white;
        }
    }

    /// <summary>
    /// 返回2点之间法线坐标
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="tf"></param>
    /// <returns></returns>
    public static Vector3 GetMidPos(Vector3 v1, Vector3 v2, Transform tf)
    {
        tf.forward = new Vector3(v2.x - v1.x, 0, v2.z - v1.z).normalized;
        Vector3 mid = (v2 + v1) / 2;
        Vector3 s1 = (v2 - v1).normalized;
        Vector3 tpos = Vector3.Cross(s1, tf.right).normalized;
        float distance = Vector3.Distance(v1, v2) / 3;
        return mid + tpos * distance;
    }
}

public class OperateCD {
    private static Dictionary<string, long> timeDic = new Dictionary<string, long>();
    public const int OPERATE_INTERVAL = 500;//操作时间间隔(毫秒)

    //判断时否可操作
    public static bool IsOperate( string key, float time = OPERATE_INTERVAL ) {
        long timestamp = GameTimer.mtime;

        if ( !timeDic.ContainsKey( key ) ) {
            timeDic.Add( key, timestamp );
            return true;
        }

        if ( timestamp - timeDic[key] >= time ) {
            timeDic[key] = timestamp;
            return true;
        }
        return false;
    }
}

public static class CDefines
{
    public static class CharCtrlStepOffset
    {
        public static readonly float Player = 0.5f;
        public static readonly float OtherPlayer = 1f;
        public static readonly float Monster = 1.5f;
    }

    // 速度（以下各值最终由服务器发送过来）

    public static class Player
    {
        public const float MaxClimbAngle = 45;
        // ****** 暂时放开Readonly，供调试效果用
        public static /*readonly*/ float JumpHeight = 1.5f;
        public static /*readonly*/ float Gravity = 9.8f;
    }

    #region Tag定义

    public static class Tag {
        public const string MainCamera = "MainCamera";
        public const string EventObject = "EventObject";
        public const string JiGuan = "jiguan";
        public const string Axis = "Axis";
        public const string Dirt = "dirt";
        public const string Water = "water";
        public const string Concrete = "concrete";
        public const string SunLight = "sunlight";
        public const string RoleLight = "rolelight";
        public const string AirWalls = "airwalls";
        public const string LoadingBackGround = "loadingBackground";
        public const string MainRoleShadow = "mainRoleShadow";
        public const string SelectedEffect = "selectedEffect";
        public const string SkyBox = "skybox";
        public const string LevelController = "levelController";
        public const string SmokeEffect = "smokeEffect";
    }

    #endregion

    #region 层 Layer

    public static class Layer {
        public const int Default = 0;
        public const int IgnoreRaycast = 2;
        public const int Water = 4;
        public const int UI = 5;
        public const int Player = 8; 
        public const int MainPlayer = 9;
        public const int Effect = 10;
        public const int UIPlayer = 11; 
        public const int Jump = 12;
        public const int Terrain = 13; 
        public const int Fly = 14;
        public const int Prop = 15; 
        public const int MainBuilding = 16; 
        public const int SkyBox = 31;

        public static class Mask {
            public const int Default = 1 << Layer.Default;
            public const int IgnoreRaycast = 1 << Layer.IgnoreRaycast;
            public const int Water = 1 << Layer.Water;
            public const int UI = 1 << Layer.UI;
            public const int Player = 1 << Layer.Player;
            public const int MainPlayer = 1 << Layer.MainPlayer;
            public const int Effect = 1 << Layer.Effect;
            public const int UIPlayer = 1 << Layer.UIPlayer;
            public const int Jump = 1 << Layer.Jump;
            public const int Terrain = 1 << Layer.Terrain;
            public const int Fly = 1 << Layer.Fly;
            public const int Prop = 1 << Layer.Prop;
            public const int MainBuilding = 1 << Layer.MainBuilding;
            public const int SkyBox = 1 << Layer.SkyBox;
        }
    }

    #endregion
}

/// <summary>
///     杂类
/// </summary>
public static class CMisc {
    public enum ZeroStyle {
        EmptryString,
        ChineseNone,
        MinusSign,
        Zero
    }

    public static int GetLengthOfANSI(string s)
    {
        byte[] bwrite;//byte内容
        bwrite = Encoding.GetEncoding("GB2312").GetBytes(s.ToCharArray());
        return bwrite.Length;
    }

    public static bool IsLarger(int a, int b, string ErrorShow = "")
    {
        if (a >= b)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static void Swap<T>( ref T t1, ref T t2 ) {
        T tmp = t1;
        t1 = t2;
        t2 = tmp;
    }

    public static string BigNumberToStr(int number, int relative = 100000)
    {
        if (number < relative)
            return number.ToString();
        else
            return (number / (relative / 10) + "W").ToString();
    }

    public static string MoneyToStr( int money ) {
        return MoneyToStr( money, ZeroStyle.ChineseNone );
    }

    public static string MoneyToStr( int money, ZeroStyle zs ) {
        if ( money == 0 ) {
            switch ( zs ) {
            case ZeroStyle.EmptryString:
                return string.Empty;
            case ZeroStyle.ChineseNone:
                return "(无)";
            case ZeroStyle.MinusSign:
                return "-";
            default:
                return "0";
            }
        }
        StringBuilder sb = new StringBuilder( 128 );
        sb.Append( money.ToString() );
        sb.Append( "铜钱" );
        return sb.ToString();
    }

    public static string OccToStr(int Occ)
    {
        switch(Occ)
        {
            case 1:
                return "战士";
            case 2:
                return "法师";
            case 3:
                return "血族";
        }
        return "null";
    }

    public static string AppendDirectorySeparatorChar( string dir ) {
        if ( dir != null && dir.Length > 0 && dir[dir.Length - 1] != Path.DirectorySeparatorChar ) {
            return dir + Path.DirectorySeparatorChar;
        } else {
            return dir;
        }
    }

    public static string ReplaceDirectorySeparatorChar( string path ) {
        if ( path != null ) {
            return path.Replace( '/', Path.DirectorySeparatorChar );
        } else {
            return path;
        }
    }

    public static bool TryParsePosition( string text, out float x, out float y, out float z ) {
        string[] xyz = text.Split( ',' );
        if ( xyz.Length == 3 ) {
            if ( float.TryParse( xyz[0].Trim(), out x ) ) {
                if ( float.TryParse( xyz[1].Trim(), out y ) ) {
                    if ( float.TryParse( xyz[2].Trim(), out z ) ) {
                        return true;
                    }
                }
            }
        }
        x = y = z = 0;
        return false;
    }

    public static float Get2DDistance( Vector3 v1, Vector3 v2 ) 
    {
        return Get2DDistance(v1.x, v1.z, v2.x, v2.z);
    }

    public static float Get2DDistance(float x,float y,float x1,float y1)
    {
        Vector2 v = Vector2.zero;
        v.x = x;
        v.y = y;

        Vector2 v1 = Vector2.zero;
        v1.x = x1;
        v1.y = y1;
        return Vector2.Distance(v, v1);
    }

    /// <summary>
    /// 返回一个模糊的距离值
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static float GetDimDistance(Vector3 v1, Vector3 v2)
    {
        return Mathf.Abs(v1.x - v2.x) + Mathf.Abs(v1.y - v2.y) + Mathf.Abs(v1.z - v2.z);
    }

    /// <summary>
    ///     判断两个表示方向的已归一化的二维向量是否接近（夹角小于某个阈值）
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="z1"></param>
    /// <param name="x2"></param>
    /// <param name="z2"></param>
    public static bool IsDirectionClose( float x1, float z1, float x2, float z2 ) {
        if ( x1 == 0 && z1 == 0 ) {
            return (x2 == 0 && z2 == 0);
        }
        if ( x2 == 0 && z2 == 0 ) {
            return false;
        } else {
            return (x1 * x2 + z1 * z2) >= 0.9998f;
        }
    }

    public static int MakeMoveData( Vector3 forward ) {
        int movedata = 0;
        movedata |= ((int)(forward.x * 100)) & 0xFF;
        movedata |= (((int)(forward.z * 100)) << 8) & 0xFF00;
        return movedata;
    }

    public static bool isLegalNumber(string str)
    {
        char[] charStr = str.ToLower().ToCharArray();
        for (int i = 0; i < charStr.Length; i++)
        {
            int num = Convert.ToInt32(charStr[i]);
            if (num < 33 || num > 126)
            {
                return true;
            }
        }
        return false;
    }
}

///////////////////////////////////////////////////

public static class TimeHelper {
    public static readonly DateTime DATETIME_1970 = new DateTime( 1970, 1, 1, 0, 0, 0 );
    public static readonly long BEGIN_OF_1970;

    static TimeHelper() {
        var csharp = new DateTime( 1, 1, 1, 0, 0, 0 );
        long delta = DATETIME_1970.Ticks - csharp.Ticks;
        BEGIN_OF_1970 = delta / (1000 * 1000 * 10);
    }

    public static string StandardDateString( DateTime dt ) {
        return dt.ToString( "yyyy-MM-dd" );
    }

    public static string StandardHourString( DateTime dt ) {
        return dt.ToString( "yyyy-MM-dd(HH:mm)" );
    }

    public static string StandardTimeString( DateTime dt, string typestr ) 
    {
        return dt.ToString( typestr );//"HH:mm"
    }

    public static string StandardFullString( DateTime dt )
    {
        return dt.ToString( "yyyy-MM-dd HH:mm:ss" );
    }

    public static string StandardMouthString( DateTime dt ) 
    {
        return dt.ToString(Localization.Get("COMMON_STANDARD_MOUTH_STRING"));
    }

    public static string UTCToDateString( uint seconds ) {
        return StandardDateString( UTCToDateTime( seconds ) );
    }

    public static string UTCToHourString( uint seconds ) {
        return StandardHourString( UTCToDateTime( seconds ) );
    }

    public static string UTCToFullString( uint seconds ) {
        return StandardFullString( UTCToDateTime( seconds ) );
    }

    public static string UTCToTimeString( uint seconds, string typestr ) {
        return StandardTimeString( UTCToDateTime( seconds ), typestr );
    }

    public static string UTCToMouthString( uint seconds ) {
        return StandardMouthString( UTCToDateTime( seconds ) );
    }

    public static DateTime UTCToDateTime( uint seconds ) {
        DateTime dt = DATETIME_1970;
        return dt.AddSeconds( seconds ).ToLocalTime();
    }

    public static uint DateTimeToUTC( DateTime dt ) {
        TimeSpan delta = dt.ToUniversalTime() - DATETIME_1970;
        return ( uint )delta.TotalSeconds;
    }

    /// <summary>
    /// 倒计时天开头
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    public static string GetCountDownDayStr( uint seconds ,bool child = true) {
        TimeSpan delta = DATETIME_1970.AddSeconds( seconds ).Subtract( DATETIME_1970 );
        StringBuilder sb = new StringBuilder();
        if ( ( int )delta.TotalDays == 0 ) {
            return GetCountDownHourStr(seconds, child);
        }
        if (child)
            return sb.AppendFormat("{0}:{1}:{2}:{3}", GetStandardTimeString(delta.TotalDays), GetStandardTimeString(delta.Hours),
                                                    GetStandardTimeString(delta.Minutes), GetStandardTimeString(delta.Seconds)).ToString();
        else
        {
            int TotalDays = Mathf.FloorToInt((float)delta.TotalDays);
            if (delta.Hours > 0)
                TotalDays += 1;
            return sb.AppendFormat("{0}{1}", TotalDays, Localization.Get("COMMON_DAY")).ToString();
        }
    }

    /// <summary>
    /// 倒计时小时开头
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    public static string GetCountDownHourStr(uint seconds, bool child = true,bool allowZero = true)
    {
        StringBuilder sb = new StringBuilder();
        TimeSpan delta = DATETIME_1970.AddSeconds( seconds ).Subtract( DATETIME_1970 );
        if ((int)delta.TotalHours == 0 && !allowZero)
        {
            return GetCountDownMinuteStr(seconds, child);
        }
        if (child)
            return sb.AppendFormat("{0}:{1}:{2}", GetStandardTimeString(delta.TotalHours),
                                                GetStandardTimeString(delta.Minutes), GetStandardTimeString(delta.Seconds)).ToString();
        else
        {
            int TotalHours = Mathf.FloorToInt((float)delta.TotalHours);
            if (delta.Minutes > 0)
                TotalHours += 1;
            return sb.AppendFormat("{0}{1}", TotalHours, Localization.Get("COMMON_HOURS")).ToString();
        }
    }

    /// <summary>
    /// 倒计时分钟开头
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    public static string GetCountDownMinuteStr(uint seconds, bool child = true)
    {
        StringBuilder sb = new StringBuilder();
        TimeSpan delta = DATETIME_1970.AddSeconds( seconds ).Subtract( DATETIME_1970 );
        if ((int)delta.TotalMinutes == 0){
            return GetCountDownSecondStr(seconds, child);
        }
        if (child)
            return sb.AppendFormat("{0}:{1}", GetStandardTimeString(delta.TotalMinutes), GetStandardTimeString(delta.Seconds)).ToString();
        else
        {
            int TotalMinutes = Mathf.FloorToInt((float)delta.TotalMinutes);
            if (delta.Seconds > 0)
                TotalMinutes += 1;
            return sb.AppendFormat("{0}{1}", TotalMinutes,Localization.Get("COMMON_MINUTE")).ToString();
        }
    }

    /// <summary>
    /// 倒计时秒开头
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    public static string GetCountDownSecondStr(uint seconds, bool child = true)
    {
        StringBuilder sb = new StringBuilder();
        TimeSpan delta = DATETIME_1970.AddSeconds(seconds).Subtract(DATETIME_1970);
        if (child)
            return GetStandardTimeString(delta.TotalSeconds);
        return sb.AppendFormat("{0}{1}", (float)delta.TotalSeconds, Localization.Get("COMMON_SECONDS")).ToString();
    }

    /// <summary>
    /// 得到花费的时间 分钟开头
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    public static string GetMinuteStr(uint seconds)
    {
        StringBuilder sb = new StringBuilder();
        TimeSpan delta = DATETIME_1970.AddSeconds( seconds ).Subtract( DATETIME_1970 );
        return sb.AppendFormat(Localization.Get("COMMON_MinuteSecond"),
            GetStandardTimeString( delta.TotalMinutes ), GetStandardTimeString( delta.Seconds ) ).ToString();
    }

    public static string GetStandardTimeString( double time ) {
        StringBuilder sb = new StringBuilder();
        time = Mathf.FloorToInt( ( float )time );
        if ( time >= 0 && time < 10 ) {
           return sb.AppendFormat("0{0}", time).ToString();
        } else if ( time >= 10 ) {
            return sb.Append(time).ToString();
        }
        return string.Empty;
    }

    /// 时间字符串 返回格式：分：秒
    public static string GetTimeString( int seconds ) {
        int min = seconds / 60;
        StringBuilder sb = new StringBuilder();
        if (min < 10)
            sb.AppendFormat("0{0}", min);
        else
            sb.Append(min);
        int sec = seconds % 60;
        if (sec < 10)
            sb.AppendFormat(":0{0}", sec);
        else
            sb.AppendFormat(":{0}", sec);

        return sb.ToString();
    }

    /// 时间字符串，返回格式：小时：分：秒
    public static string GetTimeHourString( int seconds )
    {
        StringBuilder sb = new StringBuilder();
        int mins = seconds / 60;
        int hour = mins / 60;
        if (hour < 10)
            sb.AppendFormat("0{0}", hour);
        else
            sb.Append(hour);

        int min = mins % 60;
        if (min < 10)
            sb.AppendFormat(":0{0}", min);
        else
            sb.AppendFormat(":{0}", min);

        int sec = seconds % 60;
        if (sec < 10)
            sb.AppendFormat(":0{0}", sec);
        else
            sb.AppendFormat(":{0}", sec);

        return sb.ToString();
    }
}

public static class CTime {
    //当前服务器的UTC时间
    public static uint time_tick;

    //当前服务器的datetime
    public static DateTime datetime;

    //当前服务器按“天”时间，格式：20140812
    public static int day;

    //代表服务器的年、月、日、时、分、周
    public static short year;
    public static short month;
    public static short mday;
    public static short hour;
    public static short minute;
    public static short wday;

    // 返回当前月的天数
    public static int GetDaysInMonth() {
        return DateTime.DaysInMonth( datetime.Year, datetime.Month );
    }

    //返回当天的小时
    public static int GetHourInDay() {
        return datetime.Hour;
    }

    //返回当天的分
    public static int GetMinuteInHour() {
        return datetime.Minute;
    }

    //返回当星期的天
    public static int GetDayInWeek() {
        wday = (short)((datetime.DayOfWeek == DayOfWeek.Sunday) ? 7 : (int)datetime.DayOfWeek);
        return wday;
    }

    private static int delta_time_;
    private static uint server_tick_;
    private static uint client_tick_;

    static CTime() {
        delta_time_ = 0;
        server_tick_ = TimeHelper.DateTimeToUTC( DateTime.Now );
        client_tick_ = TimeHelper.DateTimeToUTC( DateTime.Now );
        day = DateTime.Now.Year * 10000 + DateTime.Now.Month * 100 + DateTime.Now.Day;
    }

    public static void Update() {
        DateTime now = DateTime.Now;
        long adjust_time = ( long )TimeHelper.DateTimeToUTC( now ) - ( long )client_tick_;
        time_tick = ( uint )(server_tick_ + adjust_time);

        datetime = now.AddSeconds( delta_time_ );

        year = ( short )datetime.Year;
        month = ( short )datetime.Month;
        mday = ( short )datetime.Day;
        hour = ( short )datetime.Hour;
        minute = ( short )datetime.Minute;
        wday = ( short ) ( (datetime.DayOfWeek == DayOfWeek.Sunday) ? 7 : (int)datetime.DayOfWeek );
    }

    public static void OnServerNotifyTime( uint server_tick ) {
        server_tick_ = server_tick;

        client_tick_  =  TimeHelper.DateTimeToUTC( DateTime.Now );
        delta_time_ = ( int )(( long )server_tick_ - ( long )client_tick_);
        DateTime now = DateTime.Now.AddSeconds( delta_time_ );
        day = now.Year * 10000 + now.Month * 100 + now.Day;

        year = (short)now.Year;
        month = ( short )now.Month;
        mday = ( short )now.Day;
        hour = ( short )now.Hour;
        minute = ( short )now.Minute;
        wday = ( short )((now.DayOfWeek == DayOfWeek.Sunday) ? 7 : ( int )datetime.DayOfWeek);
    }

    static int[,] mon_lengths = new int[2, 12] {
	    { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 },
	    { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 }
    };

    /// <summary>
    /// 计算两个日期的日间隔
    /// </summary>
    /// <param name="lowday"></param>
    /// <param name="highday"></param>
    /// <returns></returns>
    public static int CalcDaySpan( int low_day, int high_day ) {
        if ( low_day > high_day )
            return -1;

        int low_year = low_day / 10000;
        int low_month = (low_day % 10000) / 100;
        int low_mday = low_day % 100;
        int high_year = high_day / 10000;
        int high_month = (high_day % 10000) / 100;
        int high_mday = high_day % 100;

        if ( low_year < high_year ) {
            int day_span = 0;
            for ( int year = low_year; year < high_year; ++year ) {
                if ( ((((year) % 4) == 0 && ((year) % 100) != 0) || ((year) % 400) == 0) )
                    day_span += 366;
                else
                    day_span += 365;
            }

            int isleap = 0;
            if ( ((((low_year) % 4) == 0 && ((low_year) % 100) != 0) || ((low_year) % 400) == 0) )
                isleap = 1;
            for ( int month = 0; month < low_month - 1; ++month )
                day_span -= mon_lengths[isleap, month];
            day_span -= low_mday;

            isleap = 0;
            if ( ((((high_year) % 4) == 0 && ((high_year) % 100) != 0) || ((high_year) % 400) == 0) )
                isleap = 1;
            for ( int month = 0; month < high_month - 1; ++month )
                day_span += mon_lengths[isleap, month];
            day_span += high_mday;

            return day_span;
        } else {  // 
            int isleap = 0;
            if ( ((((low_year) % 4) == 0 && ((low_year) % 100) != 0) || ((low_year) % 400) == 0) )
                isleap = 1;

            int day_span = 0;
            for ( int month = low_month; month < high_month; ++month )
                day_span += mon_lengths[isleap, month - 1];
            day_span += high_mday - low_mday;

            return day_span;
        }
    }

}


public static class CStackTrace {
    public static string GetInfo() {
        return GetInfo( 1 );
    }

    public static string GetInfo( int skipStackFrams ) {
        var st = new StackTrace( skipStackFrams, true );
        StackFrame[] sfs = st.GetFrames();
        var sb = new StringBuilder( 1024 );
        foreach ( StackFrame sf in sfs ) {
            string filename = sf.GetFileName();
            if ( filename == null || filename.Length == 0 ) {
                break;
            }
            MethodBase mb = sf.GetMethod();
            sb.AppendFormat( "\tFunc: {0}\r\n\tLine: {1}\r\n\tFile: {2}\r\n\r\n",
                            (mb == null) ? "(Unknown)" : mb.Name,
                            sf.GetFileLineNumber().ToString(),
                            filename );
        }
        return sb.ToString();
    }
}

/// <summary>
///     常用算法
/// </summary>
public static class CAlgorithm {
    public delegate int Compare<T, U>( T t, U u );

    /// <summary>
    ///     在一个升序容器里，查找是否有这样一个Item，它满足：
    ///     该Item“大于或等于”给定值，
    ///     且Item是所有满足这个条件中的第一个
    /// </summary>
    /// <typeparam name="T">容器条目类型</typeparam>
    /// <typeparam name="U">给定值（要查找的值）的类型</typeparam>
    /// <param name="container">容器</param>
    /// <param name="first">起始位置</param>
    /// <param name="last">结束位置</param>
    /// <param name="u">检索关键值</param>
    /// <param name="compare">比较方法</param>
    /// <returns>成功找到返回非负索引，未找到返回last</returns>
    public static int LowerBound<T, U>( IList<T> container, int first, int last, U u, Compare<T, U> compare ) {
        int count = last - first;
        while ( count > 0 ) {
            int count2 = count >> 1;
            int middle = first + count2;
            if ( compare( container[middle], u ) < 0 ) {
                first = ++middle;
                count -= count2 + 1;
            } else {
                count = count2;
            }
        }
        return first;
    }

    public static int LowerBound<T, U>( IList<T> container, U u, Compare<T, U> compare ) {
        return LowerBound( container, 0, container.Count, u, compare );
    }

    /// <summary>
    ///     在一个升序容器里，查找是否有这样一个Item，它满足：
    ///     该Item“大于”给定值，
    ///     且Item是所有满足这个条件中的第一个
    /// </summary>
    /// <typeparam name="T">容器条目类型</typeparam>
    /// <typeparam name="U">给定值（要查找的值）的类型</typeparam>
    /// <param name="container">容器</param>
    /// <param name="first">起始位置</param>
    /// <param name="last">结束位置</param>
    /// <param name="u">检索关键值</param>
    /// <param name="compare">比较方法</param>
    /// <returns>成功找到返回非负索引，未找到返回last</returns>
    public static int UpperBound<T, U>( IList<T> container, int first, int last, U u, Compare<T, U> compare ) {
        int count = last - first;
        while ( count > 0 ) {
            int count2 = count >> 1;
            int middle = first + count2;
            if ( compare( container[middle], u ) <= 0 ) {
                first = ++middle;
                count -= count2 + 1;
            } else {
                count = count2;
            }
        }
        return first;
    }

    public static int UpperBound<T, U>( IList<T> container, U u, Compare<T, U> compare ) {
        return UpperBound( container, 0, container.Count, u, compare );
    }

    // 在一个升序容器里，用给定值u进行查找
    // 返回一对索引值在lower和upper里
    //		lower = LowerBound(), upper = UpperBound()
    public static void EqualBound<T, U>(
        IList<T> container, int first, int last, U u, Compare<T, U> compare,
        out int lower, out int upper
        ) {
        int count = last - first;
        while ( count > 0 ) {
            int count2 = count / 2;
            int middle = first + count2;
            int c = compare( container[middle], u );
            if ( c < 0 ) {
                first = ++middle;
                count -= count2 + 1;
            } else if ( c > 0 ) {
                count = count2;
            } else {
                lower = LowerBound( container, first, middle, u, compare ) + count;
                upper = UpperBound( container, ++middle, first, u, compare );
                return;
            }
        }
        lower = upper = first;
    }

    // 堆
    public class Heap<T> where T : IComparable<T> {
        public static void HeapSort( T[] objects ) {
            for ( int i = objects.Length / 2 - 1; i >= 0; --i )
                heapAdjustFromTop( objects, i, objects.Length );
            for ( int i = objects.Length - 1; i > 0; --i ) {
                swap( objects, i, 0 );
                heapAdjustFromTop( objects, 0, i );
            }
        }

        public static void heapAdjustFromBottom( T[] objects, int n ) {
            while ( n > 0 && objects[(n - 1) >> 1].CompareTo( objects[n] ) < 0 ) {
                swap( objects, n, (n - 1) >> 1 );
                n = (n - 1) >> 1;
            }
        }

        public static void heapAdjustFromTop( T[] objects, int n, int len ) {
            while ( (n << 1) + 1 < len ) {
                int m = (n << 1) + 1;
                if ( m + 1 < len && objects[m].CompareTo( objects[m + 1] ) < 0 )
                    ++m;
                if ( objects[n].CompareTo( objects[m] ) > 0 )
                    return;
                swap( objects, n, m );
                n = m;
            }
        }

        private static void swap( T[] objects, int a, int b ) {
            T tmp = objects[a];
            objects[a] = objects[b];
            objects[b] = tmp;
        }
    }

    // 优先队列
    public class PriorityQueue<T> where T : IComparable<T> {
        private const int defaultCapacity = 16;
        private T[] buffer;
        private int heapLength;

        public PriorityQueue() {
            buffer = new T[defaultCapacity];
            heapLength = 0;
        }

        public bool Empty() {
            return heapLength == 0;
        }

        public T Top() {
            if ( heapLength == 0 ) {
                throw new OverflowException();
            }
            return buffer[0];
        }

        public void Push( T obj ) {
            if ( heapLength == buffer.Length )
                expand();
            buffer[heapLength] = obj;
            Heap<T>.heapAdjustFromBottom( buffer, heapLength );
            heapLength++;
        }

        public void Pop() {
            if ( heapLength == 0 )
                throw new OverflowException();
            --heapLength;
            swap( 0, heapLength );
            Heap<T>.heapAdjustFromTop( buffer, 0, heapLength );
        }

        private void expand() {
            Array.Resize( ref buffer, buffer.Length * 2 );
        }

        private void swap( int a, int b ) {
            T tmp = buffer[a];
            buffer[a] = buffer[b];
            buffer[b] = tmp;
        }
    }
}

/// <summary>
///     整数转成中文描述字串
/// </summary>
internal class CIntegerToChinese {
    private static readonly char[] NUM_1 = new[] { '零', '一', '二', '三', '四', '五', '六', '七', '八', '九' };
    private static readonly char[] NUM_2 = new[] { '零', '壹', '贰', '叁', '肆', '伍', '陆', '柒', '捌', '玖' };
    private static readonly char[] UNIT_1 = new[] { '十', '百', '千', '万', '十', '百', '千', '亿', '十', '百' };
    private static readonly char[] UNIT_2 = new[] { '拾', '佰', '仟', '万', '拾', '佰', '仟', '亿', '拾', '佰' };
    public static readonly CIntegerToChinese General = new CIntegerToChinese( false, true );
    private readonly bool adjustTen_;

    private readonly char[] numbers_;
    private readonly char[] units_;

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="upper">为TRUE表示用“壹贰叁……”代表“一二三……”</param>
    /// <param name="adjustTen">为TRUE表示类似于10和100000这样的值，转换为“十”和“十万”而不是“一十”和“一十万”</param>
    public CIntegerToChinese( bool upper, bool adjustTen ) {
        if ( upper ) {
            numbers_ = NUM_2;
            units_ = UNIT_2;
        } else {
            numbers_ = NUM_1;
            units_ = UNIT_1;
        }
        adjustTen_ = adjustTen;
    }

    /// <summary>
    ///     辅助函数，将正整数按个十百千位分开到数组中
    /// </summary>
    /// <param name="value">正整数值</param>
    /// <returns>整型数组，按个十百千位反序排列</returns>
    private static IList<int> Split( int value ) {
        var result = new List<int>( 12 );
        while ( value != 0 ) {
            int remainder;
            value = Math.DivRem( value, 10, out remainder );
            result.Add( remainder );
        }
        return result;
    }

    /// <summary>
    ///     将数值转换为中文表达
    /// </summary>
    /// <param name="value">要转换的整数值</param>
    /// <returns>中文表达文本</returns>
    public string Execute( int value ) {
#if false
        if ( Global.edition_type == EditionType.KOREA )
            return value.ToString();
#endif

        var sb = new StringBuilder( 64 );
        // 如果是负数则令其为正，返回串前缀“负”
        if ( value < 0 ) {
            value = -value;
            sb.Append( '负' );
        }
        // 如果不大于9，则直接返回单个字符
        if ( value <= 9 ) {
            sb.Append( numbers_[value] );
            return sb.ToString();
        }
        // 处理大于9的数值
        IList<int> splited = Split( value ); // 将数值按“个十百千……”位分开
        int unit = splited.Count - 2; // “单位”数组的下标
        bool lastIsZero = false; // 上一位数值是零吗？
        for ( int i = splited.Count - 1; i >= 0; --i ) {
            int num = splited[i];
            // 如果数值为0，则不需要转换为“零”字符，且：
            //		若当前位是“亿”或“万”时，加上当前位的“单位”
            //		（注，须避免“亿”和“万”两个单位相连）
            if ( num == 0 ) {
                if ( unit == 7 || (unit == 3 && sb[sb.Length - 1] != '亿') ) {
                    sb.Append( units_[unit] );
                }
                lastIsZero = true;
            }
                // 如果当前位数值不为0，则判断上一位是否为0
                //		若是，则需把上一位的0转换为“零”字符
            else {
                if ( lastIsZero ) {
                    sb.Append( '零' );
                    lastIsZero = false;
                }
                sb.Append( numbers_[num] );
                if ( unit >= 0 ) {
                    sb.Append( units_[unit] );
                }
            }
            --unit;
        }
        //
        if ( adjustTen_ && sb.Length >= 2 && sb[0] == '一' && sb[1] == '十' ) {
            sb.Remove( 0, 1 );
        }
        return sb.ToString();
    }
}

public class Item
{
    public int[] ItemID;
    public int Count;

    public int GetItemID(int occ)
    {
        if (ItemID == null)
            return Def.INVALID_ID;

        if (ItemID.Length == 1)
            return ItemID[0];

        if (occ < 0 || occ > ItemID.Length)
            return Def.INVALID_ID;

        return ItemID[occ - 1];
    }
}

public class Coin
{
    public int Type;
    public int amount;

    public int Silver
    {
        get
        {
            if (Type == CoinType.Silver)
                return amount;
            return Def.INVALID_ID;
        }
    }

    public int Gold
    {
        get
        {
            if (Type == CoinType.Gold)
                return amount;
            return Def.INVALID_ID;
        }
    }
    public int Stone
    {
        get
        {
            if (Type == CoinType.Stone)
                return amount;
            return Def.INVALID_ID;
        }
    }
    public int Honor
    {
        get
        {
            if (Type == CoinType.Honor)
                return amount;
            return Def.INVALID_ID;
        }
    }
}

public class ItemList : IEnumerable<ItemList.Entry>
{
    private readonly Map<int, int> dict_ = new Map<int, int>();

    public int Count
    {
        get { return dict_.Count; }
    }

    public IEnumerator<Entry> GetEnumerator()
    {
        foreach (var kvp in dict_)
            yield return new Entry(kvp.Key, kvp.Value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(int itemId, int count)
    {
        if (itemId > 0 && count > 0)
        {
            int oldCount;
            if (!dict_.TryGetValue(itemId, out oldCount))
                dict_.Add(itemId, count);
            else
                dict_[itemId] = oldCount + count;
        }
    }

    public void Clear()
    {
        dict_.Clear();
    }

    public struct Entry
    {
        public int ItemId;
        public int Count;
        public Entry(int id, int count)
        {
            ItemId = id;
            Count = count;
        }
    }
}

public class QuestAwardList : IEnumerable<KeyValuePair<int, ItemList>>
{
    private readonly Dictionary<int, ItemList> dict_ = new Dictionary<int, ItemList>();

    public int HowManyQuest
    {
        get { return dict_.Count; }
    }

    //
    public IEnumerator<KeyValuePair<int, ItemList>> GetEnumerator()
    {
        return dict_.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(int questId, int itemId, int count)
    {
        if (itemId > 0 && count > 0)
        {
            ItemList itemList;
            if (!dict_.TryGetValue(questId, out itemList))
            {
                itemList = new ItemList();
                dict_.Add(questId, itemList);
            }
            itemList.Add(itemId, count);
        }
    }
}


/// <summary>
/// 手机文本的存取和读取
/// </summary>
public class JsonIO
{
    /// <summary>
    /// 使用json格式写入缓存文件中
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="obj"></param>
    public static void JsonWrite( string filename, object obj, bool absolute_path = false ) {
        if ( !absolute_path )
            filename = CDirectory.MakeCachePath( filename );
        FileInfo t = new FileInfo( filename );
        StreamWriter sw = t.CreateText();
        if (sw != null)
        {
            JsonWriter jw = new JsonWriter();
            jw.PrettyPrint = true;
            jw.Validate = true;
            string json_data = JsonMapper.ToJson(obj);
            sw.Write(json_data);
            sw.Close();
            sw.Dispose();
        }
    }

    /// <summary>
    /// 用json格式读取缓存文件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static object JsonRead<T>( string filename ) {
        filename = CDirectory.MakeCachePath( filename );
        object result = null;
        StreamReader sr = GetStreamReader( filename );
        if ( sr == null )
            return result;
        try 
        {
            result = JsonMapper.ToObject<T>( sr );
        } 
        catch ( System.Exception ex ) 
        {
            if ( ex != null ) 
            {
                sr.Close();
                sr.Dispose();
                DeleteFile( filename );
                return result;
            }
        }
        sr.Close();
        sr.Dispose();
        return result;
    }

    /// <summary>
    /// 使用流读取指定目录下面的文本生成ArrayList
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static ArrayList LoadFile( string filename ) {
        StreamReader sr = GetStreamReader( CDirectory.MakeCachePath( filename ) );
        if ( sr == null ) {
            //GameMessgeUI.ShowPrompt( string.Format( "{0}不存在", path ) );
            return null;
        }
        string line;
        ArrayList arrlist = new ArrayList();
        while ( (line = sr.ReadLine()) != null ) {
            arrlist.Add( line );
        }
        sr.Close();
        sr.Dispose();
        return arrlist;
    }

    /// <summary>
    /// 读取指定目录下的文本生成流
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    static StreamReader GetStreamReader( string path ) {
        FileInfo t = new FileInfo( path );
        if ( !t.Exists ) {
            return null;
        }
        StreamReader sr = null;
        try {
            sr = File.OpenText( path );
        } catch ( Exception /*e*/ ) {
            //GameMessgeUI.ShowPrompt( e.StackTrace );
            return null;
        }
        return sr;
    }

    /// <summary>
    /// 删除指定目录下的文本
    /// </summary>
    /// <param name="Filename"></param>
    static void DeleteFile( string filename ) {
        if ( File.Exists( filename ) ) {
            File.Delete( filename );
        }
    }
}
internal static class EditorEnumDescribe<T>
{
    private static readonly Dictionary<int, EditorEnumAttribute> dict_ = new Dictionary<int, EditorEnumAttribute>();
    static EditorEnumDescribe()
    {
        //        Type type = typeof( EditorEnumAttribute );
        foreach (FieldInfo fi in typeof(T).GetFields())
        {
            if (fi.FieldType == typeof(int) || fi.FieldType == typeof(short))
            {
                int v = Convert.ToInt32(fi.GetValue(null));
                var attr = Attribute.GetCustomAttribute(fi, typeof(EditorEnumAttribute)) as EditorEnumAttribute;
                if (attr != null)
                    dict_[v] = attr;
            }
        }
    }

    public static EditorEnumAttribute Get(int v)
    {
        EditorEnumAttribute attr;
        if (dict_.TryGetValue(v, out attr))
            return attr;
        else
            return null;
    }

    public static string GetName(int v)
    {
        EditorEnumAttribute attr;
        if (dict_.TryGetValue(v, out attr))
            return attr.Name;
        else
            return "";
    }

    public static void SetName(int v, string name)
    {
        EditorEnumAttribute attr;
        if (dict_.TryGetValue(v, out attr))
            attr.Name = name;
    }

    public static string GetDisplay(int v)
    {
        EditorEnumAttribute attr;
        if (dict_.TryGetValue(v, out attr))
            return attr.Display;
        else
            return "";
    }
}
