using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using System.IO;
public static class AssetToolsEditor
{
    [MenuItem("Assets/删除manifest")]
    static void DeleteManifest()
    {
        string copypath = EditorUtility.SaveFolderPanel("CopyRes", Application.dataPath, "");
        if (string.IsNullOrEmpty(copypath))
            return;
        string[] files = Directory.GetFiles(copypath, "*.*", SearchOption.AllDirectories);
        for (int j = 0; j < files.Length; j++)
        {
            string child = files[j];
            child = Path.GetExtension(child);
            if (child == ".manifest" || string.IsNullOrEmpty(child))
            {
                string path = Path.GetFullPath(files[j]);
                File.Delete(path);
            }
        }
        AssetDatabase.Refresh();
    }

    [MenuItem("魔域/整理场景回滚")]
    private static void Reback()
    {
        if (!(Selection.activeObject is GameObject))
            return;

        GameObject go = Selection.activeObject as GameObject;
        Renderer[] renders = go.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renders)
        {
            r.shadowCastingMode = ShadowCastingMode.On;
            r.receiveShadows = true;
            foreach (Material m in r.sharedMaterials)
            {
                if (!m || !m.shader)
                    continue;
                string shadername = m.shader.name;
				
				if (shadername == "MOYU/VertexLit") {
					m.shader = Shader.Find ("Legacy Shaders/Diffuse");
					continue;
				}
				if (shadername == ("MOYU/AlphaTest")) {
					m.shader = Shader.Find ("Legacy Shaders/Transparent/Cutout/Diffuse");
					continue;
				}
				if (shadername == ("MOYU/AlphaBlend")) {
					m.shader = Shader.Find ("Legacy Shaders/Transparent/Diffuse");
					continue;
				}
                if (shadername.Contains("MOYU/"))
                    continue;

                if (shadername == ("Unlit/Texture"))
                    continue;

                if (shadername == "Particles/Additive (Soft)")
                    continue;
				Debug.LogError(string.Format("无法识别的shader {0}", shadername), r.gameObject);

				//EditorUtility.DisplayDialog("", string.Format("无法识别的shader {0}", shadername), "OK");
            }
        }
    }

    [MenuItem("魔域/整理场景配置")]
    public static void CheckRoot()
    {
        Transform root = (Selection.activeObject as GameObject).transform;
        if (root.tag != "root")
        {

            EditorUtility.DisplayDialog("Error", "请选择root检查", "OK");
            return;
        }

        DoCheckShader(root);
        CheckMainCamera();
		string groups = "effect/mainbuilding/prop/terrain/Reflection/collider/level/animtion/audio";
        List<string> rootGroups = new List<string>(groups.Split('/'));

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (Camera.main.transform == child)
                continue;
            if (!rootGroups.Contains(child.name))
                Debug.LogError(string.Format("错误的打组{0} ", child.name), child);
            else
                DoCheckNode(child);
        }
        EditorUtility.DisplayDialog("", "整理完毕!", "OK");
    }

    private static void DoCheckShader(Transform root) {
        Renderer[] renders = root.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renders) {
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.lightProbeUsage = LightProbeUsage.Off;
            r.reflectionProbeUsage = ReflectionProbeUsage.Off;
            r.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            foreach (Material m in r.sharedMaterials) {
                if (!m || !m.shader)
                    continue;
                string shadername = m.shader.name;

				if (shadername.EndsWith ("/Additive") || shadername == "Particles/Additive") {
					m.shader = Shader.Find ("MOYU/Particles/Additive");
					continue;
				}
				if (shadername.EndsWith ("/AlphaBlended") || shadername == "Particles/AlphaBlended") {
					m.shader = Shader.Find ("MOYU/Particles/AlphaBlended");
					continue;
				}

                Texture tex = m.mainTexture;
                if (tex)
                {
                    string path = AssetDatabase.GetAssetPath(tex);
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (!importer.DoesSourceTextureHaveAlpha())
                        m.shader = Shader.Find("MOYU/VertexLit");
                }

                if (shadername.Contains("MOYU/"))
                    continue;

				if (shadername==("Unlit/Texture"))
					continue;

                if (shadername == "Particles/Additive (Soft)")
                    continue;

				if (shadername == "Legacy Shaders/Diffuse" || shadername == "Mobile/Diffuse") {
					m.shader = Shader.Find ("MOYU/VertexLit");
					continue;
				}
                if ((shadername.StartsWith("Legacy Shaders")))
                {
					if (shadername.Contains("Legacy Shaders/Transparent/Cutout"))
                        m.shader = Shader.Find("MOYU/AlphaTest");
                    else
                        m.shader = Shader.Find("MOYU/AlphaBlend");
					continue;
                }


                 Debug.LogError(string.Format("无法识别的shader {0}", shadername), r.gameObject);
				EditorUtility.DisplayDialog("", string.Format("无法识别的shader {0}", shadername), "OK");
            }

            if (!r.enabled) {
                MeshFilter mf = r.gameObject.GetComponent<MeshFilter>();
                if (mf)
                    UnityEngine.Object.DestroyImmediate(mf);
                UnityEngine.Object.DestroyImmediate(r);
            }
        }

        RemoveAni(root.gameObject, false);
    }

    private static void RemoveAni(GameObject root,bool clear)
    {
        Animation[] anis = root.GetComponentsInChildren<Animation>(true);
        for (int i = 0; i < anis.Length; i++)
        {
            if (anis[i].GetClipCount() == 0 || clear)
                UnityEngine.Object.DestroyImmediate(anis[i]);
        }

        Animator[] Animators = root.GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < Animators.Length; i++)
        {
            if (!Animators[i].runtimeAnimatorController || clear)
                UnityEngine.Object.DestroyImmediate(Animators[i]);
        }
    }

    private static void CheckMainCamera()
    {
        if (!Camera.main)
        {
            Debug.Log("没有设置主摄像机");
            return;
        }

        RemoveScripts(Camera.main.gameObject);
    }

    private static void RemoveScripts(GameObject go)
    {
        MonoBehaviour[] components = go.GetComponents<MonoBehaviour>();
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] == null)
            {
                Debug.LogError(string.Format("the 'Main Camera' have missing component,please remove it."));
                continue;
            }

            if (components[i].ToString() == "CameraControll")
                UnityEngine.Object.DestroyImmediate(components[i], true);
            else if (components[i].ToString() == "T4MObjSC")
                UnityEngine.Object.DestroyImmediate(components[i], true);
        }
    }

    private static void DoCheckNode(Transform node) 
    {
        switch (node.name)
        {
            case "effect":
                {
                    CheckEffectNode(node);
                }
                break;
            case "mainbuilding":
                {
                    RemoveAni(node.gameObject, true);
                    RemoveCollider(node.gameObject);
                    RemoveAudio(node.gameObject);
                    CheckTagLayer(node, "Untagged", "MainBuilding");
                }
                break;
            case "prop":
                {
                    RemoveAni(node.gameObject, true);
                    RemoveCollider(node.gameObject);
                    RemoveAudio(node.gameObject);
                    CheckTagLayer(node, "Untagged", "Prop");
                }
                break;
            case "terrain":
                {
                    RemoveAni(node.gameObject, true);
                    RemoveCollider(node.gameObject);
                    RemoveAudio(node.gameObject);
                    CheckTagLayer(node, "Untagged", "Terrain");
                }
                break;
            case "Reflection":
                {
                    RemoveAni(node.gameObject, true);
                    RemoveCollider(node.gameObject);
                    RemoveAudio(node.gameObject);
                    CheckTagLayer(node, "Untagged", "Reflection");
                }
                break;
            case "collider":
                {
                    Renderer[] renders = node.gameObject.GetComponentsInChildren<Renderer>(true);
                    for (int i = 0; i < renders.Length; i++)
                        UnityEngine.Object.DestroyImmediate(renders[i], true);

                    MeshFilter[] filters = node.gameObject.GetComponentsInChildren<MeshFilter>(true);
                    for (int i = 0; i < filters.Length; i++)
                        UnityEngine.Object.DestroyImmediate(filters[i], true);

                    CheckTagLayer(node, "Untagged", "Terrain");
                }
                break;
            case "level":
                {
                   node.tag = "Untagged";
                   for (int i = 0; i < node.childCount; i++)
                   {
                       Transform child = node.GetChild(i);
                       int value;
                       if(!int.TryParse(child.name,out value))
                           Debug.LogError(string.Format("关卡命名错误{0} ", child.name), child);

                       CheckTagLayer(child, "Untagged", "Prop");

                       child.gameObject.layer = LayerMask.NameToLayer("Prop");
                       child.tag = "AI";
                   }
                }
                break;
        }
    }

    private static void CheckEffectNode(Transform node)
    {
        string groups = "prop/mainbuilding/terrain";
        List<string> rootGroups = new List<string>(groups.Split('/'));

        for (int i = 0; i < node.childCount; i++)
        {
            Transform child = node.GetChild(i);
            if (!rootGroups.Contains(child.name))
                Debug.LogError(string.Format("特效包含错误的打组{0} ", child.name), child);
            else
            {
                switch (node.name)
                {
                    case "prop":
                        {
                            CheckTagLayer(node, "Untagged", "Effect");
                        }
                        break;
                    case "mainbuilding":
                        {
                            CheckTagLayer(node, "Untagged", "MainBuilding");
                        }
                        break;
                    case "terrain":
                        {
                            CheckTagLayer(node, "Untagged", "Terrain");
                        }
                        break;
                }
            }
        }
    }

    private static void RemoveCollider(GameObject go)
    {
        Collider[] meshcolliders = go.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < meshcolliders.Length; i++)
            UnityEngine.Object.DestroyImmediate(meshcolliders[i], true);
    }

    private static void RemoveAudio(GameObject go)
    {
        AudioSource[] Audios = go.GetComponentsInChildren<AudioSource>(true);
        for (int i = 0; i < Audios.Length; i++)
            UnityEngine.Object.DestroyImmediate(Audios[i], true);
    }

    private static void CheckTagLayer(Transform node, string tag, string layer)
    {
        RemoveScripts(node.gameObject);

        node.gameObject.layer = LayerMask.NameToLayer(layer);
        node.tag = tag;

        Transform[] childs = node.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < childs.Length; i++)
        {
            childs[i].tag = tag;
            childs[i].gameObject.layer = node.gameObject.layer;
        }
    }

    [MenuItem("Assets/CompressAssets")]
    public static void CompressAssets()
    {
        EditorUtility.DisplayProgressBar("CompressAsset", "Searching Assets", 0);
        Object[] Assets = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        int i = 0;
        foreach (Object asset in Assets)
        {
            EditorUtility.DisplayProgressBar("CompressAsset", string.Format("{0}/{1}", i, Assets.Length), (float)i / (float)Assets.Length);
            i++;
            if (asset is GameObject)
            {
                Renderer[] renders = (asset as GameObject).GetComponentsInChildren<Renderer>(true);
                foreach (Renderer r in renders)
                {
                    r.shadowCastingMode = ShadowCastingMode.Off;
                    r.receiveShadows = false;
                    r.lightProbeUsage = LightProbeUsage.Off;
                }
            }

            string path = AssetDatabase.GetAssetPath(asset);
            AssetImporter importer = AssetImporter.GetAtPath(path);
            if (importer is ModelImporter)
                CompressMesh(importer as ModelImporter, path);
            else if (importer is TextureImporter && asset is Texture2D)
                CompressTex(importer as TextureImporter, path, asset as Texture2D);
            AssetDatabase.Refresh();
        }
        EditorUtility.ClearProgressBar();
    }

    static void CompressMesh(ModelImporter modelImporter, string path)
    {
        if (!modelImporter)
            return;
        modelImporter.optimizeMesh = true;
        modelImporter.isReadable = true;//这里涉及场景优化和模型打包不一样
        if (modelImporter.meshCompression == ModelImporterMeshCompression.Off)
            modelImporter.meshCompression = ModelImporterMeshCompression.Medium;
        modelImporter.generateSecondaryUV = true;
        AssetDatabase.ImportAsset(path);
    }

    public static void CompressTex(TextureImporter textureImporter, string path, Texture2D texture)
    {
        if (!textureImporter || textureImporter.textureType == TextureImporterType.Lightmap)
            return;

        TextureImporterFormat PCFormat = TextureImporterFormat.DXT1;
        TextureImporterFormat AndroidFormat = TextureImporterFormat.ETC_RGB4;
        TextureImporterFormat IOSFormat = TextureImporterFormat.PVRTC_RGB4;
        if (textureImporter.DoesSourceTextureHaveAlpha())
        { 
            PCFormat = TextureImporterFormat.DXT5;
            AndroidFormat = TextureImporterFormat.ETC2_RGBA8;
            IOSFormat = TextureImporterFormat.PVRTC_RGBA4;
        }
        textureImporter.mipmapEnabled = false;
        textureImporter.isReadable = false;
        textureImporter.npotScale = TextureImporterNPOTScale.ToNearest;
        textureImporter.filterMode = FilterMode.Bilinear;
        //textureImporter.wrapMode = TextureWrapMode.Repeat;
        textureImporter.anisoLevel = 1;
        textureImporter.textureCompression = TextureImporterCompression.Compressed;
        int size = Mathf.Max(texture.height, texture.width);

        SetPlatformTextureSettings("Standalone", size, textureImporter,PCFormat);
        SetPlatformTextureSettings("Android", size, textureImporter,AndroidFormat);
        SetPlatformTextureSettings("iPhone", size, textureImporter,IOSFormat);
        AssetDatabase.ImportAsset(path);
    }

    public static void SetPlatformTextureSettings(string platform, int size, TextureImporter textureImporter,TextureImporterFormat format)
    {
        TextureImporterPlatformSettings PlatformSet = textureImporter.GetPlatformTextureSettings(platform);
        PlatformSet.compressionQuality = 0;
        PlatformSet.crunchedCompression = false;
        PlatformSet.overridden = true;
        PlatformSet.maxTextureSize = size;
        PlatformSet.textureCompression = TextureImporterCompression.Compressed;
        PlatformSet.format = format;
        textureImporter.SetPlatformTextureSettings(PlatformSet);
    }

    [MenuItem("Assets/魔域/BuildSences")]
    static void BuildSences()
    {
        foreach (Object asset in Selection.objects)
            BuildAsset(asset, "scene", "Assets/StreamingAssets/res/scenes");
    }

    [MenuItem("Assets/魔域/BuildShaderList")]
    static void BuildShaderList()
    {
        BuildAsset(Selection.activeObject, "sl", "Assets/StreamingAssets/res/");
    }

    [MenuItem("Assets/魔域/BuildTex")]
    private static void BuildUITex()
    {
        foreach (Object asset in Selection.objects)
        {
            AssetDatabase.Refresh();
            BuildAsset(asset, "tex", "Assets/StreamingAssets/res/ui");
        }
    }

    public static void BuildAsset(Object asset, string Ext, string outputPath)
    {
        string assetfile = AssetDatabase.GetAssetPath(asset);
        string buildPath = Path.ChangeExtension(assetfile, Ext);
        AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
        buildMap[0].assetBundleName = Path.GetFileName(buildPath);
        buildMap[0].assetNames = new string[] { assetfile };
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);
        BuildPipeline.BuildAssetBundles(outputPath, buildMap, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
        AssetDatabase.Refresh();
    }
}
