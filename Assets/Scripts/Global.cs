using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleSpritePacker;
using UnityGameFramework.Runtime;
using LitJson;
/// <summary>
/// 场景名称
/// </summary>
public static class SceneName
{

    /// <summary>
    /// 登陆
    /// </summary>
    public const string LOGIN_SCENE = "Login";

    /// <summary>
    /// 异步加载场景名称
    /// </summary>
    public const string ASYNC_LOADER_SCENE = "AsyncLevelLoader";

    /// <summary>
    /// 选择角色界面
    /// </summary>
    public const string ROLE_SELECT_SCENE = "SelecteRole";

    public static bool InUnity(string scene)
    {
        return scene == LOGIN_SCENE || scene == ASYNC_LOADER_SCENE || scene == ROLE_SELECT_SCENE;
    }
}

public static class Global 
{
    public static int FrameRate = 30;
    public static bool IsInGame { get { return CGameManager.IsLoadGameScene(); } }
  //  public static bool IsGameState { get { return game_state is CGameState_Game && world != null; } }
    public static CGameManager game_mgr = null;
    public static CSceneManager scene_mgr = null;
    public static JsonData player_list = null;
    public static BaseComponent baseComponent = null;
    public static NetworkComponent network = null;
    public static IDisposable networkDisposable = null;
    public static IDisposable networkChatDisposable = null;
    public static EventComponent game_event = null;
    public static CUIManager ui_mgr = null;
    public static float Networkdelay;

    public static float realtimeSinceLevelLoaded;
    public static Font uifont = null;
    public static Font uifont_title = null;

    public static string UItext = string.Empty;
    public static bool first_enter_game = true;

    public static BadWordChecker chatBadWord_ = new BadWordChecker();
    public static List<string> welcome_words_ = new List<string>();
    public static List<string> warmprompt = new List<string>();

    public static MobileBloom bloom = null;
    public static AmplifyColorEffect Amplify = null;
    public static CResourceFactory res_factory;
    public static FPS fps;
    public static ConfigHelper publishConfig = null;
    public static string Language = "language";
    public static uint server_open_time;
    public static Camera LodCamera;
    public static JsonData MPData = null;
}