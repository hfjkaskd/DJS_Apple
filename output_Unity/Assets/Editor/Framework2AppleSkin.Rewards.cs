using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Editor authoring only. Existing source paths, controls, and data bindings are retained.
public static partial class Framework2AppleSkin
{
    private static void ApplyRewards(GameObject root, string prefabName)
    {
        switch (prefabName)
        {
            case "WithdrawMissionView": RewardMission(root); break;
            case "WithdrawFeedbackView": RewardFeedback(root); break;
            case "SlotView": RewardSlot(root); break;
            case "ProgressView": RewardStageProgress(root); break;
            case "BloatView": RewardBonusIntro(root); break;
            case "BonusWheelView": RewardWheel(root, true); break;
            case "WithDrawSuc": RewardCompensation(root); break;
            case "BloatProgressView": RewardFinalProgress(root); break;
            case "RewardView": RewardSettlement(root, false); break;
            case "ChallangeSuccessView": RewardSettlement(root, true); break;
            case "LuckySuccessView": RewardLuckySuccess(root); break;
            case "LuckySpinView": RewardWheel(root, false); break;
            case "GameBalancView": RewardWallet(root); break;
            case "NoticeView": RewardNotice(root); break;
            default: return;
        }
        RewardBindPresenter(root, prefabName);
    }

    private static void RewardBindPresenter(GameObject root, string prefabName)
    {
        if (root.GetComponent<Framework2AppleSkinMarker>() == null)
            root.AddComponent<Framework2AppleSkinMarker>();
        Framework2AppleRewardPresenter presenter = root.GetComponent<Framework2AppleRewardPresenter>();
        if (presenter == null) presenter = root.AddComponent<Framework2AppleRewardPresenter>();
        Framework2AppleRewardPresenter.Page page = Framework2AppleRewardPresenter.Page.Other;
        switch (prefabName)
        {
            case "WithdrawMissionView": page = Framework2AppleRewardPresenter.Page.AdMission; break;
            case "WithdrawFeedbackView": page = Framework2AppleRewardPresenter.Page.Feedback; break;
            case "ProgressView": page = Framework2AppleRewardPresenter.Page.StageProgress; break;
            case "SlotView": page = Framework2AppleRewardPresenter.Page.Slot; break;
            case "BloatView": page = Framework2AppleRewardPresenter.Page.BonusIntro; break;
            case "WithDrawSuc": page = Framework2AppleRewardPresenter.Page.Compensation; break;
            case "BloatProgressView": page = Framework2AppleRewardPresenter.Page.FinalProgress; break;
            case "BonusWheelView": page = Framework2AppleRewardPresenter.Page.BonusWheel; break;
        }
        presenter.Configure(page,
            RewardComponent<Text>(root, "SkinRewardAmount"),
            RewardComponent<Text>(root, "SkinCurrentBalance"),
            RewardComponent<Text>(root, "SkinTargetAmount"),
            RewardComponent<Text>(root, "SkinRemainingAmount"),
            RewardComponent<Text>(root, "SkinProgressPercent"),
            RewardComponent<Text>(root, "SkinMissionCount"),
            RewardComponent<Image>(root, "SkinMissionFill"),
            root.GetComponent<WZSDK.ProgressView>(), root.GetComponent<WZSDK.SlotView>(),
            root.GetComponent<WZSDK.BloatProgressView>(), root.GetComponent<WZSDK.BonusWheelView>(),
            root.GetComponent<WZSDK.WithdrawFeedbackView>());
        presenter.enabled = page != Framework2AppleRewardPresenter.Page.Other;
        presenter.ConfigurePropIcons(
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/res/local/boxreward/sprite/Magic.asset"),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/res/local/pops/newitempop/sprite/IconUndo.asset"));
        WZSDK.RewardView reward = root.GetComponent<WZSDK.RewardView>();
        if (reward != null) reward.ConfigureAppleSkin(presenter);
        WZSDK.ChallangeSuccessView challenge = root.GetComponent<WZSDK.ChallangeSuccessView>();
        if (challenge != null) challenge.ConfigureAppleSkin(presenter);
        WZSDK.LuckySpinView spin = root.GetComponent<WZSDK.LuckySpinView>();
        if (spin != null) spin.ConfigureAppleSkin(presenter);
    }

    private static T RewardComponent<T>(GameObject root, string path) where T : Component
    {
        Transform transform = RewardFind(root, path);
        return transform != null ? transform.GetComponent<T>() : null;
    }

    private static void RewardMission(GameObject root)
    {
        RewardFrame(root, "Test checks", 1550, -5);
        RewardMask(root, "RewardMessage");
        RewardContainer(root, "RewardMessage/Panel", 0, 0, 1100, 1450);
        RewardHideImage(root, "RewardMessage/Panel");
        RewardHideImage(root, "RewardMessage/Panel/bg");
        // This source prefab deliberately has two siblings named Top; paths below
        // are resolved against the full hierarchy, not Transform.Find's first match.
        RectTransform[] tops = root.GetComponentsInChildren<RectTransform>(true);
        for (int i = 0; i < tops.Length; ++i)
            if (RewardPath(root.transform, tops[i]) == "RewardMessage/Panel/Top")
            {
                RewardRect(tops[i], 0, 0, 1100, 1450);
                Image old = tops[i].GetComponent<Image>();
                if (old != null) old.enabled = false;
            }
        RewardContainer(root, "RewardMessage/Panel/Top/Bg2", 0, 0, 1100, 1450);
        RewardHideImage(root, "RewardMessage/Panel/Top/Bg2");
        RewardContainer(root, "RewardMessage/Panel/Middle", 0, 0, 1100, 1450);
        RewardContainer(root, "RewardMessage/Panel/Bottom", 0, 0, 1100, 1450);
        RewardHideImage(root, "RewardMessage/Panel/Middle/bg1");
        RewardArt(root, "SkinChest", "chest", 0, 470, 580, 350);
        RewardInset(root, "SkinTaskInset", 0, -130, 980, 1010);
        RewardHiddenGraphic(root, "RewardMessage/Panel/Top/Bg2/AccounBalance");
        RewardNewText(root, "SkinMissionRewardLabel", "Test reward", 0, 260, 900, 72, 55, Brown);
        RewardText(root, "RewardMessage/Panel/Top/Bg2/CurCoin", 0, 135, 920, 150, 114);
        RewardHiddenGraphic(root, "RewardMessage/Panel/Top/Bg2/Cash");
        RewardText(root, "RewardMessage/Panel/Top/Bg2/Time", -220, 330, 460, 42, 25);
        RewardText(root, "RewardMessage/Panel/Top/Bg2/State", 265, 330, 390, 42, 27);
        RewardHiddenGraphic(root, "RewardMessage/Panel/Top/Bg2/Time");
        RewardHiddenGraphic(root, "RewardMessage/Panel/Top/Bg2/State");
        RewardContainer(root, "RewardMessage/Panel/Middle/ProgressStateRoot", 0, 0, 980, 1450);
        RewardProgress(root, "RewardMessage/Panel/Middle/BG", "RewardMessage/Panel/Middle/BG/Progress", 0, -52, 700, 36);
        for (int i = 1; i <= 3; ++i)
            RewardStep(root, "RewardMessage/Panel/Middle/ProgressStateRoot/ProgressState" + i,
                (i - 2) * 330, -52, 160, 65);
        RewardNewImage(root, "SkinMissionTrack", "track", 0, -215, 960, 80, true);
        Image fill = RewardNewImage(root, "SkinMissionFill", "fill", 0, -215, 940, 60, false);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = 0;
        fill.fillAmount = 0;
        RewardNewText(root, "SkinMissionCount", "", 0, -215, 860, 65, 45, Color.white);
        RewardText(root, "RewardMessage/Panel/Bottom/Dec", 0, -315, 900, 130, 37);
        RewardText(root, "RewardMessage/Panel/Middle/QueueSize", 0, -420, 900, 65, 33);
        RewardButton(root, "RewardMessage/Panel/Bottom/GenerateMoreBtn", 0, -530, 960, 140, false);
        RewardButton(root, "RewardMessage/continuebtn", 0, -675, 960, 120, true);
        RewardClose(root, "RewardMessage/close", 490, 660);
        RewardHideImage(root, "RewardMessage/continuebtn/line");
        RewardFooter(root, -750);
    }

    private static void RewardFeedback(GameObject root)
    {
        RewardFrame(root, "Test request saved", 1160, 0);
        RewardMask(root, "RewardMessage");
        RewardContainer(root, "RewardMessage/Panel", 0, 0, 1100, 1160);
        RewardContainer(root, "RewardMessage/Panel/bg", 0, 0, 1100, 1160);
        RewardHideImage(root, "RewardMessage/Panel/bg");
        RewardHideImage(root, "RewardMessage/Panel/bg/Panel");
        RewardHiddenGraphic(root, "RewardMessage/Panel/bg/Title");
        RewardContainer(root, "RewardMessage/Panel/Top", 0, 0, 1100, 1160);
        RewardContainer(root, "RewardMessage/Panel/Bottom", 0, 0, 1100, 1160);
        RewardInset(root, "SkinSavedInset", 0, 15, 980, 920);
        RewardArt(root, "SkinSavedCash", "money", -265, 242, 300, 280);
        RewardArt(root, "SkinSavedCheck", "check", -170, 150, 125, 125);
        RewardNewText(root, "SkinRewardAmount", "", 170, 250, 550, 150, 105, Brown);
        RewardText(root, "RewardMessage/Panel/Top/Title", 0, -15, 900, 180, 47);
        RewardHiddenGraphic(root, "RewardMessage/Panel/Top/State");
        RewardButton(root, "RewardMessage/Panel/Bottom/GenerateMoreBtn", 0, -272, 960, 150, false);
        RewardButton(root, "RewardMessage/Panel/continuebtn", 0, -443, 960, 130, true);
        RewardHideImage(root, "RewardMessage/Panel/continuebtn/line");
        RewardClose(root, "RewardMessage/Panel/bg/close", 490, 530);
        RewardFooter(root, -554);
    }

    private static void RewardStageProgress(GameObject root)
    {
        RewardFrame(root, "Stage reward", 1180, -130);
        RewardMask(root, "Mask");
        RewardContainer(root, "bg", 0, 0, 1100, 1300);
        RewardHideImage(root, "bg");
        RewardHiddenGraphic(root, "bg/Title");
        RewardHideImage(root, "Panel");
        RewardArt(root, "SkinCashPile", "cashpile", 0, 625, 600, 250);
        RewardInset(root, "SkinAmountInset", 0, 190, 980, 250);
        RewardNewText(root, "SkinRewardAmount", "", 0, 230, 930, 120, 106, Brown);
        RewardNewText(root, "SkinAwardLabel", "Test reward received.", 0, 163, 930, 65, 44, Brown);
        RewardInset(root, "SkinTargetInset", 0, -105, 980, 360);
        RewardNewText(root, "SkinTargetLabel", "Balance target", 0, 20, 930, 65, 46, Brown);
        RewardContainer(root, "ProgressRoot", 0, 0, 1100, 1300);
        RewardProgress(root, "ProgressRoot/BG", "ProgressRoot/BG/Progress", -75, -155, 770, 94);
        RewardText(root, "ProgressRoot/BG/Progress/Count", 75, 130, 950, 75, 44);
        RewardNewText(root, "SkinProgressPercent", "", 430, -155, 150, 70, 47, Brown);
        RewardText(root, "dec", 0, -345, 925, 100, 32);
        RewardButton(root, "continuebtn", 0, -475, 980, 164, false);
        RewardHideImage(root, "continuebtn/line");
        RewardText(root, "bg/Panel/content", 0, -345, 940, 100, 32);
        RewardClose(root, "bg/close", 490, 425);
        RewardFooter(root, -675);
    }

    private static void RewardSlot(GameObject root)
    {
        RewardFrame(root, "Extra Test Rewards", 1370, -5);
        RewardMask(root, "bg");
        RewardHiddenGraphic(root, "Top");
        RewardHiddenGraphic(root, "Top/Text (Legacy)");
        RewardContainer(root, "LotteryPanel", 0, 0, 1100, 1370);
        RewardContainer(root, "LotteryPanel/Image (2)", 0, 330, 960, 300);
        RewardImage(root, "LotteryPanel/Image (2)", "inset", true);
        // Reel objects and their numeric Text references remain the source ones.
        // The masks and rolling transforms are retained; only the authored frame changes.
        string[] columns = { "Panel (3)", "Panel (2)", "Panel (1)", "Panel" };
        for (int i = 0; i < columns.Length; ++i)
        {
            string path = "LotteryPanel/Image (2)/" + columns[i];
            RewardContainer(root, path, -333 + i * 222, 0, 208, 250);
            RewardImage(root, path, "secondary", true);
            Transform reel = RewardFind(root, path);
            if (reel == null) continue;
            Text[] numbers = reel.GetComponentsInChildren<Text>(true);
            for (int j = 0; j < numbers.Length; ++j)
            {
                numbers[j].font = SkinFont;
                numbers[j].color = Brown;
                numbers[j].fontSize = 115;
                numbers[j].resizeTextForBestFit = false;
                numbers[j].alignment = TextAnchor.MiddleCenter;
                numbers[j].rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 195);
            }
        }
        RewardArt(root, "SkinCashPile", "cashpile", 0, -25, 730, 340);
        RewardNewText(root, "SkinPossibleLabel", "Possible reward (simulated)", 0, -235, 950, 70, 43, Brown);
        RewardNewText(root, "SkinRewardAmount", "", 0, -335, 950, 130, 110, Brown);
        RewardButton(root, "ClickBtn", 0, -500, 980, 155, false);
        RewardClose(root, "CloseBtn", 485, 600);
        RewardFooter(root, -652);
    }

    private static void RewardBonusIntro(GameObject root)
    {
        RewardFrame(root, "Bonus increase", 1370, -5);
        RewardMask(root, "Mask");
        RewardContainer(root, "bg", 0, 0, 1100, 1370);
        RewardHideImage(root, "bg");
        RewardHiddenGraphic(root, "bg/title");
        RewardHideImage(root, "bg/Panel");
        RewardContainer(root, "bg/Panel", 0, 0, 1100, 1370);
        RewardArt(root, "SkinCashPile", "cashpile", -50, 330, 680, 350);
        RewardArt(root, "SkinIncreaseArrow", "arrow", 285, 350, 220, 290);
        RewardNewText(root, "SkinTargetLabel", "Simulated target:", 0, 85, 950, 70, 45, Brown);
        RewardNewText(root, "SkinTargetAmount", "", 0, -20, 960, 150, 108, Brown);
        RewardText(root, "bg/Panel/Dec", 0, -140, 930, 100, 36);
        RewardInset(root, "SkinCurrentInset", 0, -300, 970, 230);
        RewardNewText(root, "SkinCurrentLabel", "Current balance (simulated)", 0, -240, 920, 65, 40, Brown);
        RewardArt(root, "SkinBalanceMoney", "money", -310, -325, 160, 135);
        RewardNewText(root, "SkinCurrentBalance", "", 100, -330, 620, 110, 79, Brown);
        RewardButton(root, "bg/ClickBtn", 0, -520, 980, 155, false);
        RewardClose(root, "bg/close", 485, 600);
        RewardFooter(root, -652);
    }

    private static void RewardCompensation(GameObject root)
    {
        RewardFrame(root, "Test reward", 1300, -20);
        RewardMask(root, "");
        RewardContainer(root, "PanelA", 0, 0, 1100, 1300);
        RewardContainer(root, "PanelA/bg", 0, 0, 1100, 1300);
        RewardHideImage(root, "PanelA/bg");
        RewardHideImage(root, "PanelA/bg/Panel");
        RewardHiddenGraphic(root, "PanelA/bg/Title");
        RewardArt(root, "SkinCashPile", "cashpile", 0, 220, 830, 440);
        RewardText(root, "PanelA/bg/content", 0, -90, 990, 180, 122);
        RewardInset(root, "SkinBalanceInset", 0, -300, 900, 145);
        RewardNewText(root, "SkinCurrentBalance", "", 0, -300, 850, 105, 54, Brown);
        RewardText(root, "PanelA/bg/Dec", 0, -440, 910, 140, 32);
        RewardClose(root, "PanelA/bg/close", 490, 585);
        RewardFooter(root, -625);
    }

    private static void RewardFinalProgress(GameObject root)
    {
        RewardFrame(root, "Test progress", 1400, -10);
        RewardMask(root, "Mask");
        RewardContainer(root, "bg", 0, 0, 1100, 1400);
        RewardHideImage(root, "bg");
        RewardHiddenGraphic(root, "bg/Title");
        RewardContainer(root, "bg/Panel", 0, 0, 1100, 1400);
        RewardHideImage(root, "bg/Panel");
        RewardInset(root, "SkinCurrentInset", 0, 330, 980, 300);
        RewardArt(root, "SkinCashIcon", "money", -330, 340, 245, 240);
        RewardNewText(root, "SkinCurrentBalance", "", 130, 400, 650, 125, 91, Brown);
        RewardNewText(root, "SkinSimulationLabel", "Local simulation", 130, 300, 650, 65, 40, Brown);
        RewardNewText(root, "SkinLocalDetails", "Dummy details saved locally.", 130, 235, 650, 58, 32, Brown);
        RewardContainer(root, "ProgressRoot", 0, 0, 1100, 1400);
        RewardProgress(root, "ProgressRoot/BG", "ProgressRoot/BG/Progress", -75, 100, 790, 100);
        RewardText(root, "ProgressRoot/BG/Progress/Count", 0, 0, 770, 80, 43, Color.white);
        RewardContainer(root, "ProgressRoot/OverallProgress", 425, 100, 150, 100);
        RewardHiddenGraphic(root, "ProgressRoot/OverallProgress/Fill");
        RewardHideImage(root, "ProgressRoot/OverallProgress/Bg");
        RewardText(root, "ProgressRoot/OverallProgress/OverallCount", 0, 0, 150, 95, 47);
        RewardInset(root, "SkinRemainingInset", 0, -145, 980, 280);
        RewardArt(root, "SkinRemainingMoney", "money", -340, -160, 225, 200);
        RewardNewText(root, "SkinRemainingLabel", "Remaining to goal:", 150, -80, 640, 65, 43, Brown);
        RewardNewText(root, "SkinRemainingAmount", "", 150, -185, 640, 125, 91, Brown);
        RewardButton(root, "bg/ClickBtn", 0, -390, 980, 160, false);
        RewardText(root, "bg/Panel/Dec", 0, -535, 940, 100, 32);
        RewardHiddenGraphic(root, "bg/Panel/content");
        RewardClose(root, "bg/close", 490, 630);
        RewardFooter(root, -675);
    }

    private static void RewardSettlement(GameObject root, bool challenge)
    {
        RewardFrame(root, challenge ? "Challenge success" : "Test reward", 1280, -150);
        RewardMask(root, "Mask");
        RewardContainer(root, "Plane", 0, 0, 1100, 1360);
        RewardContainer(root, "Plane/Head", 0, 0, 1100, 1360);
        RewardHideImage(root, "Plane/Head/Bg");
        RewardHiddenGraphic(root, "Plane/Head/Text");
        RewardContainer(root, "Plane/RewardRoot", 0, 0, 1100, 1360);
        Transform rewards = RewardFind(root, "Plane/RewardRoot");
        if (rewards != null)
        {
            LayoutGroup group = rewards.GetComponent<LayoutGroup>();
            if (group != null) group.enabled = false;
        }
        for (int i = 1; i <= 2; ++i)
        {
            string item = "Plane/RewardRoot/Reward" + i;
            RewardContainer(root, item, i == 1 ? 0 : 305, 0, 1100, 1360);
            RewardContainer(root, item + "/Reward1", 0, 0, 1100, 1360);
            RewardImage(root, item + "/Reward1/MoneyIcon", "cashpile", false);
            RewardContainer(root, item + "/Reward1/MoneyIcon", 0, challenge ? 650 : 260, challenge ? 600 : 820, challenge ? 245 : 420);
            RewardContainer(root, item + "/Reward1/IAAMoneyIcon", 0, 295, 650, 350);
            RewardText(root, item + "/Reward1/count", 0, challenge ? 270 : -20, 980, 180, 119);
        }
        RewardNewText(root, "SkinLocalLabel", "Local simulation", 0, -150, 950, 65, 42, Brown);
        RewardNewText(root, "SkinLocalDetails", "Dummy details saved locally.", 0, -215, 950, 65, 36, Brown);
        if (challenge)
        {
            RewardHiddenGraphic(root, "SkinLocalLabel");
            RewardHiddenGraphic(root, "SkinLocalDetails");
            RewardContainer(root, "ProgressRoot", 0, 0, 1100, 1360);
            RewardContainer(root, "ProgressRoot/ProgressStateRoot", 0, 0, 1100, 1360);
            Transform states = RewardFind(root, "ProgressRoot/ProgressStateRoot");
            if (states != null)
            {
                LayoutGroup group = states.GetComponent<LayoutGroup>();
                if (group != null) group.enabled = false;
            }
            RewardProgress(root, "ProgressRoot/BG", "ProgressRoot/BG/Progress", 0, 52, 850, 30);
            for (int i = 1; i <= 5; ++i)
                RewardStep(root, "ProgressRoot/ProgressStateRoot/ProgressState" + i,
                    -425 + (i - 1) * 164, 52, 136, 57);
            RewardStep(root, "ProgressRoot/ProgressState6", 415, 52, 150, 50);
            RewardImage(root, "ProgressRoot/ProgressState6/Finish", "wheel", false);
            RewardImage(root, "ProgressRoot/ProgressState6/Unfinished", "wheel", false);
            RewardHiddenGraphic(root, "ProgressRoot/ProgressState6/Count");
            RewardText(root, "ProgressRoot/Dec", 0, -130, 970, 125, 39);
        }
        RewardButton(root, "Plane/AdBtn", 0, -345, 980, 180, false);
        RewardContainer(root, "Plane/AdBtn/icon", -335, 0, 135, 125);
        RewardImage(root, "Plane/AdBtn/icon", "video", false);
        RewardText(root, "Plane/AdBtn/Text", 90, 0, 690, 135, 69, Color.white);
        RewardButton(root, "Plane/OnlyBtn", 0, -535, 980, 145, true);
        RewardHideImage(root, "Plane/OnlyBtn/Image");
        RewardFooter(root, -674);
    }

    private static void RewardLuckySuccess(GameObject root)
    {
        RewardFrame(root, "Lucky success", 1360, -25);
        RewardMask(root, "Mask");
        RewardContainer(root, "Plane", 0, 0, 1100, 1360);
        RewardHideImage(root, "Plane/Image");
        RewardContainer(root, "Plane/CardBg", 0, 0, 1100, 1360);
        RewardHideImage(root, "Plane/CardBg");
        RewardArt(root, "SkinLuckyWallet", "wallet", 0, 275, 680, 450);
        RewardHiddenGraphic(root, "Plane/CardBg/CurrentLuckyCoin");
        RewardText(root, "Plane/CurrentLuckyCoinTxT", 0, -20, 960, 165, 116);
        RewardText(root, "Plane/CardBg/Lucky", 0, -145, 960, 75, 43);
        RewardText(root, "Plane/CardBg/WithdrawAnytime", 0, -215, 960, 75, 35);
        RewardHiddenGraphic(root, "Plane/Text (Legacy)");
        RewardButton(root, "Plane/AdBtn", 0, -345, 980, 175, false);
        RewardImage(root, "Plane/AdBtn/icon", "video", false);
        RewardContainer(root, "Plane/AdBtn/icon", -330, 0, 135, 125);
        RewardText(root, "Plane/AdBtn/Text", 90, 0, 690, 135, 69, Color.white);
        RewardButton(root, "Plane/AbandonBtn", 0, -535, 980, 145, true);
        RewardHideImage(root, "Plane/AbandonBtn/Image");
        RewardFooter(root, -674);
    }

    private static void RewardWheel(GameObject root, bool bonus)
    {
        RewardFrame(root, "Lucky Spin", 1450, -20);
        RewardMask(root, "Mask");
        RewardContainer(root, "Plane", 0, 0, 1100, 1450);
        RewardContainer(root, "Plane/Head", 0, 0, 1100, 1450);
        RewardHideImage(root, "Plane/Head/Bg");
        RewardHiddenGraphic(root, "Plane/Head/Text");
        if (bonus)
        {
            RewardHideImage(root, "Plane/Top");
            RewardHiddenGraphic(root, "Plane/Top/Text (Legacy)");
        }
        RewardContainer(root, "Plane/Turntable", 0, bonus ? 75 : 105, 1000, 1000);
        RewardHideImage(root, "Plane/Turntable/Bg");
        RewardContainer(root, "Plane/Turntable/Table", 0, 0, 1000, 1000);
        RewardImage(root, "Plane/Turntable/Table", bonus ? "wheel8" : "wheel", false);
        RewardContainer(root, "Plane/Turntable/Pointer", 0, 465, 150, 180);
        RewardImage(root, "Plane/Turntable/Pointer", "pointer", false);
        if (!bonus)
        {
            Image disc = RewardNewImage(RewardFind(root, "Plane/Turntable/Table").gameObject,
                "SkinWheelDisc", "wheel", 0, 0, 1000, 1000, false);
            disc.rectTransform.localRotation = Quaternion.Euler(0, 0, -30);
            disc.transform.SetAsFirstSibling();
            RewardHideImage(root, "Plane/Turntable/Table");
            // Keep the original six reward slots. Source configuration still owns
            // reward assignment, angle lookup, and the resolved sprite for each icon.
            for (int i = 1; i <= 6; ++i)
            {
                float radians = (60f - (i - 1) * 60f) * Mathf.Deg2Rad;
                string item = "Plane/Turntable/Table/TTItem" + i;
                RewardContainer(root, item, Mathf.Cos(radians) * 320, Mathf.Sin(radians) * 320, 240, 200);
                RewardHideImage(root, item);
                RewardContainer(root, item + "/TTItem" + i + "Icon", 0, 15, 175, 165);
                RewardText(root, item + "/TTItem" + i + "name", 0, -88, 225, 54, 30);
            }
            RewardButton(root, "Plane/LotteryBtn", 0, -490, 980, 170, false);
            RewardNewText(root, "SkinLocalWheelLabel", "Local simulation. No real rewards.", 0, -625, 970, 65, 33, Brown);
        }
        else
        {
            Transform wheel = RewardFind(root, "Plane/Turntable/Table");
            for (int i = 0; i < 8; i++)
            {
                float angle = (67.5f - i * 45f) * Mathf.Deg2Rad;
                RewardArt(wheel.gameObject, "SkinBonusCash" + i, "money", Mathf.Cos(angle) * 330,
                    Mathf.Sin(angle) * 330, 190, 170);
            }
            // The source lottery button remains a standard Button in its original
            // path; its visual position moves to the hub shown in the approved art.
            RewardButton(root, "Plane/LotteryBtn", 0, 75, 330, 330, false);
            RewardImage(root, "Plane/LotteryBtn", "bluebadge", false);
            RewardInset(root, "SkinWheelBalanceInset", 0, -510, 960, 155);
            RewardNewText(root, "SkinLocalWheelLabel", "Local simulation only.", 0, -470, 910, 62, 39, Brown);
            RewardNewText(root, "SkinCurrentBalance", "", 0, -535, 910, 62, 39, Brown);
        }
        RewardClose(root, "ExitBtn", 490, 655);
        RewardFooter(root, -704);
    }

    private static void RewardWallet(GameObject root)
    {
        RewardFrame(root, "Lucky Wallet", 1480, -10);
        RewardMask(root, "Mask");
        RewardContainer(root, "Plane", 0, 0, 1100, 1480);
        RewardContainer(root, "Plane/Bg", 0, 0, 1100, 1480);
        RewardHideImage(root, "Plane/Bg");
        RewardContainer(root, "Plane/Bg2", 0, 0, 1100, 1480);
        RewardHideImage(root, "Plane/Bg2");
        RewardHiddenGraphic(root, "Plane/Title");
        RewardHiddenGraphic(root, "Plane/Bg2/GameBalance");
        RewardInset(root, "SkinWalletBalanceInset", 0, 420, 980, 300);
        RewardArt(root, "SkinWalletMoney", "money", -320, 430, 270, 250);
        RewardText(root, "Plane/Bg2/CurrrCoin", 150, 470, 650, 140, 105);
        RewardText(root, "Plane/Bg2/AddCoin ", 150, 340, 650, 120, 38);
        RewardButton(root, "Plane/With", 0, 180, 980, 170, false);
        RewardContainer(root, "Plane/Bg/SpecialRoot", 0, -125, 980, 380);
        RewardContainer(root, "Plane/Bg/SpecialRoot/Special1", -250, 0, 465, 370);
        RewardContainer(root, "Plane/Bg/SpecialRoot/Special2", 250, 0, 465, 370);
        RewardImage(root, "Plane/Bg/SpecialRoot/Special1", "inset", true);
        RewardImage(root, "Plane/Bg/SpecialRoot/Special2", "inset", true);
        RewardArtAt(root, "Plane/Bg/SpecialRoot/Special1", "SkinSpecialCash", "cashpile", 0, 45, 350, 240);
        RewardArtAt(root, "Plane/Bg/SpecialRoot/Special2", "SkinSpecialWheel", "wheel", 0, 45, 275, 275);
        RewardText(root, "Plane/Bg/SpecialRoot/Special1/SpecialTxt1", 0, -125, 430, 75, 46);
        RewardText(root, "Plane/Bg/SpecialRoot/Special2/SpecialTxt2", 0, -125, 430, 75, 46);
        RewardText(root, "Plane/Bg/Content", 0, -380, 970, 70, 34);
        RewardContainer(root, "Plane/LevelProgressTxt", 0, 0, 1100, 1480);
        RewardHideImage(root, "Plane/LevelProgressTxt");
        for (int i = 1; i <= 4; ++i)
            RewardText(root, "Plane/LevelProgressTxt/Dec" + i, 0, -430 - (i - 1) * 64, 970, 65, 31);
        RewardClose(root, "Plane/ExitBtn", 490, 670);
        RewardFooter(root, -723);
    }

    private static void RewardNotice(GameObject root)
    {
        RewardFrame(root,"Notice",900,0);RewardFooter(root,-400);
        var scrim=RewardNewImage(root,"SkinNoticeScrim","inset",0,0,1200,2132,false);
        scrim.sprite=null;scrim.color=new Color(0,0,0,.5f);scrim.raycastTarget=false;
        NavStretch(scrim.transform,0,0,0,0);scrim.transform.SetAsFirstSibling();
        RewardContainer(root,"Bg",0,0,1200,2132);RewardHideImage(root,"Bg");
        RewardText(root,"Bg/NoticeText",0,180,940,140,54);
        NavNode(root.transform,"SkinNoticeValue");
        NavDecor(root.transform.Find("SkinNoticeValue"),"Money","money",0,0,100,100);
        NavNewText(root.transform.Find("SkinNoticeValue"),"Amount","",0,0,100,100,113,Brown);
        var primary=NavNode(root.transform,"SkinNoticeConfirm");if(primary.GetComponent<Button>()==null)primary.gameObject.AddComponent<Button>();
        NavButton(root,"SkinNoticeConfirm",0,-240,930,180,false,76);
        NavNewText(primary,"Label","Confirm",0,0,850,145,76,Color.white);
        var exit=NavNode(root.transform,"SkinNoticeClose");if(exit.GetComponent<Button>()==null)exit.gameObject.AddComponent<Button>();
        NavImage(exit,"close",false);NavButtonTarget(exit);
    }

    private static void RewardFrame(GameObject root, string title, float height, float y)
    {
        RewardNewImage(root, "SkinSurface", "panel", 0, y, 1100, height, true).transform.SetSiblingIndex(1);
        Image heading = RewardNewImage(root, "SkinHeading", "title", 0, y + height * .5f - 35, 860, 190, true);
        RewardNewText(heading.gameObject, "SkinHeadingText", title, 0, 0, 775, 130, 65, Color.white);
        RewardArt(root, "SkinLeaf", "leaf", -370, y + height * .5f + 40, 130, 120);
    }

    private static void RewardFooter(GameObject root, float y)
    {
        RewardNewText(root, "SkinTestFooter", "TEST MODE · NO CASH PAYMENT", 0, y, 1000, 48, 29, Brown);
    }

    private static void RewardInset(GameObject root, string name, float x, float y, float width, float height)
    {
        Image image = RewardNewImage(root, name, "inset", x, y, width, height, true);
        image.transform.SetSiblingIndex(2);
    }

    private static void RewardProgress(GameObject root, string trackPath, string fillPath, float x, float y, float width, float height)
    {
        RewardContainer(root, trackPath, x, y, width, height);
        RewardImage(root, trackPath, "track", true);
        RewardContainer(root, fillPath, 0, 0, width - 20, height - 20);
        Image fill = RewardImage(root, fillPath, "fill", false);
        if (fill != null)
        {
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
        }
    }

    private static void RewardStep(GameObject root, string path, float x, float y, float size, int fontSize)
    {
        RewardContainer(root, path, x, y, size, size);
        RewardContainer(root, path + "/Finish", 0, 0, size, size);
        RewardContainer(root, path + "/Unfinished", 0, 0, size, size);
        RewardImage(root, path + "/Finish", "greenbadge", false);
        RewardImage(root, path + "/Unfinished", "goldbadge", false);
        RewardHideImage(root, path + "/Finish/bg");
        RewardText(root, path + "/Count", 0, 0, size - 20, size - 10, fontSize, Color.white);
    }

    private static void RewardButton(GameObject root, string path, float x, float y, float width, float height, bool secondary)
    {
        Transform target = RewardFind(root, path);
        if (target == null) return;
        RewardRect(target as RectTransform, x, y, width, height);
        RewardImage(root, path, secondary ? "secondary" : "button", true);
        Button button = target.GetComponent<Button>();
        if (button != null)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
            colors.pressedColor = new Color(.88f, .94f, 1f, 1f);
            colors.disabledColor = new Color(.68f, .68f, .68f, .9f);
            button.colors = colors;
        }
        Text[] labels = target.GetComponentsInChildren<Text>(true);
        for (int i = 0; i < labels.Length; ++i)
        {
            RewardRect(labels[i].rectTransform, 0, 0, width - 70, height - 35);
            RewardFormat(labels[i], secondary ? 57 : 65, secondary ? Brown : Color.white);
        }
    }

    private static void RewardClose(GameObject root, string path, float x, float y)
    {
        RewardContainer(root, path, x, y, 145, 145);
        RewardImage(root, path, "close", false);
    }

    private static void RewardMask(GameObject root, string path)
    {
        Transform target = path.Length == 0 ? root.transform : RewardFind(root, path);
        Image image = target != null ? target.GetComponent<Image>() : null;
        if (image != null) image.enabled = false;
        if (target != null)
        {
            Mask stencil = target.GetComponent<Mask>(); if (stencil != null) stencil.enabled = false;
            RectMask2D clip = target.GetComponent<RectMask2D>(); if (clip != null) clip.enabled = false;
        }
        // Some source masks are also ancestors of every interactive control.
        // A separate authored dimmer keeps the new surface below those controls.
        Transform found = root.transform.Find("SkinDimmer");
        GameObject dimmer = found != null ? found.gameObject : new GameObject("SkinDimmer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        if (found == null) dimmer.transform.SetParent(root.transform, false);
        RectTransform rect = dimmer.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(.5f, .5f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        Image dim = dimmer.GetComponent<Image>();
        dim.sprite = null;
        dim.color = new Color(.055f, .035f, .035f, .64f);
        dim.raycastTarget = true;
        dimmer.transform.SetAsFirstSibling();
        Transform surface = root.transform.Find("SkinSurface");
        if (surface != null) surface.SetSiblingIndex(1);
    }

    private static void RewardContainer(GameObject root, string path, float x, float y, float width, float height)
    {
        Transform target = RewardFind(root, path);
        if (target == null) return;
        RewardRect(target as RectTransform, x, y, width, height);
    }

    private static void RewardRect(RectTransform rect, float x, float y, float width, float height)
    {
        if (rect == null) return;
        foreach (LayoutGroup layout in rect.GetComponents<LayoutGroup>()) layout.enabled = false;
        var fitter = rect.GetComponent<ContentSizeFitter>(); if (fitter != null) fitter.enabled = false;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, height);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static void RewardText(GameObject root, string path, float x, float y, float width, float height, int size, Color? color = null)
    {
        Transform target = RewardFind(root, path);
        Text text = target != null ? target.GetComponent<Text>() : null;
        if (text == null) return;
        RewardRect(text.rectTransform, x, y, width, height);
        RewardFormat(text, size, color ?? Brown);
    }

    private static void RewardFormat(Text text, int size, Color color)
    {
        text.enabled = true;
        text.font = SkinFont;
        text.fontSize = size;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = Mathf.Max(24, Mathf.RoundToInt(size * .78f));
        text.resizeTextMaxSize = size;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        Outline outline = text.GetComponent<Outline>();
        if (outline != null)
        {
            outline.effectColor = color == Color.white ? new Color(.18f, .12f, .24f, .65f) : new Color(0, 0, 0, 0);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
        }
    }

    private static Image RewardImage(GameObject root, string path, string sprite, bool sliced)
    {
        Transform target = RewardFind(root, path);
        Image image = target != null ? target.GetComponent<Image>() : null;
        if (image == null) return null;
        image.enabled = true;
        image.sprite = SkinSprite(sprite);
        image.color = Color.white;
        image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        image.preserveAspect = !sliced;
        return image;
    }

    private static Image RewardNewImage(GameObject root, string name, string sprite, float x, float y, float width, float height, bool sliced)
    {
        Transform found = root.transform.Find(name);
        GameObject target = found != null ? found.gameObject : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        if (found == null) target.transform.SetParent(root.transform, false);
        RewardRect(target.GetComponent<RectTransform>(), x, y, width, height);
        Image image = target.GetComponent<Image>();
        image.sprite = SkinSprite(sprite);
        image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        image.preserveAspect = !sliced;
        image.color = Color.white;
        image.raycastTarget = false;
        return image;
    }

    private static Text RewardNewText(GameObject root, string name, string value, float x, float y, float width, float height, int size, Color color)
    {
        Transform found = root.transform.Find(name);
        GameObject target = found != null ? found.gameObject : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        if (found == null) target.transform.SetParent(root.transform, false);
        RewardRect(target.GetComponent<RectTransform>(), x, y, width, height);
        Text text = target.GetComponent<Text>();
        RewardFormat(text, size, color);
        text.text = value;
        return text;
    }

    private static void RewardArt(GameObject root, string name, string sprite, float x, float y, float width, float height)
    {
        RewardNewImage(root, name, sprite, x, y, width, height, false);
    }

    private static void RewardArtAt(GameObject root, string path, string name, string sprite, float x, float y, float width, float height)
    {
        Transform parent = RewardFind(root, path);
        if (parent != null) RewardArt(parent.gameObject, name, sprite, x, y, width, height);
    }

    private static void RewardHideImage(GameObject root, string path)
    {
        Transform target = RewardFind(root, path);
        Image image = target != null ? target.GetComponent<Image>() : null;
        if (image != null) image.enabled = false;
    }

    private static void RewardHiddenGraphic(GameObject root, string path)
    {
        Transform target = RewardFind(root, path);
        Graphic image = target != null ? target.GetComponent<Graphic>() : null;
        if (image != null) image.enabled = false;
    }

    private static Transform RewardFind(GameObject root, string path)
    {
        Transform[] all = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; ++i)
            if (RewardPath(root.transform, all[i]) == path) return all[i];
        return null;
    }

    private static string RewardPath(Transform root, Transform target)
    {
        if (target == root) return string.Empty;
        string path = target.name;
        while (target.parent != null && target.parent != root)
        {
            target = target.parent;
            path = target.name + "/" + path;
        }
        return path;
    }
}
