using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Editable authoring data for the approved compact cash wallet.</summary>
[CreateAssetMenu(menuName = "Apk Earning/Cash Wallet Art")]
public sealed class CashWalletArtTheme : ScriptableObject
{
    [Serializable] public sealed class SpriteImport
    {
        public string Path;
        public Rect SpriteRect;
        public Vector4 Border;
        public int MaxSize = 1024;
    }
    [Serializable] public sealed class Element
    {
        [Tooltip("t: text alias, i: image alias, b: button alias, p: hierarchy path")]
        public string Target;
        public bool Active = true;
        public bool SetRect = true;
        [Tooltip("Position in the centered 1080-wide artwork coordinate system")]
        public Vector2 Position;
        public Vector2 Size;
        public bool StyleImage;
        public bool ImageEnabled = true;
        public string SpritePath;
        public Image.Type ImageType;
        public float PixelsPerUnit = 1;
        public bool PreserveAspect;
        public Color ImageColor = Color.white;
        public bool StyleText;
        public string Text;
        public float FontSize = 40;
        public FontStyles FontStyle = FontStyles.Normal;
        public Color TextColor = new Color32(113, 57, 27, 255);
        public bool Outlined;
        public bool Wrap = true;
    }
    public bool Enabled = true;
    public string PrefabPath = "Assets/ApkEarningReference/Resources/ApkEarningReference/Panels/TXPanel.prefab";
    public string FontPath = "Assets/res/local/textmeshpro/resources/fonts & materials/802-CAI978 SDF.asset";
    public Color ScrimColor = new Color(.12f, .08f, .25f, .38f);
    public Vector2 HistoryPosition = new Vector2(0, -505);
    public Vector2 HistorySize = new Vector2(760, 80);
    public Color DividerColor = new Color32(230, 190, 136, 180);
    [Range(0, 1)] public float OutlineWidth = .22f;
    public Color OutlineColor = new Color32(111, 51, 15, 255);
    [Range(0, 1)] public float PreviewProgress = .6f;
    public List<SpriteImport> Sprites = new List<SpriteImport>();
    public List<Element> Elements = new List<Element>();
}
