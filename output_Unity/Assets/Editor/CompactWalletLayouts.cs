using UnityEngine;
using UnityEngine.UI;
using static CompactEarningLayout;

/// <summary>Initial editable layouts for both wallets and their shared supporting pages.</summary>
public static class CompactWalletLayouts
{
    public static void Seed(CompactEarningTheme theme)
    {
        Diamonds(theme);
        Conditions(theme);
        History(theme);
        HistoryRow(theme);
        Recovery(theme);
        Toast(theme);
    }

    private static CompactEarningTheme.Page Shell(CompactEarningTheme theme, string name,
        string background, string titleImage, string titleText, string close, float titleSize = 56)
    {
        var p = Page(theme, name);
        Rect(p, background, 0, 0, 936, 1200);
        Art(p, background, "CashWalletPanel", simple: true);
        Rect(p, titleImage, 20, 578, 620, 148);
        Art(p, titleImage, "CashWalletHeader", simple: true);
        Rect(p, titleText, 20, 578, 560, 112);
        Text(p, titleText, titleSize, outlined: true, bold: true);
        Rect(p, close, 430, 550, 112, 112);
        Art(p, close, "CashWalletClose", simple: true);
        return p;
    }

    private static void Primary(CompactEarningTheme.Page p, string button, string label,
        float y, float width = 676, float size = 62)
    {
        Rect(p, button, 0, y, width, 150);
        Art(p, button, "CashWalletButton", simple: true);
        Rect(p, label, 0, y, width - 68, 108);
        Text(p, label, size, outlined: true, bold: true);
    }

    private static void Diamonds(CompactEarningTheme theme)
    {
        var p = Shell(theme, "TXCoinPanel", "i:bg", "i:Title", "t:labels[0].Text", "b:Close", 62);
        Hide(p, "i:main/Image (1)");
        Hide(p, "i:main/Image (2)");
        Hide(p, "i:main/Image (3)");
        Hide(p, "p:main/value/CoinIcon");
        Rect(p, "i:CoinIcon", -206, 378, 124, 114);
        Rect(p, "t:CoinValue", 70, 378, 450, 104);
        Text(p, "t:CoinValue", 82, bold: true);
        Rect(p, "i:main/Image", -190, 284, 76, 72);
        Art(p, "i:main/Image", "CashWalletMoney", simple: true);
        Rect(p, "t:GoldValue", 65, 284, 440, 76);
        Text(p, "t:GoldValue", 54, bold: true);
        Rect(p, "t:TipValue", 0, 221, 680, 46);
        Text(p, "t:TipValue", 28);
        Rect(p, "t:labels[1].Text", 0, 167, 760, 52);
        Text(p, "t:labels[1].Text", 34);
        DisableLayout(p, "o:Items");
        Rect(p, "o:Items", 0, 12, 780, 280);
        for (int i = 0; i < 4; i++)
        {
            float x = i % 2 == 0 ? -193 : 193;
            float y = i < 2 ? 80 : -68;
            Rect(p, "b:options[" + i + "]", x, y, 350, 128);
            Art(p, "b:options[" + i + "]", "CashWalletInset", 2);
            Rect(p, "t:amounts[" + i + "]", x, y, 304, 88);
            Text(p, "t:amounts[" + i + "]", 54, bold: true);
            Rect(p, "o:selectionFrames[" + i + "]", x, y, 350, 128);
            DisableImage(p, "o:selectionFrames[" + i + "]");
        }
        Rect(p, "o:TXProgress", 0, -266, 800, 242);
        Art(p, "o:TXProgress", "CashWalletInset", 2);
        Rect(p, "t:TXTip", 0, -188, 728, 54);
        Text(p, "t:TXTip", 32, bold: true);
        Rect(p, "t:IitemTip", 0, -266, 716, 98);
        Text(p, "t:IitemTip", 31);
        Rect(p, "i:main/TXProgress/Bar", 0, -351, 724, 66);
        Art(p, "i:main/TXProgress/Bar", "CashWalletProgressTrack", simple: true);
        Rect(p, "i:TipBar", 0, -351, 706, 54);
        p.Elements.Add(new CashWalletArtTheme.Element
        {
            Target = "i:TipBar", SetRect = false, StyleImage = true,
            SpritePath = ArtFolder + "CashWalletProgressFill.png", ImageType = Image.Type.Filled
        });
        Rect(p, "t:TipBarValue", 0, -351, 650, 58);
        Text(p, "t:TipBarValue", 30, bold: true);
        Primary(p, "b:TXBtn", "t:labels[2].Text", -474);
    }

    private static void HideConditionLegacy(CompactEarningTheme.Page p)
    {
        Hide(p, "o:1");
        Hide(p, "o:2");
        Hide(p, "l:label_37");
        Hide(p, "i:main/Image (2)");
        Hide(p, "o:ItemIcon");
        Hide(p, "t:ItemCount");
        Hide(p, "o:Items");
        Hide(p, "o:Progress");
        Hide(p, "t:Tip");
        Hide(p, "o:PlayerInfoPos");
    }

    private static void Conditions(CompactEarningTheme theme)
    {
        var p = Shell(theme, "LocalWithdrawalConditions", "i:main/Image", "i:Title1", "t:title", "b:close", 46);
        HideConditionLegacy(p);
        Rect(p, "o:Requirements", 0, 0, 820, 860);
        Rect(p, "t:subtitle", 0, 407, 740, 60);
        Text(p, "t:subtitle", 42, bold: true);
        Rect(p, "t:notice", 0, 335, 744, 76);
        Text(p, "t:notice", 31);
        Rect(p, "i:main/Image (1)", 0, -15, 816, 654);
        Art(p, "i:main/Image (1)", "CashWalletInset", 2);
        for (int i = 0; i < 7; i++)
        {
            string row = "rows[" + i + "]";
            float y = 238 - i * 85;
            Rect(p, "o:" + row + ".Root", 0, y, 754, 80);
            Rect(p, "t:" + row + ".Name", -108, y + 17, 496, 37);
            Text(p, "t:" + row + ".Name", 31, bold: true);
            Rect(p, "t:" + row + ".Value", -108, y - 20, 496, 37);
            Text(p, "t:" + row + ".Value", 27);
            Rect(p, "t:" + row + ".Status", 267, y, 190, 66);
            Text(p, "t:" + row + ".Status", 28);
        }
        Primary(p, "b:action", "t:actionCaption", -457, 738, 48);
    }

    private static void History(CompactEarningTheme theme)
    {
        var p = Shell(theme, "LocalCashHistory", "i:Card", "i:Heading", "t:titleLabel", "b:closeButton", 50);
        Rect(p, "i:Balance", 0, 315, 800, 225);
        Art(p, "i:Balance", "CashWalletInset", 2);
        Rect(p, "t:Caption", 0, 385, 720, 55);
        Text(p, "t:Caption", 36);
        Rect(p, "i:Bank", -220, 294, 144, 134);
        Art(p, "i:Bank", "CashWalletMoney", simple: true);
        Rect(p, "t:balanceLabel", 98, 294, 442, 126);
        Text(p, "t:balanceLabel", 100, bold: true);
        Rect(p, "o:rowParent", 0, -166, 800, 620);
        p.RowLayouts.Add(new CompactEarningTheme.LayoutRule
        {
            Target = "o:rowParent", Horizontal = false, ControlSize = false, Spacing = 18,
            Alignment = TextAnchor.UpperCenter
        });
    }

    private static void HistoryRow(CompactEarningTheme theme)
    {
        var p = Page(theme, "LocalCashHistoryRow");
        p.AddTestCaption = false;
        Rect(p, "o:$root", 0, 0, 780, 188);
        Rect(p, "o:readyVisual", 0, 0, 780, 188);
        Art(p, "o:readyVisual", "CashWalletInset", 2);
        Rect(p, "o:pendingVisual", 0, 0, 780, 188);
        Art(p, "o:pendingVisual", "CashWalletInset", 2);
        p.PreserveActiveTargets.Add("o:readyVisual");
        p.PreserveActiveTargets.Add("o:pendingVisual");
        Rect(p, "t:titleLabel", -165, 45, 376, 58);
        Text(p, "t:titleLabel", 34, bold: true);
        Rect(p, "t:timeLabel", -165, -40, 376, 72);
        Text(p, "t:timeLabel", 27);
        Rect(p, "t:amountLabel", 214, -39, 266, 86);
        Text(p, "t:amountLabel", 53, bold: true);
        Rect(p, "b:actionButton", 235, 48, 244, 72);
        Text(p, "t:actionLabel", 36, "Withdraw >", bold: true);
        Rect(p, "t:statusLabel", 224, 48, 270, 72);
        Text(p, "t:statusLabel", 29);
        p.PreserveActiveTargets.Add("b:actionButton");
        p.PreserveActiveTargets.Add("t:actionLabel");
        p.PreserveActiveTargets.Add("t:statusLabel");
    }

    private static void Recovery(CompactEarningTheme theme)
    {
        var p = Shell(theme, "LocalEarningRecovery", "i:main/Image", "i:Title1", "t:title", "b:Close", 48);
        HideConditionLegacy(p);
        Rect(p, "o:Requirements", 0, 100, 820, 600);
        for (int i = 0; i < 7; i++) Hide(p, "o:rows[" + i + "].Root");
        Hide(p, "t:notice");
        Rect(p, "i:main/Image (1)", 0, 140, 800, 470);
        Art(p, "i:main/Image (1)", "CashWalletInset", 2);
        Rect(p, "t:body", 0, 140, 704, 380);
        Text(p, "t:body", 38);
        Primary(p, "b:retry", "t:actionCaption", -245);
        Rect(p, "b:close", 0, -432, 600, 112);
        Art(p, "b:close", "CashWalletInset", 2);
        Rect(p, "p:main/ReturnToGame/Text (TMP)", 0, -432, 540, 82);
        Text(p, "p:main/ReturnToGame/Text (TMP)", 42, bold: true);
    }

    private static void Toast(CompactEarningTheme theme)
    {
        var p = Page(theme, "Tip");
        p.AddTestCaption = false;
        Rect(p, "o:$root", 0, 0, 840, 220);
        Art(p, "o:$root", "CashWalletInset", 2);
        Rect(p, "t:content", 0, 0, 748, 162);
        Text(p, "t:content", 36);
    }

    private static void DisableImage(CompactEarningTheme.Page p, string target)
    {
        p.Elements.Add(new CashWalletArtTheme.Element
        {
            Target = target, SetRect = false, StyleImage = true, ImageEnabled = false
        });
    }
}
