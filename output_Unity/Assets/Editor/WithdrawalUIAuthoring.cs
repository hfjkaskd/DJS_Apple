using System;
using System.Collections.Generic;
using System.IO;
using FruitWithdrawal;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>One-time authoring of saved prefabs. Never constructs UI in the player.</summary>
public static class WithdrawalUIAuthoring
{
    private const string Panels = "Assets/Resources/WithdrawalImported/Panels/";
    private static TMP_FontAsset font;
    private static Sprite buttonSprite;
    private static Sprite tileSprite;
    private static Sprite cashSprite;
    private static Sprite diamondSprite;
    private static Sprite closeSprite;
    private static GameObject root;
    private static Transform main;
    private static WithdrawalPageView view;
    private static List<WithdrawalTextBinding> texts;
    private static List<WithdrawalButtonBinding> buttons;
    private static List<WithdrawalObjectBinding> objects;

    public static void BuildAndRender() { Build(); WithdrawalUIRenderValidation.Run(); }

    [MenuItem("Tools/Withdrawal/Build Imported Flow")]
    public static void Build()
    {
        AssetDatabase.Refresh();
        var cash = AssetDatabase.LoadAssetAtPath<GameObject>(Panels + "TXPanel.prefab");
        var coin = AssetDatabase.LoadAssetAtPath<GameObject>(Panels + "TXCoinPanel.prefab");
        if (cash == null || coin == null) throw new InvalidOperationException("Run ImportNutWithdrawal.py first.");
        font = cash.GetComponentInChildren<TMP_Text>(true).font;
        closeSprite = cash.transform.Find("main/Close").GetComponent<Image>().sprite;
        buttonSprite = coin.transform.Find("main/TX").GetComponent<Image>().sprite;
        tileSprite = coin.transform.Find("main/Items/item1").GetComponent<Image>().sprite;
        cashSprite = cash.transform.Find("main/ItemIcon").GetComponent<Image>().sprite;
        cashSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WithdrawalImported/Art/Resources/atlas/golds/US1.asset");
        diamondSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WithdrawalImported/Art/Resources/atlas/items/Coin.asset");
        string[] spriteIds = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/WithdrawalImported" });
        foreach (string guid in spriteIds)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string n = System.IO.Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            if (n == "gold" || n == "icon_gold" || n == "cash") cashSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (n == "coin" || n == "icon_coin" || n == "diamond") diamondSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        BuildCash(); BuildDiamonds(); BuildChannel(); BuildConfirm();
        BuildProgress("TXProgress0Panel", WithdrawalScreen.Conditions);
        BuildProgress("TXProgress1Panel", WithdrawalScreen.Review);
        BuildProgress("TXProgress2Panel", WithdrawalScreen.Complete);
        BuildProgress("TXProgress3Panel", WithdrawalScreen.Rejected);
        BuildRecords(); BuildRewards(); BuildRules(); BuildHostAndHud();
        if (AssetDatabase.LoadAssetAtPath<WithdrawalConfig>("Assets/Resources/WithdrawalConfig.asset") == null)
        {
            var config = ScriptableObject.CreateInstance<WithdrawalConfig>();
            config.Rules = WithdrawalRules.CreateDefault();
            AssetDatabase.CreateAsset(config, "Assets/Resources/WithdrawalConfig.asset");
        }
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("[WithdrawalUI] Authored 11 screens, HUD, resource host and test configuration.");
    }

    private static void Begin(string source)
    {
        root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Panels + source + ".prefab"));
        root.name = source;
        root.SetActive(true);
        var previous = root.GetComponent<WithdrawalPageView>();
        if (previous != null) Object.DestroyImmediate(previous);
        var all = root.GetComponentsInChildren<Transform>(true);
        for (int i = all.Length - 1; i >= 0; i--) if (all[i] != null && all[i].name.StartsWith("Port_", StringComparison.Ordinal)) Object.DestroyImmediate(all[i].gameObject);
        main = root.transform.Find("main");
        if (main == null) throw new InvalidOperationException(source + " has no main content.");
        main.localScale = Vector3.one;
        ((RectTransform)main).anchoredPosition = Vector2.zero;
        foreach (TMP_Text t in root.GetComponentsInChildren<TMP_Text>(true))
        {
            t.text = ""; t.raycastTarget = false;
            t.enableAutoSizing = true; t.fontSizeMin = Mathf.Min(24, t.fontSize); t.fontSizeMax = Mathf.Max(32, t.fontSize);
        }
        foreach (Button b in root.GetComponentsInChildren<Button>(true))
        {
            b.onClick = new Button.ButtonClickedEvent();
            b.navigation = new Navigation { mode = Navigation.Mode.None };
        }
        texts = new List<WithdrawalTextBinding>(); buttons = new List<WithdrawalButtonBinding>(); objects = new List<WithdrawalObjectBinding>();
        view = root.AddComponent<WithdrawalPageView>();
        view.Content = (RectTransform)main;
        BindButton("main/Close", WithdrawalAction.Close);
        Transform close = main.Find("Close");
        if (close != null)
        {
            close.gameObject.SetActive(true);
            var closeImage = close.GetComponent<Image>();
            if (closeImage != null)
            {
                closeImage.sprite = closeSprite;
                closeImage.preserveAspect = true;
                closeImage.color = Color.white;
            }
        }
    }

    private static void End(string name)
    {
        Bounds bounds = ActiveBounds(main);
        float top = bounds.max.y;
        Image testPlate = CreateImage("Port_TestPlate", main, 0, top + 54, 990, 58, null);
        testPlate.color = new Color(.12f, .06f, .2f, .94f);
        TMP_Text test = Text("Port_Test", main, 0, top + 54, 960, 46, "TEST MODE - NO REAL PAYOUT", 32);
        test.color = new Color(1f, .91f, .55f, 1);
        BindText(test, "test");
        TMP_Text notice = Text("Port_Notice", main, 0, bounds.min.y - 50, 960, 70, "", 28);
        notice.color = new Color(.7f, .13f, .09f); BindText(notice, "notice");
        view.Texts = texts.ToArray(); view.Buttons = buttons.ToArray(); view.Objects = objects.ToArray();
        bounds = ActiveBounds(main);
        ((RectTransform)main).anchoredPosition = -new Vector2(bounds.center.x, bounds.center.y);
        view.DesignCenter = new Vector2(bounds.center.x, bounds.center.y);
        view.DesignSize = new Vector2(Mathf.Max(1080, bounds.size.x + 40), Mathf.Max(1200, bounds.size.y + 60));
        var rect = (RectTransform)root.transform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        root.name = name;
        PrefabUtility.SaveAsPrefabAsset(root, Panels + name + ".prefab");
        Object.DestroyImmediate(root);
    }

    private static void BuildCash()
    {
        Begin("TXPanel");
        Hide("main/1"); Hide("main/2"); Hide("main/PlayerInfoPos");
        Set("main/Title1/Text (TMP)", "TEST CASH WALLET");
        Bind("main/ItemCount", "balance", "$0.00");
        Bind("main/TXBtn/Text (TMP)", "apply", "Apply $0.40"); BindButton("main/TXBtn", WithdrawalAction.Apply);
        Bind("main/Progress/Text (TMP)", "ratio", "0%"); Bind("main/Progress/Tip/Value", "target", "Earn $0.40 more");
        Bind("main/Tip", "summary", "View all conditions in Rules before applying.");
        Set("main/Image/label_37", "TEST REWARDS");
        Image progress = Find<Image>("main/Progress/Bar"); if (progress != null) { progress.type = Image.Type.Filled; progress.fillMethod = Image.FillMethod.Horizontal; view.ProgressFill = progress; }
        Transform items = main.Find("Items");
        Position("main/Items", 0, 235, 100, 100);
        Position("main/Progress", 0, 105, 840, 50);
        Position("main/Tip", 0, 12, 820, 80);
        var summary = Find<TMP_Text>("main/Tip"); summary.fontSizeMax = 32;
        for (int i = 0; i < 3; i++)
        {
            Button b = NewButton("Port_Tier" + i, items, (i - 1) * 295, 0, 280, 140, i == 0 ? "$0.40" : i == 1 ? "$0.60" : "$500", WithdrawalAction.SelectTier, i, tileSprite);
            BindText(b.GetComponentInChildren<TMP_Text>(), "tier" + i);
            BindObject(b.gameObject, "tier" + i);
            TMP_Text selected = Text("Port_Selected" + i, b.transform, 0, -48, 230, 35, "SELECTED", 23);
            BindObject(selected.gameObject, "selected" + i);
            selected.gameObject.SetActive(i == 0);
        }
        AddWalletNav(); End("TXPanel");
    }

    private static void BuildDiamonds()
    {
        Begin("TXCoinPanel"); Set("main/Title/label_98", "TEST DIAMONDS");
        Set("main/value", "100 diamonds = 1 test dollar");
        Bind("main/GoldValue", "amount", "$500.00"); Bind("main/CoinValue", "balance", "0");
        Bind("main/TX/label_1", "apply", "Apply $500.00"); BindButton("main/TX", WithdrawalAction.Apply);
        Set("main/label_164", "DIAMOND TARGET");
        Bind("main/TXProgress/Value", "target", "Earn 50,000 diamonds");
        Bind("main/TXProgress/Bar/Value", "ratio", "0%");
        Bind("main/TXProgress/Tip", "summary", "All conditions are disclosed in Rules.");
        Image progress = Find<Image>("main/TXProgress/Bar"); if (progress != null) { progress.type = Image.Type.Filled; progress.fillMethod = Image.FillMethod.Horizontal; view.ProgressFill = progress; }
        for (int i = 0; i < 4; i++)
        {
            string path = "main/Items/item" + (i + 1); Bind(path + "/Value", "tier" + i, "$" + (i + 1) * 500); BindButton(path, WithdrawalAction.SelectTier, i);
            Transform item = root.transform.Find(path); BindObject(item.gameObject, "tier" + i);
            foreach (Transform child in item) if (child.name.ToLowerInvariant().Contains("select")) child.gameObject.SetActive(false);
            TMP_Text selected = Text("Port_Selected" + i, item, 0, -45, 220, 32, "SELECTED", 22); BindObject(selected.gameObject, "selected" + i); selected.gameObject.SetActive(i == 0);
        }
        AddWalletNav(); End("TXCoinPanel");
    }

    private static void AddWalletNav()
    {
        float y = ActiveBounds(main).min.y - 100;
        var a = NewButton("Port_Cash", main, -330, y, 300, 88, "Cash", WithdrawalAction.Cash);
        NewButton("Port_Diamonds", main, 0, y, 300, 88, "Diamonds", WithdrawalAction.Diamonds);
        var reward = NewButton("Port_Rewards", main, 330, y, 300, 88, "Rewards", WithdrawalAction.Rewards); BindText(reward.GetComponentInChildren<TMP_Text>(), "rewards");
        NewButton("Port_Rules", main, -330, y - 108, 300, 88, "Rules", WithdrawalAction.Rules);
        NewButton("Port_Progress", main, 0, y - 108, 300, 88, "Progress", WithdrawalAction.Progress);
        NewButton("Port_Records", main, 330, y - 108, 300, 88, "Records", WithdrawalAction.Records);
    }

    private static void BuildChannel()
    {
        Begin("TXChannelPanel"); Hide("main/InputInfo");
        Set("main/Title/label_107", "TEST CHANNEL");
        Set("main/label_154", "Choose a simulated destination.\nNo account, email or phone number is needed.");
        Position("main/label_154", 0, 350, 820, 160);
        var channelHint = Find<TMP_Text>("main/label_154"); channelHint.fontSizeMax = 38;
        Set("main/Sure/label_155", "Continue"); BindButton("main/Sure", WithdrawalAction.Confirm);
        var b0 = NewButton("Port_Channel0", main, 0, 20, 760, 150, "Test wallet", WithdrawalAction.SelectTier, 0);
        var b1 = NewButton("Port_Channel1", main, 0, -160, 760, 150, "Test bank", WithdrawalAction.SelectTier, 1);
        BindText(b0.GetComponentInChildren<TMP_Text>(), "channel0"); BindText(b1.GetComponentInChildren<TMP_Text>(), "channel1");
        BindText(Text("Port_Amount", main, 0, 205, 780, 75, "$0.40", 48), "amount");
        End("TXChannelPanel");
    }

    private static void BuildConfirm()
    {
        Begin("TXUserInfoSurePanel"); Set("main/Title/label_113", "CONFIRM TEST");
        Bind("main/Tip", "details", "SIMULATED APPLICATION\nNo payment account is collected.");
        var destination = Find<Image>("main/Image"); destination.sprite = cashSprite; destination.preserveAspect = true;
        Position("main/Image", 0, 450, 240, 200);
        Position("main/bg1", 0, 125, 900, 440);
        Position("main/Tip", 0, 125, 820, 400);
        var detail = Find<TMP_Text>("main/Tip"); if (detail != null) { detail.enableWordWrapping = true; detail.fontSizeMax = 42; }
        BindButton("main/btnParent/Get", WithdrawalAction.Confirm); Set("main/btnParent/Get/label_24", "Submit test");
        BindButton("main/btnParent/ReEnter", WithdrawalAction.Back); Set("main/btnParent/ReEnter/label_115", "Back to wallet");
        End("TXUserInfoSurePanel");
    }

    private static void BuildProgress(string name, WithdrawalScreen state)
    {
        Begin(name); Hide("main/KeFu");
        Set("main/Title/label_42", "TEST PROGRESS"); Set("main/Title/label_80", "TEST PROGRESS");
        Set("main/CurrentState/label_81", "SIMULATED APPLICATION");
        Bind("main/label_43", "amount", "$0.40"); Bind("main/GoldCount", "amount", "$0.40");
        Transform steps = main.Find("Step");
        if (steps != null) steps.gameObject.SetActive(false);
        BindText(Text("Port_Status", main, 0, 280, 840, 70, "Conditions in progress", 40), "status");
        BindText(Text("Port_Channel", main, 0, 205, 840, 50, "Test wallet", 30), "channel");
        BindText(Text("Port_Id", main, 0, 150, 840, 40, "", 26), "requestId");
        TMP_Text content = Text("Port_Conditions", main, 0, -130, 820, 450, "Complete the disclosed conditions", 36);
        content.alignment = TextAlignmentOptions.TopLeft; content.enableWordWrapping = true; BindText(content, "conditions");
        BindText(Text("Port_Step", main, 0, -395, 850, 85, "", 32), "step");
        BindButton("main/Sure", WithdrawalAction.Back); Set("main/Sure/label_50", "Back to wallet");
        float bottom = ActiveBounds(main).min.y - 105;
        NewButton("Port_Rules", main, -325, bottom, 300, 88, "Rules", WithdrawalAction.Rules);
        NewButton("Port_Records", main, 0, bottom, 300, 88, "Records", WithdrawalAction.Records);
        NewButton("Port_Cancel", main, 325, bottom, 300, 88, "Cancel test", WithdrawalAction.Cancel);
        if (state == WithdrawalScreen.Review)
            NewButton("Port_Reject", main, 0, bottom - 112, 620, 100, "Test rejection / return units", WithdrawalAction.RejectTest);
        End(name);
    }

    private static void BuildRecords()
    {
        Begin("TXRecordPanel"); Set("main/Title/label_102", "TEST RECORDS");
        Bind("main/GoldCount", "cash", "$0.00");
        BindButton("main/Modified", WithdrawalAction.Back); Set("main/Modified/label_103", "Back to wallet");
        Transform content = main.Find("Scroll View/Viewport/Content");
        Transform template = content.Find("Item");
        float height = ((RectTransform)template).rect.height;
        if (height < 150) height = 240;
        for (int i = 0; i < 8; i++)
        {
            GameObject item = i == 0 ? template.gameObject : Object.Instantiate(template.gameObject, content, false);
            if (i > 0) item.name = "Port_Record" + i;
            RectTransform rt = (RectTransform)item.transform; rt.anchorMin = rt.anchorMax = new Vector2(.5f, 1); rt.pivot = new Vector2(.5f, 1); rt.anchoredPosition = new Vector2(0, -i * (height + 20));
            BindObject(item, "record" + i);
            BindText(item.transform.Find("Count").GetComponent<TMP_Text>(), "recordAmount" + i);
            BindText(item.transform.Find("State").GetComponent<TMP_Text>(), "recordStatus" + i);
            BindText(item.transform.Find("Time").GetComponent<TMP_Text>(), "recordTime" + i);
            item.transform.Find("Tip").GetComponent<TMP_Text>().text = "TEST APPLICATION";
            Button button = item.GetComponent<Button>(); buttons.Add(new WithdrawalButtonBinding { Button = button, Action = WithdrawalAction.SelectRecord, Index = i });
            item.SetActive(false);
        }
        var rect = (RectTransform)content; rect.sizeDelta = new Vector2(rect.sizeDelta.x, 8 * (height + 20));
        BindText(Text("Port_Empty", main, 0, 350, 810, 50, "No applications yet.", 30), "empty");
        End("TXRecordPanel");
    }

    private static void BuildRewards()
    {
        Begin("RewardGetPanel");
        Set("main/Title/label", "FRUIT TEST REWARD");
        Hide("main/MoreGetBtn"); Hide("main/GetBtn");
        Image icon = CreateImage("Port_CashIcon", main, -140, 180, 180, 160, cashSprite); icon.preserveAspect = true;
        Image gem = CreateImage("Port_DiamondIcon", main, 170, 180, 160, 160, diamondSprite); gem.preserveAspect = true;
        BindText(Text("Port_CashAmount", main, 0, 20, 800, 125, "+$0.00", 84), "rewardCash");
        BindText(Text("Port_DiamondAmount", main, 0, -100, 830, 90, "+0 diamonds", 48), "rewardDiamonds");
        BindText(Text("Port_RewardDetail", main, 0, -225, 820, 130, "Match fruits to earn test rewards.", 34), "rewardDetail");
        foreach (var binding in texts) binding.Text.color = new Color(1, .96f, .75f);
        Position("main/Close", 465, 690, 95, 95);
        NewButton("Port_Claim", main, 0, -420, 780, 160, "Claim test rewards", WithdrawalAction.Claim);
        End("RewardGetPanel");
    }

    private static void BuildRules()
    {
        Begin("TXChannelPanel"); Hide("main/InputInfo"); Hide("main/label_154");
        Set("main/Title/label_107", "FULL TEST RULES");
        Transform bg = main.Find("Image"); if (bg != null) ((RectTransform)bg).sizeDelta = new Vector2(1010, 1840);
        RectTransform title = (RectTransform)main.Find("Title"); title.anchoredPosition = new Vector2(0, 790);
        RectTransform close = (RectTransform)main.Find("Close"); close.anchoredPosition = new Vector2(445, 800);
        RectTransform sure = (RectTransform)main.Find("Sure"); sure.anchoredPosition = new Vector2(0, -800);
        Set("main/Sure/label_155", "Back to wallet"); BindButton("main/Sure", WithdrawalAction.Back);
        TMP_Text text = Text("Port_RulesText", main, 0, 0, 850, 1320, "ALL CONDITIONS", 37); text.alignment = TextAlignmentOptions.TopLeft; text.enableWordWrapping = true; BindText(text, "rules");
        End("RulesPanel");
    }

    private static void BuildHostAndHud()
    {
        Directory.CreateDirectory("Assets/res/local/withdrawal");
        var go = new GameObject("WithdrawalHost", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(WithdrawalHost));
        var rect = (RectTransform)go.transform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.sizeDelta = Vector2.zero;
        var host = go.GetComponent<WithdrawalHost>(); host.PageRoot = rect;
        string[] names = { "TXPanel", "TXCoinPanel", "TXChannelPanel", "TXUserInfoSurePanel", "TXProgress0Panel", "TXProgress1Panel", "TXProgress2Panel", "TXProgress3Panel", "TXRecordPanel", "RewardGetPanel", "RulesPanel" };
        host.Screens = new WithdrawalScreenDefinition[names.Length];
        for (int i = 0; i < names.Length; i++) host.Screens[i] = new WithdrawalScreenDefinition { Screen = (WithdrawalScreen)i, ResourcePath = "WithdrawalImported/Panels/" + names[i] };
        var hostPrefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/res/local/withdrawal/WithdrawalHost.prefab"); Object.DestroyImmediate(go);
        var hudGo = new GameObject("WithdrawalHud", typeof(RectTransform), typeof(WithdrawalHud));
        var hr = (RectTransform)hudGo.transform; hr.anchorMin = new Vector2(0, 1); hr.anchorMax = Vector2.one; hr.pivot = new Vector2(.5f, 1); hr.anchoredPosition = new Vector2(0, -340); hr.sizeDelta = new Vector2(-70, 150);
        var hud = hudGo.GetComponent<WithdrawalHud>();
        // The temporary binding lists only serve page authoring; HUD binds its own standard Buttons.
        hud.CashButton = NewButton("Cash", hr, -280, -46, 390, 100, "$0.00", WithdrawalAction.Cash, 0, tileSprite);
        hud.DiamondButton = NewButton("Diamonds", hr, 165, -46, 390, 100, "0", WithdrawalAction.Diamonds, 0, tileSprite);
        hud.Cash = hud.CashButton.GetComponentInChildren<TMP_Text>(); hud.Diamonds = hud.DiamondButton.GetComponentInChildren<TMP_Text>();
        hud.Cash.rectTransform.anchoredPosition = new Vector2(40, 0); hud.Cash.rectTransform.sizeDelta = new Vector2(280, 80);
        hud.Diamonds.rectTransform.anchoredPosition = new Vector2(40, 0); hud.Diamonds.rectTransform.sizeDelta = new Vector2(280, 80);
        CreateImage("CashIcon", hud.CashButton.transform, -125, 0, 95, 88, cashSprite).preserveAspect = true;
        CreateImage("DiamondIcon", hud.DiamondButton.transform, -125, 0, 95, 88, diamondSprite).preserveAspect = true;
        hud.RewardsButton = NewButton("Rewards", hr, 420, -46, 95, 100, "+", WithdrawalAction.Rewards, 0, tileSprite);
        hud.Rewards = Text("RewardCount", hr, 235, -121, 440, 42, "Rewards", 27);
        var label = Text("TestLabel", hr, -265, -121, 400, 42, "TEST / NO CASH VALUE", 26); label.color = Color.white;
        GameObject hudPrefab = PrefabUtility.SaveAsPrefabAsset(hudGo, "Assets/res/local/withdrawal/WithdrawalHud.prefab"); Object.DestroyImmediate(hudGo);
        AttachHud("Assets/res/local/home/Home.prefab", hudPrefab); AttachHud("Assets/res/local/coreplay/CorePlayUI.prefab", hudPrefab);
        var catalog = AssetDatabase.LoadAssetAtPath<GameResCatalog>("Assets/Resources/GameResCatalog.asset");
        catalog.entries.RemoveAll(e => e.key == "res/local/withdrawal/withdrawalhost");
        catalog.entries.Add(new GameResCatalog.Entry { key = "res/local/withdrawal/withdrawalhost", asset = hostPrefab });
        EditorUtility.SetDirty(catalog);
    }

    private static void AttachHud(string path, GameObject prefab)
    {
        var instance = PrefabUtility.LoadPrefabContents(path);
        Transform prior = instance.transform.Find("WithdrawalHud"); if (prior != null) Object.DestroyImmediate(prior.gameObject);
        var child = (GameObject)PrefabUtility.InstantiatePrefab(prefab, instance.transform); child.name = "WithdrawalHud";
        if (path.Contains("/coreplay/"))
        {
            var compact = child.GetComponent<WithdrawalHud>();
            var r = (RectTransform)child.transform; r.anchorMin = r.anchorMax = new Vector2(.5f, 0); r.pivot = new Vector2(.5f, .5f);
            r.anchoredPosition = new Vector2(-465, 88); r.sizeDelta = new Vector2(120, 96);
            PrefabUtility.RecordPrefabInstancePropertyModifications(r);
            var b = (RectTransform)compact.CashButton.transform; b.anchoredPosition = Vector2.zero; b.sizeDelta = new Vector2(120, 96);
            PrefabUtility.RecordPrefabInstancePropertyModifications(b);
            var icon = (RectTransform)b.Find("CashIcon"); icon.anchoredPosition = Vector2.zero; icon.sizeDelta = new Vector2(80, 80); PrefabUtility.RecordPrefabInstancePropertyModifications(icon);
            foreach (GameObject hidden in new[] { compact.Cash.gameObject, compact.DiamondButton.gameObject, compact.RewardsButton.gameObject, compact.Rewards.gameObject })
            { hidden.SetActive(false); PrefabUtility.RecordPrefabInstancePropertyModifications(hidden); }
            var label = child.transform.Find("TestLabel").GetComponent<TMP_Text>(); label.text = "TEST"; label.rectTransform.anchoredPosition = new Vector2(0, -66); label.rectTransform.sizeDelta = new Vector2(120, 24); label.fontSizeMin = 20; label.fontSizeMax = 23;
            PrefabUtility.RecordPrefabInstancePropertyModifications(label); PrefabUtility.RecordPrefabInstancePropertyModifications(label.rectTransform);
        }
        PrefabUtility.SaveAsPrefabAsset(instance, path); PrefabUtility.UnloadPrefabContents(instance);
    }

    private static Bounds ActiveBounds(Transform parent)
    {
        var result = new Bounds(Vector3.zero, Vector3.zero); bool started = false;
        foreach (Graphic graphic in parent.GetComponentsInChildren<Graphic>(false))
        {
            if (graphic.transform == parent) continue;
            Vector3[] corners = new Vector3[4]; graphic.rectTransform.GetWorldCorners(corners);
            for (int i = 0; i < 4; i++) { Vector3 p = parent.InverseTransformPoint(corners[i]); if (!started) { result = new Bounds(p, Vector3.zero); started = true; } else result.Encapsulate(p); }
        }
        return result;
    }

    private static T Find<T>(string path) where T : Component { Transform t = root.transform.Find(path); return t != null ? t.GetComponent<T>() : null; }
    private static void Position(string path, float x, float y, float width, float height)
    {
        var rt = Find<RectTransform>(path); if (rt == null) return;
        rt.anchorMin = rt.anchorMax = new Vector2(.5f, .5f); rt.pivot = new Vector2(.5f, .5f);
        rt.anchoredPosition = new Vector2(x, y); rt.sizeDelta = new Vector2(width, height);
    }
    private static void Hide(string path) { Transform t = root.transform.Find(path); if (t != null) t.gameObject.SetActive(false); }
    private static void Set(string path, string value) { var t = Find<TMP_Text>(path); if (t != null) t.text = value; }
    private static void Bind(string path, string key, string value) { var t = Find<TMP_Text>(path); if (t != null) { t.text = value; BindText(t, key); } }
    private static void BindText(TMP_Text text, string key) { texts.Add(new WithdrawalTextBinding { Key = key, Text = text }); }
    private static void BindObject(GameObject item, string key) { objects.Add(new WithdrawalObjectBinding { Key = key, Object = item }); }
    private static void BindButton(string path, WithdrawalAction action, int index = 0) { var b = Find<Button>(path); if (b != null) buttons.Add(new WithdrawalButtonBinding { Button = b, Action = action, Index = index }); }
    private static RectTransform Rect(string name, Transform parent, float x, float y, float width, float height)
    {
        var go = new GameObject(name, typeof(RectTransform)); var rt = (RectTransform)go.transform; rt.SetParent(parent, false); rt.anchorMin = rt.anchorMax = new Vector2(.5f, .5f); rt.anchoredPosition = new Vector2(x, y); rt.sizeDelta = new Vector2(width, height); return rt;
    }
    private static Image CreateImage(string name, Transform parent, float x, float y, float width, float height, Sprite sprite)
    {
        var rt = Rect(name, parent, x, y, width, height); var image = rt.gameObject.AddComponent<Image>(); image.sprite = sprite; image.raycastTarget = false; return image;
    }
    private static TMP_Text Text(string name, Transform parent, float x, float y, float width, float height, string value, float size)
    {
        var rt = Rect(name, parent, x, y, width, height); var t = rt.gameObject.AddComponent<TextMeshProUGUI>(); t.font = font; t.text = value; t.fontSize = size; t.enableAutoSizing = true; t.fontSizeMin = Mathf.Min(size, 26); t.fontSizeMax = size; t.color = new Color(.17f, .11f, .28f); t.alignment = TextAlignmentOptions.Center; t.enableWordWrapping = true; t.raycastTarget = false; return t;
    }
    private static Button NewButton(string name, Transform parent, float x, float y, float width, float height, string label, WithdrawalAction action, int index = 0, Sprite sprite = null)
    {
        var image = CreateImage(name, parent, x, y, width, height, sprite != null ? sprite : buttonSprite); image.raycastTarget = true; var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image; button.navigation = new Navigation { mode = Navigation.Mode.None };
        Text("Label", image.transform, 0, 0, width - 24, height - 12, label, height < 110 ? 33 : 43);
        buttons.Add(new WithdrawalButtonBinding { Button = button, Action = action, Index = index }); return button;
    }
}
