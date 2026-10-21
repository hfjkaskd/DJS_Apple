using System;
using FruitApkEarning;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Saved reference requirements composition; row visibility remains owned by the Host.</summary>
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

        // The reference keeps the body and action stationary as four diamond
        // requirements become seven cash requirements. Only the rows change.
        RectTransform card = background.rectTransform;
        ConditionPosition(card, view.ArtworkRoot, 49, 214, 981, 1030);
        ConditionDisableLayouts(card);
        ConditionImage(background, "CashWalletPanel", 4);
        card.SetAsFirstSibling();
        Transform previousHeaderSlot = card.Find("ReferenceConditionHeader");
        if (previousHeaderSlot != null) previousHeaderSlot.gameObject.SetActive(false);

        ConditionPosition(inset.rectTransform, view.ArtworkRoot, 64, 322, 952, 910);
        ConditionDisableLayouts(inset.rectTransform);
        ConditionImage(inset, "CashWalletInset", 5);
        inset.transform.SetSiblingIndex(1);

        ConditionPosition(heading.rectTransform, view.ArtworkRoot, 155, 124, 768, 160);
        ConditionImage(heading, "CashWalletHeader", 1);
        heading.type = Image.Type.Simple;
        CenterText(view.GetText("title"), heading.rectTransform, 48);
        TMP_Text title = view.GetText("title");
        title.fontSharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(ArtFolder + "CashWalletOutlined.mat");
        title.color = new Color32(255, 250, 222, 255);
        title.enableWordWrapping = false;
        // Retain the existing orchard materials, without adding a decorative
        // layer to the reference's title/body hierarchy.
        Transform apple = heading.transform.Find("OrchardAppleAccent");
        if (apple != null) apple.gameObject.SetActive(false);
        heading.transform.SetAsLastSibling();

        ConditionPageText(view.GetText("subtitle"), view.ArtworkRoot, 260, 285, 560, 44, 36);
        ConditionPageText(view.GetText("notice"), view.ArtworkRoot, 120, 352, 840, 64, 32);
        // This test-only disclosure fits below the action, so it does not push
        // the reference's subtitle, notice or task block down the page.
        ConditionPageText(view.GetText("TestMode"), view.ArtworkRoot, 120, 1208, 840, 26, 21);

        RectTransform rows = (RectTransform)requirements.transform;
        ConditionPosition(rows, view.ArtworkRoot, 120, 442, 840, 568);
        rows.gameObject.SetActive(true);
        ConditionVertical(rows, new RectOffset(), 6);
        foreach (ContentSizeFitter fitter in rows.GetComponents<ContentSizeFitter>()) fitter.enabled = false;
        rows.SetAsLastSibling();

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
            ConditionFixedElement(row, 76);
            foreach (ContentSizeFitter fitter in row.GetComponents<ContentSizeFitter>()) fitter.enabled = false;
            var horizontal = ConditionEnsure<HorizontalLayoutGroup>(rowObject);
            ConditionDisableOtherLayouts(row, horizontal);
            horizontal.enabled = true;
            horizontal.padding = new RectOffset(14, 14, 6, 6);
            horizontal.spacing = 20;
            horizontal.childAlignment = TextAnchor.MiddleLeft;
            horizontal.childControlWidth = horizontal.childControlHeight = true;
            horizontal.childForceExpandWidth = horizontal.childForceExpandHeight = false;
            horizontal.childScaleWidth = horizontal.childScaleHeight = false;

            RectTransform labels = ConditionContainer(row, "ReferenceConditionLabelStack");
            var labelElement = ConditionFixedElement(labels, 64);
            labelElement.flexibleWidth = 1;
            ConditionVertical(labels, new RectOffset(), 4);
            ConditionText(name, labels, 32, 30, TextAlignmentOptions.Left);
            ConditionText(value, labels, 28, 27, TextAlignmentOptions.Left);
            name.enableWordWrapping = value.enableWordWrapping = false;
            name.fontStyle = FontStyles.Bold;
            ConditionText(status, row, 64, 27, TextAlignmentOptions.Center);
            var statusElement = status.GetComponent<LayoutElement>();
            statusElement.minWidth = statusElement.preferredWidth = 206;
            statusElement.flexibleWidth = 0;
            labels.SetAsFirstSibling();
            status.transform.SetAsLastSibling();
            row.SetAsLastSibling();
            // Runtime and static fixtures already select four diamond rows or
            // seven cash rows. Do not substitute an authoring-only row count.
            rowObject.SetActive(savedActive);
        }

        ConditionPosition((RectTransform)action.transform, view.ArtworkRoot, 182, 1067, 716, 138);
        ConditionDisableLayouts((RectTransform)action.transform);
        var actionImage = action.GetComponent<Image>();
        if (actionImage == null) throw new InvalidOperationException("Requirements action Button must own its Image.");
        ConditionImage(actionImage, "CashWalletButton", 2);
        actionImage.pixelsPerUnitMultiplier = actionImage.sprite.rect.height / 138 / actionImage.pixelsPerUnit;
        action.targetGraphic = actionImage;
        CenterText(view.GetText("actionCaption"), (RectTransform)action.transform, 48);
        action.transform.SetAsLastSibling();

        RectTransform closeRect = (RectTransform)close.transform;
        ConditionPosition(closeRect, view.ArtworkRoot, 921, 162, 114, 118);
        ConditionDisableLayouts(closeRect);
        var closeImage = close.GetComponent<Image>();
        if (closeImage == null) throw new InvalidOperationException("Requirements close Button must own its Image.");
        ConditionImage(closeImage, "CashWalletClose", 1);
        closeImage.type = Image.Type.Simple;
        close.targetGraphic = closeImage;
        close.transform.SetAsLastSibling();

        // Four rows end at 764; seven end at 1010, leaving a 57-unit
        // separation before the fixed action. Long screens retain these
        // dimensions rather than stretching the content or whitespace.
        LayoutRebuilder.ForceRebuildLayoutImmediate(rows);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rows);
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

    private static void ConditionPosition(RectTransform rect, RectTransform parent,
        float x, float y, float width, float height)
    {
        ConditionParent(rect, parent);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void ConditionDisableLayouts(RectTransform rect)
    {
        foreach (LayoutGroup layout in rect.GetComponents<LayoutGroup>()) layout.enabled = false;
        foreach (ContentSizeFitter fitter in rect.GetComponents<ContentSizeFitter>()) fitter.enabled = false;
        foreach (LayoutElement element in rect.GetComponents<LayoutElement>()) element.enabled = false;
    }

    private static void ConditionPageText(TMP_Text text, RectTransform parent,
        float x, float y, float width, float height, float size)
    {
        ConditionText(text, parent, height, size, TextAlignmentOptions.Center);
        ConditionPosition(text.rectTransform, parent, x, y, width, height);
        ConditionDisableLayouts(text.rectTransform);
        text.transform.SetAsLastSibling();
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
