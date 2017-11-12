using Model;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
/*>--------------------------------------------------------------------
 场景物件包围盒，处理点击、选中效果

规则:
1.只有可选中的物体才能被选中
2.点击和选中分别为不同事件处理，点击包含选中
3.在同一时间只能选中一个单位
4.只有Npc和友方才能产生选中
>--------------------------------------------------------------------<*/

//可选择场景物件碰撞检测基类
public abstract class CSelectableBound : MonoBehaviour
{
    protected bool CanSelect;
    public bool isCameravisible = true;
    protected virtual void Initialize() { }

    protected CSelectableBound() { } // disable public call
    public bool LeftClick()
    {
        OnLeftClick();
        Select();

        return CanSelect;
    }

    protected virtual void Update() 
    {
        if (BindObject == null || BindObject.disposed)
            return;
        if (Time.frameCount % 3 != 0)
            return;
        isCameravisible = CClientCommon.isCameraWithinScreen(Camera.main.WorldToScreenPoint(BindObject.Transform.position));
    }

    public void Select()
    {
        if (BindObject == null || BindObject.disposed)
            return;
        OnSelect();
    }

    protected virtual void OnLeftClick() { }
    protected virtual void OnSelect()
    {
        if (CanSelect)
            CSelectEffect.Create(this.BindObject);
    }

    public CBaseObject BindObject { get; protected set; }
   /* public static CSelectableBound BindToObject<T>(CBaseObject obj) where T : CSelectableBound
    {
        CSelectableBound Bound = CClientCommon.AddComponent<T>(obj.RoleConfig.BoxCollider) as CSelectableBound;
        Bound.BindObject = obj;
        Bound.Initialize();
        return Bound;
    }*/

    public static implicit operator GameObject(CSelectableBound obj)
    {
        return obj == null ? null : obj.gameObject;
    }

    void OnTriggerEnter(Collider other)
    {
       /* CRoleObject ro = BindObject as CRoleObject;
        if (!ro)
            return;
        CTrigger trigger = other.gameObject.GetComponent<CTrigger>();
        if (!trigger)
            return;
        switch (trigger.Eventtype)
        {
            case CTrigger.EventType.Auto:
            case CTrigger.EventType.OpenLevel:
                {
                    Global.world.OnOpenLevel(trigger);
                }
                break;
            case CTrigger.EventType.CloseLevel:
                {
                    Global.world.OnCloseLevel(trigger);
                }
                break;
            case CTrigger.EventType.PointTrigger:
                {
                    Global.world.OnPointTrigger(trigger);
                }
                break;
            case CTrigger.EventType.JumpPoint:
                {
                    Global.world.OnInJumpPoint(trigger);
                }
                break;
            case CTrigger.EventType.TPPoint:
                {
                    Global.world.OnInTPPoint(trigger);
                }
                break;
        }
        Global.world.OnDoTriggerEvent(trigger);*/
    }

    void OnTriggerExit(Collider other)
    {
        CRoleObject ro = BindObject as CRoleObject;
        if (!ro)
            return;
        CTrigger trigger = other.gameObject.GetComponent<CTrigger>();
        if (!trigger)
            return;
        switch (trigger.Eventtype)
        {
            case CTrigger.EventType.Auto:
                {
                   // Global.world.OnCloseLevel(trigger);
                }
                break;
        }
    }
}


public class CMonsterBound : CSelectableBound
{
    protected override void OnLeftClick()
    {
        CanSelect = false;
        //CRoleObject ro = BindObject as CRoleObject;
        //if (ro == null || ro.cr == null)
        //    return;

        //if (CClientRole.IsEnemyCamp(ro.cr, Global.MainPlayer))
        //    Global.MainPlayer.AttackTarget = ro.cr.SN;

        //CanSelect = true;
    }
}

public class CPlayerBound : CSelectableBound
{
    protected override void Initialize()
    {
        if (this.BindObject is CRoleObject)
        {
         /*   CRoleObject ro = this.BindObject as CRoleObject;
            if (ro && ro.IsMainPlayer())
            {
                this.enabled = true;
                this.gameObject.layer = CDefines.Layer.MainPlayer;
                BoxCollider collider = this.gameObject.GetComponent<BoxCollider>();
                collider.isTrigger = true;
                Rigidbody rb = this.gameObject.AddComponent(typeof(Rigidbody)) as Rigidbody;
                rb.isKinematic = true;
            }*/
        }
    }

    protected override void OnLeftClick()
    {
        CanSelect = false;
        CRoleObject ro = BindObject as CRoleObject;

      /*  if (ro == null || ro.cr == null)
            return;

        if (!Global.world.Ref.IsDungeon)
        {
            PopupFuncContent menu = new PopupFuncContent();
            menu.data1 = ro.cr.SN;
            menu.funclist.Add(PopupFuncContent.PopupTag.GroupInvite);
            menu.funclist.Add(PopupFuncContent.PopupTag.TeamInvite);
            CPopupMenu.Create(menu);
        }

        if (CClientRole.IsEnemyCamp(ro.cr, Global.MainPlayer))
            Global.MainPlayer.AttackTarget = ro.cr.SN;
*/
        CanSelect = true;
    }
}

public class CMapObjectBound : CSelectableBound
{
    private CMapObject mapobj;
    protected override void Initialize()
    {
        if (this.BindObject is CMapObject)
            mapobj = this.BindObject as CMapObject;

     /*   if (mapobj.reference.ClickEffect == MapObjetType.Obstacle)
        {
            NavMeshObstacle Obstacle = this.gameObject.AddComponent<NavMeshObstacle>();
            Obstacle.carving = true;
            Obstacle.shape = NavMeshObstacleShape.Box;
        }*/
    }

    protected override void OnLeftClick()
    {
     //   CanSelect = mapobj.reference.CanChoose;
    }

    void OnTriggerEnter(Collider other)
    {
       /* if (mapobj.reference.ClickEffect == MapObjetType.AddBuff)
        {
            Global.world.OnTouchObject(mapobj.ActorData.SN);
        }*/
    }
}

