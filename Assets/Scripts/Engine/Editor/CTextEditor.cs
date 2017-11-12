using UnityEngine;
using UnityEditor.UI;
using UnityEditor;
using System;
using UnityEngine.UI;

[CustomEditor(typeof(CText))]
public class CTextEditor : Editor
{
    public static string[] colors = new string[] { "FFF9CE", "00FF01", "1000FF"};
    public static string sdColor = "000000";    // 阴影 styleIdx = 1
    public static string olColor = "FFEFB7";    // ffefb7 外发光 styleIdx = 2

    CText text;

    void Awake()
    {
        GetTextAsset();
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CText text = target as CText;
        GUI.changed = false;
        if (text.font != null)
        {
            NGUILinkEditor.RegisterUndo("CImage Change", text);
            if (string.IsNullOrEmpty(text.FontName) || text.FontName != text.font.name)
                text.FontName = text.font.name;
        }
        
        EditorGUILayout.LabelField("预制颜色");
        if (null != colors && colors.Length > 0)
        {
            //text.color = GetColor("", text.colorIdx);

            for (int i = 0; i < colors.Length; i++)
            {
                if (i % 4 == 0)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
                GUIStyle myStyle = GUI.skin.box;
                myStyle.fontStyle = FontStyle.Bold;
                myStyle.normal.textColor = GetColor("", i);
                myStyle.fixedWidth = 80;
                if (GUILayout.Button(colors[i], myStyle))
                {
                    if (text)
                    {
                        text.colorIdx = i;
                        //text.color = GetColor("", text.colorIdx);
                        //SetStyle(text, 1);
                        //SetStyle(text);
                        SetTextColorFromColorText(text, colors[i]);
                    }
                }
            }
        }
        EditorUtility.SetDirty(text);
    }

    public static void SetStyle(CText text, int styleIdx = 0)
    {
        //if (text.colorIdx >= colors.Length)
        //    text.colorIdx = colors.Length - 1;
        //string[] styleStr = colors[text.colorIdx].Split(';');
        //text.color = GetColor(styleStr[0]);

        //if (!text.shadowEff)
        //    text.shadowEff = text.gameObject.AddComponent<UnityEngine.UI.Shadow>();
        //if (!text.outlineEff)
        //    text.outlineEff = text.gameObject.AddComponent<Outline>();

        //text.shadowEff.enabled = false;
        //text.outlineEff.enabled = false;

        //if (styleStr.Length > 1)
        //{
        //    for (int i = 1; i < styleStr.Length; i++)
        //    {
        //        string[] osStr = styleStr[i].Split(':');
        //        if (osStr[0] == "s")
        //        {
        //            text.shadowEff.enabled = true;
        //            text.shadowEff.effectColor = GetColor(osStr[1]);
        //        }
        //        else if (osStr[0] == "o")
        //        {
        //            text.outlineEff.enabled = true;
        //            text.outlineEff.effectColor = GetColor(osStr[1]);
        //        }
        //    }
        //}
        if (!text) return;
        UnityEngine.UI.Shadow[] shadows = text.GetComponentsInChildren<UnityEngine.UI.Shadow>();
        for (int i = 0; i < shadows.Length; i++)
        {
            DestroyImmediate(shadows[i]);
        }
        
        if(styleIdx == 1)
        {
            UnityEngine.UI.Shadow sd = text.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            sd.effectColor = GetColor(sdColor);
        }
        else if (styleIdx == 2)
        {
            UnityEngine.UI.Outline ol = text.gameObject.AddComponent<UnityEngine.UI.Outline>();
            ol.effectColor = GetColor(olColor);
        }
    }

    public static void SetTextColorFromColorText(CText text, string color)
    {
        for (int i = 0; i < colors.Length; i++)
        {
            if (colors[i] == color)
            {
                text.colorIdx = i;
                text.color = GetColor("", text.colorIdx);
                break;
            }
        }
    }

    public static Color32 GetColor(string colorStr, int index = 0)
    {
        string cStr = colorStr;
        if(string.IsNullOrEmpty(cStr))
        {
            if (index > colors.Length || index < 0)
                index = 0;
            cStr = colors[index];
        }
        byte r = 0;
        byte g = 0;
        byte b = 0;
        byte a = 255;
        byte.TryParse(cStr.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out r);
        byte.TryParse(cStr.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out g);
        byte.TryParse(cStr.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out b);

        Color32 c = new Color32(r, g, b, a);
        return c;
    }

    public static void GetTextAsset()
    {
        TextAsset asset = Resources.Load("ColorText") as TextAsset;
        byte[] b = System.Text.Encoding.Default.GetBytes(asset.text);
        string str = System.Text.Encoding.UTF8.GetString(b);
        colors = str.Replace("\r\n", ",").Split(',');
    }
}