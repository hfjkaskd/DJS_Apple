using System;
using System.IO;
using CorePlay;
using FruitApkEarning;
using FruitEarningVariants;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>One-time authoring of the saved test host and existing gameplay bindings.</summary>
public static class EarningVariantAuthoring
{
    private const string ControllerPath = "Assets/Resources/EarningVariants/EarningVariantController.prefab";
    private const string GameplayPath = "Assets/res/local/coreplay/CorePlayUI.prefab";
    private const string FontPath = "Assets/Framework2Replica/Resources/Framework2Replica/External/Assets/Font/Roboto-Regular.ttf";
    private static readonly Color Ink = new Color32(42, 48, 58, 255);
    private static readonly Color Paper = new Color32(255, 250, 233, 255);
    private static readonly Color Blue = new Color32(24, 100, 170, 255);
    private static readonly Color Green = new Color32(33, 128, 77, 255);
    private static Font font;

    [MenuItem("Tools/Earning Variants/Prepare A-B Test Prefabs")]
    public static void Prepare()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer)
            throw new InvalidOperationException("Stop Play Mode and active builds before authoring test prefabs.");
        Framework2ReplicaAssetPreparation.Prepare();
        font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        if (font == null) throw new InvalidOperationException("The imported original Roboto font is missing.");
        EnsureFolder("Assets/Resources/EarningVariants");
        CreateController();
        BindGameplay();
        AssetDatabase.SaveAssets();
        Debug.Log("EARNING_VARIANT_AUTHORING_PASS controller=" + ControllerPath + " gameplay=" + GameplayPath);
    }

    private static void CreateController()
    {
        var root = new GameObject("EarningVariantController");
        try
        {
            var controller = root.AddComponent<EarningVariantController>();
            controller.RuntimeParent = new GameObject("RuntimeParent").transform;
            controller.RuntimeParent.SetParent(root.transform, false);

            var overlay = new GameObject("TestOverlay", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            overlay.transform.SetParent(root.transform, false);
            var canvas = overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 30000;
            var scaler = overlay.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f;
            var modalRoot = Rect("ModalRoot", overlay.transform);
            Stretch(modalRoot);

            RectTransform selector = Modal("SelectorPanel", modalRoot, 940, 720, "Choose test variant", out GameObject selectorPanel);
            controller.SelectorPanel = selectorPanel;
            controller.CurrentVariantText = Label("CurrentVariant", selector, 50, 142, 840, 166,
                "Active: A / Original\nSeparate progress and balances. Switching reloads the saved game.", 33);
            controller.ChooseAButton = Action("ChooseA", selector, 55, 342, 830, 100, "A / ORIGINAL", Blue);
            controller.ChooseBButton = Action("ChooseB", selector, 55, 470, 830, 100, "B / FRAMEWORK 2", Green);
            Label("SelectionNote", selector, 55, 602, 830, 58, "Both versions remain installed. No payment service is connected.", 25);
            controller.CloseSelectorButton = Close(selector);

            RectTransform rules = Modal("RulesPanel", modalRoot, 960, 1200, "Simulation rules", out GameObject rulesPanel);
            controller.RulesPanel = rulesPanel;
            Label("RulesBody", rules, 58, 140, 844, 840,
                "This is a complete local test simulation.\nNo real cash payment, ad delivery or ad revenue is available.\n\n" +
                "B / Framework 2 follows the original requirements:\n" +
                "1. Complete level 1.\n2. Complete level 2.\n3. Complete 3 simulated ad tasks.\n" +
                "4. Reach a balance of 200.\n5. Reach a balance of 3,000.\n\n" +
                "The normal balance is capped at 2,999.5. Normal rewards cannot reach the final 3,000 target.\n\n" +
                "Use fictional account details only. All orders and progress are stored locally on this device. Nothing is submitted to a payment provider.", 31);
            controller.CloseRulesButton = Action("CloseRules", rules, 70, 1035, 820, 95, "I UNDERSTAND / CONTINUE TEST", Blue);

            RectTransform ad = Modal("AdPanel", modalRoot, 940, 790, "Local ad simulation", out GameObject adPanel);
            controller.AdPanel = adPanel;
            controller.AdPlacementText = Label("AdPlacement", ad, 58, 154, 824, 280,
                "LOCAL AD SIMULATION\nTest placement\nNo ad is requested, no revenue is generated.", 32);
            controller.CompleteAdButton = Action("CompleteAd", ad, 60, 490, 820, 100, "SIMULATE COMPLETION", Green);
            controller.CancelAdButton = Action("CancelAd", ad, 60, 620, 820, 92, "CANCEL", Blue);

            RectTransform error = Modal("ErrorPanel", modalRoot, 940, 850, "Test could not load", out GameObject errorPanel);
            controller.ErrorPanel = errorPanel;
            controller.ErrorText = Label("ErrorText", error, 60, 150, 820, 480,
                "The selected test could not initialize.\nYour A and B saves have been retained.", 30);
            controller.ErrorText.resizeTextForBestFit = true;
            controller.ErrorText.resizeTextMinSize = 20;
            controller.ErrorText.resizeTextMaxSize = 30;
            controller.ErrorSwitchButton = Action("ErrorSwitch", error, 60, 675, 820, 100, "CHANGE VARIANT", Blue);

            // Last sibling: the disclosure stays visible above every test panel.
            var disclosure = Rect("PersistentTestDisclosure", overlay.transform);
            disclosure.anchorMin = new Vector2(0, 0);
            disclosure.anchorMax = new Vector2(1, 0);
            disclosure.pivot = new Vector2(0.5f, 0);
            disclosure.anchoredPosition = Vector2.zero;
            disclosure.sizeDelta = new Vector2(0, 112);
            var disclosureImage = disclosure.gameObject.AddComponent<Image>();
            disclosureImage.color = new Color32(15, 34, 53, 250);
            Label("Disclosure", disclosure, 22, 12, 770, 45, "TEST SIMULATION · NO CASH PAYMENT", 31, new Color32(255, 226, 113, 255));
            Label("NoRevenue", disclosure, 22, 61, 770, 30, "Local test data only. No ad revenue. A and B saves are separate.", 21, Color.white);
            controller.OpenSelectorButton = Action("OpenSelector", disclosure, 800, 20, 112, 73, "A / B", Blue, 29);
            controller.RulesButton = Action("Rules", disclosure, 928, 20, 132, 73, "Rules", Blue, 28);

            selectorPanel.SetActive(false);
            rulesPanel.SetActive(false);
            adPanel.SetActive(false);
            errorPanel.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(root, ControllerPath);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    private static void BindGameplay()
    {
        var root = PrefabUtility.LoadPrefabContents(GameplayPath);
        try
        {
            var owner = root.GetComponentInChildren<CorePlayUI>(true);
            var original = root.GetComponentInChildren<ApkEarningTopView>(true);
            if (owner == null || original == null || original.LevelSettingsRow == null || original.GameplayViewport == null)
                throw new InvalidOperationException("CorePlayUI is missing its original A HUD or viewport bindings.");
            var binding = owner.GetComponent<EarningVariantGameplayView>();
            if (binding == null) binding = owner.gameObject.AddComponent<EarningVariantGameplayView>();
            binding.ATopRoot = original.gameObject;
            binding.ALevelSettingsRow = original.LevelSettingsRow.gameObject;
            binding.GameplayViewport = original.GameplayViewport;
            binding.Owner = owner;
            AuthorBackdrop(owner);
            // Existing A anchors, dimensions and active states stay authored as before.
            EditorUtility.SetDirty(binding);
            PrefabUtility.SaveAsPrefabAsset(root, GameplayPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void AuthorBackdrop(CorePlayUI owner)
    {
        RectTransform field = owner.m_FieldPresentationRoot;
        if (field == null || field.parent == null) throw new InvalidOperationException("The shared gameplay artwork root is missing.");
        const string name = "VariantBBackdrop";
        Transform existing = field.parent.Find(name);
        RectTransform backdrop = existing == null ? Rect(name, field.parent) : existing as RectTransform;
        if (backdrop == null) throw new InvalidOperationException("The B backdrop must use an authored RectTransform.");
        backdrop.anchorMin = backdrop.anchorMax = new Vector2(.5f, .5f);
        backdrop.pivot = field.pivot;
        backdrop.anchoredPosition = Vector2.zero;
        backdrop.sizeDelta = field.rect.size;
        backdrop.localScale = Vector3.one;
        backdrop.SetAsFirstSibling();
        var image = backdrop.GetComponent<Image>();
        if (image == null) image = backdrop.gameObject.AddComponent<Image>();
        image.color = Color.white;
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
        image.raycastTarget = false;
        image.sprite = null; // ChangeGameBg reuses its already-loaded Sprite; no new texture dependency.
        owner.m_FieldBackdrop = image;
        backdrop.gameObject.SetActive(false); // The original A layout never displays this layer.
        EditorUtility.SetDirty(owner);
    }

    private static RectTransform Modal(string name, Transform parent, float width, float height, string title, out GameObject panel)
    {
        var shade = Rect(name, parent);
        Stretch(shade);
        var shadeImage = shade.gameObject.AddComponent<Image>();
        shadeImage.color = new Color(0, 0, 0, .72f);
        panel = shade.gameObject;
        var card = Rect("Card", shade);
        card.anchorMin = card.anchorMax = new Vector2(.5f, .5f);
        card.pivot = new Vector2(.5f, .5f);
        card.anchoredPosition = new Vector2(0, 42);
        card.sizeDelta = new Vector2(width, height);
        card.gameObject.AddComponent<Image>().color = Paper;
        Label("Title", card, 44, 34, width - 160, 72, title, 43).fontStyle = FontStyle.Bold;
        return card;
    }

    private static Button Close(RectTransform card)
    {
        return Action("Close", card, card.sizeDelta.x - 104, 26, 76, 76, "X", Blue, 34);
    }

    private static Button Action(string name, Transform parent, float x, float y, float width, float height, string caption, Color color, int size = 32)
    {
        var rect = Rect(name, parent);
        Place(rect, x, y, width, height);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
        colors.pressedColor = new Color(.75f, .75f, .75f, 1f);
        colors.disabledColor = new Color(.58f, .58f, .58f, 1f);
        button.colors = colors;
        var text = Label("Label", rect, 12, 8, width - 24, height - 16, caption, size, Color.white);
        text.alignment = TextAnchor.MiddleCenter;
        text.fontStyle = FontStyle.Bold;
        return button;
    }

    private static Text Label(string name, Transform parent, float x, float y, float width, float height, string value, int size, Color? color = null)
    {
        var rect = Rect(name, parent);
        Place(rect, x, y, width, height);
        var text = rect.gameObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.color = color ?? Ink;
        text.text = value;
        text.alignment = TextAnchor.UpperLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.lineSpacing = 1.05f;
        text.raycastTarget = false;
        return text;
    }

    private static RectTransform Rect(string name, Transform parent)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.gameObject.layer = 5;
        return rect;
    }

    private static void Place(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static void EnsureFolder(string path)
    {
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
