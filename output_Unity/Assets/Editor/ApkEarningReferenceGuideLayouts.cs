using System;
using FruitApkEarning;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Saved layout recipes for the welcome, reward guidance, loading and notice
/// pages actually opened by ApkEarningHost. Existing artwork and direct bindings
/// are retained; imported, unreachable tutorial prototypes are not activated.
/// </summary>
public static partial class ApkEarningReferenceLayoutAuthoring
{
    public static void ApplyGuidePages()
    {
        Edit("TXRecordGuidePanel", GuideWelcome);
        Edit("TXGuideTargetCompletePanel", GuideTargetComplete);
        Edit("TXGuideLoadingPanel", GuideLoading);
        Edit("LocalDiamondUnlockGuide", GuideDiamondUnlock);
        Edit("Tip", GuideToast);
        // NewbieGuidePanel, RewardPanel and TXLevelPanel are imported prototypes.
        // The host uses LocalDiamondUnlockGuide, LocalSuccess/LocalLuckyReward
        // and LocalUserLevel respectively. Keep those unused sources unchanged.
    }

    private static void GuideWelcome(ApkEarningPageView view)
    {
        Prepare(view);
        GuidePanel(view, "main/Image", new Rect(48, 700, 984, 550), "CashWalletPanel", 3);
        GuideHeader(view, "main/Title", "labels[0].Text", new Rect(160, 545, 760, 160), 64);
        GuideText(view, "tip", new Rect(95, 755, 890, 140), 50);

        // The source's horizontal layout computes the five nodes at runtime.
        // Bake its evenly spaced result, including the four meaningful labels,
        // instead of copying the overlapping on-disk RectTransform cache.
        GuidePlace(view, "main/HideLevel", new Rect(48, 920, 984, 260));
        GuidePlace(view, "main/HideLevel/Levels", new Rect(48, 920, 984, 260));
        GuideProgress(view, "main/HideLevel/Progress", "main/HideLevel/Progress/Bar",
            new Rect(153, 980, 774, 58));
        string[] nodes = { "1", "2", "3", "4", "7" };
        for (int i = 0; i < nodes.Length; i++)
        {
            float x = 90 + i * 195;
            string path = "main/HideLevel/Levels/" + nodes[i];
            GuidePlace(view, path, new Rect(x, 950, 120, 135));
            GuidePlace(view, path + "/BG", new Rect(x + 4, i == 4 ? 931 : 950, 112, i == 4 ? 137 : 118));
            GuidePlace(view, path + "/Level", new Rect(x + 17, 960, 86, 90), 62);
            RectTransform badge = FollowupNode(view, path + "/Get");
            badge.gameObject.SetActive(i != 2);
            GuidePlace(view, path + "/Get", new Rect(x - 25, 1085, 170, 70));
            string labelPath = path + (i == 4 ? "/Get/label_160" : "/Get/label_1");
            GuidePlace(view, labelPath, new Rect(x - 19, 1096, 158, 48), i == 4 ? 25 : 31);
            TMP_Text label = FollowupNode(view, labelPath).GetComponent<TMP_Text>();
            label.enableWordWrapping = false;
            Image badgeImage = badge.GetComponent<Image>();
            if (badgeImage != null) GuideFitSlice(badgeImage, 70);
        }
        GuideButton(view, "startButton", new Rect(243.65f, 1362, 592.7f, 192), 72);
        GuideText(view, "TestMode", new Rect(215, 1200, 650, 28), 23);
    }

    private static void GuideTargetComplete(ApkEarningPageView view)
    {
        Prepare(view);
        // Source TXGuideTargetCompletePanel: separate heading, reward inset and
        // bottom action. Keep the orchard header compact instead of stretching
        // it across the source's baked illustration area (896 x 362).
        GuidePanel(view, "main/Image", new Rect(92.5f, 383, 895, 1076), "CashWalletPanel", 3);
        GuidePanel(view, "main/Image (1)", new Rect(163.5f, 553, 753, 556), "CashWalletInset", 4);
        view.GetImage("main/Image (1)").transform.SetAsLastSibling();
        GuideHeader(view, "main/Title", "labels[1].Text", new Rect(140, 300, 800, 160), 61);
        GuidePlace(view, "main/Image (2)", new Rect(326, 564.5f, 428, 425));
        Image glow = view.GetImage("main/Image (2)");
        glow.enabled = true; glow.transform.SetAsLastSibling();
        Image icon = view.GetImage("goldImage");
        FollowupPosition(view, icon.rectTransform, new Rect(327, 638, 426, 258), new Vector2(.5f, .5f));
        icon.preserveAspect = true; icon.transform.SetAsLastSibling();
        GuideText(view, "tip", new Rect(184.89f, 909.27f, 710.23f, 161.45f), 112);
        GuideButton(view, "continueButton", new Rect(158, 1178, 764, 192), 76);
        GuideText(view, "TestMode", new Rect(190, 1396, 700, 28), 22);
        // The action itself remains the visual and interactive Button. The old
        // hand covers its caption and is not needed for a single visible action.
        GameObject hand = view.GetObject("path:main/TXbtn/Hand");
        if (hand != null) hand.SetActive(false);
    }

    private static void GuideLoading(ApkEarningPageView view)
    {
        Prepare(view);
        // The reference is a short, wide transient message, not a full wallet.
        GuidePanel(view, "main/Image", new Rect(48, 714, 984, 460), "CashWalletPanel", 3);
        GuideText(view, "label", new Rect(110, 815, 860, 138), 54);
        GuideProgress(view, "main/Progress", "Progress", new Rect(125, 1036, 830, 60));
        GuideText(view, "TestMode", new Rect(230, 1110, 620, 30), 23);
    }

    private static void GuideDiamondUnlock(ApkEarningPageView view)
    {
        Prepare(view);
        // Preserve the configured target's geometry, its standard Button and
        // its complete DiamondCard visual hierarchy. An older source tutorial
        // points at a different HUD; those coordinates cannot be copied here.
        RectTransform target = (RectTransform)view.GetObject("Pos").transform;
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Vector3 bottom = view.ArtworkRoot.InverseTransformPoint(corners[0]);
        Vector3 top = view.ArtworkRoot.InverseTransformPoint(corners[1]);
        float targetTop = view.ArtworkRoot.rect.yMax - top.y;
        float targetBottom = view.ArtworkRoot.rect.yMax - bottom.y;
        GuidePanel(view, "ApkArtwork/Tip", new Rect(72, targetTop - 115, 936, 650), "CashWalletPanel", 3);
        GuideText(view, "body", new Rect(150, targetBottom + 55, 780, 170), 45);
        GuideText(view, "TestMode", new Rect(190, targetTop + 445, 700, 30), 23);
        // This shared text has both title/body aliases; leave one readable
        // paragraph, not a duplicated heading generated from a second binding.
        Transform accent = FollowupNode(view, "ApkArtwork/Tip").Find("YellowScrewDecoration");
        if (accent != null) accent.gameObject.SetActive(false);
        foreach (string key in new[] { "Hand", "Arraw" })
        {
            GameObject decoration = view.GetObject(key);
            if (decoration != null) decoration.SetActive(false);
        }
        target.SetAsLastSibling();
        var button = view.GetButton("continueButton");
        if (button == null || button.targetGraphic == null || !button.targetGraphic.transform.IsChildOf(button.transform))
            throw new InvalidOperationException("The diamond guide must retain a Button with its own child visual.");
    }

    private static void GuideToast(ApkEarningPageView view)
    {
        // Toast is a compact root, not a full-screen page. Its anchoring follows
        // the live canvas width, with safe edge padding even on narrow displays.
        var rect = (RectTransform)view.transform;
        view.FitWidth = false;
        rect.anchorMin = new Vector2(.06f, .5f);
        rect.anchorMax = new Vector2(.94f, .5f);
        rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0, 220);
        rect.localScale = Vector3.one;
        var image = view.GetImage("$root");
        image.raycastTarget = false;
        var text = view.GetText("content");
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = new Vector2(42, 28);
        text.rectTransform.offsetMax = new Vector2(-42, -28);
        text.rectTransform.localScale = Vector3.one;
        text.enableAutoSizing = true;
        text.enableWordWrapping = true;
        text.alignment = TextAlignmentOptions.Center;
        text.margin = Vector4.zero;
        SetFontSize(text, 38);
        text.fontSizeMin = 28;
        text.raycastTarget = false;
    }

    private static void GuidePlace(ApkEarningPageView view, string path, Rect bounds, float fontSize = 0)
    {
        FollowupPlace(view, path, bounds, fontSize);
    }

    private static void GuideText(ApkEarningPageView view, string key, Rect bounds, float fontSize)
    {
        TMP_Text text = view.GetText(key);
        if (text == null) throw new InvalidOperationException("Guide text binding is missing: " + key);
        FollowupPosition(view, text.rectTransform, bounds, new Vector2(.5f, .5f));
        text.gameObject.SetActive(true); text.enabled = true;
        text.enableAutoSizing = true; text.enableWordWrapping = true;
        text.alignment = TextAlignmentOptions.Center;
        text.margin = Vector4.zero; text.raycastTarget = false;
        SetFontSize(text, fontSize);
        text.transform.SetAsLastSibling();
    }

    private static void GuidePanel(ApkEarningPageView view, string key, Rect bounds, string artwork, float ppu)
    {
        Image image = view.GetImage(key);
        if (image == null)
        {
            image = FollowupNode(view, key).GetComponent<Image>();
        }
        if (image == null) throw new InvalidOperationException("Guide panel binding is missing: " + key);
        image.gameObject.SetActive(true); image.enabled = true;
        image.sprite = Artwork(artwork); image.type = Image.Type.Sliced;
        image.preserveAspect = false; image.pixelsPerUnitMultiplier = ppu;
        image.raycastTarget = image.GetComponent<Button>() != null;
        FollowupPosition(view, image.rectTransform, bounds, new Vector2(.5f, .5f));
        image.transform.SetAsFirstSibling();
    }

    private static void GuideHeader(ApkEarningPageView view, string imageKey, string textKey, Rect bounds, float size)
    {
        Header(view, imageKey, textKey, bounds.x, bounds.y, bounds.width, bounds.height);
        FollowupPosition(view, view.GetImage(imageKey).rectTransform, bounds, new Vector2(.5f, .5f));
        GuideText(view, textKey, new Rect(bounds.x + 42, bounds.y + 24, bounds.width - 84, bounds.height - 48), size);
        Transform accent = view.GetImage(imageKey).transform.Find("OrchardAppleAccent");
        if (accent != null) accent.gameObject.SetActive(false);
    }

    private static void GuideButton(ApkEarningPageView view, string key, Rect bounds, float size)
    {
        Button(view, key, bounds.x, bounds.y, bounds.width, bounds.height, size);
        FollowupPosition(view, (RectTransform)view.GetButton(key).transform, bounds, new Vector2(.5f, .5f));
    }

    private static void GuideProgress(ApkEarningPageView view, string trackKey, string fillKey, Rect bounds)
    {
        Image track = view.GetImage(trackKey), fill = view.GetImage(fillKey);
        if (track == null || fill == null) throw new InvalidOperationException("Guide progress binding is missing.");
        FollowupPosition(view, track.rectTransform, bounds, new Vector2(.5f, .5f));
        track.type = Image.Type.Sliced; track.preserveAspect = false;
        GuideFitSlice(track, bounds.height);
        RectTransform rect = fill.rectTransform;
        rect.SetParent(track.transform, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0, .5f);
        rect.pivot = new Vector2(0, .5f);
        rect.anchoredPosition = new Vector2(10, 0);
        rect.sizeDelta = new Vector2(bounds.width - 20, bounds.height - 16);
        rect.localScale = Vector3.one;
        fill.type = Image.Type.Sliced; fill.preserveAspect = false;
        GuideFitSlice(fill, bounds.height - 16);
        foreach (var slot in view.Images)
            if (slot.Target == fill)
            {
                slot.ResizeWidthWithProgress = true;
                slot.FullProgressWidth = bounds.width - 20;
                slot.FullProgressHeight = bounds.height - 16;
                slot.FullProgressPixelsPerUnit = fill.pixelsPerUnitMultiplier;
            }
    }

    private static void GuideFitSlice(Image image, float height)
    {
        if (image.sprite == null || image.sprite.border.sqrMagnitude <= 0) return;
        image.type = Image.Type.Sliced;
        image.pixelsPerUnitMultiplier = image.sprite.rect.height / height / image.pixelsPerUnit;
    }
}
