using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class ConfigHelper {
    private bool debug = false;       // 是否调试模式

    public bool isDebugOpen {
        get {
            return debug;
        }
    }

    public static ConfigHelper LoadConfig(TextAsset asset) {
        ConfigHelper config = new ConfigHelper();
        try
        {
            XmlTextReader xr = new XmlTextReader(new StringReader(asset.text));
            while (xr.Read()) {
                if (xr.NodeType == XmlNodeType.Element) {
                    if (xr.Name == "game") {
                        config.debug = bool.Parse(xr.GetAttribute("debug"));
                    }
                }
            }
            xr.Close();
        }
        catch (Exception ex)
        {
            LOG.LogError("[ConfigHelper] load fail." + ex.Message);
        }

        return config;
    }

    public static void SetFrameRate() {
        Application.targetFrameRate = 20;
        //降低FPS：省电，减少手机发热
        if (Application.isMobilePlatform) {
            Application.targetFrameRate = 30;
            QualitySettings.softParticles = false;
            QualitySettings.realtimeReflectionProbes = false;
            QualitySettings.billboardsFaceCameraPosition = false;
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.lodBias = 0.5f;
            QualitySettings.shadowCascades = 0;
            QualitySettings.antiAliasing = 0;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            QualitySettings.pixelLightCount = 0;
            QualitySettings.shadowDistance = 40;
            QualitySettings.shadowProjection = ShadowProjection.StableFit;
            QualitySettings.particleRaycastBudget = 0;
        }
        QualitySettings.blendWeights = BlendWeights.FourBones;
    }
}
