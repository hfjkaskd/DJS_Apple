using System;
using FruitApkEarning;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reference compositions for account, history and reward pages. These recipes
/// configure saved prefabs; all account, claim and history bindings stay intact.
/// </summary>
public static partial class ApkEarningReferenceLayoutAuthoring
{
    public static void ApplyRelatedPages()
    {
        Edit("TXUserInfoPanel", Account);
        Edit("LocalCashHistoryRow", HistoryRow);
        Edit("LocalCashHistory", History);
        // Save the base before its variants so inherited reward bindings survive.
        Edit("SuccessPanel", Reward);
        Edit("LocalSuccess", Reward);
        Edit("LocalLuckyReward", Reward);
    }

    private static void Account(ApkEarningPageView v)
    {
        Prepare(v);
        HideImage(v, "main/Image (1)");
        Header(v, "main/Title", "labels[1].Text", 185, 205, 665, 116);
        SetFontSize(v.GetText("labels[1].Text"), 52);
        Text(v, "TestMode", 190, 342, 700, 36, 27);

        var help = v.GetButton("ChannelInfoBtn");
        Rect(v, (RectTransform)help.transform, 908, 225, 72, 72);
        help.transform.SetAsLastSibling();
        foreach (var caption in help.GetComponentsInChildren<TMP_Text>(true))
            if (caption.transform != help.transform)
                CenterText(caption, (RectTransform)help.transform, 44);

        var group = v.GetObject("ToggleGroup");
        if (group != null)
            foreach (var layout in group.GetComponents<LayoutGroup>())
                layout.enabled = false;

        for (int i = 0; i < 3; i++)
        {
            string key = "channels[" + i + "]";
            var button = v.GetButton(key + ".Button");
            var icon = v.GetImage(key + ".Icon");
            var checkmark = v.GetImage(key + ".Checkmark");
            if (button == null || icon == null || checkmark == null)
                throw new InvalidOperationException("Incomplete account channel bindings: " + key);

            Rect(v, (RectTransform)button.transform, 130 + i * 280, 432, 260, 166);
            button.transform.SetAsLastSibling();
            // Keep the actual channel artwork inside the standard Button. Its
            // sprite is still selected by the existing payment-channel logic.
            var buttonRect = (RectTransform)button.transform;
            if (icon.rectTransform != buttonRect)
            {
                // The reference channels occupy three complete rectangular
                // tiles. Give each actual Button its own visible tile instead
                // of letting a square logo define a smaller painted surface.
                var tile = button.GetComponent<Image>();
                if (tile == null) tile = button.gameObject.AddComponent<Image>();
                tile.sprite = Artwork("CashWalletInset");
                tile.type = Image.Type.Sliced;
                tile.pixelsPerUnitMultiplier = 5;
                tile.color = Color.white;
                tile.raycastTarget = true;
                button.targetGraphic = tile;
                RelatedStretch(icon.rectTransform, buttonRect, 18, 10, 18, 10);
                icon.raycastTarget = false;
            }
            else
            {
                // Accept a prefab whose channel artwork is on the Button
                // itself without ever attempting SetParent(transform).
                icon.raycastTarget = true;
                button.targetGraphic = icon;
            }
            icon.preserveAspect = true;
            RelatedLocal(checkmark.rectTransform, buttonRect, 82, 122, 96, 78);
            checkmark.raycastTarget = false;
            checkmark.transform.SetAsLastSibling();
        }

        // This is also the validation message binding, so it remains a complete,
        // readable block between channel selection and the input form.
        Text(v, "labels[0].Text", 145, 690, 790, 118, 38);
        RelatedInput(v, "InputName", 130, 865, 820, 151);
        RelatedInput(v, "InputEmail", 130, 1045, 820, 151);
        RelatedInput(v, "InputAreaNumber", 130, 1045, 250, 151);
        RelatedInput(v, "InputNumber", 402, 1045, 548, 151);
        Button(v, "GetBtn", 160, 1310, 760, 187, 72);
        CenterText(v.GetText("labels[2].Text"), (RectTransform)v.GetButton("GetBtn").transform, 72);

        // The reference dismisses the form from a separate centered footer.
        // It remains the original CloseBtn, preserving all return behavior.
        CloseButton(v, "CloseBtn", 477, 1655, 126);
        Panel(v, v.GetImage("main/Image"), 48, 180, 984, 1420, ppu: 3);
    }

    private static void RelatedInput(ApkEarningPageView v, string key,
        float x, float y, float width, float height)
    {
        var input = v.GetInput(key);
        if (input == null) throw new InvalidOperationException("Missing account input: " + key);
        bool active = input.gameObject.activeSelf;
        var image = input.GetComponent<Image>();
        Panel(v, image, x, y, width, height, "CashWalletInset", 4);
        input.transform.SetAsLastSibling();
        image.raycastTarget = true;
        input.targetGraphic = image;
        var root = (RectTransform)input.transform;
        if (input.textViewport == null || input.textComponent == null)
            throw new InvalidOperationException("Missing input viewport/text: " + key);
        RelatedStretch(input.textViewport, root, 26, 20, 26, 20);
        RelatedStretch(input.textComponent.rectTransform, input.textViewport, 0, 0, 0, 0);
        RelatedInputText(input.textComponent);
        if (input.placeholder is TMP_Text placeholder)
        {
            RelatedStretch(placeholder.rectTransform, input.textViewport, 0, 0, 0, 0);
            RelatedInputText(placeholder);
        }
        // Email and telephone modes are mutually exclusive at runtime.
        input.gameObject.SetActive(active);
    }

    private static void RelatedInputText(TMP_Text text)
    {
        SetFontSize(text, 40);
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.enableWordWrapping = false;
        text.raycastTarget = false;
    }

    private static void History(ApkEarningPageView v)
    {
        Prepare(v);
        Panel(v, v.GetImage("Balance"), 110, 454, 860, 254, "CashWalletInset", 4);
        Panel(v, v.GetImage("Card"), 62, 285, 956, 1350, ppu: 3);
        Header(v, "Heading", "titleLabel", 106, 246, 868, 154);
        SetFontSize(v.GetText("titleLabel"), 58);
        CloseButton(v, "closeButton", 936, 268, 108);
        Text(v, "TestMode", 210, 408, 660, 34, 27);
        Text(v, "Caption", 170, 474, 740, 62, 42);
        Icon(v, v.GetImage("Bank"), 229, 553, 160, 144);
        Text(v, "balanceLabel", 405, 545, 490, 144, 108);
        v.GetText("balanceLabel").color = v.EmphasisColor;

        var rows = v.GetObject("rowParent");
        if (rows == null) throw new InvalidOperationException("Missing history row parent.");
        Rect(v, (RectTransform)rows.transform, 110, 744, 860, 778);
        var layout = rows.GetComponent<VerticalLayoutGroup>();
        if (layout == null) throw new InvalidOperationException("History needs its configured vertical layout.");
        layout.enabled = true;
        layout.padding = new RectOffset();
        layout.spacing = 24;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childScaleWidth = layout.childScaleHeight = false;
        rows.transform.SetAsLastSibling();
    }

    private static void HistoryRow(ApkEarningPageView v)
    {
        // A record is an independently instantiated fixed-size row, not a page.
        // Do not give it a portrait ArtworkRoot or the full-page fitting recipe.
        var root = (RectTransform)v.transform;
        v.FitWidth = false;
        root.anchorMin = root.anchorMax = new Vector2(.5f, .5f);
        root.pivot = new Vector2(.5f, .5f);
        root.anchoredPosition = Vector2.zero;
        root.sizeDelta = new Vector2(860, 240);
        root.localScale = Vector3.one;
        var element = root.GetComponent<LayoutElement>();
        if (element != null)
        {
            element.minHeight = element.preferredHeight = 240;
            element.minWidth = element.preferredWidth = 860;
            element.flexibleHeight = 0;
        }

        var ready = v.GetObject("readyVisual");
        var pending = v.GetObject("pendingVisual");
        RelatedRecordBackground(ready, root);
        RelatedRecordBackground(pending, root);

        // A saved lower surface creates the reference's two-band record card.
        // The ready/pending backgrounds and their state bindings are untouched.
        var detailTransform = root.Find("ReferenceRecordDetail");
        Image detail;
        if (detailTransform == null)
        {
            var node = new GameObject("ReferenceRecordDetail", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            node.layer = root.gameObject.layer;
            detail = node.GetComponent<Image>();
        }
        else detail = detailTransform.GetComponent<Image>();
        if (detail == null) throw new InvalidOperationException("Record detail surface needs an Image.");
        RelatedStretch(detail.rectTransform, root, 9, 76, 9, 9);
        detail.sprite = Artwork("CashWalletInset");
        detail.type = Image.Type.Sliced;
        detail.pixelsPerUnitMultiplier = 5;
        detail.color = Color.white;
        detail.raycastTarget = false;
        detail.transform.SetSiblingIndex(2);

        RelatedRowText(v, "titleLabel", root, 32, 8, 445, 64, 39, TextAlignmentOptions.MidlineLeft);
        RelatedRowText(v, "timeLabel", root, 32, 120, 445, 70, 31, TextAlignmentOptions.MidlineLeft);
        RelatedRowText(v, "amountLabel", root, 490, 108, 335, 108, 70, TextAlignmentOptions.MidlineRight);
        v.GetText("amountLabel").color = v.EmphasisColor;

        var action = v.GetButton("actionButton");
        var actionLabel = v.GetText("actionLabel");
        if (action == null || actionLabel == null)
            throw new InvalidOperationException("Missing history action bindings.");
        RelatedLocal((RectTransform)action.transform, root, 560, 8, 274, 64);
        // In the source prefab the visible TMP label is the Button's Graphic,
        // rather than an invisible hit target detached from the artwork.
        if (actionLabel.transform != action.transform)
            CenterText(actionLabel, (RectTransform)action.transform, 39);
        else
        {
            SetFontSize(actionLabel, 39);
            actionLabel.alignment = TextAlignmentOptions.Center;
        }
        actionLabel.color = v.EmphasisColor;
        actionLabel.fontStyle |= FontStyles.Underline;
        actionLabel.raycastTarget = true;
        action.targetGraphic = actionLabel;
        RelatedRowText(v, "statusLabel", root, 505, 5, 328, 68, 30, TextAlignmentOptions.MidlineRight);
        action.transform.SetAsLastSibling();
    }

    private static void RelatedRecordBackground(GameObject node, RectTransform root)
    {
        if (node == null) throw new InvalidOperationException("Missing history state background.");
        var image = node.GetComponent<Image>();
        if (image == null) throw new InvalidOperationException("History state background needs an Image.");
        RelatedStretch(image.rectTransform, root, 0, 0, 0, 0);
        image.sprite = Artwork("CashWalletInset");
        image.type = Image.Type.Sliced;
        image.pixelsPerUnitMultiplier = 4;
        image.color = new Color32(255, 217, 158, 255);
        image.raycastTarget = false;
        image.transform.SetAsFirstSibling();
    }

    private static void RelatedRowText(ApkEarningPageView v, string key, RectTransform root,
        float x, float y, float w, float h, float size, TextAlignmentOptions alignment)
    {
        var text = v.GetText(key);
        if (text == null) throw new InvalidOperationException("Missing history text: " + key);
        RelatedLocal(text.rectTransform, root, x, y, w, h);
        SetFontSize(text, size);
        text.alignment = alignment;
        text.raycastTarget = false;
        text.transform.SetAsLastSibling();
    }

    private static void Reward(ApkEarningPageView v)
    {
        Prepare(v);
        // Settlement uses a full-page light surface like the reference, while
        // the orchard remains visible through it. A borderless wash lets the
        // original brown amount/claim text stay legible over the purple tree.
        var background = v.GetImage("bg");
        if (background == null) throw new InvalidOperationException("Missing reward background.");
        background.gameObject.SetActive(true);
        background.enabled = true;
        background.sprite = null;
        background.type = Image.Type.Simple;
        background.color = new Color(1, .972f, .886f, .82f);
        background.raycastTarget = false;
        Rect(v, background.rectTransform, 0, 0, 1080, 1920);
        background.transform.SetAsFirstSibling();
        Header(v, "main/Title", "labels[2].Text", 100, 342, 880, 178);
        SetFontSize(v.GetText("labels[2].Text"), 72);

        var title = v.GetImage("main/Title");
        Transform crest = title.transform.Find("OrchardReceiptCrest");
        if (crest == null) crest = v.ArtworkRoot.Find("OrchardReceiptCrest");
        if (crest == null) crest = title.transform.Find("ScrewDecoration");
        if (crest == null) crest = v.ArtworkRoot.Find("ScrewDecoration");
        if (crest != null)
        {
            Rect(v, (RectTransform)crest, 433, 172, 214, 220);
            crest.SetAsLastSibling();
        }
        Text(v, "TestMode", 190, 555, 700, 38, 28);

        var items = v.GetObject("Items");
        if (items == null) throw new InvalidOperationException("Missing reward item group.");
        Rect(v, (RectTransform)items.transform, 110, 727, 860, 470);
        var layout = items.GetComponent<HorizontalLayoutGroup>();
        if (layout == null) throw new InvalidOperationException("Reward page needs its configured item layout.");
        layout.enabled = true;
        layout.padding = new RectOffset();
        layout.spacing = 48;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = layout.childControlHeight = false;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;
        layout.childScaleWidth = layout.childScaleHeight = false;
        // A single cash item and a cash/diamond pair both center automatically;
        // no reward amount, item activation or runtime population is changed.
        items.transform.localScale = Vector3.one * 1.12f;
        items.transform.SetAsLastSibling();

        Button(v, "MoreGetBtn", 160, 1320, 760, 190, 76);
        CenterText(v.GetText("moreLabel"), (RectTransform)v.GetButton("MoreGetBtn").transform, 76);
        var video = v.GetImage("main/MoreGetBtn/ad");
        if (video != null)
        {
            RelatedLocal(video.rectTransform, (RectTransform)v.GetButton("MoreGetBtn").transform, 56, 60, 78, 70);
            video.raycastTarget = false;
            video.preserveAspect = true;
            video.transform.SetAsLastSibling();
        }

        var claim = v.GetButton("GetBtn");
        var claimText = v.GetText("labels[1].Text");
        if (claim == null || claimText == null)
            throw new InvalidOperationException("Missing ordinary claim button.");
        Rect(v, (RectTransform)claim.transform, 300, 1616, 480, 92);
        if (claimText.transform != claim.transform)
            CenterText(claimText, (RectTransform)claim.transform, 48);
        else
        {
            SetFontSize(claimText, 48);
            claimText.alignment = TextAlignmentOptions.Center;
        }
        claimText.fontStyle |= FontStyles.Underline;
        claimText.raycastTarget = true;
        claim.targetGraphic = claimText;
        claim.transform.SetAsLastSibling();

        // Runtime chooses whether this existing control is available. Keep its
        // saved state rather than introducing a second way to accept a reward.
        var close = v.GetButton("Close");
        if (close != null)
        {
            bool active = close.gameObject.activeSelf;
            CloseButton(v, "Close", 938, 210, 104);
            close.gameObject.SetActive(active);
        }
    }

    private static void RelatedLocal(RectTransform rect, RectTransform parent,
        float x, float y, float width, float height)
    {
        if (rect == null || parent == null || rect == parent)
            throw new InvalidOperationException("Invalid nested reference layout target.");
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static void RelatedStretch(RectTransform rect, RectTransform parent,
        float left, float top, float right, float bottom)
    {
        if (rect == null || parent == null || rect == parent)
            throw new InvalidOperationException("Invalid nested reference stretch target.");
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(.5f, .5f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }
}
