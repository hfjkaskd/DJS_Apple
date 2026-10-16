using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Authoring-only values. The resulting prefabs are identical on all player platforms.</summary>
[CreateAssetMenu(menuName = "Apk Earning/Compact Flow Theme")]
public sealed class CompactEarningTheme : ScriptableObject
{
    [Serializable] public sealed class LayoutRule
    {
        public string Target;
        public float Spacing = 20;
        public TextAnchor Alignment = TextAnchor.MiddleCenter;
        public bool Horizontal = true, ControlSize;
        public Vector2 ItemSize;
    }
    [Serializable] public sealed class Page
    {
        public string Name;
        public bool AddTestCaption = true;
        public Vector2 TestPosition = new Vector2(0, 470);
        public float TestWidth = 760;
        public List<CashWalletArtTheme.Element> Elements = new List<CashWalletArtTheme.Element>();
        public List<string> DisableLayouts = new List<string>();
        public List<string> PreserveActiveTargets = new List<string>();
        public List<string> SendToBackTargets = new List<string>();
        public List<LayoutRule> RowLayouts = new List<LayoutRule>();
    }
    public bool Enabled = true;
    public string PanelFolder = "Assets/ApkEarningReference/Resources/ApkEarningReference/Panels/";
    public string FontPath = "Assets/ApkEarningReference/Visuals/MonoBehaviour/zh_Custom SDF.asset";
    public string PlainMaterialPath = "Assets/OrchardEarningArt/CashWallet/CashWalletPlain.mat";
    public string OutlinedMaterialPath = "Assets/OrchardEarningArt/CashWallet/CashWalletOutlined.mat";
    public Color Ink = new Color32(113, 57, 27, 255);
    public Color Scrim = new Color(.12f, .08f, .25f, .38f);
    public Color SelectedOptionColor = new Color32(210, 246, 160, 255);
    public Color NormalOptionColor = Color.white;
    public float BodyFontSize = 38, TestFontSize = 24;
    public float PanelPPU = 1.5f, InsetPPU = 2, ButtonPPU = 3, HeaderPPU = 2;
    public List<Page> Pages = new List<Page>();
}
