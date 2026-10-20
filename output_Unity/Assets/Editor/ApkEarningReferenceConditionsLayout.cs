using System;
using FruitApkEarning;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Saved content-driven requirements layout; row visibility remains owned by the Host.</summary>
public static partial class ApkEarningReferenceLayoutAuthoring
{
    private static void ApplyConditionPage() => Edit("LocalWithdrawalConditions", Conditions);

    private static void Conditions(ApkEarningPageView view)
    {
        Prepare(view);
        Image background = view.GetImage("main/Image");
        Image heading = view.GetImage("Title1");
        Image inset = view.GetImage("main/Image (1)");
        var requirements = view.GetObject("Requirements");
        Button action = view.GetButton("action"), close = view.GetButton("close");
        if (background == null || heading == null || inset == null || requirements == null ||
            action == null || close == null)
            throw new InvalidOperationException("Requirements page is missing an existing background, container or Button binding.");

        // Keep every binding alive, but do not restore unused source-page visuals.
        foreach (string key in new[] { "Level1", "Level2", "1", "2", "Items", "Progress", "PlayerInfoPos", "ItemIcon", "ItemCount", "Tip" })
        {
            GameObject legacy = view.GetObject(key);
            if (legacy != null) legacy.SetActive(false);
        }
        HideImage(view, "main/Image (2)");
        foreach (string key in new[] { "Title", "main/Image/label_37" })
        {
            TMP_Text legacy = view.GetText(key);
            if (legacy != null && legacy != view.GetText("title")) legacy.gameObject.SetActive(false);
        }

        RectTransform card = background.rectTransform;
        ConditionParent(card, view.ArtworkRoot);
        card.anchorMin = card.anchorMax = new Vector2(.5f, .94f);
        card.pivot = new Vector2(.5f, 1);
        card.anchoredPosition = Vector2.zero;
        card.sizeDelta = new Vector2(984, 0);
        ConditionImage(background, "CashWalletPanel", 4);
        card.SetAsLastSibling();
        ConditionNaturalElement(card);
        var cardLayout = ConditionVertical(card, new RectOffset(60, 60, 72, 56), 16);
        var cardFit = ConditionEnsure<ContentSizeFitter>(card.gameObject);
        cardFit.enabled = true;
        cardFit.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        cardFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        RectTransform headerSlot = ConditionContainer(card, "ReferenceConditionHeader");
        ConditionFixedElement(headerSlot, 160);
        ConditionParent(heading.rectTransform, headerSlot);
        ConditionCentered(heading.rectTransform, new Vector2(0, 0), new Vector2(760, 148));
        ConditionImage(heading, "CashWalletHeader", 1);
        heading.type = Image.Type.Simple;
        CenterText(view.GetText("title"), heading.rectTransform, 48);
        TMP_Text title = view.GetText("title");
        title.fontSharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(ArtFolder + "CashWalletOutlined.mat");
        title.color = new Color32(255, 250, 222, 255);
        title.enableWordWrapping = false;
        Transform apple = heading.transform.Find("OrchardAppleAccent");
        if (apple != null)
        {
            apple.gameObject.SetActive(true);
            var appleRect = (RectTransform)apple;
            appleRect.anchorMin = appleRect.anchorMax = new Vector2(.025f, .85f);
            appleRect.pivot = new Vector2(.5f, .5f);
            appleRect.anchoredPosition = Vector2.zero;
            appleRect.sizeDelta = new Vector2(100, 122);
            appleRect.localScale = Vector3.one;
        }

        ConditionText(view.GetText("TestMode"), card, 40, 26, TextAlignmentOptions.Center);
        ConditionText(view.GetText("subtitle"), card, 70, 44, TextAlignmentOptions.Center);
        ConditionText(view.GetText("notice"), card, 100, 33, TextAlignmentOptions.Center);

        RectTransform rows = (RectTransform)requirements.transform;
        ConditionParent(rows, card);
        rows.gameObject.SetActive(true);
        ConditionNaturalElement(rows);
        ConditionVertical(rows, new RectOffset(12, 12, 12, 12), 8);
        // The parent layout consumes this group's preferred height. A second
        // ContentSizeFitter here would fight the parent's driven RectTransform.
        foreach (ContentSizeFitter fitter in rows.GetComponents<ContentSizeFitter>()) fitter.enabled = false;
        ConditionParent(inset.rectTransform, rows);
        ConditionImage(inset, "CashWalletInset", 5);
        var insetElement = ConditionEnsure<LayoutElement>(inset.gameObject);
        insetElement.enabled = true;
        insetElement.ignoreLayout = true;
        inset.rectTransform.anchorMin = Vector2.zero;
        inset.rectTransform.anchorMax = Vector2.one;
        inset.rectTransform.offsetMin = inset.rectTransform.offsetMax = Vector2.zero;
        inset.transform.SetAsFirstSibling();

        for (int i = 0; i < 7; i++)
        {
            string key = "rows[" + i + "]";
            GameObject rowObject = view.GetObject(key + ".Root");
            TMP_Text name = view.GetText(key + ".Name"), value = view.GetText(key + ".Value");
            TMP_Text status = view.GetText(key + ".Status");
            if (rowObject == null || name == null || value == null || status == null)
                throw new InvalidOperationException("Incomplete requirements row binding: " + key);
            bool savedActive = rowObject.activeSelf;
            RectTransform row = (RectTransform)rowObject.transform;
            ConditionParent(row, rows);
            ConditionFixedElement(row, 104);
            foreach (ContentSizeFitter fitter in row.GetComponents<ContentSizeFitter>()) fitter.enabled = false;
            var horizontal = ConditionEnsure<HorizontalLayoutGroup>(rowObject);
            ConditionDisableOtherLayouts(row, horizontal);
            horizontal.enabled = true;
            horizontal.padding = new RectOffset(20, 20, 8, 8);
            horizontal.spacing = 24;
            horizontal.childAlignment = TextAnchor.MiddleLeft;
            horizontal.childControlWidth = horizontal.childControlHeight = true;
            horizontal.childForceExpandWidth = horizontal.childForceExpandHeight = false;
            horizontal.childScaleWidth = horizontal.childScaleHeight = false;

            RectTransform labels = ConditionContainer(row, "ReferenceConditionLabelStack");
            var labelElement = ConditionFixedElement(labels, 88);
            labelElement.flexibleWidth = 1;
            ConditionVertical(labels, new RectOffset(), 8);
            ConditionText(name, labels, 40, 34, TextAlignmentOptions.Left);
            ConditionText(value, labels, 40, 29, TextAlignmentOptions.Left);
            name.enableWordWrapping = value.enableWordWrapping = false;
            name.fontStyle = FontStyles.Bold;
            ConditionText(status, row, 88, 31, TextAlignmentOptions.Center);
            var statusElement = status.GetComponent<LayoutElement>();
            statusElement.minWidth = statusElement.preferredWidth = 210;
            statusElement.flexibleWidth = 0;
            labels.SetAsFirstSibling();
            status.transform.SetAsLastSibling();
            row.SetAsLastSibling();
            // Runtime and static fixtures already select four diamond rows or
            // seven cash rows. Do not substitute an authoring-only row count.
            rowObject.SetActive(savedActive);
        }

        ConditionParent((RectTransform)action.transform, card);
        ConditionFixedElement((RectTransform)action.transform, 150);
        var actionImage = action.GetComponent<Image>();
        if (actionImage == null) throw new InvalidOperationException("Requirements action Button must own its Image.");
        ConditionImage(actionImage, "CashWalletButton", 2);
        action.targetGraphic = actionImage;
        CenterText(view.GetText("actionCaption"), (RectTransform)action.transform, 49);
        action.transform.SetAsLastSibling();

        ConditionParent((RectTransform)close.transform, card);
        var closeElement = ConditionEnsure<LayoutElement>(close.gameObject);
        closeElement.enabled = true;
        closeElement.ignoreLayout = true;
        RectTransform closeRect = (RectTransform)close.transform;
        closeRect.anchorMin = closeRect.anchorMax = closeRect.pivot = Vector2.one;
        closeRect.anchoredPosition = new Vector2(-18, -18);
        closeRect.sizeDelta = new Vector2(108, 108);
        var closeImage = close.GetComponent<Image>();
        if (closeImage == null) throw new InvalidOperationException("Requirements close Button must own its Image.");
        ConditionImage(closeImage, "CashWalletClose", 1);
        closeImage.type = Image.Type.Simple;
        close.targetGraphic = closeImage;
        close.transform.SetAsLastSibling();

        headerSlot.SetSiblingIndex(0);
        view.GetText("TestMode").transform.SetSiblingIndex(1);
        view.GetText("subtitle").transform.SetSiblingIndex(2);
        view.GetText("notice").transform.SetSiblingIndex(3);
        rows.SetSiblingIndex(4);
        action.transform.SetSiblingIndex(5);
        // 1920-high reference: 728 fixed units + (12*2 + N*104 + (N-1)*8).
        // Four rows yield 1192 (62.1%); seven yield 1528 (79.6%). Longer screens
        // retain this content height instead of stretching blank areas.
        cardLayout.enabled = true;
        LayoutRebuilder.ForceRebuildLayoutImmediate(card);
        LayoutRebuilder.ForceRebuildLayoutImmediate(card);
    }

    private static T ConditionEnsure<T>(GameObject node) where T : Component
    {
        T existing = node.GetComponent<T>();
        return existing != null ? existing : node.AddComponent<T>();
    }

    private static RectTransform ConditionContainer(RectTransform parent, string name)
    {
        Transform existing = parent.Find(name);
        var rect = existing != null ? existing as RectTransform :
            new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        if (rect == null) throw new InvalidOperationException("Requirements layout node is not a RectTransform: " + name);
        rect.gameObject.layer = parent.gameObject.layer;
        ConditionParent(rect, parent);
        rect.gameObject.SetActive(true);
        return rect;
    }

    private static void ConditionParent(RectTransform rect, RectTransform parent)
    {
        if (rect == null || parent == null || rect == parent)
            throw new InvalidOperationException("Invalid requirements layout parent.");
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = rect.sizeDelta = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static void ConditionCentered(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void ConditionImage(Image image, string sprite, float ppu)
    {
        image.gameObject.SetActive(true);
        image.enabled = true;
        image.sprite = Artwork(sprite);
        image.type = Image.Type.Sliced;
        image.pixelsPerUnitMultiplier = ppu;
        image.preserveAspect = false;
        image.color = Color.white;
        image.raycastTarget = image.GetComponent<Button>() != null;
    }

    private static LayoutElement ConditionNaturalElement(RectTransform rect)
    {
        var element = ConditionEnsure<LayoutElement>(rect.gameObject);
        element.enabled = true;
        element.ignoreLayout = false;
        element.layoutPriority = 2;
        element.minWidth = element.preferredWidth = -1;
        element.minHeight = element.preferredHeight = -1;
        element.flexibleWidth = element.flexibleHeight = 0;
        return element;
    }

    private static LayoutElement ConditionFixedElement(RectTransform rect, float height)
    {
        var element = ConditionNaturalElement(rect);
        element.minHeight = element.preferredHeight = height;
        return element;
    }

    private static VerticalLayoutGroup ConditionVertical(RectTransform rect, RectOffset padding, float spacing)
    {
        var layout = ConditionEnsure<VerticalLayoutGroup>(rect.gameObject);
        ConditionDisableOtherLayouts(rect, layout);
        layout.enabled = true;
        layout.padding = padding;
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childScaleWidth = layout.childScaleHeight = false;
        return layout;
    }

    private static void ConditionDisableOtherLayouts(RectTransform rect, LayoutGroup keep)
    {
        foreach (LayoutGroup layout in rect.GetComponents<LayoutGroup>())
            if (layout != keep) layout.enabled = false;
    }

    private static void ConditionText(TMP_Text text, RectTransform parent, float height,
        float size, TextAlignmentOptions alignment)
    {
        if (text == null) throw new InvalidOperationException("Missing requirements text binding.");
        ConditionParent(text.rectTransform, parent);
        text.gameObject.SetActive(true);
        text.enabled = true;
        text.enableAutoSizing = true;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.alignment = alignment;
        text.margin = Vector4.zero;
        text.raycastTarget = false;
        SetFontSize(text, size);
        ConditionFixedElement(text.rectTransform, height);
    }
}
