using UnityEngine;
using UnityEngine.UI;
using UnityEditor.UI;
using UnityEditor;

[CustomEditor(typeof(CImage))]
public class CImageEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CImage image = target as CImage;
        GUI.changed = false;
        if (image.sprite != null)
        {
            NGUILinkEditor.RegisterUndo("CImage Change", image);
            if (string.IsNullOrEmpty(image.SpriteName) || image.SpriteName != image.sprite.name)
                image.SpriteName = image.sprite.name;
            if (string.IsNullOrEmpty(image.AtlasName) || image.AtlasName != image.sprite.texture.name.ToLower())
                image.AtlasName = image.sprite.texture.name.ToLower();
        }
        EditorUtility.SetDirty(image);
    }
}
