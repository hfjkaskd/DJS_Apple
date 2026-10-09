using System;
using System.Collections.Generic;
using System.IO;
using FruitWithdrawal;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Creates editable, saved UI assets. All hierarchy and styling live in the prefabs in builds.</summary>
public static class WithdrawalFruitUIAuthoring
{
    private const string Panels = "Assets/Resources/WithdrawalImported/Panels/";
    private const string Art = "Assets/WithdrawalFruitArt/";
    private const string Common = "Assets/res/local/common/sprite/";
    private static readonly Color Brown = new Color32(113, 57, 27, 255);
    private static readonly Color Green = new Color32(16, 92, 26, 255);
    private static readonly Color Peach = new Color32(255, 237, 205, 255);
    private static TMP_FontAsset font;
    private static Material titleMaterial;
    private static Sprite panel, green, orange, cash, gem, check, hourglass, bank;
    private static GameObject root;
    private static RectTransform main;
    private static WithdrawalPageView view;
    private static readonly List<WithdrawalTextBinding> texts = new List<WithdrawalTextBinding>();
    private static readonly List<WithdrawalButtonBinding> buttons = new List<WithdrawalButtonBinding>();
    private static readonly List<WithdrawalObjectBinding> objects = new List<WithdrawalObjectBinding>();
    private static readonly List<WithdrawalProgressBinding> progress = new List<WithdrawalProgressBinding>();

    public static void BuildAndRender() { Build(); WithdrawalUIRenderValidation.Run(); }

    [MenuItem("Tools/Withdrawal/Build Fruit Style Interfaces")]
    public static void Build()
    {
        AssetDatabase.Refresh();
        Directory.CreateDirectory(Art);
        font = Load<TMP_FontAsset>("Assets/res/local/textmeshpro/resources/fonts & materials/802-CAI978 SDF.asset");
        panel = Load<Sprite>(Common + "pop/PopBg.asset");
        green = Load<Sprite>(Common + "btn/Btn_Normal.asset");
        orange = Load<Sprite>(Common + "btn/Btn_Orange.asset");
        cash = ImportIcon("CashStack.png");
        gem = ImportIcon("Diamond.png");
        bank = ImportIcon("Bank.png");
        check = Load<Sprite>("Assets/WithdrawalImported/Art/Sprite/done.asset");
        hourglass = ImportIcon("Hourglass.png");
        titleMaterial = AssetDatabase.LoadAssetAtPath<Material>(Art + "FruitTitle.mat");
        if (titleMaterial == null) { titleMaterial = new Material(font.material); AssetDatabase.CreateAsset(titleMaterial, Art + "FruitTitle.mat"); }
        titleMaterial.SetColor("_OutlineColor", new Color32(100, 43, 12, 255));
        titleMaterial.SetFloat("_OutlineWidth", .18f);
        titleMaterial.SetFloat("_FaceDilate", .03f);
        EditorUtility.SetDirty(titleMaterial);
        Wallet(false); Wallet(true); Rewards(); Channel(); Confirm(); Rules();
        Conditions(); Review(); Result(true); Result(false); Records();
        RestyleExistingHud();
        AssetDatabase.SaveAssets();
        Debug.Log("[WithdrawalFruitUI] Saved 11 fruit-style pages and restyled the existing gameplay entry.");
    }

    private static T Load<T>(string path) where T : Object
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null) throw new InvalidOperationException("Missing fruit UI asset: " + path);
        return asset;
    }

    private static Sprite ImportIcon(string name)
    {
        string path = Art + name;
        if (!File.Exists(path)) throw new InvalidOperationException("Missing fruit UI art: " + path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 512;
        importer.textureCompression = TextureImporterCompression.Compressed;
        importer.SaveAndReimport();
        return Load<Sprite>(path);
    }

    private static void Begin(string name, string title)
    {
        texts.Clear(); buttons.Clear(); objects.Clear(); progress.Clear();
        root = new GameObject(name, typeof(RectTransform), typeof(WithdrawalPageView));
        var rect = (RectTransform)root.transform;
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.sizeDelta = Vector2.zero;
        view = root.GetComponent<WithdrawalPageView>();
        var shade = Pic("ModalShade", rect, 0, 0, 1080, 1920, null);
        shade.color = new Color(.06f, .13f, .17f, .58f); shade.raycastTarget = true;
        shade.rectTransform.anchorMin = Vector2.zero; shade.rectTransform.anchorMax = Vector2.one; shade.rectTransform.sizeDelta = Vector2.zero;
        shade.gameObject.AddComponent<Orange.ProtectedAreaAdaptReverse>();
        main = Rect("Content", rect, 0, 0, 1000, 1500);
        view.Content = main; view.DesignSize = new Vector2(1080, 1680); view.DesignCenter = Vector2.zero;
        Slice(Pic("Panel", main, 0, 0, 1000, 1500, panel), 3.6f);
        var titlePlate = Pic("TitlePlate", main, 0, 727, 760, 150, orange); Slice(titlePlate, 2.3f);
        var label = Label("Title", titlePlate.transform, 0, 3, 700, 106, title, 57);
        label.color = Color.white; label.fontSharedMaterial = titleMaterial;
        Button close = Button("Close", main, 437, 718, 112, 112, "X", WithdrawalAction.Close, orange);
        close.GetComponentInChildren<TMP_Text>().fontSizeMax = 62;
        Bound("test", main, 0, 632, 840, 45, "TEST MODE - NO CASH VALUE", 28);
        Bound("notice", main, 0, -727, 870, 38, "", 25);
    }

    private static void End()
    {
        view.Texts = texts.ToArray(); view.Buttons = buttons.ToArray(); view.Objects = objects.ToArray(); view.Progress = progress.ToArray();
        PrefabUtility.SaveAsPrefabAsset(root, Panels + root.name + ".prefab");
        Object.DestroyImmediate(root);
    }

    private static void Wallet(bool diamonds)
    {
        Begin(diamonds ? "TXCoinPanel" : "TXPanel", diamonds ? "DIAMOND WALLET" : "CASH WALLET");
        Button("CashTab", main, -215, 547, 420, 110, "Cash", WithdrawalAction.Cash, diamonds ? null : green);
        Button("DiamondTab", main, 215, 547, 420, 110, "Diamonds", WithdrawalAction.Diamonds, diamonds ? green : null);
        Icon("Currency", main, -292, 366, 202, diamonds ? gem : cash);
        var balance = Bound("balance", main, 107, 383, 595, 135, diamonds ? "50,600" : "$500.00", 94); balance.color = diamonds ? Brown : Green;
        Label("BalanceCaption", main, 110, 290, 595, 52, diamonds ? "100 diamonds = 1 test dollar" : "Available balance", 31);
        Card("FrozenCard", main, 0, 214, 850, 70);
        Bound("frozen", main, 0, 214, 808, 52, "In application: $0.00", 30);
        if (!diamonds)
        {
            for (int i = 0; i < 3; i++) TierTile(i, (i - 1) * 288, 46, 272, 228, false);
        }
        else for (int i = 0; i < 4; i++) TierTile(i, (i % 2 == 0 ? -215 : 215), 76 - (i / 2) * 192, 414, 175, true);
        view.ProgressFill = Bar("BalanceProgress", main, 0, diamonds ? -261 : -180, 842, 42);
        Bound("target", main, 0, diamonds ? -321 : -249, 850, 56, "Balance target reached", 34).color = Green;
        Bound("summary", main, 0, -405, 830, 96, "Review all conditions before applying.", 29);
        var apply = Button("Apply", main, 0, -534, 855, 140, "CHOOSE CHANNEL", WithdrawalAction.Apply, green);
        Bind(apply.GetComponentInChildren<TMP_Text>(), "apply");
        Button("Rules", main, -291, -661, 266, 94, "Rules", WithdrawalAction.Rules);
        var rewards = Button("Rewards", main, 0, -661, 290, 94, "Rewards", WithdrawalAction.Rewards);
        Bind(rewards.GetComponentInChildren<TMP_Text>(), "rewards");
        Button("Records", main, 291, -661, 266, 94, "Records", WithdrawalAction.Records);
        End();
    }

    private static void TierTile(int i, float x, float y, float w, float h, bool diamonds)
    {
        var button = Button("Tier" + i, main, x, y, w, h, "", WithdrawalAction.SelectTier, null, i);
        Obj(button.gameObject, "tier" + i);
        Object.DestroyImmediate(button.GetComponentInChildren<TMP_Text>().gameObject);
        var selected = Rect("Selection", button.transform, 0, 0, w, h);
        var rim = Pic("GreenBorder", selected, 0, 0, w, h, green); Slice(rim, 4);
        Card("Inner", selected, 0, 0, w - 12, h - 12).color = new Color32(244, 255, 220, 255);
        Obj(selected.gameObject, "selected" + i); selected.gameObject.SetActive(i == 0);
        var amount = Bound("tier" + i, button.transform, 0, diamonds ? 35 : 56, w - 32, 66, i == 0 ? "$0.40" : "$500.00", diamonds ? 42 : 45);
        amount.color = Green;
        Icon("CurrencyIcon", button.transform, diamonds ? -134 : 0, -43, diamonds ? 64 : 114, diamonds ? gem : cash);
        if (diamonds) Bound("tierCost" + i, button.transform, 38, -43, 250, 54, "50,000", 37);
        var badge = Icon("SelectedBadge", selected, w / 2 - 13, h / 2 - 5, 69, check);
        badge.transform.SetAsLastSibling();
    }

    private static void Rewards()
    {
        Begin("RewardGetPanel", "HARVEST REWARD");
        Icon("CashReward", main, -103, 365, 310, cash);
        Icon("DiamondReward", main, 238, 355, 186, gem);
        Icon("StarLeft", main, -340, 416, 70, Load<Sprite>("Assets/res/local/pops/rate/sprite/Star.asset"));
        Icon("StarRight", main, 326, 507, 55, Load<Sprite>("Assets/res/local/pops/rate/sprite/Star.asset"));
        Bound("rewardCash", main, 0, 123, 850, 147, "+$1.30", 100).color = Green;
        Bound("rewardDiamonds", main, 0, 7, 850, 84, "+1,300 diamonds", 56);
        Bound("rewardDetail", main, 0, -102, 835, 100, "Fruit matches + level clear", 33);
        Card("AfterClaim", main, 0, -275, 845, 204);
        Label("CashAfterLabel", main, -207, -234, 378, 47, "Cash after claim", 30);
        Label("DiamondAfterLabel", main, 207, -234, 400, 47, "Diamonds after claim", 30);
        Bound("afterCash", main, -207, -305, 385, 77, "$500.00", 53).color = Green;
        Bound("afterDiamonds", main, 207, -305, 395, 77, "50,600", 53);
        Button("Claim", main, 0, -507, 850, 145, "CLAIM REWARDS", WithdrawalAction.Claim, green);
        Button("BackToGame", main, 0, -654, 804, 102, "Back to game", WithdrawalAction.Close);
        End();
    }

    private static void AmountCard(float y = 478)
    {
        Card("AmountCard", main, 0, y, 850, 215);
        Icon("CashIcon", main, -279, y, 200, cash);
        Bound("amount", main, 107, y + 3, 572, 136, "$500.00", 87).color = Green;
    }

    private static void Channel()
    {
        Begin("TXChannelPanel", "CHOOSE CHANNEL"); AmountCard();
        for (int i = 0; i < 2; i++)
        {
            float y = 142 - i * 260;
            var button = Button("Channel" + i, main, 0, y, 850, 229, "", WithdrawalAction.SelectTier, null, i);
            Object.DestroyImmediate(button.GetComponentInChildren<TMP_Text>().gameObject);
            var rim = Pic("Selected", button.transform, 0, 0, 850, 229, green); Slice(rim, 4);
            Card("Inside", rim.transform, 0, 0, 838, 217).color = new Color32(255, 250, 232, 255);
            Obj(rim.gameObject, "channelSelected" + i);
            Icon("DestinationIcon", button.transform, -300, 0, 156, i == 0 ? cash : bank);
            Bound("channel" + i, button.transform, 88, 39, 575, 65, i == 0 ? "Test wallet" : "Test bank", 45);
            Label("DestinationNote", button.transform, 88, -48, 575, 83, "No account details required", 31);
            Icon("SelectedCheck", rim.transform, 383, 93, 76, check);
        }
        Label("Hint", main, 0, -320, 835, 100, "Choose a simulated destination.\nNo payment account is connected.", 32);
        Button("Continue", main, 0, -511, 850, 140, "CONTINUE", WithdrawalAction.Confirm, green);
        Button("Back", main, 0, -654, 804, 102, "Back to wallet", WithdrawalAction.Back);
        End();
    }

    private static void Confirm()
    {
        Begin("TXUserInfoSurePanel", "CONFIRM REQUEST"); AmountCard();
        Card("Details", main, 0, 129, 850, 357);
        Pair("ChannelLabel", "Channel", "channel", 243, "Test wallet");
        Pair("ReserveLabel", "Reserve amount", "reserve", 127, "$500.00");
        Pair("AfterLabel", "Available after submit", "afterBalance", 11, "$0.00");
        Card("Hint", main, 0, -175, 850, 112);
        Bound("hint", main, 0, -175, 785, 90, "Your test balance will be reserved.", 31);
        Button("Conditions", main, 0, -331, 804, 104, "View all conditions", WithdrawalAction.Rules);
        Button("Submit", main, 0, -511, 850, 140, "SUBMIT TEST", WithdrawalAction.Confirm, green);
        Button("Back", main, 0, -654, 804, 102, "Back", WithdrawalAction.Back);
        End();
    }

    private static void Pair(string name, string caption, string key, float y, string value)
    {
        Label(name, main, -188, y, 414, 87, caption, 31).alignment = TextAlignmentOptions.MidlineLeft;
        Bound(key, main, 226, y, 346, 87, value, 38).alignment = TextAlignmentOptions.MidlineRight;
    }

    private static void Rules()
    {
        Begin("RulesPanel", "WITHDRAWAL RULES");
        Card("AmountCard", main, 0, 521, 850, 147);
        Icon("CashIcon", main, -284, 521, 140, cash);
        Bound("amount", main, 95, 521, 560, 109, "$500.00", 74).color = Green;
        for (int i = 0; i < 8; i++)
        {
            var card = Card("Rule" + i, main, 0, 376 - i * 98, 850, 88);
            Obj(card.gameObject, "rule" + i);
            Label("Number", card.transform, -375, 0, 62, 60, (i + 1).ToString(), 32).color = Green;
            Bound("rules" + i, card.transform, 26, 0, 727, 78, "Rule condition", 31).alignment = TextAlignmentOptions.MidlineLeft;
        }
        Bound("ruleFootnote", main, 0, -446, 820, 137, "One active test. Cancelling returns reserved units.\nAll rewards and results have no cash value.", 28);
        Button("Back", main, 0, -623, 850, 140, "BACK", WithdrawalAction.Back, green);
        End();
    }

    private static void Steps(int active)
    {
        var line = Pic("JourneyLine", main, 0, 369, 616, 13, null); line.color = new Color32(189, 184, 169, 255);
        string[] names = { "Conditions", "Review", "Result" };
        for (int i = 0; i < 3; i++)
        {
            var icon = Pic("Step" + i, main, (i - 1) * 303, 369, 86, 86, Load<Sprite>(Common + (i <= active ? "btn/MidBtn_Green.asset" : "btn/MidBtn_Gray.asset")));
            Slice(icon, 2.2f);
            if (i < active) Icon("Complete", icon.transform, 0, 2, 53, check);
            else { var number = Label("Number", icon.transform, 0, 4, 74, 70, (i + 1).ToString(), 47); number.color = Color.white; number.fontSharedMaterial = titleMaterial; }
            Label("StepLabel" + i, main, (i - 1) * 303, 297, 270, 54, names[i], 31);
        }
    }

    private static void RequestHeader(string title, string name)
    {
        Begin(name, title);
        Card("AmountCard", main, 0, 524, 850, 150);
        Icon("CashIcon", main, -297, 524, 139, cash);
        Bound("amount", main, 72, 524, 580, 114, "$500.00", 76).color = Green;
    }

    private static void Conditions()
    {
        RequestHeader("REQUEST PROGRESS", "TXProgress0Panel"); Steps(0);
        for (int i = 0; i < 7; i++)
        {
            var card = Card("Condition" + i, main, 0, 209 - i * 88, 850, 80);
            Obj(card.gameObject, "condition" + i);
            Bound("cond" + i, card.transform, -28, 13, 745, 43, "Complete fruit levels: 0 / 10", 28).alignment = TextAlignmentOptions.MidlineLeft;
            Image fill = Bar("Progress", card.transform, -47, -23, 729, 17);
            progress.Add(new WithdrawalProgressBinding { Key = "condition" + i, Fill = fill });
            Obj(Icon("Done", card.transform, 374, 0, 48, check).gameObject, "done" + i);
        }
        Bound("step", main, 0, -402, 820, 78, "Previous progress counts toward this request.", 30);
        Button("KeepPlaying", main, 0, -521, 850, 139, "KEEP PLAYING", WithdrawalAction.Close, green);
        Button("Rules", main, -218, -654, 413, 98, "Rules", WithdrawalAction.Rules);
        Button("Cancel", main, 218, -654, 413, 98, "Cancel request", WithdrawalAction.Cancel);
        End();
    }

    private static void Review()
    {
        RequestHeader("SIMULATED REVIEW", "TXProgress1Panel"); Steps(1);
        Card("ReviewCard", main, 0, 13, 850, 445);
        Icon("Hourglass", main, 0, 100, 263, hourglass);
        Bound("countdown", main, 0, -122, 800, 130, "00:42", 100);
        Bound("reviewHint", main, 0, -309, 810, 137, "All conditions completed.\nReview completes automatically.", 35);
        Button("Game", main, 0, -511, 850, 139, "BACK TO GAME", WithdrawalAction.Close, green);
        Button("Records", main, -288, -654, 268, 98, "Records", WithdrawalAction.Records);
        Button("Cancel", main, 0, -654, 286, 98, "Cancel request", WithdrawalAction.Cancel);
        Button("Reject", main, 288, -654, 268, 98, "Test reject", WithdrawalAction.RejectTest);
        End();
    }

    private static void Result(bool completed)
    {
        Begin(completed ? "TXProgress2Panel" : "TXProgress3Panel", completed ? "TEST COMPLETE" : "UNITS RETURNED");
        Icon("Cash", main, -20, 408, 293, cash);
        if (completed) Icon("Complete", main, 143, 338, 139, check);
        else Icon("Return", main, 182, 357, 148, Load<Sprite>("Assets/res/local/coreplay/sprite/item/Undo_Normal.asset"));
        Bound("amount", main, 0, 188, 850, 139, "$500.00", 99).color = Green;
        Card("ResultCard", main, 0, -22, 850, 228);
        Bound("resultTitle", main, 0, 34, 810, 77, completed ? "Simulation completed" : "Test request closed", 46);
        Bound("resultDetail", main, 0, -64, 804, 99, completed ? "No real money was sent." : "Reserved units returned to wallet.", 32);
        if (completed)
        {
            Label("Saved", main, 0, -197, 820, 68, "Result saved in Records", 32);
            for (int i = 0; i < 3; i++) { Icon("StepDone" + i, main, (i - 1) * 280, -310, 92, check); Label("StepLabel" + i, main, (i - 1) * 280, -388, 265, 47, i == 0 ? "Conditions" : i == 1 ? "Review" : "Result", 30); }
        }
        else
        {
            Card("BalanceCard", main, 0, -243, 850, 104);
            Pair("BalanceLabel", "Wallet balance", "afterBalance", -243, "$500.00");
            Label("RetryHint", main, 0, -382, 800, 88, "You can try another test request.", 32);
        }
        Button("Primary", main, 0, -525, 850, 140, completed ? "VIEW RECORDS" : "TRY ANOTHER REQUEST", completed ? WithdrawalAction.Records : WithdrawalAction.Back, green);
        Button("Secondary", main, 0, -664, 804, 98, completed ? "Back to wallet" : "Records", completed ? WithdrawalAction.Back : WithdrawalAction.Records);
        End();
    }

    private static void Records()
    {
        Begin("TXRecordPanel", "REQUEST RECORDS");
        Card("RecordsHeader", main, 0, 502, 850, 165);
        Icon("RecordsCash", main, -307, 504, 147, cash);
        Label("Header", main, 72, 534, 567, 64, "Test applications", 44);
        Bound("empty", main, 79, 469, 571, 72, "Your recent test requests", 28);
        var viewport = Rect("Viewport", main, 0, -34, 870, 842);
        var catcher = viewport.gameObject.AddComponent<Image>(); catcher.color = new Color(1, 1, 1, .001f);
        viewport.gameObject.AddComponent<RectMask2D>();
        var content = Rect("RecordsContent", viewport, 0, 0, 850, 1648);
        content.anchorMin = content.anchorMax = new Vector2(.5f, 1); content.pivot = new Vector2(.5f, 1);
        var scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 60; scroll.inertia = true;
        for (int i = 0; i < 8; i++)
        {
            var button = Button("Record" + i, content, 0, -i * 205 - 95, 850, 185, "", WithdrawalAction.SelectRecord, null, i);
            var rt = (RectTransform)button.transform; rt.anchorMin = rt.anchorMax = new Vector2(.5f, 1);
            Object.DestroyImmediate(button.GetComponentInChildren<TMP_Text>().gameObject);
            Obj(button.gameObject, "record" + i);
            Bound("recordTime" + i, button.transform, -53, 52, 724, 42, "2026-10-09 12:30 UTC", 26).alignment = TextAlignmentOptions.MidlineLeft;
            Bound("recordAmount" + i, button.transform, -249, -23, 315, 84, "$500.00", 45).color = Green;
            Bound("recordStatus" + i, button.transform, 140, -23, 420, 99, "Test complete\nNo payment", 28).alignment = TextAlignmentOptions.MidlineLeft;
            Label("Chevron", button.transform, 386, 0, 49, 77, ">", 47);
        }
        Label("ScrollHint", main, 0, -487, 800, 45, "Scroll for more - tap a request for details", 26);
        Button("Back", main, 0, -619, 850, 144, "BACK TO WALLET", WithdrawalAction.Back, green);
        End();
    }

    private static void RestyleExistingHud()
    {
        // Keep nested prefab IDs and game placement intact; Home has no withdrawal entry.
        string path = "Assets/res/local/withdrawal/WithdrawalHud.prefab";
        var instance = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var hud = instance.GetComponent<WithdrawalHud>();
            Image button = hud.CashButton.GetComponent<Image>(); button.sprite = green; button.color = Color.white; Slice(button, 3);
            Image icon = hud.CashButton.transform.Find("CashIcon").GetComponent<Image>(); icon.sprite = cash; icon.preserveAspect = true;
            TMP_Text label = instance.transform.Find("TestLabel").GetComponent<TMP_Text>(); label.font = font; label.fontSharedMaterial = titleMaterial; label.color = Color.white;
            PrefabUtility.SaveAsPrefabAsset(instance, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(instance); }
    }

    private static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
    {
        var go = new GameObject(name, typeof(RectTransform)); go.layer = 5;
        var r = (RectTransform)go.transform; r.SetParent(parent, false); r.anchorMin = r.anchorMax = new Vector2(.5f, .5f); r.sizeDelta = new Vector2(w, h); r.anchoredPosition = new Vector2(x, y); return r;
    }
    private static Image Pic(string name, Transform parent, float x, float y, float w, float h, Sprite sprite)
    {
        var r = Rect(name, parent, x, y, w, h); var image = r.gameObject.AddComponent<Image>(); image.sprite = sprite; image.raycastTarget = false; return image;
    }
    private static Image Icon(string name, Transform parent, float x, float y, float size, Sprite sprite)
    { var image = Pic(name, parent, x, y, size, size, sprite); image.preserveAspect = true; return image; }
    private static void Slice(Image image, float multiplier) { image.type = Image.Type.Sliced; image.pixelsPerUnitMultiplier = multiplier; }
    private static Image Card(string name, Transform parent, float x, float y, float w, float h)
    { var image = Pic(name, parent, x, y, w, h, panel); Slice(image, 8); image.color = Peach; return image; }
    private static TMP_Text Label(string name, Transform parent, float x, float y, float w, float h, string value, float size)
    {
        var r = Rect(name, parent, x, y, w, h); var t = r.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = font; t.text = value; t.fontSize = size; t.enableAutoSizing = true; t.fontSizeMin = size * .74f; t.fontSizeMax = size;
        t.color = Brown; t.alignment = TextAlignmentOptions.Center; t.enableWordWrapping = true; t.raycastTarget = false; return t;
    }
    private static TMP_Text Bound(string key, Transform parent, float x, float y, float w, float h, string value, float size)
    { var label = Label(key, parent, x, y, w, h, value, size); Bind(label, key); return label; }
    private static void Bind(TMP_Text text, string key) { texts.Add(new WithdrawalTextBinding { Key = key, Text = text }); }
    private static void Obj(GameObject item, string key) { objects.Add(new WithdrawalObjectBinding { Key = key, Object = item }); }
    private static Button Button(string name, Transform parent, float x, float y, float w, float h, string caption, WithdrawalAction action, Sprite sprite = null, int index = 0)
    {
        var image = sprite == null ? Card(name, parent, x, y, w, h) : Pic(name, parent, x, y, w, h, sprite);
        if (sprite != null) Slice(image, 2.5f);
        image.raycastTarget = true; var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        var colors = button.colors; colors.pressedColor = new Color(.84f, .9f, .78f); colors.disabledColor = new Color(.70f, .72f, .62f, .8f); button.colors = colors;
        var label = Label("Label", image.transform, 0, 4, w - 38, h - 18, caption, h > 120 ? 50 : 37);
        if (sprite != null) { label.color = Color.white; label.fontSharedMaterial = titleMaterial; }
        buttons.Add(new WithdrawalButtonBinding { Button = button, Action = action, Index = index }); return button;
    }
    private static Image Bar(string name, Transform parent, float x, float y, float w, float h)
    {
        Image bg = Pic(name, parent, x, y, w, h, Load<Sprite>(Common + "ProgressBg.asset")); Slice(bg, 3);
        var track = Rect("FillTrack", bg.transform, 0, 0, w - 8, h - 8);
        Image fill = Pic("Fill", track, 0, 0, 0, 0, Load<Sprite>(Common + "Filler.asset"));
        Slice(fill, 3);
        fill.rectTransform.anchorMin = Vector2.zero; fill.rectTransform.anchorMax = new Vector2(.6f, 1);
        fill.rectTransform.offsetMin = fill.rectTransform.offsetMax = Vector2.zero;
        return fill;
    }
}
