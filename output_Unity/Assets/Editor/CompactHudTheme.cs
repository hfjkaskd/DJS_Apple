using UnityEngine;

/// <summary>Editor authoring settings; the resulting static hierarchy is saved in Prefabs.</summary>
[CreateAssetMenu(menuName = "Orchard/Compact Gameplay HUD")]
public sealed class CompactHudTheme : ScriptableObject
{
    public bool Enabled = true;
    public string TopPrefab = "Assets/res/local/withdrawal/ApkEarningTop.prefab";
    public string CorePrefab = "Assets/res/local/coreplay/CorePlayUI.prefab";
    public string Font = "Assets/ApkEarningReference/Visuals/MonoBehaviour/zh_Custom SDF.asset";
    public string PlainMaterial = "Assets/OrchardEarningArt/CashWallet/CashWalletPlain.mat";
    public string OutlinedMaterial = "Assets/OrchardEarningArt/CashWallet/CashWalletOutlined.mat";
    public string Controls = "Assets/OrchardEarningArt/CompactHud/HudControls.png";
    public string Card = "Assets/OrchardEarningArt/CashWallet/CashWalletPanel.png";
    public string Action = "Assets/OrchardEarningArt/CashWallet/CashWalletButton.png";
    public string Track = "Assets/OrchardEarningArt/CashWallet/CashWalletProgressTrack.png";
    public string Fill = "Assets/OrchardEarningArt/CashWallet/CashWalletProgressFill.png";
    public float ReferenceWidth = 1080;
    public float RowY = -64, ViewportTop = 132;
    public Vector2 CashPosition = new Vector2(-174, -64), DiamondPosition = new Vector2(202, -64);
    public Vector2 CashSize = new Vector2(374, 90), DiamondSize = new Vector2(360, 90);
    public Vector2 CurrencyIconSize = new Vector2(74, 74), CurrencyValueSize = new Vector2(188, 72);
    public Vector2 ChevronSize = new Vector2(54, 54);
    public Vector2 LevelPosition = new Vector2(-438, 0), LevelSize = new Vector2(108, 108);
    public Vector2 LevelNumberPosition = new Vector2(0, 9), LevelNumberSize = new Vector2(84, 52);
    public Vector2 LevelCaptionPosition = new Vector2(0, -27), LevelCaptionSize = new Vector2(80, 26);
    public float LevelNumberFontSize = 50, LevelCaptionFontSize = 20;
    public Vector2 SettingsPosition = new Vector2(450, 0), SettingsSize = new Vector2(96, 96);
    public bool ShowTaskStrip = false;
    public Vector2 TaskPosition = new Vector2(0, -214), TaskSize = new Vector2(1024, 92);
    public Vector2 TaskLabelPosition = new Vector2(-338, 0), TaskLabelSize = new Vector2(306, 60);
    public Vector2 ProgressPosition = new Vector2(60, 0), ProgressSize = new Vector2(460, 42);
    public Vector2 ProgressFillInset = new Vector2(6, 6);
    public Vector2 ViewPosition = new Vector2(423, 0), ViewSize = new Vector2(156, 70);
    public float BalanceFontSize = 54, TaskFontSize = 38, RatioFontSize = 38;
    public float CardPixelsPerUnit = 3.5f, TaskPixelsPerUnit = 4, ButtonPixelsPerUnit = 6;
    public Color Ink = new Color32(113, 57, 27, 255);
}
