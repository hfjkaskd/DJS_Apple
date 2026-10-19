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
    public float RowY = -98, ViewportTop = 320;
    public Vector2 CashPosition = new Vector2(-174, -98), DiamondPosition = new Vector2(202, -98);
    public Vector2 CashSize = new Vector2(374, 108), DiamondSize = new Vector2(360, 108);
    public Vector2 LevelPosition = new Vector2(-438, 0), LevelSize = new Vector2(146, 146);
    public Vector2 SettingsPosition = new Vector2(450, 0), SettingsSize = new Vector2(120, 120);
    public Vector2 TaskPosition = new Vector2(0, -214), TaskSize = new Vector2(1024, 92);
    public Vector2 DisclosurePosition = new Vector2(0, -277), DisclosureSize = new Vector2(890, 40);
    public Vector2 TaskLabelPosition = new Vector2(-338, 0), TaskLabelSize = new Vector2(306, 60);
    public Vector2 ProgressPosition = new Vector2(60, 0), ProgressSize = new Vector2(460, 42);
    public Vector2 ViewPosition = new Vector2(423, 0), ViewSize = new Vector2(156, 70);
    public float BalanceFontSize = 62, TaskFontSize = 38, RatioFontSize = 38, DisclosureFontSize = 23;
    public float CardPixelsPerUnit = 3.5f, TaskPixelsPerUnit = 4, ButtonPixelsPerUnit = 6;
    public Color Ink = new Color32(113, 57, 27, 255);
}
