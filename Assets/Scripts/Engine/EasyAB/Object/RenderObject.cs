using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ZRender {

    /*! IRenderObject
     \brief
       资源实例基类,由IRenderResource实例化出来
     \author    figo   
     \date      August,2016
     */
    public abstract class IRenderObject : PListNode {
        public bool complete { get; private set; }
        private bool visible = true;
        private bool skinvisible = true;
        //private float opacity = 1.0f;
        private Vector3 init_position = Vector3.zero;
        private Vector3 init_scale = Vector3.one;
        private Quaternion init_rotation = Quaternion.identity;
        private Transform parent_transform = null;
        private bool set_parent_transform = false;
        private int layer;
        private IRenderResource owner;
        private IRenderObject parent;
        private PList children;
        private Map<Type,IController> ctrls;
        private TimerEvent timer;
        public bool destroy { get; private set; }

        public GameObject gameObject { get; protected set; }

        public void AddChild(IRenderObject child) {
            if (children == null) {
                children = new PList();
                children.Init();
            }
            children.AddTail(child);
            if (this.gameObject != null &&
                child.gameObject != null) {
                child.gameObject.transform.parent = this.gameObject.transform;
            }
        }

        public void RemoveChild(IRenderObject child) {
            if (this.children == null)
                return;
            children.Remove(child);
            if (this.gameObject != null &&
                child.gameObject != null &&
                child.gameObject.transform.parent == this.gameObject.transform) {
                child.gameObject.transform.parent = null;
            }
        }

        public T AddController<T>() where T : IController, new() {
            if (ctrls == null)
                ctrls = new Map<Type, IController>();
            T c = new T();
            c.Create(this);
            ctrls.Add(typeof(T), c);
            return c;
        }

        public void RemoveController<T>() where T : IController {
            if (ctrls == null)
                return;
            if (ctrls.ContainsKey(typeof(T))) {
                IController c = ctrls[typeof(T)];
                if (c != null)
                    c.Destroy();
                ctrls.Remove(typeof(T));
            }
        }

        public void RemoveController(IController ctrl) {
            if (ctrls == null)
                return;
            if (ctrls.ContainsKey(ctrl.GetType())) {
                IController c = ctrls[ctrl.GetType()];
                if (c != null)
                    c.Destroy();
                ctrls.Remove(ctrl.GetType());
            }
        }

        public T GetController<T>() where T : IController {
            if (ctrls == null)
                return default(T);
            if (ctrls.ContainsKey(typeof(T))) {
                return ctrls[typeof(T)] as T;
            } else {
                return default(T);
            }
        }

        public int GetInstanceId()
        {
            if (this.gameObject)
                return this.gameObject.GetInstanceID();
            return Def.INVALID_ID;
        }

        public IRenderObject GetParent() { return this.parent; }

        public void SetParent(IRenderObject parent) {
            if (this.parent != null) {
                this.parent.RemoveChild(this);
            }

            this.parent = parent;
            if (this.parent != null) {
                this.parent.AddChild(this);
            }
        }

        public void SetParentTransform(Transform ts) {
            // ensure that the parent transform is
            if (ts == null) {
                // fix when parent tranform has been destroyed after create
                if (this.set_parent_transform)
                    Destroy();
                return;
            }

            if (this.gameObject != null)
                this.gameObject.transform.parent = ts;

            //这里会重置init_position等数据导致位置不对
            //SetPosition(Vector3.zero);
            //SetRotation(Vector3.zero);
            //SetScale(Vector3.one);
            this.parent_transform = ts;
            this.set_parent_transform = true;
        }

        public IRenderResource GetOwner() { return this.owner; }

        public void SetOwner(IRenderResource owner) { this.owner = owner; }

        public Vector3 GetPosition() {
            if (this.gameObject != null)
                return this.gameObject.transform.position;
            else
                return init_position;
        }

        public void SetPosition(Vector3 position) {
            if (this.gameObject != null) {
                this.gameObject.transform.localPosition = position;
            }
            this.init_position = position;
        }

        public void SetRotation(Quaternion rotation) {
            if (this.gameObject != null) {
                this.gameObject.transform.localRotation = rotation;
            }
            this.init_rotation = rotation;
        }

        public void SetRotation(Vector3 euler) {
            Quaternion rotation = Quaternion.Euler(euler);
            SetRotation(rotation);
        }

#if false
        public void LookAt(Vector3 position) {
            Vector3 vec = position - this.init_position;
            vec.y = 0f;
            vec.Normalize();
            Quaternion quaternion = Quaternion.LookRotation(vec);
            SetRotation(quaternion);
        }
#endif

        public Vector3 GetForward() {
            if (this.gameObject != null)
                return this.gameObject.transform.forward;
            else {
                return Vector3.zero;
            }
        }

        public void SetForward(Vector3 forward) {
            Quaternion quaternion = Quaternion.LookRotation(forward);
            SetRotation(quaternion);
        }

        public virtual void SetScale(Vector3 scale) {
            if (this.gameObject != null) {
                this.gameObject.transform.localScale = scale;
            }
            this.init_scale = scale;
        }

        public bool IsVisible() { return visible; }

        public void SetVisible(bool visible) {
            this.visible = visible;

            if (!complete)
                return;

            if (this.gameObject) {
                Renderer[] components = this.gameObject.GetComponentsInChildren<Renderer>(true);
                if (components != null) {
                    for (int i = 0; i < components.Length; ++i) {
                        Renderer renderer = components[i];
                        renderer.enabled = this.visible;
                    }
                }

                //if (this.gameObject.GetComponent<Renderer>() != null)
                //    this.gameObject.GetComponent<Renderer>().enabled = this.visible;

                //Terrain terrain = this.gameObject.GetComponent<Terrain>();
                //if (terrain != null)
                //    terrain.enabled = this.visible;

                //Light light = this.gameObject.GetComponent<Light>();
                //if (light != null)
                //    light.enabled = this.visible;
            }

            OnVisible();
        }

        public void SetSkinVisible(bool visible)
        {
            this.skinvisible = visible;
            if (!complete)
                return;
            OnSkinVisible();
        }

        public bool IsSkinVisible() { return skinvisible; }

        public int GetLayer() { return this.layer; }

        public void SetLayer(int layer) {
            if (layer == 0)
                return;
            this.layer = layer;

            if (children != null) {
                for (PListNode n = children.next; n != children; n = n.next) {
                    IRenderObject child = (IRenderObject)n;
                    if (child.layer == 0)
                        child.layer = layer;
                }
            }

            if (this.gameObject != null) {
                SetLayerRecursively(this.gameObject);
            }
        }

        /*
        public void SetLayer(int layer, int ignore_layer, bool ignore_children) {
            if (this.gameObject != null) {
                if ((gameObject.layer & ignore_layer) == 0)
                    gameObject.layer = layer;
                if (!ignore_children) {

                }
            }
        }
        */

        private void SetLayerRecursively(GameObject go) {
            go.layer = this.layer;
            foreach (Transform transform in go.transform) {
                SetLayerRecursively(transform.gameObject);
            }
        }

        protected void ApplyParticleScale(float scale) {
            if (this.gameObject == null)
                return;
            CClientCommon.ApplyParticleScale(this.gameObject.transform, scale);
        }

        public void Create() {
            if (destroy) return;
            this.OnCreate();
            complete = true;
            if (this.parent != null) {
                if (this.gameObject != null && this.parent.gameObject != null) {
                    this.gameObject.transform.parent = this.parent.gameObject.transform;
                }

                // Inherit the parent's layer, when child doesn't assign a layer.
                if (this.layer == 0 && this.parent.layer != 0)
                    this.layer = this.parent.layer;
            }

           ApplyInitPosition();

            if (children != null) {
                for (PListNode n = children.next; n != children; n = n.next) {
                    IRenderObject child = (IRenderObject)n;
                    if (this.gameObject != null && child.gameObject != null) {
                        child.gameObject.transform.parent = this.gameObject.transform;
                        child.ApplyInitPosition();
                    }
                }
            }

            SetLayer(this.layer);

            if (!this.visible)
                SetVisible(this.visible);

            if (!this.skinvisible)
                SetSkinVisible(this.skinvisible);
        }

        public void Destroy() {
            if (destroy) return;

            try {
                if (children != null) {
                    for (PListNode n = children.next, next; n != children; n = next) {
                        next = n.next;
                        IRenderObject child = (IRenderObject)n;
                        if (child != null)
                            child.SetParent(null);
                    }
                    children = null;
                }

                if (ctrls != null)
                {
                    foreach (IController c in ctrls.Values){
                        if (c != null)
                            c.Destroy();
                    }
                    ctrls.Clear();
                    ctrls = null;
                }

                if (timer != null)
                    timer.Clear();

                this.SetParent(null);
                this.OnDestroy();

                if (this.owner != null) {
                    this.owner.RemoveInstance(this);
                    this.owner = null;
                }
            } catch (Exception e) {
                LOG.LogError(e.ToString(),this.gameObject);
            }
            destroy = true;
        }

        public void Update() {
            if (destroy)
                return;

            if (ctrls != null) {
                foreach (var v in ctrls.Values) {
                    IController c = v as IController;
                    if (c != null && c.enabled)
                        c.Update();
                }
            }
            OnUpdate();

            if (children != null) {
                for (PListNode n = children.next, next; n != children; n = next) {
                    next = n.next;
                    IRenderObject child = (IRenderObject)n;
                    if (child != null)
                        child.Update();
                }
            }

            if (timer != null)
                timer.Process();
        }

        public void LateUpdate()
        {
            if (destroy)
                return;

            if (ctrls != null)
            {
                foreach (var v in ctrls.Values)
                {
                    IController c = v as IController;
                    if (c != null && c.enabled)
                        c.LateUpdate();
                }
            }
            OnLateUpdate();
        }

        protected virtual void OnVisible() { }
        protected virtual void OnSkinVisible() { }
        protected virtual void OnCreate() { }
        protected virtual void OnDestroy() { }
        protected virtual void OnUpdate() { }

        protected virtual void OnLateUpdate() { }

        protected void ApplyInitPosition() {
            if (this.gameObject == null)
                return;
            SetParentTransform(this.parent_transform);
            SetPosition(this.init_position);
            SetRotation(this.init_rotation);
            SetScale(this.init_scale);
        }

        //--------------------------------------------------------------------
        public void AddTimer(TimerEventObject.TimerProc proc, object obj, int p1, int p2) {
            AddTimer(1, 1, proc, obj, p1, p2);
        }

        public void AddTimer(float time, TimerEventObject.TimerProc proc, object obj, int p1, int p2) {
            int frame = Convert.ToInt32(Application.targetFrameRate * time);
            AddTimer(frame, frame, proc, obj, p1, p2);
        }

        private void AddTimer(int start, int interval, TimerEventObject.TimerProc proc, object obj, int p1, int p2) {
            if (timer == null)
                timer = new TimerEvent(1);
            timer.Add(start, interval, proc, obj, p1, p2);
        }
    }
}
