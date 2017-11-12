using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
public struct Def
{
    public const int INVALID_ID = -1;               // 无效ID
    public const int MAX_MAP_STATE = 30;            // 最大地图状态]
    public const int MaxLoginRoleCount = 4;
    public const int SKILL_MAX_PLAYER_SKILL = 7;       //玩家最大技能个数
    public const int SKILL_NORMAL_ATTACK = 0;           //普通攻击的技能INDEX
    public const int SKILL_XP_ATTACK = 6;           //XP攻击的技能INDEX
    public const int SKILL_FINAL_ATTACK = 4;            //终结技能(大招)INDEX
    public const int SKILL_MICRO_ATTACK = 5;           //微操攻击的技能INDEX
    public const int COMBO_INTERVAL = 500;              //分段技能间隔（MS）
    public const int NPC_CHAT_DISTANCE = 3;                 //NPC对话距离
    public const int SEARCH_DISTANCE = 2;                 //技能搜索距离
    public const int MAX_BAG_COUNT = 200;                 //玩家的背包格子
    public const int MAX_PLAYER_QUEST = 50;               //玩家能接的最大任务数量
    public const string EQUIP_ID = "201";
    public const float AUTO_ATK_DIS = 3;              //自动战斗的 找怪范围
    public const int MAX_ENHANCE_LEVEL = 300; //装备最大追加等级
    public const int GEM_MAX_LEVEL = 9;//宝石最大等级
}

public class BuffRemoveType
{
    public const int Never = 0;//不失效
    public const int TP = 1;//回城后消失
    public const int Dead = 2;//死亡后消失
    public const int Hit = 3;//被击后消失（昏睡
    public const int HitOver = 4;//被击效果完后消失（护盾）
    public const int OffLine = 5;//离线后消失
}

public class MapType
{
    public const int CITY = 1;//"城市"
    public const int FIELD = 1;//"野外"
    public const int DUNGEON = 3;//"副本"
    public const int PVP = 3;//"pvp战场"
}

public class MapObjetType
{
    public const int None = 0;//无效果
    public const int Collect = 1;//采集物品
    public const int TP = 2;//传送点
    public const int ItemBox = 3;//宝箱
    public const int AddBuff = 4;//添加BUFF
    public const int Obstacle = 5;//动态阻挡 
}

public class CoinType
{
    public const int Silver = 1;//银币
    public const int Gold = 2;//金币
    public const int Stone = 3;//魔石
    public const int Honor = 4;//荣誉
}

public class PropertyState
{
    // 角色状态
    public const int STATE_MOVE = 0x00000001;           // 移动的状态,走,跑
    public const int STATE_JUMP = 0x00000002;           // 跳跃状态
    public const int STATE_CAST = 0x00000004;           // 施放技能
    public const int STATE_CAST_EX = 0x00000008;        // 不能被打断的施放

    public const int STATE_FIGHT = 0x00000010;          // 是否在战斗状态中
    public const int STATE_DIE = 0x00000020;            // 死亡状态
    public const int STATE_NPC_RETURN = 0x00000040;     // NPC离开战斗状态
    public const int STATE_HITBACK = 0x00000080;        // 硬直状态

    public const int STATE_HP_SHIELD = 0x00000100;      // 有血盾的状态
    public const int STATE_SOUL = 0x00000200;           // 武魂变身状态
    public const int STATE_CRAWL = 0x00000400;          // 爬行状态
    public const int STATE_FLY = 0x00000800;            // 飞行状态

    public const int RESTRICT_MOVE = 0x00001000;        // 禁止移动状态
    public const int RESTRICT_SKILL = 0x00002000;       // 禁止使用技能状态
    public const int RESTRICT_ITEM = 0x00004000;        // 禁止使用道具状态
    public const int RESTRICT_ATTACK = 0x00008000;      // 禁止攻击状态

    public const int IMMU_RESTRICT_MOVE = 0x00010000;   // 免疫禁止移动
    public const int IMMU_RESTRICT_SKILL = 0x00020000;  // 免疫禁止使用技能
    public const int IMMU_RESTRICT_ITEM = 0x00040000;   // 免疫禁止使用道具
    public const int IMMU_RESTRICT_ATTACK = 0x00080000; // 免疫禁止攻击
    public const int IMMU_DAMAGE = 0x00100000;          // 免疫伤害

    public const int STATE_HANG = 0x01000000;           // 离线挂机状态
    public const int STATE_PROTECT = 0x02000000;        // 保护模式
}

public class EquipKind
{
    public const int EQUIP_ARMS_WEAPON = 1;// "武器"
    public const int EQUIP_PROTECTION_HEAD = 2;//"头盔"
    public const int EQUIP_PROTECTION_SHOULDER = 3;//"肩膀"
    public const int EQUIP_PROTECTION_NECK = 4;// "项链"
    public const int EQUIP_PROTECTION_BODY = 5;// "衣服"
    public const int EQUIP_PROTECTION_FOREARM = 6;//"护腕"
    public const int EQUIP_PROTECTION_BELT = 7;//"腰带"
    public const int EQUIP_PROTECTION_CRUS = 8;//"鞋子"
    public const int EQUIP_PROTECTION_RING1 = 9;//"戒指"
    public const int EQUIP_PROTECTION_RING2 = 10;//"戒指"
    public const int MAX_EQUIP = 10; //最大装备数量
}

public class DiamondSlot
{
    public const int DIAMOND_FIRST = 0;
    public const int DIAMOND_SECOND = 1;
    public const int DIAMOND_THIRD = 2;
}


public class RoleOccupation
{
    public const int Warrior = 1;//战士：10001 
    public const int Mage = 2;//法师：10011 
    public const int Vampire = 3;//血族 10021 
}

public class RoleGender
{
    public const int Male = 1;//男
    public const int FeMale = 2;//女
}

public class EquipMask
{
    public const int Body = 0;//身体
    public const int LeftWP = 1;//左手武器
    public const int RightWP = 2;//右手武器
    public const int Wing = 3;//翅膀
    public const int OrbsLeg = 4;//法器脚部
    public const int OrbsHead = 5;//法器头部
    public const int OrbsBody = 6;//法器身体
    public const int Suit = 7;//套装特效
}

public class Numerical
{
    public const int EnhanceNum = 10000;//装备强化基数
}

public enum TeleportType
{
    Shoe = 1,//小飞鞋
}

public class LoadSceneType
{
    public const int AsyncLevel = 1;//通过过图场景切换
    public const int SyncLevel = 2;//当前场景直接切换
}

public enum SocialType
{
    Normal = 1,
    Friend = 2,
    Black = 3,
    Foe = 4,
    FriendToGift = 5,
}

public enum BattleResultType
{
    Fail = 0,
    Success = 1,
    Neutrality = 2,
}
[System.AttributeUsage(System.AttributeTargets.Field)]
public class EditorEnumAttribute : System.Attribute
{
    public EditorEnumAttribute(string name)
        : this(Def.INVALID_ID, name, String.Empty)
    {
    }

    public EditorEnumAttribute(int id, string name)
        : this(id, name, String.Empty)
    {
    }

    public EditorEnumAttribute(string name, string display)
        : this(Def.INVALID_ID, name, display)
    {
    }

    public EditorEnumAttribute(int id, string name, string display)
    {
        this.id = id;
        this.name = name;
        this.display = display;
    }

    public string Name { get { return name; } set { name = value; } }
    public string Comment { get { return name; } }
    public string Display { get { return display; } set { display = value; } }
    public int ID { get { return id; } }

    private int id;
    private string name;
    private string display;      //客户端用于显示的自定义字符串
}
//活动分类
public class ActivityKind
{
    [EditorEnum("日常活动")]
    public const int COMMON = 0;
    [EditorEnum("运营活动")]
    public const int OPERATE = 1;
    [EditorEnum("奖励活动")]
    public const int AWARD = 2;
    [EditorEnum("目标性活动")]
    public const int TARGET = 3;
    [EditorEnum("帮会活动")]
    public const int GROUP = 4;
    [EditorEnum("独立显示活动")]
    public const int SINGLE = 5;
    [EditorEnum("开服活动")]
    public const int SERVEROPEN = 6;
    [EditorEnum("日常限时活动")]
    public const int COMMONTIMELIMIT = 7;
}
//活动类型
public class ActivityType
{
    [EditorEnum("无")]
    public const int None = 0;
    [EditorEnum("team_fight")]
    public const int Team_Fight = 1;
}
public struct TimePeriodConfig
{
   // [FieldDef("[开-月", 50, "TimeMonth")]
    public short OpenMonth;
   // [FieldDef("开-日", 50, "TimeDay")]
    public short OpenDay;
    //[FieldDef("开-周", 50, "TimeWeekDay")]
    public short OpenWeekDay;
    //[FieldDef("开-时", 50, "TimeHour")]
    public short OpenHour;
   // [FieldDef("开-分]", 50, "TimeMinute")]
    public short OpenMinute;
   // [FieldDef("[关-月", 50, "TimeMonth")]
    public short CloseMonth;
    //[FieldDef("关-日", 50, "TimeDay")]
    public short CloseDay;
   // [FieldDef("关-周", 50, "TimeWeekDay")]
    public short CloseWeekDay;
   // [FieldDef("关-时", 50, "TimeHour")]
    public short CloseHour;
  //  [FieldDef("关-分]", 50, "TimeMinute")]
    public short CloseMinute;
}
public class ETeamFightLevel
{
    [EditorEnum("黑铁")]
    public const int TFL_IRON = 1;
    [EditorEnum("青铜")]
    public const int TFL_BRONZE = 2;
    [EditorEnum("白银")]
    public const int TFL_SILVER = 3;
    [EditorEnum("黄金")]
    public const int TFL_GLOD = 4;
    [EditorEnum("白金")]
    public const int TFL_PLATINUM = 5;
}
//活动进入结果
public enum ActvEnterResult
{
    [Describe("进入成功")]
    SUCCESS = 0,
    [Describe("未到等级")]
    LEVEL_LIMIT = 1,
    [Describe("等级过高")]
    LEVEL_OVER = 2,
    [Describe("次数限制")]
    COUNT_LIMIT = 3,
    [Describe("未开放")]
    TIME_LIMIT = 4,
    [Describe("人数限制")]
    PLAYER_COUNT_LIMIT = 5,
    [Describe("没有权限")]
    NO_ACCESS = 6,
    [Describe("提前结束")]
    PRE_OVER = 7,
    [Describe("活动结束")]
    FINISHED = 8,
    [Describe("不在安全区，无法进入")]
    NOT_IN_SAFEZONE = 9,
    [Describe("该活动系统暂未开启")]
    NOT_OPEN = 10,
    [Describe("内部错误")]
    INTERNAL_ERROR = 100,

}
public class PropertyType
{
    public const string LEVEL = "level";
    public const string HP = "hp";
    public const string MaxHP = "health";
    public const string XP = "xp";
    public const string MAXXP = "maxxp";
    public const string EXP = "exp";
    public const string COIN = "coin";
    public const string Gold = "gold";
    public const string Ep = "ep";
    public const string TITLE = "title";
    public const string FIGHTVALUE = "battlePower";
    public const string STRENGTH = "strength";//力量
    public const string INTELLIGENCE = "intelligence";//智力
    public const string ENDURANCE = "endurance";//耐力
    public const string VITALITY = "vitality";//体质
    public const string DEXTERITY = "dexterity";//敏捷
    public const string PHYSICALATTACK = "physicalAttack ";//物理攻击
    public const string MINPHYSICALATTACK = "minPhysicalAttack";//物攻min
    public const string MAXPHYSICALATTACK = "maxPhysicalAttack";//物攻max
    public const string MAGICATTACK = "magicAttack";//魔法攻击	
    public const string MINMAGICATTACK = "minMagicAttack";//魔攻min	
    public const string MAXMAGICATTACK = "maxMagicAttack";//魔攻max	
    public const string ARMOR = "armor";//物理防御	
    public const string RESISTANCE = "resistance";//魔法防御	
    public const string FP = "fp";//怒气值	
    public const string XPRECORVERY = "xpRecovery";//XP回速
    public const string HITRATING = "hitRating";//命中值	
    public const string DODGERATING = "dodgeRating";//闪避值
    public const string CRITICALCHANCE = "criticalChance";//暴击率
    public const string CRITICALRATING = "criticalRating";//暴击值
    public const string CRITICALDAMAGE = "criticalDamage";//暴击伤害
    public const string CRITICALDAMAGERATING = "criticalDamageRating";//暴击伤害值
    public const string CRITICALRESISTANCE = "criticalResistance";//抗暴击率
    public const string CRITICALRESISTANCERATING = "criticalResistanceRating";//抗暴击率值
    public const string CRITICALDAMAGEREDUCTION = "criticalDamageReduction";//抗暴击伤害
    public const string CRITICALDAMAGEREDUCTIONRATING = "criticalDamageReductionRating";//抗暴击伤害值
    public const string DAMAGEREDUCTION = "damageReduction";//伤害减免
    public const string PHYSICALDAMAGEREDUCTION = "physicalDamageReduction";//物理伤害减免
    public const string MAGICDAMAGEREDUCTION = "magicDamageReduction";//魔法伤害减免
    public const string DAMAGEINCREASE = "damageIncrease";//伤害增加
    public const string PYSICALDAMEGEINCREASE = "physicalDamageIncrease";//物理伤害增加
    public const string MAGICDAMAGEINCREASE = "magicDamageIncrease";//魔法伤害增加
    public const string TRUEDAMAGEINCREASE = "trueDamageIncrease";//最终伤害增加
    public const string TRUEDAMAGEREDUCTIN = "trueDamageReduction";//最终伤害减免
    public const string MOVESPEED = "moveSpeed";//移动速度
    public const string HIT = "hit";//命中率
    public const string DODGE = "dodge";//闪避率
    public const string BODYSTATE = "body_state";//xp变身状态
    public const string XPSTATE = "state";//xp变身状态
    public const string XPDURATION = "duration";//xp持续时间
    public const string MOUNTING = "mounting_id";//坐骑id
}


#region 网络事件相关
public class EventTag
{
    public const string Login = "login";
    public const string Account = "account:auth";
    public const string ChatLogin = "chat:auth";
    public const string Create = "character:create";
    public const string PlayerDBList = "character:list";
    public const string BackSelectRole = "logout";
    public const string BackLogin = "backLogin";
    public const string EnterGame = "enter";
    public const string EnterWorld = "enterworld";
    public const string EnterWorldSuccess = "enter_success";
    public const string GotoScene = "goto_scene";
    

    #region 移动同步
    public const string Move = "move";
    public const string StopMove = "stop";
    public const string Postion = "moved";
    public const string EndPostion = "stop";
    public const string TpPos = "fly_to_pos";
    public const string Transposition = "transposition"; // 微操技能移动同步协议
    public const string JumpTo = "jump_to";// 强行设置幻兽位置协议
    #endregion


    #region 技能相关
    public const string fireskill = "cast";

    public const string fireball = "missile";
    public const string skillresult = "cast_result";
    public const string damage = "damage";
    public const string skillhit = "be_hit";
    public const string FireBallHit = "cast_done";
    public const string SpellLevelup = "skill:level_up";
    public const string SpellLevelupEvent = "spell_level_up";
    public const string ChangeBody = "skill:change_body";
    #endregion

    public const string propschange = "synchro_props";
   

    public const string EnterPlayer = "appear";
    public const string EnterMonster = "appear_animal";
    public const string DeleteRole = "disappear";
    public const string BagSell = "bag:sell";
    public const string BagMultiSell = "bag:multi_sell";
    public const string BagLost = "bag:lost";
    public const string BagGain = "bag:gain";
    public const string BagStore = "bag:store";
    public const string BagUse = "bag:use";
    public const string PropChange = "prop_changed";
    public const string Equip = "equipments:equip";
    public const string UnEquip = "equipments:unequip";
    public const string EnhanceEquip = "equipments:enhance";
    public const string GemEmbed = "equipments:embed";
    public const string GemUnEmbed = "equipments:unembed";
    public const string GemUp = "equipments:gemup";
    public const string AddFriend = "relation:add_friend_apply";
    public const string RecAddFriend = "relation:recv_add_friend_apply";
    public const string DealFriend = "relation:dealwith_friend_apply";
    public const string FindFriend = "relation:get_role_info";
    public const string AddBlack = "relation:add_black_apply";
    public const string DelFriend = "relation:delete_friend_apply";
    public const string DelBlack = "relation:delete_black_apply";
    public const string FriendGift = "relation:use_friendly_value_item";
    public const string RecFriendGift = "relation:add_friendly_value";
    public const string GainEudemon = "eudemons:gain";
    public const string ComposeEudemon = "eudemons:compose";
    public const string LostEudemon = "eudemons:lost";
    public const string FormatEudemon = "eudemons:formation";
    public const string FormatEudemonChanged = "eudemons:formation_changed";
    public const string EvolveEudemon = "eudemons:evolve";
    public const string Eudemonevolved = "eudemons:evolved";
    public const string EudemonLevelUp = "eudemons:levelup";

    public const string GroupCreate = "rolegroup:create_group";
    public const string GroupCreateSucess = "rolegroup:create_group_sucess";
    public const string GroupCreateFail = "rolegroup:create_group_fail";
    public const string GroupAddToGroup = "rolegroup:add_to_group";
    public const string GroupListRequest = "groups:group_list_to_client";
    public const string GroupApply = "rolegroup:join_request";
    public const string GroupInfoRequest = "group:group_info";
    public const string GroupInvite = "relation:add_friendly_value";
    public const string GroupInviteAgree = "relation:add_friendly_value";
    public const string GroupInviteRefuse = "relation:add_friendly_value";
    public const string GroupQuit = "group:remove_member";
    public const string GroupAgree = "group:handle_join_request";
    public const string GroupMemberListRequest = "group:member_list";
    public const string GroupApplyListRequest = "group:join_request_list";
    public const string GroupDeclaration = "group:modify_declaration";
    public const string GroupNotice = "group:modify_notice";
    public const string GroupChangeName = "rolegroup:modify_groupname";
    public const string GroupChangeNameSucess = "group:modify_group_name_success";
    public const string GroupChangeNameFail = "rolegroup:modify_group_name_fail";
    public const string GroupMail = "relation:add_friendly_value";
    public const string GroupMemberAdd = "group:add_member";
    public const string GroupMemberUpdate = "relation:add_friendly_value";
    public const string GroupMemberRemove = "rolegroup:remove_from_group";
    public const string GroupAppoint = "group:handle_member_type";
    public const string GroupAppointChief = "group:handle_leader";
    public const string GroupTransfer = "group:handle_transfer";
    public const string GroupAppointLeader = "group:handle_chief";
    public const string GroupAutoJoin = "group:make_auto_join";
    

    public const string TitleLevelUp = "titles:levelup";
    public const string FightValueDetails = "battlepower:details";

    public const string GemGain = "gems:gain";
    public const string GemLost = "gems:lost";
    public const string GemStore = "gems:store";

    #region 任务
    public const string QuestAccept = "quest:accept";
    public const string QuestSubmit = "quest:submit";
    public const string QuestUpdate = "update_quest";
    public const string QuestDelete = "del_quest";
    public const string QuestAdd = "add_quest";
    public const string QuestCancel = "quest:give_up";
    public const string Questdialogue = "quest:dialogue_over";

    public const string PlayPlot = "play_plot";
    public const string PlayerTrigger = "skill:trigger_object";
    public const string BattleResult = "instance_result";
    public const string ExitDuplicate = "exit_instance";

    public const string Hint = "hint";//公用提示，两参数 第一个类型，第二个提示ID;
    #endregion

    #region 时装坐骑
    public const string DressExpire = "dresses:expired";
    public const string MountExpire = "mounts:expired";
    public const string ConfirmDressExpire = "dresses:confirm";
    public const string ConfirmMountExpire = "mounts:confirm";
    public const string UnMount = "mounts:unmount";
    public const string Mount = "mounts:mount";
    public const string UnDress = "dresses:undress";
    public const string Wear = "dresses:wear";
    public const string DressActive = "dresses:active";
    public const string MountActive = "mounts:active";
    public const string SetMount = "skill:set_mount";
    #endregion

    public const string AddBuff = "add_buff";
    public const string RemoveBuff = "del_buff";

    #region 摆摊
    public const string StallsSubmit = "stalls:submit";
    public const string StallsSubmitting = "stalls:submitting";
    public const string StallsSubmited = "stalls:submitted";

    public const string StallsCancel = "stalls:cancel";
    public const string StallsCancelled = "stalls:cancelled";

    public const string StallsRenew = "stalls:renew";
    public const string StallsRenewed = "stalls:renewed";

    public const string StallsRefresh = "stalls:refresh";
    public const string StallsRefreshed = "stalls:refreshed";

    public const string Stalls = "stalls";
    public const string StallsList = "stalls:list";

    public const string StallsBuy = "stalls:buy";
    public const string StallsSold = "stalls:sold";

    public const string StallsUpdate = "stalls:update";
    public const string StallsClaim = "stalls:claim";
    public const string StallsClaimed = "stalls:claimed";
    public const string StallsExpired = "stalls:expired";
    #endregion

    #region 拍卖
    public const string AuctionsListRqst = "auctions:list";
    public const string AuctionsListRsp = "auctions";
    public const string AuctionNewPrice = "auctions:refresh";
    public const string AuctionAddPrice = "auctions:bid";
    public const string AuctionNewBidder = "bid:fail";
    #endregion

    #region 活动
    public const string Activity = "activity:";
    public const string EnterPrepareScene = "enter_prepare_scene";
    public const string EnterPrepareSceneSuccess = "team_fight:enter_prepare_scene_success";
    public const string EnterPrepareSceneReset = "team_fight:auto_pass";
    public const string FightEndTime = "team_fight:fight_end_time";
    public const string FightKillNotify = "team_fight:kill_notify";
    public const string FightResult = "team_fight:fight_result";
    public const string FightLevel = "team_fight:start_level";
    public const string FightFinalResult = "team_fight:final_result";
    #endregion
    #region 邮件
    public const string MailRequestList = "mail:list";
    public const string MailListRsp = "mail:list";
    public const string MailReadRqst = "mail:read";
    public const string MailClaimRqst = "mail:get_attachment";
    public const string MailClaimRsp = "mail:get_attachment";
    public const string MailDeleteRqst = "mail:delete";
    public const string MailDeleteRsp = "mail:delete";
    public const string MailClaimAllRqst = "mail:get_all_attachment";
    public const string MailClaimAllRsp = "mail:get_all_attachment";
    public const string MailReceived = "mail:received";

    #endregion
    #region 副本
    public const string AskChangeMap = "instance";
    public const string ExitMap = "exit_instance";
    public const string OnTouchObject = "skill:touch_obj";
    #endregion

    #region 心态包
    public const string Ping = "ping";
    public const string Pong = "pong";
    #endregion

    #region 组队
    public const string TeamInvite = "team:add_team_member";
    #endregion
}

public class ActionTag
{
    public const string Login = "Login";
    public const string CreateRole = "createRole";
    public const string LoginCreatRole = "2createRole";
    public const string LoginSelectedRole = "2selectRole";
    public const string LoginRoleSelected = "LoginRoleSelected";
    public const string StartGame = "StartGame";
    public const string SellItem = "SellItem";
    public const string EquipItem = "EquipItem";
    public const string UnEquipItem = "UnEquipItem";

    public const string HpChanged = "hp_changed";
    public const string LevelChanged = "level_changed";
    public const string ExpChanged = "exp_changed";
    public const string xpChanged = "xp_changed";
    public const string CoinChanged = "coin_changed";
    public const string OpenSendGift = "openSendGift";

}

public class ChannelTag
{
    public const string Channel_Private = "chat:private";
}

public class BackGroundName
{
    public const string UIBG = "backGroud";
    public const string RADERBG = "ditu";
    public const string CUP = "jiangbei";
}
#endregion

    #region 军团相关
public enum GroupRight
{
    /// <summary>
    /// 领袖
    /// </summary>
    GR_CHIEF = 1,
    /// <summary>
    /// 领袖伴侣
    /// </summary>
    GR_CHIEF_MATE,
    /// <summary>
    /// 军团长伴侣
    /// </summary>
    GR_LEADER_MATE,
    /// <summary>
    /// 副团长伴侣
    /// </summary>
    GR_VICELEADER_MATE,
    /// <summary>
    /// 军团长
    /// </summary>
    GR_LEADER,
    /// <summary>
    /// 副军团长
    /// </summary>
    GR_VICELEADER,
    /// <summary>
    /// 元老
    /// </summary>
    GR_FOUNDING,
    /// <summary>
    /// 议员
    /// </summary>
    GR_SENATOR,
    /// <summary>
    /// 玫瑰女神
    /// </summary>
    GR_ROSE,
    /// <summary>
    /// 团员
    /// </summary>
    GR_MEMBER,
    /// <summary>
    /// 预备团员
    /// </summary>
    GR_RESERVE,
    GR_MAX
}
public enum GroupTitleClass
{
    GTC_Family,
    GTC_Team,
    GTC_Group,
    GTC_Default,
    GTC_Max
}
#endregion
    #region 任务相关

public class QuestType
{
    public const int Mian = 1;//主线
    public const int Branch = 2;//支线
    public const int Daily = 3;//日常
}

public class QuestCompleteConditionType
{
    public const int KillMonster = 11001;			// 杀怪
    public const int TalkWithNpc = 15002;           // 与NPC进行对话
    public const int NoCompleteCondition = 0;       // 无完成条件
}

public class DescribeAttribute : Attribute
{
    private string name;

    public DescribeAttribute(string name)
    {
        this.Name = name;
    }

    public readonly string Data;
    public readonly int Tag;
    public static readonly DescribeAttribute Empty = new DescribeAttribute(string.Empty, string.Empty, 0);
    public DescribeAttribute(string name, string data)
        : this(name)
    {
        this.Data = data;
    }
    public DescribeAttribute(string name, int tag)
        : this(name)
    {
        this.Tag = tag;
    }
    public DescribeAttribute(string name, string data, int tag)
        : this(name, data)
    {
        this.Tag = tag;
    }
    public string Name { get { return name; } set { name = value; } }
}


public static class EnumDescribe<T>
{
    private static readonly Dictionary<T, DescribeAttribute> dict_ = new Dictionary<T, DescribeAttribute>();

    static EnumDescribe()
    {
        FieldInfo[] fiList = typeof(T).GetFields();
        foreach (FieldInfo fi in fiList)
        {
            object[] attrList = fi.GetCustomAttributes(typeof(DescribeAttribute), true);
            if (attrList.Length > 0)
            {
                var da = attrList[0] as DescribeAttribute;
                if (da != null)
                {
                    var t = (T)fi.GetValue(typeof(T));
                    dict_[t] = da;

                    //add by cdh 20161229
                    if (!typeof(T).IsEnum)
                        da.Name = Localization.Get(string.Format("{0}_{1}", typeof(T).FullName, fi.GetValue(null)));
                    else
                    {
                        if (fi.FieldType.Name == typeof(T).Name)
                        {
                            da.Name = Localization.Get(string.Format("{0}_{1}", typeof(T).FullName, Convert.ToInt32(t)));
                        }
                    }
                }
            }
        }
    }

    public static DescribeAttribute Get(T t)
    {
        DescribeAttribute result;
        if (dict_.TryGetValue(t, out result))
        {
            return result;
        }
        else
        {
            return new DescribeAttribute(t.ToString());
        }
    }

    public static void SetName(T t, string name)
    {
        DescribeAttribute attr;
        if (dict_.TryGetValue(t, out attr))
            attr.Name = name;
    }
}
#endregion
#region 离散数据id
public class DiscreteTag
{
    public const long FriendGift = 799000044;
    public const long ChangeGroupName = 799000045;
    public const long CreateGroup = 799000070;
}
#endregion

