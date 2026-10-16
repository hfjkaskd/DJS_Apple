using UnityEngine;
using static CompactEarningLayout;

/// <summary>Initial, editable layout recipes for account and request pages.</summary>
public static class CompactAccountLayouts
{
    public static void Seed(CompactEarningTheme theme)
    {
        Account(theme);
        Confirmation(theme);
        OtherAccount(theme);
        Verification(theme);
        Progress(theme);
    }

    private static CompactEarningTheme.Page Shell(CompactEarningTheme theme, string name,
        string background, string title, string titleText, float titleSize = 56)
    {
        var p = Page(theme, name);
        Rect(p, background, 0, 0, 936, 1200);
        Art(p, background, "CashWalletPanel", simple: true);
        Rect(p, title, 20, 578, 620, 148);
        Art(p, title, "CashWalletHeader", simple: true);
        Rect(p, titleText, 20, 578, 560, 112);
        Text(p, titleText, titleSize, outlined: true, bold: true);
        Rect(p, "b:CloseBtn", 430, 550, 112, 112);
        Art(p, "b:CloseBtn", "CashWalletClose", simple: true);
        return p;
    }

    private static void Primary(CompactEarningTheme.Page p, string button, string text,
        float y, float width = 676, float fontSize = 64)
    {
        Rect(p, button, 0, y, width, 160);
        Art(p, button, "CashWalletButton", simple: true);
        Rect(p, text, 0, y, width - 70, 116);
        Text(p, text, fontSize, outlined: true, bold: true);
    }

    private static void Input(CompactEarningTheme.Page p, string alias, string path,
        string placeholder, float x, float y, float width)
    {
        Rect(p, "u:" + alias, x, y, width, 128);
        Art(p, "u:" + alias, "CashWalletInset", 3);
        Rect(p, "p:" + path + "/Text Area", x, y, width - 64, 94);
        Rect(p, "p:" + path + "/Text Area/Text", x, y, width - 72, 90);
        Text(p, "p:" + path + "/Text Area/Text", 40);
        Rect(p, "p:" + path + "/Text Area/" + placeholder, x, y, width - 72, 90);
        Text(p, "p:" + path + "/Text Area/" + placeholder, 38);
    }

    private static void Account(CompactEarningTheme theme)
    {
        var p = Shell(theme, "TXUserInfoPanel", "i:main/Image", "i:main/Title", "t:labels[1].Text");
        Hide(p, "i:main/Image (1)");
        Hide(p, "t:Tip (3)");
        DisableLayout(p, "o:ToggleGroup");
        Rect(p, "o:ToggleGroup", 0, 300, 760, 150);
        for (int i = 0; i < 3; i++)
        {
            float x = (i - 1) * 255;
            string name = i == 0 ? "Toggle" : "Toggle" + i;
            string path = "main/ToggleGroup/" + name;
            Rect(p, "b:channels[" + i + "].Button", x, 300, 226, 154);
            // Channel logos are changed by SelectChannel. Preserve those sprites.
            Rect(p, "i:channels[" + i + "].Icon", x, 300, 220, 140);
            Rect(p, "p:" + path + "/Background/Image", x, 300, 216, 136);
            Hide(p, "p:" + path + "/Background/Image");
            Rect(p, "i:channels[" + i + "].Checkmark", x + 72, 242, 68, 52);
            if (i > 0) Hide(p, "i:channels[" + i + "].Checkmark");
        }
        Rect(p, "b:ChannelInfoBtn", 350, 410, 66, 66);
        Rect(p, "t:labels[0].Text", 0, 145, 740, 96);
        Text(p, "t:labels[0].Text", 34);
        Input(p, "InputName", "main/InputName", "label_110", 0, 0, 760);
        Input(p, "InputEmail", "main/InputEmail", "label_112", 0, -154, 760);
        Input(p, "InputAreaNumber", "main/InputAreaNumber", "Placeholder", -285, -154, 190);
        Input(p, "InputNumber", "main/InputNumber", "label_111", 110, -154, 540);
        p.PreserveActiveTargets.Add("u:InputEmail");
        p.PreserveActiveTargets.Add("u:InputAreaNumber");
        p.PreserveActiveTargets.Add("u:InputNumber");
        Primary(p, "b:GetBtn", "t:labels[2].Text", -390);
    }

    private static void Confirmation(CompactEarningTheme theme)
    {
        var p = Shell(theme, "TXUserInfoSurePanel", "i:bg", "i:Title", "t:labels[0].Text", 62);
        DisableLayout(p, "o:btnParent");
        Rect(p, "o:btnParent", 0, -320, 800, 400);
        Rect(p, "i:Image", 0, 320, 340, 194);
        Rect(p, "i:bg1", 0, 40, 800, 320);
        Art(p, "i:bg1", "CashWalletInset", 2);
        Rect(p, "t:Tip", 0, 40, 704, 260);
        Text(p, "t:Tip", 40);
        Primary(p, "b:GetBtn", "t:labels[1].Text", -256);
        Rect(p, "b:ReEnterBtn", 0, -440, 510, 106);
        Art(p, "b:ReEnterBtn", "CashWalletInset", 3);
        Rect(p, "t:labels[2].Text", 0, -440, 460, 78);
        Text(p, "t:labels[2].Text", 44, bold: true);
    }

    private static void OtherAccount(CompactEarningTheme theme)
    {
        var p = Shell(theme, "TXChannelPanel", "i:main/Image", "i:Title", "t:labels[1].Text");
        Rect(p, "t:labels[0].Text", 0, 240, 740, 230);
        Text(p, "t:labels[0].Text", 42);
        Input(p, "InputInfo", "main/InputInfo", "label_156", 0, -40, 760);
        Primary(p, "b:SureBtn", "t:labels[2].Text", -355);
    }

    private static void Verification(CompactEarningTheme theme)
    {
        var p = Shell(theme, "LocalWithdrawalVerification", "i:main/Image", "i:Title1", "t:labels[0].Text", 52);
        Rect(p, "i:TitleTip", 0, 375, 740, 72);
        DisableImage(p, "i:TitleTip");
        Rect(p, "t:Tip", 0, 375, 700, 70);
        Text(p, "t:Tip", 42);
        Rect(p, "i:main/Image (1)", 0, 284, 800, 220);
        Art(p, "i:main/Image (1)", "CashWalletInset", 2);
        Rect(p, "i:main/Image (3)", -220, 280, 164, 152);
        Art(p, "i:main/Image (3)", "CashWalletMoney", simple: true);
        Rect(p, "t:GoldCount", 105, 280, 440, 135);
        Text(p, "t:GoldCount", 114, bold: true);
        Rect(p, "i:main/Image (2)", 0, -83, 800, 434);
        Art(p, "i:main/Image (2)", "CashWalletInset", 2);
        Rect(p, "t:labels[2].Text", 0, 80, 700, 64);
        Text(p, "t:labels[2].Text", 40, bold: true);
        Rect(p, "o:Step", 0, -100, 780, 320);
        for (int i = 1; i <= 3; i++)
        {
            float y = 80 - 100 * i;
            string path = "main/Step/" + i;
            Rect(p, "p:" + path, 0, y, 760, 80);
            Rect(p, "p:" + path + "/Content", 0, y, 760, 80);
            Rect(p, "p:" + path + "/Done", -314, y, 60, 60);
            Rect(p, "p:" + path + "/Loading", -314, y, 60, 60);
            Hide(p, "p:" + path + "/Loading");
            Rect(p, "t:labels[" + (i + 2) + "].Text", 50, y, 620, 82);
            Text(p, "t:labels[" + (i + 2) + "].Text", 38);
        }
        Hide(p, "p:main/Step/1/Content/Line");
        Hide(p, "p:main/Step/2/Content/Line");
        Hide(p, "p:main/Step/2/Image (1)");
        Primary(p, "b:SureBtn", "t:labels[1].Text", -440, 738, 52);
    }

    private static void Progress(CompactEarningTheme theme)
    {
        var p = Shell(theme, "TXProgress0Panel", "i:main/Image", "i:Title", "t:labels[0].Text", 52);
        Rect(p, "t:labels[2].Text", 0, 352, 744, 106);
        Text(p, "t:labels[2].Text", 40, bold: true);
        Rect(p, "o:KeFu", 0, 262, 744, 64);
        DisableImage(p, "i:KeFu");
        Rect(p, "t:KeFu", 0, 262, 700, 62);
        Text(p, "t:KeFu", 32);
        Rect(p, "o:Step", 0, -50, 800, 600);
        float[] rowY = { 154, 43, -116, -291 };
        for (int i = 1; i <= 4; i++)
        {
            string path = "main/Step/" + i;
            float y = rowY[i - 1];
            Rect(p, "p:" + path, 0, y, 780, 100);
            if (i < 4) Rect(p, "p:" + path + "/Content", 0, y, 780, 100);
            if (i < 3) Rect(p, "p:" + path + "/Done", -326, y, 60, 60);
            Rect(p, "p:" + path + "/Loading", -326, y, 60, 60);
            if (i == 1) Hide(p, "p:" + path + "/Loading");
            if (i == 2) Hide(p, "p:" + path + "/Done");
        }
        Rect(p, "t:labels[3].Text", 42, 154, 640, 92);
        Text(p, "t:labels[3].Text", 36);
        Rect(p, "t:labels[4].Text", 42, 43, 640, 92);
        Text(p, "t:labels[4].Text", 36);
        Hide(p, "p:main/Step/1/Content/Time");
        Hide(p, "p:main/Step/2/Content/Time");
        Hide(p, "p:main/Step/1/Content/Image (1)");
        Hide(p, "p:main/Step/2/Content/Image (1)");
        Hide(p, "p:main/Step/3/Content/Image (1)");
        Rect(p, "i:main/Step/3/Content/Image (2)", 42, -116, 676, 198);
        Art(p, "i:main/Step/3/Content/Image (2)", "CashWalletInset", 3);
        Rect(p, "t:Tip", 42, -76, 610, 90);
        Text(p, "t:Tip", 34);
        Rect(p, "t:Des", 42, -165, 610, 78);
        Text(p, "t:Des", 30);
        Rect(p, "t:labels[5].Text", 42, -291, 640, 112);
        Text(p, "t:labels[5].Text", 34);
        Primary(p, "b:SureBtn", "t:labels[1].Text", -466, 738, 50);
    }

    private static void DisableImage(CompactEarningTheme.Page p, string target)
    {
        p.Elements.Add(new CashWalletArtTheme.Element
        {
            Target = target, SetRect = false, StyleImage = true, ImageEnabled = false
        });
    }
}
