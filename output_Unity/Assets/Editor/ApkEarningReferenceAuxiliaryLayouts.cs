using System;
using FruitApkEarning;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Saved layouts for the auxiliary pages in the earning flow. Channel, review
/// and account-level section positions are based on the reference project's
/// 1080 x 1920 prefabs. Recovery and test-video are local-only pages: they use
/// the same readable content/action hierarchy without claiming APK parity.
/// </summary>
public static partial class ApkEarningReferenceLayoutAuthoring
{
    public static void ApplyAuxiliaryPages()
    {
        Edit("LocalUserLevel", AuxiliaryAccountLevel);
        Edit("LocalRewardVideoTest", AuxiliaryTestVideo);
        Edit("LocalEarningRecovery", AuxiliaryRecovery);
        Edit("TXChannelPanel", AuxiliaryChannel);
        Edit("TXTooFastHintPanel", AuxiliaryRequirementsReview);
    }

    private static void AuxiliaryAccountLevel(ApkEarningPageView v)
    {
        Prepare(v);
        // Source: a centered level summary, one progress row, then Continue.
        // The status sentence stays in its own row rather than crowding a badge.
        AuxiliaryImage(v, "Card", new Rect(62, 552, 956, 850), "CashWalletPanel", 4);
        AuxiliaryHeader(v, "Heading", "Title", new Rect(170, 518, 740, 154), 62);
        AuxiliaryLabel(v, "levelLabel", new Rect(150, 724, 780, 128), 84);
        AuxiliaryLabel(v, "ProgressCaption", new Rect(150, 884, 780, 64), 42);
        AuxiliaryProgress(v, "ProgressTrack", "progressFill", "progressLabel", new Rect(120, 982, 840, 78), 34);
        // The Host currently provides a status sentence, not a numeric fraction.
        // Give it a separate readable line below the empty progress track.
        AuxiliaryLabel(v, "progressLabel", new Rect(150, 1082, 780, 54), 34);
        AuxiliaryCopyTextAppearance(v, "progressLabel", "ProgressCaption");
        AuxiliaryAction(v, "continueButton", "Caption", new Rect(158, 1165, 764, 166), 64);
        AuxiliaryClose(v, "closeButton", new Rect(912, 532, 108, 108));
        // This source page has no local TestMode slot. The Host supplies its
        // existing global disclosure; no duplicate caption is created here.
    }

    private static void AuxiliaryChannel(ApkEarningPageView v)
    {
        Prepare(v);
        // Source sections: title, full-width instructions, account field,
        // confirmation. Use the full instruction width: there is no portrait.
        AuxiliaryImage(v, "main/Image", new Rect(62, 323, 956, 1199), "CashWalletPanel", 3);
        AuxiliaryHeader(v, "Title", "labels[1].Text", new Rect(205, 338, 670, 124), 48);
        AuxiliaryLabel(v, "labels[0].Text", new Rect(129, 550, 822, 264), 46);

        TMP_InputField input = v.GetInput("InputInfo");
        if (input == null || input.textViewport == null || input.textComponent == null)
            throw new InvalidOperationException("Channel account field bindings are incomplete.");
        AuxiliaryPosition(v, (RectTransform)input.transform, new Rect(129, 930, 822, 176));
        Image face = input.GetComponent<Image>();
        if (face == null) throw new InvalidOperationException("Channel account field has no visible input face.");
        AuxiliaryStyle(face, "CashWalletInset", 4);
        input.targetGraphic = face;
        face.raycastTarget = true;
        AuxiliaryStretch(input.textViewport, new Vector2(36, 18), new Vector2(-36, -18));
        AuxiliaryStretch(input.textComponent.rectTransform, Vector2.zero, Vector2.zero);
        AuxiliaryInputText(input.textComponent);
        if (input.placeholder is TMP_Text placeholder)
        {
            AuxiliaryStretch(placeholder.rectTransform, Vector2.zero, Vector2.zero);
            AuxiliaryInputText(placeholder);
        }
        // Keep the input's existing content validation and single-line editing
        // behavior. Only its viewport and visible content area are adjusted.
        AuxiliaryAction(v, "SureBtn", "labels[2].Text", new Rect(158, 1232, 764, 192), 68);
        AuxiliaryClose(v, "CloseBtn", new Rect(904, 330, 108, 108));
        AuxiliaryFooter(v, 1460, 26);
    }

    private static void AuxiliaryRequirementsReview(ApkEarningPageView v)
    {
        Prepare(v);
        // Source task rows are at y=874/969 and y=1107/1198. Restore those
        // sections and the bottom action while removing the old portrait gap.
        AuxiliaryImage(v, "bg", new Rect(62, 422, 956, 1199), "CashWalletPanel", 3);
        foreach (string key in new[] { "bg (1)", "bg (2)", "bg (3)" })
            HideImage(v, key);
        AuxiliaryHeader(v, "Title", "labels[1].Text", new Rect(160, 396, 760, 150), 54);
        AuxiliaryLabel(v, "labels[0].Text", new Rect(126, 606, 828, 224), 42);
        AuxiliaryLabel(v, "LevelTip", new Rect(126, 874, 828, 70), 42);
        AuxiliaryProgress(v, "main/LevelPregress", "LevelPregress", "LevelPregressValue",
            new Rect(120, 969, 840, 73), 40);
        AuxiliaryLabel(v, "VideoTip", new Rect(126, 1107, 828, 70), 40);
        AuxiliaryProgress(v, "main/VideoPregress", "VideoPregress", "VideoPregressValue",
            new Rect(120, 1198, 840, 73), 40);
        // A zero-progress track is cream, so use the existing brown task text
        // appearance for both fractions, including the portion beyond the fill.
        AuxiliaryCopyTextAppearance(v, "LevelPregressValue", "LevelTip");
        AuxiliaryCopyTextAppearance(v, "VideoPregressValue", "LevelTip");
        AuxiliaryAction(v, "IKnowBtn", "labels[2].Text", new Rect(158, 1364, 764, 192), 68);
        // Like LocalUserLevel, this page retains the Host's global disclosure.
    }

    private static void AuxiliaryTestVideo(ApkEarningPageView v)
    {
        Prepare(v);
        AuxiliaryHideLegacyRows(v);
        AuxiliaryImage(v, "main/Image", new Rect(62, 415, 956, 1090), "CashWalletPanel", 3);
        AuxiliaryImage(v, "main/Image (1)", new Rect(118, 564, 844, 400), "CashWalletInset", 4);
        AuxiliaryHeader(v, "Title1", "title", new Rect(150, 370, 780, 150), 54);
        AuxiliaryLabel(v, "body", new Rect(156, 590, 768, 348), 44);
        AuxiliaryAction(v, "retry", "actionCaption", new Rect(158, 1030, 764, 170), 56);
        AuxiliaryAction(v, "close", "cancelCaption", new Rect(268, 1250, 544, 114), 46, false);
        AuxiliaryClose(v, "Close", new Rect(912, 390, 108, 108));
        AuxiliaryFooter(v, 1414, 28);
    }

    private static void AuxiliaryRecovery(ApkEarningPageView v)
    {
        Prepare(v);
        AuxiliaryHideLegacyRows(v);
        AuxiliaryImage(v, "main/Image", new Rect(62, 380, 956, 1190), "CashWalletPanel", 3);
        AuxiliaryImage(v, "main/Image (1)", new Rect(118, 534, 844, 480), "CashWalletInset", 4);
        AuxiliaryHeader(v, "Title1", "title", new Rect(140, 335, 800, 150), 50);
        AuxiliaryLabel(v, "body", new Rect(156, 564, 768, 420), 40);
        AuxiliaryAction(v, "retry", "actionCaption", new Rect(158, 1060, 764, 170), 62);
        // This local page's secondary label has no alias; use the existing
        // descendant on the bound standard Button, without making a new label.
        AuxiliaryAction(v, "close", null, new Rect(238, 1280, 604, 114), 44, false);
        AuxiliaryClose(v, "Close", new Rect(912, 355, 108, 108));
        AuxiliaryFooter(v, 1450, 28);
    }

    private static void AuxiliaryPosition(ApkEarningPageView v, RectTransform rect, Rect bounds)
    {
        // Position through ArtworkRoot without flattening the form or state
        // hierarchy. Every saved node and every runtime alias is retained.
        FollowupPosition(v, rect, bounds, new Vector2(.5f, .5f));
    }

    private static void AuxiliaryStyle(Image image, string sprite, float ppu)
    {
        if (image == null) throw new InvalidOperationException("Missing auxiliary page Image.");
        image.gameObject.SetActive(true);
        image.enabled = true;
        image.sprite = Artwork(sprite);
        image.color = Color.white;
        image.type = Image.Type.Sliced;
        image.preserveAspect = false;
        image.pixelsPerUnitMultiplier = ppu;
        image.raycastTarget = image.GetComponent<Button>() != null;
    }

    private static void AuxiliaryImage(ApkEarningPageView v, string key, Rect bounds, string sprite, float ppu)
    {
        Image image = v.GetImage(key);
        AuxiliaryStyle(image, sprite, ppu);
        AuxiliaryPosition(v, image.rectTransform, bounds);
    }

    private static void AuxiliaryLabel(ApkEarningPageView v, string key, Rect bounds, float size)
    {
        TMP_Text text = v.GetText(key);
        if (text == null) throw new InvalidOperationException("Missing auxiliary text binding: " + key);
        AuxiliaryPosition(v, text.rectTransform, bounds);
        text.gameObject.SetActive(true);
        text.enabled = true;
        text.enableAutoSizing = true;
        text.enableWordWrapping = true;
        text.margin = Vector4.zero;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        SetFontSize(text, size);
    }

    private static void AuxiliaryHeader(ApkEarningPageView v, string imageKey, string textKey, Rect bounds, float fontSize)
    {
        AuxiliaryImage(v, imageKey, bounds, "CashWalletHeader", 1);
        Image image = v.GetImage(imageKey);
        image.type = Image.Type.Simple;
        Transform accent = image.transform.Find("OrchardAppleAccent");
        if (accent != null) accent.gameObject.SetActive(false);
        AuxiliaryLabel(v, textKey, new Rect(bounds.x + 40, bounds.y + 24, bounds.width - 80, bounds.height - 48), fontSize);
    }

    private static void AuxiliaryAction(ApkEarningPageView v, string buttonKey, string labelKey, Rect bounds, float size, bool primary = true)
    {
        Button button = v.GetButton(buttonKey);
        if (button == null) throw new InvalidOperationException("Missing auxiliary Button: " + buttonKey);
        Image image = button.GetComponent<Image>();
        AuxiliaryStyle(image, primary ? "CashWalletButton" : "CashWalletInset", primary ? 2 : 4);
        AuxiliaryPosition(v, (RectTransform)button.transform, bounds);
        if (primary) image.pixelsPerUnitMultiplier = image.sprite.rect.height / bounds.height / image.pixelsPerUnit;
        button.targetGraphic = image;
        TMP_Text label = labelKey != null ? v.GetText(labelKey) : button.GetComponentInChildren<TMP_Text>(true);
        if (label == null) throw new InvalidOperationException("Missing visible auxiliary Button label: " + buttonKey);
        // The visual caption must remain inside the standard Button.
        CenterText(label, (RectTransform)button.transform, size);
        label.enableAutoSizing = true;
        label.enableWordWrapping = true;
        label.margin = Vector4.zero;
    }

    private static void AuxiliaryClose(ApkEarningPageView v, string key, Rect bounds)
    {
        Button button = v.GetButton(key);
        if (button == null) throw new InvalidOperationException("Missing auxiliary close Button: " + key);
        Image image = button.GetComponent<Image>();
        AuxiliaryStyle(image, "CashWalletClose", 1);
        image.type = Image.Type.Simple;
        AuxiliaryPosition(v, (RectTransform)button.transform, bounds);
        button.targetGraphic = image;
    }

    private static void AuxiliaryProgress(ApkEarningPageView v, string trackKey, string fillKey, string textKey, Rect bounds, float size)
    {
        Image track = v.GetImage(trackKey);
        Image fill = v.GetImage(fillKey);
        if (track == null || fill == null || track == fill)
            throw new InvalidOperationException("Auxiliary progress must have separate track/fill bindings.");
        AuxiliaryStyle(track, "CashWalletProgressTrack", 2);
        AuxiliaryPosition(v, track.rectTransform, bounds);
        track.pixelsPerUnitMultiplier = track.sprite.rect.height / bounds.height / track.pixelsPerUnit;
        AuxiliaryStyle(fill, "CashWalletProgressFill", 2);
        // The level page imported these as siblings. Save the fill inside its
        // track so standard width updates stay left-aligned at every resolution.
        fill.transform.SetParent(track.transform, false);
        RectTransform rect = fill.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0, .5f);
        rect.pivot = new Vector2(0, .5f);
        rect.anchoredPosition = new Vector2(12, 0);
        rect.sizeDelta = new Vector2(bounds.width - 24, bounds.height - 20);
        rect.localScale = Vector3.one;
        fill.pixelsPerUnitMultiplier = fill.sprite.rect.height / rect.sizeDelta.y / fill.pixelsPerUnit;
        foreach (var slot in v.Images)
            if (slot.Target == fill)
            {
                slot.ResizeWidthWithProgress = true;
                slot.FullProgressWidth = bounds.width - 24;
                slot.FullProgressHeight = bounds.height - 20;
                slot.FullProgressPixelsPerUnit = fill.pixelsPerUnitMultiplier;
            }
        CenterText(v.GetText(textKey), track.rectTransform, size);
        // Re-applying the recipe must not put the fill in front of its value.
        fill.transform.SetAsFirstSibling();
        v.GetText(textKey).transform.SetAsLastSibling();
    }

    private static void AuxiliaryFooter(ApkEarningPageView v, float y, float h)
    {
        AuxiliaryLabel(v, "TestMode", new Rect(160, y, 760, h), 22);
    }

    private static void AuxiliaryCopyTextAppearance(ApkEarningPageView v, string targetKey, string sourceKey)
    {
        TMP_Text target = v.GetText(targetKey);
        TMP_Text source = v.GetText(sourceKey);
        if (target == null || source == null)
            throw new InvalidOperationException("Missing existing auxiliary text appearance.");
        target.fontSharedMaterial = source.fontSharedMaterial;
        target.color = source.color;
        target.enableVertexGradient = source.enableVertexGradient;
        target.colorGradient = source.colorGradient;
    }

    private static void AuxiliaryStretch(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(.5f, .5f);
        rect.offsetMin = min;
        rect.offsetMax = max;
        rect.localScale = Vector3.one;
    }

    private static void AuxiliaryInputText(TMP_Text text)
    {
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.margin = Vector4.zero;
        text.raycastTarget = false;
        SetFontSize(text, 44);
        text.enableAutoSizing = true;
    }

    private static void AuxiliaryHideLegacyRows(ApkEarningPageView v)
    {
        for (int i = 0; i < 7; i++)
        {
            GameObject row = v.GetObject("rows[" + i + "].Root");
            if (row == null) throw new InvalidOperationException("Missing inherited auxiliary row binding.");
            row.SetActive(false);
        }
        foreach (string key in new[] { "subtitle", "notice" })
        {
            TMP_Text label = v.GetText(key);
            if (label != null) label.gameObject.SetActive(false);
        }
        foreach (string key in new[] { "main/1", "main/2", "main/Image (2)", "main/ItemIcon", "main/ItemCount",
            "main/Items", "main/Progress", "main/Tip", "main/PlayerInfoPos", "main/Image/label_37" })
        {
            GameObject legacy = v.GetObject(key);
            if (legacy != null) legacy.SetActive(false);
        }
    }
}
