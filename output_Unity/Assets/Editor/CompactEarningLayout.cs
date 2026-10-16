using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Initial configuration helpers; edits remain in the saved ScriptableObject.</summary>
public static class CompactEarningLayout
{
    public const string ArtFolder = "Assets/OrchardEarningArt/CashWallet/";
    public static CompactEarningTheme.Page Page(CompactEarningTheme theme, string name, float testY = 470)
    {
        var page = new CompactEarningTheme.Page { Name = name, TestPosition = new Vector2(0, testY) };
        theme.Pages.Add(page); return page;
    }
    public static void Rect(CompactEarningTheme.Page page, string target, float x, float y, float w, float h)
    { page.Elements.Add(new CashWalletArtTheme.Element { Target = target, Position = new Vector2(x,y), Size = new Vector2(w,h) }); }
    public static void Art(CompactEarningTheme.Page page, string target, string fileName, float ppu = 1, bool simple = false)
    { page.Elements.Add(new CashWalletArtTheme.Element { Target = target, SetRect = false, StyleImage = true, SpritePath = ArtFolder + fileName + ".png", PixelsPerUnit = ppu, ImageType = simple ? Image.Type.Simple : Image.Type.Sliced }); }
    public static void Text(CompactEarningTheme.Page page, string target, float size, string value = null, bool outlined = false, bool bold = false)
    { page.Elements.Add(new CashWalletArtTheme.Element { Target = target, SetRect = false, StyleText = true, Text = value, FontSize = size, Outlined = outlined, FontStyle = bold ? FontStyles.Bold : FontStyles.Normal, TextColor = outlined ? new Color32(255,255,244,255) : new Color32(113,57,27,255) }); }
    public static void Hide(CompactEarningTheme.Page page, string target)
    { page.Elements.Add(new CashWalletArtTheme.Element { Target = target, SetRect = false, Active = false }); }
    public static void DisableLayout(CompactEarningTheme.Page page, string target) { page.DisableLayouts.Add(target); }
}
