using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Editor authoring configuration. Only the saved prefab artwork is used by players.</summary>
[CreateAssetMenu(menuName = "Apk Earning/Orchard Art Theme")]
public sealed class OrchardEarningTheme : ScriptableObject
{
    [Serializable]
    public sealed class SpriteRule
    {
        public string SourcePath;
        public string ReplacementPath;
        public Image.Type ImageType = Image.Type.Sliced;
        public bool KeepImageType;
        public bool PreserveAspect;
        [Min(.01f)] public float PixelsPerUnitMultiplier = 1;
        public Color Tint = Color.white;
        public bool AddAppleAccent;
        public string OverlayText;
    }

    [Serializable]
    public sealed class AuthoredImage
    {
        public string PrefabPath;
        public string HierarchyIndices;
        public string SourcePath;
        public string LastReplacementPath;
    }

    [Header("Authoring scope")]
    public bool Enabled = true;
    public string PanelFolder = "Assets/ApkEarningReference/Resources/ApkEarningReference/Panels";
    public string[] AdditionalPrefabs = {
        "Assets/res/local/withdrawal/ApkEarningTop.prefab",
        "Assets/res/local/withdrawal/ApkEarningHost.prefab"
    };

    [Header("Exact image replacements; currency and payment icons are intentionally absent")]
    public List<SpriteRule> SpriteRules = new List<SpriteRule>();

    [Header("Typography")]
    public string DisplayFontPath = "Assets/res/local/textmeshpro/resources/fonts & materials/802-CAI978 SDF.asset";
    public string DisplayMaterialPath = "Assets/OrchardEarningArt/OrchardDisplay.mat";
    [Min(1)] public float DisplayFontMinimumSize = 55;
    public Color BodyColor = new Color32(113, 57, 27, 255);
    public Color HeadingColor = new Color32(122, 61, 26, 255);
    public Color ButtonTextColor = new Color32(255, 255, 244, 255);
    public Color AccentTextColor = new Color32(53, 110, 29, 255);
    public Color DisplayOutlineColor = new Color32(111, 51, 15, 255);
    [Range(0, .3f)] public float DisplayOutlineWidth = .1f;

    [Header("Orchard accents")]
    public string AppleCrestPath = "Assets/OrchardEarningArt/AppleCrest.png";
    public string AppleAccentPath = "Assets/res_server/server_images/fruit/Fruit_16.png";
    public bool AddTitleApples = true;
    public Vector2 AppleAccentAnchor = new Vector2(0, .8f);
    public Vector2 AppleAccentPosition = new Vector2(8, 8);
    public Vector2 AppleAccentSize = new Vector2(86, 94);

    [Header("Receipt layout")]
    public bool RestyleSuccessLayout = true;
    public Vector2 SuccessPanelSize = new Vector2(980, 1400);
    public Vector2 SuccessPanelPosition = new Vector2(0, 40);
    public Vector2 SuccessTitleSize = new Vector2(860, 150);
    public Vector2 SuccessTitlePosition = new Vector2(0, 530);
    public Vector2 SuccessLabelSize = new Vector2(760, 100);
    public Vector2 SuccessCrestSize = new Vector2(420, 168);
    public Vector2 SuccessCrestPosition = new Vector2(0, 140);

    [Header("HUD test label")]
    public string TestBackdropPath = "Assets/res/local/common/sprite/pop/PopBg.asset";
    public Vector2 TestBackdropPadding = new Vector2(32, 16);
    public Color TestBackdropColor = new Color(1, 1, 1, .96f);
    [Min(.01f)] public float TestBackdropPixelsPerUnitMultiplier = 10;

    [Header("Reward icon caption")]
    public Vector2 RewardOverlayPosition = new Vector2(0, 10);
    public Vector2 RewardOverlaySize = new Vector2(180, 58);
    public float RewardOverlayFontSize = 48;

    [Header("Gameplay milestone sprites")]
    public string NormalMilestonePath = "Assets/res/local/common/sprite/pop/PopCloseBg.asset";
    public string PassedMilestonePath = "Assets/res/local/common/sprite/btn/MidBtn_Green.asset";
    public string ActiveMilestonePath = "Assets/res/local/common/sprite/btn/Btn_Orange.asset";

    [Header("Runtime diamond option resources")]
    public string NormalOptionTexture = "Assets/res/local/common/sprite/pop/PopBg.png";
    public string SelectedOptionTexture = "Assets/res/local/common/sprite/btn/Btn_Normal.png";
    public Vector4 NormalOptionBorder = new Vector4(230, 230, 230, 230);
    public Vector4 SelectedOptionBorder = new Vector4(127, 103, 128, 103);
    [Min(.01f)] public float OptionPixelsPerUnitMultiplier = 4.5f;

    // Remember exact original images so a second apply can refresh parameters without
    // confusing a shared native card with a title or changing a business icon.
    [HideInInspector] public List<AuthoredImage> AuthoredImages = new List<AuthoredImage>();
    [HideInInspector] public int ThemeVersion;
}
