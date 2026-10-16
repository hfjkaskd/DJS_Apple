using UnityEngine;
using UnityEngine.UI;
using static CompactEarningLayout;

/// <summary>Saved authoring presets for the reward, introduction and timed sequence pages.</summary>
public static class CompactRewardLayouts
{
    public static void Seed(CompactEarningTheme theme)
    {
        Reward(theme, "SuccessPanel", false);
        // Apply the base first; the variant retains the gameplay bindings and overrides.
        Reward(theme, "LocalSuccess", false);
        Reward(theme, "LocalLuckyReward", true);
        RewardItem(theme, "Item_Big");
        RewardItem(theme, "LocalLuckyItem");
        Introduction(theme);
        TargetComplete(theme);
        DiamondGuide(theme);
        Loading(theme);
        Sequence(theme);
    }

    private static void Reward(CompactEarningTheme theme, string name, bool lucky)
    {
        var p = Page(theme, name, 440);
        Rect(p, "i:bg", 0, 0, 936, 1120);
        Art(p, "i:bg", "CashWalletPanel", 1.5f);
        Rect(p, "i:main/Title", 0, 550, 730, 148);
        Art(p, "i:main/Title", "CashWalletHeader", 1, true);
        Rect(p, "t:labels[2].Text", 0, 550, 650, 105);
        Text(p, "t:labels[2].Text", 62, lucky ? "Lucky Reward" : "Level Complete", true, true);
        if (lucky)
        {
            Hide(p, "i:main/Title/Ribbon");
            Rect(p, "i:main/Title/ScrewDecoration", -320, 600, 125, 145);
            Art(p, "i:main/Title/ScrewDecoration", "CashWalletApple", 1, true);
        }
        else
        {
            Rect(p, "p:main/Title/OrchardReceiptCrest", -320, 600, 125, 145);
            Art(p, "p:main/Title/OrchardReceiptCrest", "CashWalletApple", 1, true);
        }
        Rect(p, "b:Close", 430, 515, 108, 108);
        Art(p, "b:Close", "CashWalletClose", 1, true);
        p.PreserveActiveTargets.Add("b:Close");
        Rect(p, "o:Items", 0, 95, 760, 390);
        p.RowLayouts.Add(new CompactEarningTheme.LayoutRule
        {
            Target = "o:Items", Horizontal = true, Spacing = 20,
            ControlSize = false, ItemSize = new Vector2(340, 390)
        });
        Rect(p, "b:MoreGetBtn", 0, -265, 676, 160);
        Art(p, "b:MoreGetBtn", "CashWalletButton", 1, true);
        Rect(p, "t:moreLabel", 30, -265, 550, 108);
        Text(p, "t:moreLabel", 62, "Claim 10X", true, true);
        Rect(p, "i:main/MoreGetBtn/ad", -240, -265, 68, 62);
        Rect(p, "b:GetBtn", 0, -425, 620, 78);
        Text(p, "t:labels[1].Text", 42, "Claim");
        p.PreserveActiveTargets.Add("b:GetBtn");
        p.PreserveActiveTargets.Add("t:labels[1].Text");
    }

    private static void RewardItem(CompactEarningTheme theme, string name)
    {
        var p = Page(theme, name);
        p.AddTestCaption = false;
        Rect(p, "o:$root", 0, 0, 340, 390);
        Rect(p, "i:Glow", 0, 65, 290, 290);
        Rect(p, "i:icon", 0, 65, 265, 220);
        Rect(p, "t:count", 0, -100, 330, 112);
        Text(p, "t:count", 66, null, false, true);
        Hide(p, "o:max");
    }

    private static void Introduction(CompactEarningTheme theme)
    {
        var p = Page(theme, "TXRecordGuidePanel", 420);
        Rect(p, "i:main/Image", 0, 0, 936, 1080);
        Art(p, "i:main/Image", "CashWalletPanel", 1.5f);
        // The imported guide places its title before this image. A full-height
        // card must sit behind it, otherwise it covers the lower half of the header.
        p.SendToBackTargets.Add("i:main/Image");
        Rect(p, "i:main/Title", 0, 530, 700, 148);
        Art(p, "i:main/Title", "CashWalletHeader", 1, true);
        Rect(p, "t:labels[0].Text", 0, 530, 610, 110);
        Text(p, "t:labels[0].Text", 62, "Welcome", true, true);
        Rect(p, "p:main/Title/OrchardAppleAccent", -310, 575, 120, 140);
        Art(p, "p:main/Title/OrchardAppleAccent", "CashWalletApple", 1, true);
        Rect(p, "t:tip", 0, 265, 740, 155);
        Text(p, "t:tip", 44);
        Rect(p, "p:main/HideLevel", 0, 20, 780, 250);
        Rect(p, "p:main/HideLevel/Levels", 0, 20, 780, 250);
        DisableLayout(p, "p:main/HideLevel/Levels");
        Rect(p, "i:main/HideLevel/Progress", 0, 50, 650, 48);
        Art(p, "i:main/HideLevel/Progress", "CashWalletProgressTrack", 1, true);
        Rect(p, "i:main/HideLevel/Progress/Bar", 0, 50, 638, 34);
        Art(p, "i:main/HideLevel/Progress/Bar", "CashWalletProgressFill", 1, true);
        string[] names = { "1", "2", "3", "4", "7" };
        for (int i = 0; i < names.Length; i++)
        {
            float x = -300 + i * 150;
            string node = "p:main/HideLevel/Levels/" + names[i];
            Rect(p, node, x, 50, 130, 140);
            Rect(p, node + "/BG", x, 50, 108, 118);
            Rect(p, node + "/Level", x, 50, 86, 90);
            Text(p, node + "/Level", 52, null, false, true);
            string label = node + (i == 4 ? "/Get/label_160" : "/Get/label_1");
            if (i < 4)
            {
                // Repeated miniature badges were unreadable on phones. The
                // numbered path and one final milestone carry the same guide.
                Hide(p, node + "/Get");
            }
            else
            {
                Rect(p, node + "/Get", 225, -70, 300, 80);
                Art(p, node + "/Get", "CashWalletButton", 1, true);
                Rect(p, label, 225, -70, 270, 58);
                Text(p, label, 36, "Withdraw all", true, true);
            }
        }
        Rect(p, "b:startButton", 0, -350, 676, 160);
        Art(p, "b:startButton", "CashWalletButton", 1, true);
        Rect(p, "t:labels[6].Text", 0, -350, 590, 110);
        Text(p, "t:labels[6].Text", 64, "Start", true, true);
    }

    private static void TargetComplete(CompactEarningTheme theme)
    {
        var p = Page(theme, "TXGuideTargetCompletePanel", 420);
        Rect(p, "i:main/Image", 0, 0, 936, 1080);
        Art(p, "i:main/Image", "CashWalletPanel", 1.5f);
        Rect(p, "i:main/Title", 0, 530, 760, 148);
        Art(p, "i:main/Title", "CashWalletHeader", 1, true);
        Rect(p, "t:labels[1].Text", 0, 530, 680, 110);
        Text(p, "t:labels[1].Text", 53, null, true, true);
        Hide(p, "i:main/Image (1)");
        Rect(p, "i:main/Image (2)", 0, 190, 350, 350);
        Rect(p, "i:goldImage", 0, 190, 270, 230);
        Art(p, "i:goldImage", "CashWalletMoney", 1, true);
        Rect(p, "t:tip", 0, -30, 740, 165);
        Text(p, "t:tip", 122, null, false, true);
        Rect(p, "b:continueButton", 0, -330, 676, 160);
        Art(p, "b:continueButton", "CashWalletButton", 1, true);
        Rect(p, "t:labels[0].Text", 0, -330, 590, 110);
        Text(p, "t:labels[0].Text", 64, null, true, true);
        Hide(p, "i:main/TXbtn/Hand");
    }

    private static void DiamondGuide(CompactEarningTheme theme)
    {
        var p = Page(theme, "LocalDiamondUnlockGuide", 380);
        // Preserve the actual Button/card relationship; the four old spotlight
        // strips are unnecessary once the guide is a centered modal card.
        foreach (string side in new[] { "Above", "Below", "Left", "Right" })
            Hide(p, "p:ApkArtwork/Shade" + side);
        Rect(p, "p:ApkArtwork/Tip", 0, 0, 936, 1000);
        Art(p, "p:ApkArtwork/Tip", "CashWalletPanel", 1.5f);
        p.SendToBackTargets.Add("p:ApkArtwork/Tip");
        Rect(p, "p:ApkArtwork/Tip/YellowScrewDecoration", -320, 490, 125, 145);
        Art(p, "p:ApkArtwork/Tip/YellowScrewDecoration", "CashWalletApple", 1, true);
        Rect(p, "t:TipValue", 0, -155, 740, 210);
        Text(p, "t:TipValue", 44);
        Rect(p, "o:Pos", 0, 145, 760, 180);
        Rect(p, "b:Button", 0, 145, 760, 180);
        string card = "p:ApkArtwork/Pos/Button/DiamondCard";
        Rect(p, card, 0, 145, 760, 180);
        Rect(p, card + "/BG", 0, 145, 760, 180);
        Art(p, card + "/BG", "CashWalletInset", 2);
        Rect(p, card + "/Icon", -285, 145, 140, 125);
        Rect(p, "t:diamondAmount", -75, 145, 230, 112);
        Text(p, "t:diamondAmount", 72, null, false, true);
        Rect(p, card + "/Click", 230, 145, 235, 130);
        Art(p, card + "/Click", "CashWalletButton", 1, true);
        Rect(p, card + "/Click/label_185", 230, 145, 200, 95);
        Text(p, card + "/Click/label_185", 49, "Cash", true, true);
        Hide(p, "o:Hand");
        Hide(p, "o:Arraw");
    }

    private static void Loading(CompactEarningTheme theme)
    {
        var p = Page(theme, "TXGuideLoadingPanel", 165);
        Rect(p, "i:main/Image", 0, 0, 900, 470);
        Art(p, "i:main/Image", "CashWalletPanel", 1.5f);
        Rect(p, "t:label", 0, 30, 740, 110);
        Text(p, "t:label", 44);
        Rect(p, "i:main/Progress", 0, -115, 740, 70);
        Art(p, "i:main/Progress", "CashWalletProgressTrack", 1, true);
        Rect(p, "i:Progress", 0, -115, 722, 54);
        Art(p, "i:Progress", "CashWalletProgressFill", 1, true);
        // Loading uses Image.fillAmount, unlike the wallet's resized width.
        p.Elements[p.Elements.Count - 1].ImageType = Image.Type.Filled;
    }

    private static void Sequence(CompactEarningTheme theme)
    {
        var p = Page(theme, "LocalEarningSequence");
        // These mutually exclusive timed states retain the host footer; placing
        // one fixed caption on the root would overlap the short receipt state.
        p.AddTestCaption = false;
        for (int i = 0; i < 4; i++)
        {
            string prefix = "messageLayouts[" + i + "]";
            Rect(p, "i:" + prefix + ".Artwork", 0, 0, 936, 720);
            Art(p, "i:" + prefix + ".Artwork", "CashWalletPanel", 1.5f);
            Rect(p, "t:" + prefix + ".Title", 0, 200, 740, 110);
            Text(p, "t:" + prefix + ".Title", 58, null, false, true);
            Rect(p, "t:" + prefix + ".Body", 0, -40, 735, 230);
            Text(p, "t:" + prefix + ".Body", 43);
        }
        Hide(p, "p:ApkArtwork/Message/Activating/You");
        Hide(p, "p:ApkArtwork/Message/Activated/You");
        Rect(p, "i:ApkArtwork/Receipt/Banner", 0, 0, 900, 490);
        Art(p, "i:ApkArtwork/Receipt/Banner", "CashWalletPanel", 1.5f);
        Rect(p, "t:receiptAmount", 0, 0, 740, 190);
        Text(p, "t:receiptAmount", 132, null, false, true);
        Rect(p, "o:bonusesGroup", 0, 0, 936, 1320);
        Art(p, "o:bonusesGroup", "CashWalletPanel", 1.5f);
        p.PreserveActiveTargets.Add("o:bonusesGroup");
        Hide(p, "p:ApkArtwork/Bonuses/AvatarMask");
        Hide(p, "p:ApkArtwork/Bonuses/CashDecorationLeft");
        Hide(p, "p:ApkArtwork/Bonuses/CashDecorationRight");
        Hide(p, "p:ApkArtwork/Bonuses/PlayerRibbon");
        Rect(p, "p:ApkArtwork/Bonuses/Heading", 0, 610, 760, 148);
        Art(p, "p:ApkArtwork/Bonuses/Heading", "CashWalletHeader", 1, true);
        Rect(p, "t:bonusesTitle", 0, 610, 680, 105);
        Text(p, "t:bonusesTitle", 56, null, true, true);
        for (int i = 0; i < 5; i++)
        {
            float y = 385 - i * 190;
            string row = "p:ApkArtwork/Bonuses/Bonus" + (i + 1);
            Rect(p, row, 0, y, 780, 164);
            Art(p, row, "CashWalletInset", 2);
            Hide(p, row + "/BonusBadge");
            Rect(p, row + "/Icon", -265, y, 155, 105);
            Rect(p, "t:bonusCaptions[" + i + "]", 100, y, 490, 126);
            Text(p, "t:bonusCaptions[" + i + "]", 36);
        }
    }
}
