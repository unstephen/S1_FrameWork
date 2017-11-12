using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using SimpleSpritePacker;

static public class MOYU_UIToolsEditor
{
    public static Font font, titlefont;
    static void LoadFont()
    {
        if (!font)
            font = Resources.Load("UI/Login/uifont", typeof(Font)) as Font;
        if (!titlefont)
            titlefont = Resources.Load("UI/Login/uifont_title", typeof(Font)) as Font;

        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/UI相关/BuildTex")]
    private static void BuildUITex()
    {
        foreach (Object asset in Selection.objects)
        {
            AssetDatabase.Refresh();
            if (asset is Texture)
                AssetToolsEditor.BuildAsset(asset, "tex", "Assets/StreamingAssets/res/ui/tex");
        }
    }

    #region UI打包


    [MenuItem("Assets/UI相关/一键UI打包/all")]
    private static void BuildAllUI()
    {
        BuildAllUISelf();
        BuildAllSprite();
        BuildAllTex();
    }

    [MenuItem("Assets/UI相关/一键UI打包/UI")]
    private static void BuildAllUISelf()
    {
        Selection.activeObject = AssetDatabase.LoadAssetAtPath("Assets/Resources/UIPrefab", typeof(Object));
        Object[] uguis = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        List<Object> uguilist = new List<Object>();
        for (int i = 0; i < uguis.Length; i++)
        {
            if (uguis[i] is GameObject)
                uguilist.Add(uguis[i]);
        }
        Selection.objects = uguilist.ToArray();
        BuildUISelf();
    }

    [MenuItem("Assets/UI相关/一键UI打包/Sprite")]
    private static void BuildAllSprite()
    {
        Selection.activeObject = AssetDatabase.LoadAssetAtPath("Assets/Resources/UI/Sprite", typeof(Object));
        Object[] sprites = Selection.GetFiltered(typeof(SPInstance), SelectionMode.DeepAssets);
        List<Object> spritelist = new List<Object>();
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] is SPInstance)
                spritelist.Add(sprites[i]);
        }
        Selection.objects = spritelist.ToArray();
        BuildSprite();
    }

    [MenuItem("Assets/UI相关/一键UI打包/rawImage")]
    private static void BuildAllTex()
    {
        Selection.activeObject = AssetDatabase.LoadAssetAtPath("Assets/Resources/UI/RawImage", typeof(Object));
        Object[] texs = Selection.GetFiltered(typeof(Texture), SelectionMode.DeepAssets);
        List<Object> texlist = new List<Object>();
        for (int i = 0; i < texs.Length; i++)
        {
            if (texs[i] is Texture)
                texlist.Add(texs[i]);
        }
        Selection.objects = texlist.ToArray();
        BuildUITex();
    }

    [MenuItem("Assets/UI相关/BuildUI/self")]
    static void BuildUISelf()
    {
        BuildUI(false);
    }

    [MenuItem("Assets/UI相关/BuildUI/child")]
    static void BuildUIChild()
    {
        BuildUI(true);
    }
    static void BuildUI(bool child)
    {
        UnityEngine.Object[] objects = Selection.objects;
        foreach (Object asset in objects)
        {
            if (asset is GameObject)
            {
                GameObject UI = asset as GameObject;
                if (child)
                    RepairUGUI(UI);

                Image[] images = UI.GetComponentsInChildren<Image>(true);

                if (images != null && images.Length > 0)
                {
                    for (int i = 0; i < images.Length; i++)
                    {
                        if (images[i] is CImage)
                            continue;
                        EditorUtility.DisplayDialog("Error", asset.name + "包含Image，请执行>> UI脚本更换 <<", "OK");
                        return;
                    }
                }

                Text[] textarray = UI.GetComponentsInChildren<Text>(true);
                if (textarray != null && textarray.Length > 0)
                {
                    for (int i = 0; i < textarray.Length; i++)
                    {
                        if (textarray[i] is CText)
                            continue;
                        EditorUtility.DisplayDialog("Error", asset.name + "包含Text，请执行>> UI脚本更换 <<", "OK");
                        return;
                    }
                }
               
                CImage[] sprites = UI.GetComponentsInChildren<CImage>(true);
                for (int j = 0; j < sprites.Length; j++)
                {
                    CImage sprite = sprites[j];
                    if (child)
                        BuildSprite(sprite.sprite);
                    sprite.sprite = null;
                }

                CText[] texts = UI.GetComponentsInChildren<CText>(true);
                for (int j = 0; j < texts.Length; j++)
                    texts[j].font = null;
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
                AssetToolsEditor.BuildAsset(asset, "ui", "Assets/StreamingAssets/res/ui/uiprefab");

               RepairUGUI(UI);
            }
        }
        AssetDatabase.Refresh();
    }

    #endregion


    #region 图集打包

    [MenuItem("Assets/UI相关/BuildSprite")]
    private static void BuildSprite()
    {
        foreach (Object asset in Selection.objects)
        {
            BuildSprite(asset);
        }
        AssetDatabase.Refresh();
    }

    static void BuildSprite(Object asset)
    {
        AssetDatabase.Refresh();
        if (asset is SimpleSpritePacker.SPInstance)
            AssetToolsEditor.BuildAsset(asset, "sp", "Assets/StreamingAssets/res/ui/sprite");
    }

    #endregion


    #region UI修复

    [MenuItem("Assets/UI相关/UI修复")]
    static void RepairUGUI()
    {
        LoadFont();

        foreach (Object asset in Selection.objects)
        {
            if (asset is GameObject)
                RepairUGUI(asset as GameObject);
        }
        AssetDatabase.Refresh();
    }

    static void RepairUGUI(GameObject asset)
    {
        LoadFont();
        Dictionary<string, SPInstance> spdic = new Dictionary<string, SPInstance>();
        GameObject UI = asset as GameObject;
        CImage[] sprites = UI.GetComponentsInChildren<CImage>(true);
        for (int j = 0; j < sprites.Length; j++)
        {
            CImage sprite = sprites[j];
            if (string.IsNullOrEmpty(sprite.AtlasName)) 
                continue;
            SPInstance sp;
            if (!spdic.TryGetValue(sprite.AtlasName, out sp))
            {
                sp = Resources.Load(string.Format("UI/Sprite/{0}", sprite.AtlasName), typeof(SPInstance)) as SPInstance;
                if (!sp)
                    continue;
                spdic[sprite.AtlasName] = sp;
            }
            sprite.sprite = sp.GetSprite(sprite.SpriteName); ;
            //sprite.material = Resources.Load(string.Format("UI/UIScenes/mat/{0}",) as Material;
        }

        CText[] texts = UI.GetComponentsInChildren<CText>(true);
        for (int j = 0; j < texts.Length; j++)
        {
            CText t = texts[j];
            if (t.FontName == font.name)
                t.font = font;
            else
                t.font = titlefont;
        }

        AssetDatabase.Refresh();
    }
    #endregion


    [MenuItem("Assets/UI相关/UI脚本更换")]
    static void ReplaceScripts()
    {
        foreach (Object asset in Selection.objects)
        {
            if (asset is GameObject)
                ReplaceScript(asset as GameObject);
        }
        AssetDatabase.Refresh();
    }
    static void ReplaceScript(GameObject asset)
    {
        GameObject UI = asset as GameObject;
        Image[] Images = UI.GetComponentsInChildren<Image>(true);
        for (int j = 0; j < Images.Length; j++)
        {
            Image image = Images[j];
            Sprite sprite = image.sprite;
            Material mat = image.material;
            GameObject go = image.gameObject;
            UnityEngine.Object.DestroyImmediate(image, true);
            CImage cimage = go.AddComponent<CImage>();
            cimage.sprite = sprite;
            cimage.material = mat;
            cimage.color = image.color;
            cimage.raycastTarget = image.raycastTarget;
            cimage.type = image.type;
            cimage.preserveAspect = image.preserveAspect;
            if (!cimage.sprite)
            {
                Debug.Log(go.name, go);
                continue;
            }
            cimage.AtlasName = cimage.sprite.texture.name.ToLower();
            cimage.SpriteName = cimage.sprite.name;
        }

        Text[] Texts = UI.GetComponentsInChildren<Text>(true);
        for (int j = 0; j < Texts.Length; j++)
        {
            Text text = Texts[j];
            Font f = text.font;
            Material m = text.material;
            GameObject go = text.gameObject;
            UnityEngine.Object.DestroyImmediate(text, true);
            CText ctext = go.AddComponent<CText>();
            ctext.font = f;
            ctext.text = text.text;
            ctext.fontStyle = text.fontStyle;
            ctext.fontSize = text.fontSize;
            ctext.lineSpacing = text.lineSpacing;
            ctext.supportRichText = text.supportRichText;
            ctext.alignment = text.alignment;
            ctext.alignByGeometry = text.alignByGeometry;
            ctext.horizontalOverflow = text.horizontalOverflow;
            ctext.verticalOverflow = text.verticalOverflow;
            ctext.resizeTextForBestFit = text.resizeTextForBestFit;
            ctext.color = text.color;
            ctext.material = text.material;
            ctext.raycastTarget = text.raycastTarget;
            if (!text.font)
            {
                Debug.Log(ctext.name, ctext);
                continue;
            }
            ctext.FontName = text.font.name;
        }
        AssetDatabase.Refresh();
        SaveUGUI(asset);
        CheckUGUI(asset);
    }

    public static void SaveUGUI(GameObject UI)
    {
        CImage[] sprites = UI.GetComponentsInChildren<CImage>(true);
        for (int j = 0; j < sprites.Length; j++)
        {
            CImage sprite = sprites[j];
            if (!sprite.sprite)
                continue;
            sprite.AtlasName = sprite.sprite.texture.name.ToLower(); ;
            sprite.SpriteName = sprite.sprite.name;
        }

        CText[] texts = UI.GetComponentsInChildren<CText>(true);
        for (int j = 0; j < texts.Length; j++)
        {
            CText t = texts[j];
            if (!t.font)
                continue;
            t.FontName = t.font.name;
        }

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    public static void CheckUGUI(GameObject UI)
    {
        Canvas[] Canvas = UI.GetComponentsInChildren<Canvas>(true);
        for (int j = 0; j < Canvas.Length; j++)
        {
            if (Canvas[j].gameObject == UI)
                continue;
            UnityEngine.Object.DestroyImmediate(Canvas[j], true);
        }

        Transform[] gos = UI.GetComponentsInChildren<Transform>(true);
        for (int j = 0; j < gos.Length; j++)
        {
            Transform go = gos[j];
            MonoBehaviour[] monos = go.GetComponents<MonoBehaviour>();

            for (int i = 0; i < monos.Length; i++)
            {
                if (!monos[i])
                {
                    Debug.LogError(string.Format("{0} 有脚本丢失", go.name));
                }
            }
        }


        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }
}
