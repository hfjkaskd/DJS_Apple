using System;
using System.Collections.Generic;
using FruitApkEarning;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One-time editor authoring of page-specific reference layouts into existing prefabs.
/// Runtime bindings and accounting are retained; the player builds no UI hierarchy.
/// Coordinates describe a 1080 x 1920 portrait design, with responsive saved anchors.
/// </summary>
public static partial class ApkEarningReferenceLayoutAuthoring
{
    private const string PanelFolder = "Assets/ApkEarningReference/Resources/ApkEarningReference/Panels/";
    private const string ArtFolder = "Assets/OrchardEarningArt/CashWallet/";

    [MenuItem("Tools/Apk Earning/Apply Reference Page Composition")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before authoring saved page layouts.");
        Edit("TXPanel", CashWallet);
        Edit("TXCoinPanel", DiamondWallet);
        ApplyRelatedPages();
        ApplyConditionPage();
        ApplyFollowupPages();
        ApplyAuxiliaryPages();
        ApplyGuidePages();
        AssetDatabase.SaveAssets();
        Debug.Log("[ReferenceLayout] Authored every reachable earning page and toast; compact gameplay HUD retained.");
    }

    public static void ApplyAndValidate()
    {
        Apply();
        ApkEarningValidation.ValidateEnlargedModalFixtures();
    }

    public static void ApplyValidateAndAnimate()
    {
        ApplyAndValidate();
        ApkEarningFlowAnimationValidation.Run();
    }

    public static void ApplyValidateAllAndAnimate()
    {
        Apply();
        ApkEarningValidation.ValidateAllFlowLayouts();
        ApkEarningFlowAnimationValidation.Run();
    }

    private static void Edit(string name, Action<ApkEarningPageView> action)
    {
        string path = PanelFolder + name + ".prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var view = root.GetComponent<ApkEarningPageView>();
            if (view == null) throw new InvalidOperationException("Missing page bindings: " + name);
            action(view);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static void Prepare(ApkEarningPageView view)
    {
        if (view.ArtworkRoot == null) throw new InvalidOperationException("Page has no artwork root.");
        view.ReferenceWidth = 1080;
        view.MinimumReferenceHeight = 1920;
        view.FitWidth = true;
        view.ArtworkRoot.anchorMin = view.ArtworkRoot.anchorMax = new Vector2(.5f, .5f);
        view.ArtworkRoot.pivot = new Vector2(.5f, .5f);
        view.ArtworkRoot.anchoredPosition = Vector2.zero;
        view.ArtworkRoot.sizeDelta = new Vector2(1080, 1920);
        view.ArtworkRoot.localScale = Vector3.one;
    }

    private static void Rect(ApkEarningPageView view, RectTransform rect, float x, float y, float w, float h)
    {
        if (rect == null) throw new InvalidOperationException("Missing layout target.");
        if (rect == view.ArtworkRoot) throw new InvalidOperationException("Do not position the design root.");
        rect.SetParent(view.ArtworkRoot, false);
        if (rect.parent != view.ArtworkRoot)
            throw new InvalidOperationException("Unpack the nested prefab before composing " + rect.name);
        rect.anchorMin = new Vector2(x / 1080, 1 - (y + h) / 1920);
        rect.anchorMax = new Vector2((x + w) / 1080, 1 - y / 1920);
        rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static Sprite Artwork(string name)
    {
        foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(ArtFolder + name + ".png"))
            if (asset is Sprite sprite) return sprite;
        throw new InvalidOperationException("Missing orchard sprite: " + name);
    }

    private static void Panel(ApkEarningPageView v, Image image, float x, float y, float w, float h,
        string sprite = "CashWalletPanel", float ppu = 4)
    {
        if (image == null) throw new InvalidOperationException("Missing panel Image.");
        image.gameObject.SetActive(true);
        image.enabled = true;
        image.sprite = Artwork(sprite);
        image.color = Color.white;
        image.preserveAspect = false;
        image.type = Image.Type.Sliced;
        image.pixelsPerUnitMultiplier = ppu;
        image.raycastTarget = image.GetComponent<Button>() != null;
        Rect(v, image.rectTransform, x, y, w, h);
        image.transform.SetAsFirstSibling();
    }

    private static void Text(ApkEarningPageView v, string key, float x, float y, float w, float h, float size)
    {
        var text = v.GetText(key);
        if (text == null) throw new InvalidOperationException("Missing text: " + key);
        text.gameObject.SetActive(true);
        text.enabled = true;
        Rect(v, text.rectTransform, x, y, w, h);
        SetFontSize(text, size);
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        text.transform.SetAsLastSibling();
    }

    private static void SetFontSize(TMP_Text text, float size)
    {
        text.fontSize = text.fontSizeMax = size;
        text.fontSizeMin = size * .8f;
    }

    private static void CenterText(TMP_Text text, RectTransform parent, float size)
    {
        if (text == null) return;
        text.transform.SetParent(parent, false);
        if (text.transform.parent != parent)
            throw new InvalidOperationException("Cannot bind the visual label inside its control: " + text.name);
        var rect = text.rectTransform;
        rect.anchorMin = new Vector2(.06f, .14f);
        rect.anchorMax = new Vector2(.94f, .86f);
        rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = rect.sizeDelta = Vector2.zero;
        rect.localScale = Vector3.one;
        text.enabled = true;
        text.gameObject.SetActive(true);
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        SetFontSize(text, size);
    }

    private static void Button(ApkEarningPageView v, string key, float x, float y, float w, float h, float fontSize = 62)
    {
        var button = v.GetButton(key);
        if (button == null) throw new InvalidOperationException("Missing button: " + key);
        var image = button.GetComponent<Image>();
        Panel(v, image, x, y, w, h, "CashWalletButton", 2);
        // Keep the sliced end caps proportional when the reference uses a
        // shorter action row than the source artwork's natural height.
        image.pixelsPerUnitMultiplier = image.sprite.rect.height / h / image.pixelsPerUnit;
        button.targetGraphic = image;
        button.transform.SetAsLastSibling();
        foreach (var label in button.GetComponentsInChildren<TMP_Text>(true))
            CenterText(label, (RectTransform)button.transform, fontSize);
    }

    private static void Header(ApkEarningPageView v, string imageKey, string textKey,
        float x, float y, float w, float h)
    {
        var heading = v.GetImage(imageKey);
        Panel(v, heading, x, y, w, h, "CashWalletHeader", 1);
        heading.type = Image.Type.Simple;
        heading.transform.SetAsLastSibling();
        Text(v, textKey, x + 40, y + 22, w - 70, h - 42, 64);
        var text = v.GetText(textKey);
        text.fontSharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(ArtFolder + "CashWalletOutlined.mat");
        text.color = new Color32(255, 250, 222, 255);
        Transform apple = heading.transform.Find("OrchardAppleAccent");
        if (apple != null)
        {
            var rect = (RectTransform)apple;
            rect.anchorMin = rect.anchorMax = new Vector2(.025f, .85f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(125, 145);
            rect.localScale = Vector3.one;
        }
    }

    private static void HideImage(ApkEarningPageView v, string key)
    {
        var image = v.GetImage(key);
        if (image != null) image.enabled = false;
    }

    private static void Icon(ApkEarningPageView v, Image image, float x, float y, float w, float h)
    {
        if (image == null) throw new InvalidOperationException("Missing icon.");
        image.enabled = true;
        image.gameObject.SetActive(true);
        image.preserveAspect = true;
        image.raycastTarget = false;
        Rect(v, image.rectTransform, x, y, w, h);
    }

    private static void CloseButton(ApkEarningPageView v, string key, float x, float y, float size = 112)
    {
        var button = v.GetButton(key);
        Panel(v, button.GetComponent<Image>(), x, y, size, size, "CashWalletClose", 1);
        button.GetComponent<Image>().type = Image.Type.Simple;
        button.targetGraphic = button.GetComponent<Image>();
        button.transform.SetAsLastSibling();
    }

    private static void Progress(ApkEarningPageView v, string trackKey, string fillKey, string labelKey,
        float x, float y, float width, float height)
    {
        var track = v.GetImage(trackKey);
        var fill = v.GetImage(fillKey);
        Panel(v, track, x, y, width, height, "CashWalletProgressTrack", 2);
        track.pixelsPerUnitMultiplier = track.sprite.rect.height / height / track.pixelsPerUnit;
        track.transform.SetAsLastSibling();
        fill.transform.SetParent(track.transform, false);
        fill.enabled = true;
        fill.gameObject.SetActive(true);
        var rect = fill.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0, .5f);
        rect.pivot = new Vector2(0, .5f);
        rect.anchoredPosition = new Vector2(12, 0);
        rect.sizeDelta = new Vector2(width - 24, height - 20);
        rect.localScale = Vector3.one;
        fill.type = Image.Type.Sliced;
        fill.preserveAspect = false;
        fill.pixelsPerUnitMultiplier = fill.sprite.rect.height / (height - 20) / fill.pixelsPerUnit;
        foreach (var slot in v.Images)
            if (slot.Target == fill)
            {
                slot.ResizeWidthWithProgress = true;
                slot.FullProgressWidth = width - 24;
                slot.FullProgressHeight = height - 20;
                slot.FullProgressPixelsPerUnit = fill.pixelsPerUnitMultiplier;
            }
        var label = v.GetText(labelKey);
        CenterText(label, track.rectTransform, Mathf.Min(42, height * .56f));
    }

    private static void CashWallet(ApkEarningPageView v)
    {
        // This page previously nested the reusable PlayerInfo prefab. Its bound
        // statistics and history visuals now belong to separate saved modules.
        foreach (GameObject module in new[] {
            v.GetImage("main/PlayerInfoPos/PlayerInfo/Image").gameObject,
            v.GetButton("HistoryButton").gameObject })
        {
            var nested = PrefabUtility.GetOutermostPrefabInstanceRoot(module);
            if (nested != null && nested != v.gameObject)
                PrefabUtility.UnpackPrefabInstance(nested, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
        Prepare(v);
        Panel(v, v.GetImage("main/PlayerInfoPos/PlayerInfo/Image"), 58, 1256, 964, 330, ppu: 4);
        Panel(v, v.GetImage("main/Image (1)"), 115, 292, 850, 328, ppu: 5);
        Panel(v, v.GetImage("main/Image"), 48, 214, 984, 1032, ppu: 2);
        Header(v, "main/Title1", "StageLabel", 156, 124, 768, 160);
        var previousTitle = v.GetText("Title");
        if (previousTitle != null && previousTitle != v.GetText("StageLabel")) previousTitle.enabled = false;
        // Keep the status in the card footer so the reference's amount and
        // progress sections do not move to make room for an extra heading row.
        Text(v, "TestMode", 220, 1190, 640, 24, 21);
        Text(v, "labels[0].Text", 220, 319, 640, 70, 42);
        Icon(v, v.GetImage("goldImage"), 202, 455, 108, 100);
        Text(v, "GoldCount", 306, 434, 470, 132, 112);
        Progress(v, "main/Progress", "Progress", "ProgressValue", 120, 714, 840, 73);
        Text(v, "Tip", 150, 836, 780, 100, 44);
        Button(v, "TXBtn", 206, 998, 668, 192, 78);
        CenterText(v.GetText("TXText"), (RectTransform)v.GetButton("TXBtn").transform, 78);
        Text(v, "Stats.Time", 160, 1263, 760, 65, 36);
        StatsColumn(v, "Stats.PassCount", "Stats.PlayersCaption", "Players cleared\ntoday", 98);
        StatsColumn(v, "Stats.ChallengeTimes", "Stats.AttemptsCaption", "Average attempts", 400);
        StatsColumn(v, "Stats.GoldGetCount", "Stats.AmountCaption", "Average\nwithdrawal amount", 702);
        var divider = v.ArtworkRoot.Find("CashStatsDivider");
        if (divider != null) divider.gameObject.SetActive(false);
        var history = v.GetButton("HistoryButton");
        Panel(v, history.GetComponent<Image>(), 33, 1610, 1013, 119, ppu: 6);
        history.targetGraphic = history.GetComponent<Image>();
        history.transform.SetAsLastSibling();
        CenterText(v.GetText("HistoryCaption"), (RectTransform)history.transform, 46);
        CloseButton(v, "Close", 916, 165);
    }

    private static void StatsColumn(ApkEarningPageView v, string valueKey, string captionKey,
        string caption, float x)
    {
        TMP_Text value = v.GetText(valueKey);
        TMP_Text label = v.GetText(captionKey);
        if (label == null)
        {
            var node = new GameObject(captionKey, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            node.layer = v.gameObject.layer;
            label = node.GetComponent<TextMeshProUGUI>();
            label.font = value.font;
            label.fontSharedMaterial = value.fontSharedMaterial;
            label.color = value.color;
            var slots = new List<ApkEarningPageView.TextSlot>(v.Texts) {
                new ApkEarningPageView.TextSlot { Key = captionKey, Target = label }
            };
            v.Texts = slots.ToArray();
        }
        label.text = caption;
        label.enableAutoSizing = true;
        label.enableWordWrapping = true;
        Text(v, captionKey, x, 1354, 280, 90, 34);
        Text(v, valueKey, x, 1480, 280, 82, 56);
        value.enableAutoSizing = true;
        value.enableWordWrapping = false;
        value.fontSizeMin = 36;
        value.text = "--";
    }

    private static void DiamondWallet(ApkEarningPageView v)
    {
        Prepare(v);
        Panel(v, v.GetImage("main/Image (1)"), 18, 267, 1044, 110, ppu: 6);
        Panel(v, v.GetImage("main/Image (2)"), 68, 800, 945, 587, ppu: 5);
        Panel(v, v.GetImage("main/Image (3)"), 97, 390, 885, 271, ppu: 5);
        Panel(v, v.GetImage("TXProgress"), 64, 1395, 944, 278, ppu: 6);
        Panel(v, v.GetImage("bg"), 34, 108, 1012, 1798, ppu: 2);
        Header(v, "Title", "labels[0].Text", 160, 108, 760, 162);
        Icon(v, v.GetImage("CoinIcon"), 180, 294, 58, 56);
        Text(v, "CoinValue", 350, 287, 380, 70, 56);
        Text(v, "TestMode", 220, 1855, 640, 24, 21);
        Icon(v, v.GetImage("main/Image"), 222, 455, 96, 100);
        Text(v, "GoldValue", 380, 437, 410, 150, 110);
        Text(v, "TipValue", 230, 684, 620, 76, 40);
        Text(v, "labels[1].Text", 130, 818, 820, 60, 40);
        for (int i = 0; i < 4; i++)
        {
            var option = v.GetButton("options[" + i + "]");
            Panel(v, option.GetComponent<Image>(), i % 2 == 0 ? 126 : 586,
                i < 2 ? 884 : 1140, 368, 220, ppu: 6);
            option.transform.SetAsLastSibling();
            option.targetGraphic = option.GetComponent<Image>();
            CenterText(v.GetText("amounts[" + i + "]"), (RectTransform)option.transform, 72);
            var frame = v.GetObject("selectionFrames[" + i + "]");
            if (frame != null)
            {
                frame.transform.SetParent(option.transform, false);
                var rect = (RectTransform)frame.transform;
                rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                var image = frame.GetComponent<Image>();
                if (image != null) image.enabled = false;
            }
        }
        Text(v, "TXTip", 115, 1417, 850, 58, 38);
        Text(v, "IitemTip", 120, 1475, 840, 108, 37);
        v.GetText("TXTip").alignment = v.GetText("IitemTip").alignment = TextAlignmentOptions.Left;
        Progress(v, "main/TXProgress/Bar", "TipBar", "TipBarValue", 104, 1597, 860, 52);
        Button(v, "TXBtn", 158, 1684, 764, 171, 72);
        CenterText(v.GetText("labels[2].Text"), (RectTransform)v.GetButton("TXBtn").transform, 72);
        CloseButton(v, "Close", 928, 211, 108);
    }
}
