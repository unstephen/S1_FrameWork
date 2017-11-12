using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TweenWidth))]
public class TweenWidthEditor : UITweenerEditor
{
	public override void OnInspectorGUI ()
	{
		GUILayout.Space(6f);
		NGUIEditorTools.SetLabelWidth(120f);

		TweenWidth tw = target as TweenWidth;
		GUI.changed = false;

		float from = EditorGUILayout.FloatField("From", tw.from);
        float to = EditorGUILayout.FloatField("To", tw.to);

		if (from < 0) from = 0;
		if (to < 0) to = 0;

		if (GUI.changed)
		{
			NGUIEditorTools.RegisterUndo("Tween Change", tw);
			tw.from = from;
			tw.to = to;
            EditorUtility.SetDirty(tw);
		}

		DrawCommonProperties();
	}
}
