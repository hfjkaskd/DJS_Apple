using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Authoring of saved static UI only. No original controller paths or events are replaced.
public static partial class Framework2AppleSkin
{
    private static void ApplyNavigation(GameObject root, string prefabName)
    {
        switch (prefabName)
        {
            case "GamePlayerView": NavHud(root); break;
            case "SettingsView": NavSettings(root); break;
            case "UserLevelView": NavUserLevel(root); break;
            case "ReStartView": NavRestart(root); break;
            case "Tutorial": NavTutorial(root); break;
            case "UIEffectView": NavEffects(root); break;
            case "LocalContentErrorTip": NavContentError(root); break;
        }
    }

    private static void NavHud(GameObject root)
    {
        Transform top = root.transform.Find("Area/TopButtons");
        var topRect = top as RectTransform;
        if (topRect == null) return;
        NavRect(topRect, 0, -20, 1200, 330);
        topRect.anchorMin = topRect.anchorMax = new Vector2(.5f, 1);
        topRect.pivot = new Vector2(.5f, 1);
        NavClearGraphic(root.transform.Find("Area"));
        NavRect(root, "Area/TopButtons/LV", -500, 75, 190, 190);
        NavImage(root.transform.Find("Area/TopButtons/LV"), "bluebadge", false);
        NavGraphicsOff(root.transform.Find("Area/TopButtons/LV/levelText"));
        Text number = NavNewText(root.transform.Find("Area/TopButtons/LV"), "NavLevelNumber", "", 0, 1, 150, 150, 102, Color.white);
        NavOutline(number);

        NavRect(root, "Area/TopButtons/CurMoney", 0, 75, 780, 180);
        NavRect(root, "Area/TopButtons/CurMoney/Bg1", 0, 0, 780, 180);
        NavImage(root.transform.Find("Area/TopButtons/CurMoney/Bg1"), "secondary", true);
        NavRect(root, "Area/TopButtons/CurMoney/Bg1/MoneyIcon", -295, 0, 155, 155);
        NavRect(root, "Area/TopButtons/CurMoney/Bg1/MoneyIcon/MoneyIcon", 0, 0, 155, 155);
        NavImage(root.transform.Find("Area/TopButtons/CurMoney/Bg1/MoneyIcon/MoneyIcon"), "money", false);
        NavText(root, "Area/TopButtons/CurMoney/Bg1/CoinTxt", -22, 0, 365, 125, 73, Brown);
        NavButton(root, "Area/TopButtons/CurMoney/CoinBtn", 270, 0, 240, 155, false, 65);
        NavHide(root, "Area/TopButtons/CurMoney/CoinBtn/bg");
        NavRect(root, "Area/TopButtons/CurMoney/CoinBtn/HandPos", 30, -20, 1, 1);
        Transform setting = root.transform.Find("Area/TopButtons/Setting");
        NavRect(setting, 505, 75, 180, 180);
        // The original gear icon is retained. Its Button and visible graphic share this root.
        Image gear = setting != null ? setting.GetComponent<Image>() : null;
        if (gear != null)
        {
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath("Assets/OrchardEarningArt/CompactHud/HudControls.png"))
                if (asset is Sprite sprite && sprite.name == "HudSettings") gear.sprite = sprite;
            gear.type = Image.Type.Simple; gear.preserveAspect = true; gear.color = Color.white;
            Transform oldGlyph = setting.Find("NavGearIcon");
            if (oldGlyph != null) oldGlyph.gameObject.SetActive(false);
            gear.raycastTarget = true;
        }
        NavButtonTarget(setting);

        // Retained secondary-currency branch stays in its original source visibility policy.
        NavRect(root, "Area/TopButtons/GemMoney", 0, -90, 780, 150);
        NavRect(root, "Area/TopButtons/GemMoney/Bg1", 0, 0, 780, 150);
        NavImage(root.transform.Find("Area/TopButtons/GemMoney/Bg1"), "secondary", true);
        NavText(root, "Area/TopButtons/GemMoney/Bg1/GemTxt", -22, 0, 365, 110, 65, Brown);
        NavRect(root, "Area/TopButtons/GemMoney/Bg1/MoneyIcon", -295, 0, 140, 140);
        NavRect(root, "Area/TopButtons/GemMoney/Bg1/MoneyIcon/MoneyIcon", 0, 0, 140, 140);
        NavImage(root.transform.Find("Area/TopButtons/GemMoney/Bg1/MoneyIcon/MoneyIcon"), "diamond", false);
        NavButton(root, "Area/TopButtons/GemMoney/GemBtn", 270, 0, 240, 140, false, 61);
        NavHide(root, "Area/TopButtons/GemMoney/GemBtn/bg");

        // Source controllers keep updating these nodes. Presentation mirrors below remove duplicate
        // giant paths/circles while preserving task values, delayed fill behavior and source navigation.
        foreach (string path in new[] { "Tips", "WithdrawTop", "WithdrawPrompt", "Stage3Root" })
        {
            Transform source = top.Find(path);
            NavRect(source, 0, -90, 1120, 115);
            NavGraphicsOff(source);
        }

        var presenter = root.GetComponent<Framework2AppleNavigationPresenter>();
        if (presenter == null) presenter = root.AddComponent<Framework2AppleNavigationPresenter>();
        presenter.LevelValue = number;
        presenter.SourceStageRoot = top.Find("Stage3Root").gameObject;
        presenter.SourceProgress = NavGet<Image>(root, "Area/TopButtons/Stage3Root/BG/Progress");
        presenter.SourceProgressText = NavGet<Text>(root, "Area/TopButtons/Stage3Root/BG/ProgressTxT");
        presenter.SourcePercentText = NavGet<Text>(root, "Area/TopButtons/Stage3Root/OverallProgress/OverallCount");
        presenter.SecondaryCurrencyRoot = top.Find("GemMoney").gameObject;
        presenter.PrimaryTaskPosition = new Vector2(0, -90);
        presenter.SecondaryTaskPosition = new Vector2(0, -255);

        RectTransform early = NavNode(top, "NavEarlyTaskRoot"); NavRect(early, 0, -90, 1120, 115); NavImage(early, "secondary", true);
        presenter.EarlyTaskRoot = early.gameObject;
        NavDecor(early, "NavTrack", "track", -330, 0, 310, 70, true);
        presenter.EarlyFill = NavDecor(early, "NavFill", "fill", -330, 0, 290, 52, true);
        NavFill(presenter.EarlyFill);
        presenter.EarlyTaskText = NavNewText(early, "NavObjective", "", 150, 0, 670, 90, 49, Brown);

        RectTransform ad = NavNode(top, "NavAdTaskRoot"); NavRect(ad, 0, -90, 1120, 115); NavImage(ad, "secondary", true);
        presenter.AdTaskRoot = ad.gameObject;
        NavDecor(ad, "NavTrack", "track", -235, 0, 590, 76, true);
        presenter.AdFill = NavDecor(ad, "NavFill", "fill", -235, 0, 566, 56, true); NavFill(presenter.AdFill);
        presenter.AdCountText = NavNewText(ad, "NavCount", "", -235, 0, 530, 82, 61, Color.white); NavOutline(presenter.AdCountText);
        NavNewText(ad, "NavObjective", "Simulated checks", 320, 0, 430, 90, 46, Brown);

        RectTransform balance = NavNode(top, "NavBalanceTaskRoot"); NavRect(balance, 0, -90, 1120, 115); NavImage(balance, "secondary", true);
        presenter.BalanceTaskRoot = balance.gameObject;
        NavDecor(balance, "NavTrack", "track", -355, 0, 315, 70, true);
        presenter.BalanceFill = NavDecor(balance, "NavFill", "fill", -355, 0, 293, 50, true); NavFill(presenter.BalanceFill);
        presenter.BalanceCountText = NavNewText(balance, "NavBalanceCount", "", 105, 0, 600, 85, 45, Brown);
        presenter.BalancePercentText = NavNewText(balance, "NavPercent", "", 468, 0, 130, 85, 46, Brown);
        early.gameObject.SetActive(true); ad.gameObject.SetActive(false); balance.gameObject.SetActive(false);

        // The source Lucky branch is retained and made compact when it later unlocks.
        NavRect(root, "Area/TopButtons/LuckyRoot", 0, -225, 1120, 120);
        NavImage(root.transform.Find("Area/TopButtons/LuckyRoot"), "secondary", true);
        NavRect(root, "Area/TopButtons/LuckyRoot/bg", -400, 0, 250, 90);
        NavClearGraphic(root.transform.Find("Area/TopButtons/LuckyRoot/bg"));
        NavText(root, "Area/TopButtons/LuckyRoot/bg/Lukcy", 0, 25, 240, 40, 28, Brown);
        NavText(root, "Area/TopButtons/LuckyRoot/bg/CurCoin", 0, -17, 240, 50, 39, Brown);
        NavHide(root, "Area/TopButtons/LuckyRoot/bg/Text (Legacy) (1)");
        NavRect(root, "Area/TopButtons/LuckyRoot/BG", 130, 0, 750, 26);
        NavImage(root.transform.Find("Area/TopButtons/LuckyRoot/BG"), "track", true);
        NavRect(root, "Area/TopButtons/LuckyRoot/BG/LuckyProgress", 0, 0, 738, 18);
        NavImage(root.transform.Find("Area/TopButtons/LuckyRoot/BG/LuckyProgress"), "fill", true);
        NavFill(NavGet<Image>(root, "Area/TopButtons/LuckyRoot/BG/LuckyProgress"));
        Transform luckyStates = top.Find("LuckyRoot/LuckyRootProgressStateRoot");
        NavRect(luckyStates, 130, 0, 750, 100);
        if (luckyStates != null)
        {
            foreach (LayoutGroup layout in luckyStates.GetComponents<LayoutGroup>()) layout.enabled = false;
            for (int i = 0; i < luckyStates.childCount; i++)
            {
                var item = luckyStates.GetChild(i); NavRect(item, -330 + 110 * i, 0, 78, 78);
                foreach (Image image in item.GetComponentsInChildren<Image>(true))
                {
                    if (image.name == "Unfinished") image.sprite = SkinSprite("secondary");
                    if (image.name == "Finish") image.sprite = SkinSprite("greenbadge");
                    if (image.name == "LuckyIcon") image.sprite = SkinSprite("goldbadge");
                }
                Transform count = item.Find("Count"); NavRect(count, 0, 0, 70, 70); NavFormat(count == null ? null : count.GetComponent<Text>(), 38, Brown);
            }
        }
        var hud = root.GetComponent<MahjongSlide.Withdrawal.MahjongFramework2CurrencyHud>();
        if (hud != null)
        {
            var data = new SerializedObject(hud);
            data.FindProperty("gameplayTopPadding").floatValue = 16;
            data.FindProperty("secondaryCurrencyRowHeight").floatValue = 165;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void NavSettings(GameObject root)
    {
        NavShell(root, 1120, 1510, 0);
        NavRect(root, "Plane/Bg2", 0, 325, 970, 450); NavImage(root.transform.Find("Plane/Bg2"), "inset", true);
        NavRect(root, "Plane/Bg2/PlayerIcon", -310, 35, 265, 300);
        NavText(root, "Plane/Bg2/UserNameText", 125, 145, 520, 85, 58, Brown, TextAnchor.MiddleLeft);
        NavText(root, "Plane/Bg2/UserIDText", 125, 62, 520, 70, 39, Brown, TextAnchor.MiddleLeft);
        NavText(root, "Plane/Bg2/UserLv", 15, -12, 300, 70, 48, Brown, TextAnchor.MiddleLeft);
        NavButton(root, "Plane/Bg2/UserLevelBtn", 130, -130, 550, 140, false, 56);
        NavHide(root, "Plane/Bg2/UserLevelBtn/Text/Image");
        NavToggle(root, "Plane/Sound", "s_Toggle", "s_Icon", -30, "Sound");
        NavToggle(root, "Plane/Vibration", "v_Toggle", "v_Icon", -230, "Vibration");
        NavHide(root, "Plane/Muslc");
        NavHide(root, "Plane/WRBtn");
        NavButton(root, "Plane/Bg2/PrivacyPolicyBtn", 0, -745, 970, 135, true, 48);
        NavButton(root, "Plane/Bg2/TermsofUseBtn", 0, -905, 970, 135, true, 48);
        NavHide(root, "Plane/Bg2/PrivacyPolicyBtn/Text/Image"); NavHide(root, "Plane/Bg2/TermsofUseBtn/Text/Image");
        NavDecor(root.transform.Find("Plane/Bg2/PrivacyPolicyBtn"), "NavArrow", "arrow", 415, 0, 50, 70);
        NavDecor(root.transform.Find("Plane/Bg2/TermsofUseBtn"), "NavArrow", "arrow", 415, 0, 50, 70);
        root.transform.Find("Plane/Bg2/PrivacyPolicyBtn/NavArrow").localRotation = Quaternion.Euler(0, 0, 180);
        root.transform.Find("Plane/Bg2/TermsofUseBtn/NavArrow").localRotation = Quaternion.Euler(0, 0, 180);
        NavTitle(root, "Settings", 695);
        NavClose(root, "Plane/ExitBtn", 505, 665);
        NavFooter(root.transform.Find("Plane"), -705, 1000);
    }

    private static void NavToggle(GameObject root, string path, string toggleName, string knobName, float y, string label)
    {
        Transform row = root.transform.Find(path); NavRect(row, 0, y, 970, 180); NavImage(row, "inset", true);
        NavText(root, path + "/Text", -195, 0, 470, 120, 59, Brown, TextAnchor.MiddleLeft);
        NavLiteral(root.transform.Find(path + "/Text"), label);
        Transform toggle = root.transform.Find(path + "/" + toggleName); NavRect(toggle, 310, -36, 264, 120);
        NavRect(root, path + "/" + toggleName + "/Background", 0, 36, 264, 124);
        NavImage(root.transform.Find(path + "/" + toggleName + "/Background"), "track", true);
        NavRect(root, path + "/" + toggleName + "/Checkmark", 0, 36, 264, 124);
        NavImage(root.transform.Find(path + "/" + toggleName + "/Checkmark"), "fill", true);
        // Original runtime sets this knob to (+/-88, 36); preserve that exact socket geometry.
        Transform knob = root.transform.Find(path + "/" + toggleName + "/" + knobName); NavRect(knob, 88, 36, 100, 100);
        Image knobImage = NavImage(knob, "knob", false);
        if (knobImage != null) { knobImage.color = Color.white; knobImage.preserveAspect = true; knob.gameObject.SetActive(true); knob.SetAsLastSibling(); }
    }

    private static void NavUserLevel(GameObject root)
    {
        NavShell(root, 1120, 1400, 20);
        NavRect(root, "Plane/Bg2", 0, 185, 970, 690); NavImage(root.transform.Find("Plane/Bg2"), "inset", true);
        NavGraphicsOff(root.transform.Find("Plane/CurLevelText"));
        NavNewText(root.transform.Find("Plane"), "NavCurrentLevelLabel", "Current Level", 0, 485, 900, 100, 68, Brown);
        NavDecor(root.transform.Find("Plane"), "NavLevelBadge", "goldbadge", 0, 265, 350, 350);
        var value = NavNewText(root.transform.Find("Plane"), "NavUserLevelNumber", "", 0, 265, 280, 280, 160, Color.white); NavOutline(value);
        var presenter = root.GetComponent<Framework2AppleNavigationPresenter>(); if (presenter == null) presenter = root.AddComponent<Framework2AppleNavigationPresenter>();
        presenter.UserLevelValue = value;
        NavRect(root, "Plane/LevelProgress", -85, 35, 710, 105);
        NavRect(root, "Plane/LevelProgress/Background", 0, 0, 710, 105); NavImage(root.transform.Find("Plane/LevelProgress/Background"), "track", true);
        NavStretch(root.transform.Find("Plane/LevelProgress/Fill Area"), 14, 14, 14, 14);
        NavStretch(root.transform.Find("Plane/LevelProgress/Fill Area/Fill"), 0, 0, 0, 0); NavImage(root.transform.Find("Plane/LevelProgress/Fill Area/Fill"), "fill", true);
        var slider = NavGet<Slider>(root, "Plane/LevelProgress"); if (slider != null) { slider.targetGraphic = NavGet<Image>(root, "Plane/LevelProgress/Background"); slider.interactable = false; }
        NavText(root, "Plane/LevelProgress/LevelProgressText", 455, 0, 185, 100, 60, Brown);
        NavText(root, "Plane/Text", 0, -195, 950, 180, 61, Brown);
        NavButton(root, "Plane/KeepPlayingBtn", 0, -450, 970, 175, false, 72);
        NavTitle(root, "User Level", 650); NavClose(root, "Plane/ExitBtn", 505, 620); NavFooter(root.transform.Find("Plane"), -625, 1000);
    }

    private static void NavRestart(GameObject root)
    {
        NavShell(root, 1120, 930, 50);
        NavRect(root, "Plane/Bg2", 0, 0, 970, 650); NavImage(root.transform.Find("Plane/Bg2"), "inset", true);
        Transform plane = root.transform.Find("Plane");
        foreach (Transform child in plane) if (child.name == "Bg3") child.gameObject.SetActive(false);
        NavHide(root, "Plane/PropIcon"); NavHide(root, "Plane/PropDesc");
        NavText(root, "Plane/LevelProgress", 0, 210, 900, 185, 65, Brown);
        NavButton(root, "Plane/ReStartBtn", 0, -25, 930, 170, false, 72);
        NavButton(root, "Plane/ContinueBtn", 0, -235, 930, 155, true, 64);
        NavHide(root, "Plane/ContinueBtn/bg");
        NavTitle(root, "Restart", 445); NavClose(root, "Plane/ExitBtn", 500, 415); NavFooter(plane, -405, 1000);
    }

    private static void NavTutorial(GameObject root)
    {
        Transform guide = root.transform.Find("GuidePlane/GuideTextFather/GuideText");
        NavRect(guide, 0, 0, 800, 245);
        NavImage(root.transform.Find("GuidePlane/GuideTextFather/GuideText/Bg"), "inset", true);
        NavStretch(root.transform.Find("GuidePlane/GuideTextFather/GuideText/Bg"), 0, 0, 0, 0);
        NavText(root, "GuidePlane/GuideTextFather/GuideText/GTContent", 0, 0, 720, 200, 51, Brown);
        // Hand sprite, target follow and source spotlight shader remain unchanged.
        NavRect(root, "GuidePlane/HandFather/Hander/Hand/HandImage", -65, -50, 225, 225);
    }

    private static void NavEffects(GameObject root)
    {
        foreach (Text text in root.GetComponentsInChildren<Text>(true)) NavFormat(text, Mathf.Clamp(text.fontSize, 38, 64), Brown);
        foreach (string path in new[] { "FlyEffectParent/FlyMoney", "FlyEffectParent/FlyIAAMoney", "FlyEffectParent/FlyMoneyTip/Text/Image", "FlyEffectParent/FlyIAAMoneyTip/Text/Image" })
            NavImage(root.transform.Find(path), "money", false);
        foreach (string path in new[] { "FlyEffectParent/FlyGem", "FlyEffectParent/FlyGemTip/Text/Image" }) NavImage(root.transform.Find(path), "diamond", false);
        Text cash = NavGet<Text>(root, "FlyEffectParent/FlyMoneyTip/Text"); if (cash != null) { NavFormat(cash, 64, new Color(.12f, .75f, .05f)); NavOutline(cash); }
        NavImage(root.transform.Find("LevelTargetEffect/LTEPlane"), "panel", true);
        NavImage(root.transform.Find("LevelTargetEffect/LTEPlane/Dialog"), "inset", true);
        NavImage(root.transform.Find("LevelTargetEffect/LTEPlane/MiniDialog"), "inset", true);
        NavImage(root.transform.Find("CongratulationEffect/Bg"), "panel", true);
        NavImage(root.transform.Find("GetRewardEffect/Plane"), "inset", true);
    }

    private static void NavContentError(GameObject root)
    {
        NavRect(root.transform, 0, 0, 1110, 300); NavImage(root.transform, "inset", true);
        NavDecor(root.transform, "NavErrorIcon", "error", -410, 0, 180, 180);
        NavText(root, "Text", 100, 0, 750, 245, 53, Brown, TextAnchor.MiddleLeft);
    }

    // Called only on the B-specific copy. The original host prefab used by A remains untouched.
    private static void ApplyHost(GameObject root)
    {
        const string modal = "TestOverlay/ModalRoot/";
        NavHostPanel(root, modal + "SelectorPanel", "Choose variant", 1300);
        NavButton(root, modal + "SelectorPanel/Card/ChooseA", 0, 300, 960, 235, true, 58);
        NavButton(root, modal + "SelectorPanel/Card/ChooseB", 0, 20, 960, 235, false, 58);
        NavLiteral(root.transform.Find(modal + "SelectorPanel/Card/ChooseA/Label"), "A · Existing flow");
        NavLiteral(root.transform.Find(modal + "SelectorPanel/Card/ChooseB/Label"), "B · Framework 2");
        NavDecor(root.transform.Find(modal + "SelectorPanel/Card/ChooseB"), "NavSelected", "check", -385, 0, 100, 100);
        NavText(root, modal + "SelectorPanel/Card/CurrentVariant", 0, -240, 930, 240, 45, Brown);
        NavText(root, modal + "SelectorPanel/Card/SelectionNote", 0, -445, 930, 150, 37, Brown);
        NavClose(root, modal + "SelectorPanel/Card/Close", 500, 570);

        NavHostPanel(root, modal + "RulesPanel", "Test rules", 1630);
        NavText(root, modal + "RulesPanel/Card/RulesBody", 0, 20, 960, 1210, 48, Brown, TextAnchor.UpperLeft);
        NavButton(root, modal + "RulesPanel/Card/CloseRules", 0, -650, 970, 155, false, 60);
        NavLiteral(root.transform.Find(modal + "RulesPanel/Card/CloseRules/Label"), "Continue");

        NavHostPanel(root, modal + "AdPanel", "Local ad simulation", 1190);
        NavDecor(root.transform.Find(modal + "AdPanel/Card"), "NavVideo", "video", 0, 280, 320, 240);
        NavText(root, modal + "AdPanel/Card/AdPlacement", 0, 20, 930, 275, 49, Brown);
        NavButton(root, modal + "AdPanel/Card/CompleteAd", 0, -245, 970, 155, false, 64);
        NavButton(root, modal + "AdPanel/Card/CancelAd", 0, -425, 970, 140, true, 57);
        NavLiteral(root.transform.Find(modal + "AdPanel/Card/CompleteAd/Label"), "Complete");
        NavLiteral(root.transform.Find(modal + "AdPanel/Card/CancelAd/Label"), "Cancel");

        NavHostPanel(root, modal + "ErrorPanel", "Unable to load", 1120);
        NavDecor(root.transform.Find(modal + "ErrorPanel/Card"), "NavError", "error", 0, 265, 295, 280);
        NavText(root, modal + "ErrorPanel/Card/ErrorText", 0, -25, 950, 315, 50, Brown);
        NavButton(root, modal + "ErrorPanel/Card/ErrorSwitch", 0, -350, 970, 165, false, 60);
        NavLiteral(root.transform.Find(modal + "ErrorPanel/Card/ErrorSwitch/Label"), "Choose variant");

        Transform footer = root.transform.Find("TestOverlay/PersistentTestDisclosure");
        var footerRect = footer as RectTransform;
        if (footerRect != null)
        {
            footerRect.anchorMin = new Vector2(0, 0); footerRect.anchorMax = new Vector2(1, 0); footerRect.pivot = new Vector2(.5f, 0);
            footerRect.anchoredPosition = new Vector2(0, 18); footerRect.sizeDelta = new Vector2(-50, 95);
        }
        NavImage(footer, "secondary", true);
        NavText(root, "TestOverlay/PersistentTestDisclosure/Disclosure", -165, 0, 735, 80, 30, Brown);
        NavLiteral(root.transform.Find("TestOverlay/PersistentTestDisclosure/Disclosure"), "TEST MODE · NO CASH PAYMENT");
        NavHide(root, "TestOverlay/PersistentTestDisclosure/NoRevenue");
        NavButton(root, "TestOverlay/PersistentTestDisclosure/OpenSelector", 345, 0, 125, 75, false, 30);
        NavButton(root, "TestOverlay/PersistentTestDisclosure/Rules", 480, 0, 125, 75, true, 30);
    }

    private static void NavHostPanel(GameObject root, string path, string title, float height)
    {
        Transform mask = root.transform.Find(path); Image maskImage = mask == null ? null : mask.GetComponent<Image>();
        if (maskImage != null) { maskImage.sprite = null; maskImage.color = new Color(.02f, .03f, .07f, .6f); maskImage.raycastTarget = true; }
        Transform card = root.transform.Find(path + "/Card"); NavRect(card, 0, 25, 1120, height); NavImage(card, "panel", true);
        NavDecor(card, "NavTitleBacking", "title", 0, height / 2 - 30, 820, 175);
        NavText(root, path + "/Card/Title", 0, height / 2 - 30, 760, 135, 68, Color.white); NavLiteral(root.transform.Find(path + "/Card/Title"), title);
        root.transform.Find(path + "/Card/Title")?.SetAsLastSibling();
        NavFooter(card, -height / 2 + 65, 1000);
    }

    private static void NavShell(GameObject root, float width, float height, float y)
    {
        NavRect(root, "Plane", 0, y, width, height);
        NavRect(root, "Plane/Bg", 0, 0, width, height); NavImage(root.transform.Find("Plane/Bg"), "panel", true);
        Image mask = NavGet<Image>(root, "Mask");
        if (mask != null) { mask.sprite = null; mask.color = new Color(.02f, .03f, .07f, .6f); mask.raycastTarget = true; }
    }

    private static void NavTitle(GameObject root, string fallback, float y)
    {
        Transform plane = root.transform.Find("Plane");
        Image backing = NavDecor(plane, "NavTitleBacking", "title", 0, y, 790, 180);
        backing.transform.SetAsLastSibling();
        NavText(root, "Plane/Title", 0, y, 720, 140, 77, Color.white);
        Transform text = plane.Find("Title"); if (text != null) { text.SetAsLastSibling(); NavOutline(text.GetComponent<Text>()); }
        NavDecor(plane, "NavLeaf", "leaf", -365, y + 58, 125, 115);
    }

    private static void NavClose(GameObject root, string path, float x, float y)
    {
        Transform target = root.transform.Find(path); if (target == null) return;
        NavRect(target, x, y, 145, 145); NavImage(target, "close", false); NavButtonTarget(target); target.SetAsLastSibling();
        foreach (Text label in target.GetComponentsInChildren<Text>(true)) label.enabled = false;
    }

    private static void NavFooter(Transform parent, float y, float width) => NavNewText(parent, "NavTestFooter", "TEST MODE · NO CASH PAYMENT", 0, y, width, 60, 31, Brown);

    private static T NavGet<T>(GameObject root, string path) where T : Component
    {
        Transform t = root.transform.Find(path); return t == null ? null : t.GetComponent<T>();
    }

    private static RectTransform NavNode(Transform parent, string name)
    {
        if (parent == null) return null;
        RectTransform existing = parent.Find(name) as RectTransform;
        if (existing != null) return existing;
        var node = new GameObject(name, typeof(RectTransform)); node.transform.SetParent(parent, false); return node.GetComponent<RectTransform>();
    }

    private static void NavRect(GameObject root, string path, float x, float y, float w, float h) => NavRect(root.transform.Find(path), x, y, w, h);

    private static void NavRect(Transform target, float x, float y, float w, float h)
    {
        var rect = target as RectTransform; if (rect == null) return;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = new Vector2(x, y); rect.sizeDelta = new Vector2(w, h); rect.localScale = Vector3.one; rect.localRotation = Quaternion.identity;
    }

    private static void NavStretch(Transform target, float left, float bottom, float right, float top)
    {
        var rect = target as RectTransform; if (rect == null) return;
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.pivot = new Vector2(.5f, .5f);
        rect.offsetMin = new Vector2(left, bottom); rect.offsetMax = new Vector2(-right, -top); rect.localScale = Vector3.one;
    }

    private static Image NavImage(Transform target, string key, bool sliced)
    {
        if (target == null) return null;
        Image image = target.GetComponent<Image>(); if (image == null) image = target.gameObject.AddComponent<Image>();
        image.enabled = true; image.sprite = SkinSprite(key); image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        image.preserveAspect = !sliced; image.color = Color.white; image.raycastTarget = target.GetComponent<Selectable>() != null;
        image.pixelsPerUnitMultiplier = 1;
        return image;
    }

    private static Image NavDecor(Transform parent, string name, string key, float x, float y, float w, float h, bool sliced = false)
    {
        RectTransform rect = NavNode(parent, name); NavRect(rect, x, y, w, h); Image image = NavImage(rect, key, sliced); if (image != null) image.raycastTarget = false; return image;
    }

    private static void NavText(GameObject root, string path, float x, float y, float w, float h, int size, Color color, TextAnchor alignment = TextAnchor.MiddleCenter)
    {
        Transform target = root.transform.Find(path); if (target == null) return;
        NavRect(target, x, y, w, h); NavFormat(target.GetComponent<Text>(), size, color, alignment);
    }

    private static void NavFormat(Text text, int size, Color color, TextAnchor alignment = TextAnchor.MiddleCenter)
    {
        if (text == null) return;
        text.enabled = true; text.font = SkinFont; text.fontSize = size; text.resizeTextForBestFit = true;
        text.resizeTextMinSize = Mathf.Max(20, Mathf.RoundToInt(size * .8f)); text.resizeTextMaxSize = size;
        text.fontStyle = FontStyle.Bold; text.color = color; text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate; text.raycastTarget = false;
        foreach (Shadow shadow in text.GetComponents<Shadow>()) shadow.enabled = false;
    }

    private static Text NavNewText(Transform parent, string name, string value, float x, float y, float w, float h, int size, Color color)
    {
        RectTransform rect = NavNode(parent, name); if (rect == null) return null; NavRect(rect, x, y, w, h);
        Text text = rect.GetComponent<Text>(); if (text == null) text = rect.gameObject.AddComponent<Text>();
        text.text = value; NavFormat(text, size, color); return text;
    }

    private static void NavOutline(Text text)
    {
        if (text == null) return;
        Outline outline = text.GetComponent<Outline>(); if (outline == null) outline = text.gameObject.AddComponent<Outline>();
        outline.enabled = true; outline.effectColor = new Color(.04f, .15f, .42f, .9f); outline.effectDistance = new Vector2(2, -2);
    }

    private static void NavLiteral(Transform target, string value)
    {
        if (target == null) return;
        WZSDK.LocalizedText localized = target.GetComponent<WZSDK.LocalizedText>(); if (localized != null) Object.DestroyImmediate(localized);
        Text text = target.GetComponent<Text>(); if (text != null) text.text = value;
    }

    private static void NavButton(GameObject root, string path, float x, float y, float w, float h, bool secondary, int size)
    {
        Transform target = root.transform.Find(path); if (target == null) return;
        NavRect(target, x, y, w, h); NavImage(target, secondary ? "secondary" : "button", true); NavButtonTarget(target);
        foreach (Text text in target.GetComponentsInChildren<Text>(true))
        {
            NavRect(text.transform, 0, 0, w - 70, h - 25); NavFormat(text, size, secondary ? Brown : Color.white);
            if (!secondary) NavOutline(text);
        }
    }

    private static void NavButtonTarget(Transform target)
    {
        if (target == null) return;
        Button button = target.GetComponent<Button>(); if (button == null) return;
        button.targetGraphic = target.GetComponent<Image>();
        var colors = button.colors; colors.normalColor = colors.highlightedColor = colors.selectedColor = Color.white;
        colors.pressedColor = new Color(.82f, .89f, 1); colors.disabledColor = new Color(.7f, .74f, .78f); button.colors = colors;
    }

    private static void NavFill(Image image)
    {
        if (image == null) return;
        image.type = Image.Type.Filled; image.fillMethod = Image.FillMethod.Horizontal; image.fillOrigin = 0; image.fillAmount = 0;
    }

    private static void NavHide(GameObject root, string path)
    {
        Transform target = root.transform.Find(path); if (target != null) target.gameObject.SetActive(false);
    }

    private static void NavClearGraphic(Transform target)
    {
        if (target == null) return;
        Graphic graphic = target.GetComponent<Graphic>(); if (graphic != null) { graphic.color = Color.clear; graphic.raycastTarget = false; }
    }

    private static void NavGraphicsOff(Transform target)
    {
        if (target == null) return;
        foreach (Graphic graphic in target.GetComponentsInChildren<Graphic>(true)) graphic.enabled = false;
    }
}
