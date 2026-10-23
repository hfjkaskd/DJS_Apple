using System;
using FruitApkEarning;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Restores the reference page compositions into saved prefabs. Coordinates were
/// measured from C:/Projects/com.nut.sort.relax/Assets/Resources/prefabs/panels,
/// at the source 1080 x 1920 design. Existing orchard sprites and state bindings
/// are retained. Baked illustration space is reflowed for the orchard panels,
/// and English text receives its own usable width. The runtime hierarchy stays intact.
/// </summary>
public static partial class ApkEarningReferenceLayoutAuthoring
{
    public static void ApplyFollowupPages()
    {
        Edit("TXUserInfoSurePanel", FollowupAccountConfirmation);
        Edit("LocalWithdrawalVerification", FollowupVerification);
        Edit("TXProgress0Panel", FollowupStage);
        Edit("LocalEarningSequence", FollowupSequence);
        // LocalDiamondUnlockGuide intentionally retains its current layout.
        // The source OriginalNewbieGuideView.ShowCoinGuide positions its live
        // target at rewardsTop.Coin. The orchard HUD has different geometry;
        // copying the initial import's old coordinates would highlight the
        // wrong place. A source screenshot of the preceding reward preview
        // is not evidence for that interactive guide's final placement.
    }

    private static void FollowupAccountConfirmation(ApkEarningPageView view)
    {
        FollowupRestore(view, ConfirmationReference);
        // Source btnParent has MiddleCenter vertical layout, two 764 x 192
        // buttons and 50.68 spacing. Its disk RectTransform cache overlaps;
        // these are the actual positions after that original layout runs.
        // The orchard frame needs extra bottom padding, so move both controls
        // up 30 px and shorten the secondary control without changing its binding.
        FollowupPosition(view, (RectTransform)view.GetButton("GetBtn").transform,
            new Rect(158, 1088.66f, 764, 192), new Vector2(.5f, .5f));
        FollowupPosition(view, (RectTransform)view.GetButton("ReEnterBtn").transform,
            new Rect(158, 1331.34f, 764, 160), new Vector2(.5f, .5f));
        CenterText(view.GetText("labels[1].Text"), (RectTransform)view.GetButton("GetBtn").transform, 70);
        CenterText(view.GetText("labels[2].Text"), (RectTransform)view.GetButton("ReEnterBtn").transform, 70);
        FollowupVisibleImage(view, "main/bg");
        FollowupVisibleImage(view, "main/bg1");
        FollowupVisibleImage(view, "main/Title");
        FollowupPlace(view, "main/bg1", new Rect(116, 792.49f, 848, 235.02f));
        FollowupPlace(view, "main/Tip", new Rect(146, 819.28f, 788, 180.65f), 50);
        FollowupFooter(view, 1512, 26);
    }

    private static void FollowupVerification(ApkEarningPageView view)
    {
        FollowupRestore(view, VerificationReference);
        FollowupVisibleImage(view, "main/TitleTip");
        FollowupVisibleImage(view, "main/Image");
        FollowupVisibleImage(view, "main/Image (1)");
        FollowupVisibleImage(view, "main/Image (2)");
        // Keep the amount inset inside the painted outer frame, rather than
        // covering its top bevel as the old all-in-one illustration did.
        FollowupPlace(view, "main/Image (1)", new Rect(116, 548, 848, 327));
        FollowupPlace(view, "main/Title1", new Rect(220, 566, 640, 84));
        FollowupPlace(view, "main/Title1/label_37", new Rect(255, 572, 570, 72), 54);
        CenterText(view.GetText("labels[1].Text"), (RectTransform)view.GetButton("SureBtn").transform, 60);
        FollowupFooter(view, 1403, 26);
    }

    private static void FollowupStage(ApkEarningPageView view)
    {
        FollowupRestore(view, StageReference);
        FollowupVisibleImage(view, "main/Image");
        FollowupVisibleImage(view, "main/Title");
        FollowupVisibleImage(view, "main/Step/3/Content/Image (2)");
        // The source aux-steps-panel.png bakes a portrait into the left column.
        // The orchard panel has no portrait: use the full width for English.
        FollowupPlace(view, "main/Title", new Rect(160, 284, 760, 112));
        FollowupPlace(view, "main/Title/label_42", new Rect(190, 298, 700, 84), 52);
        FollowupPlace(view, "main/label_43", new Rect(150, 436, 780, 116), 44);
        FollowupPlace(view, "main/KeFu", new Rect(160, 580, 760, 54));
        FollowupPlace(view, "main/KeFu/Value", new Rect(170, 584, 740, 46), 32);
        FollowupPlace(view, "main/Sure", new Rect(158, 1430, 764, 192));
        CenterText(view.GetText("labels[1].Text"), (RectTransform)view.GetButton("SureBtn").transform, 60);
        FollowupFooter(view, 1392, 28);
    }

    private static void FollowupSequence(ApkEarningPageView view)
    {
        FollowupRestore(view, SequenceReference);
        // The reference assessment has one paragraph, not two overlapping
        // title/body labels at the same baseline. Other message styles retain
        // their existing distinct title and body bindings.
        view.GetText("messageLayouts[0].Title").gameObject.SetActive(false);
        IllustratedMessageCards(view);
        // The imported avatar and player ribbon are intentionally hidden.
        // Compact their vacant area instead of introducing replacement decoration.
        FollowupPlace(view, "Bonuses", new Rect(40, 270, 1000, 1380));
        FollowupPlace(view, "Bonuses/Heading", new Rect(86, 330, 908, 100));
        FollowupPlace(view, "Bonuses/Heading/Title", new Rect(126, 346, 828, 68), 43);
        for (int i = 0; i < 5; i++)
        {
            string path = "Bonuses/Bonus" + (i + 1);
            float top = 470 + i * 220;
            FollowupPlace(view, path, new Rect(80, top, 920, 200));
            FollowupPlace(view, path + "/Icon", new Rect(i == 4 ? 492 : 436, top + 23,
                i == 4 ? 96 : 208, 112));
            FollowupPlace(view, path + "/Caption", new Rect(130, top + 138, 820, 52), 38);
        }
        FollowupPlace(view, "Bonuses/Bonus3/Icon/OrchardRewardMultiplier", new Rect(660, 961, 110, 54), 38);
        FollowupPlace(view, "Bonuses/Bonus5/Icon/LevelNumber", new Rect(507, 1411, 66, 62), 48);
        // Preserve Message/Bonuses/Receipt group visibility, bonus CanvasGroups,
        // flight-icon visibility and every animated parent/child relationship.
        // This page intentionally uses the Host's existing test-mode footer.
    }

    private readonly struct FollowupBox
    {
        public readonly string Path;
        public readonly Rect Bounds;
        public readonly Vector2 Pivot;
        public readonly float FontSize;
        public FollowupBox(string path, float x, float y, float w, float h,
            float pivotX = .5f, float pivotY = .5f, float fontSize = 0)
        { Path = path; Bounds = new Rect(x, y, w, h); Pivot = new Vector2(pivotX, pivotY); FontSize = fontSize; }
    }

    private static void FollowupRestore(ApkEarningPageView view, FollowupBox[] boxes)
    {
        Prepare(view);
        // Parents precede children in these source-derived tables. Positioning
        // through the artwork coordinate space preserves the source hierarchy,
        // including Step -> Content, bonuses and the receipt flight origins.
        foreach (FollowupBox box in boxes)
        {
            RectTransform rect = FollowupNode(view, box.Path);
            FollowupPosition(view, rect, box.Bounds, box.Pivot);
            if (box.FontSize > 0)
            {
                TMP_Text text = rect.GetComponent<TMP_Text>();
                if (text == null) throw new InvalidOperationException("Reference text binding is not TMP: " + box.Path);
                SetFontSize(text, box.FontSize);
                text.enableAutoSizing = true;
                text.alignment = box.Path.StartsWith("main/Step/", StringComparison.Ordinal)
                    ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.Center;
                text.margin = Vector4.zero;
            }
            Transform accent = rect.Find("OrchardAppleAccent");
            if (accent != null) accent.gameObject.SetActive(false);
        }
    }

    private static RectTransform FollowupNode(ApkEarningPageView view, string path)
    {
        GameObject bound = view.GetObject("path:" + path) ?? view.GetObject("path:ApkArtwork/" + path);
        Transform node = bound != null ? bound.transform : view.transform.Find(path);
        if (node == null) node = view.ArtworkRoot.Find(path);
        var rect = node as RectTransform;
        if (rect == null || rect == view.ArtworkRoot)
            throw new InvalidOperationException("Missing source-layout RectTransform: " + view.name + "/" + path);
        return rect;
    }

    private static void FollowupPlace(ApkEarningPageView view, string path, Rect bounds, float fontSize = 0)
    {
        RectTransform rect = FollowupNode(view, path);
        FollowupPosition(view, rect, bounds, new Vector2(.5f, .5f));
        if (fontSize <= 0) return;
        TMP_Text text = rect.GetComponent<TMP_Text>();
        if (text == null) throw new InvalidOperationException("The orchard layout needs its existing text: " + path);
        SetFontSize(text, fontSize);
        text.enableAutoSizing = true;
        text.enableWordWrapping = true;
        text.alignment = TextAlignmentOptions.Center;
        text.margin = Vector4.zero;
    }

    private static void FollowupPosition(ApkEarningPageView view, RectTransform rect, Rect bounds, Vector2 pivot)
    {
        if (rect == null || rect.parent == null || rect == view.ArtworkRoot)
            throw new InvalidOperationException("Source layout requires an existing child RectTransform.");
        foreach (LayoutGroup layout in rect.GetComponents<LayoutGroup>()) layout.enabled = false;
        foreach (ContentSizeFitter fitter in rect.GetComponents<ContentSizeFitter>()) fitter.enabled = false;
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.pivot = pivot;
        rect.sizeDelta = bounds.size;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        Rect design = view.ArtworkRoot.rect;
        rect.position = view.ArtworkRoot.TransformPoint(new Vector3(
            design.xMin + bounds.x + bounds.width * pivot.x,
            design.yMax - bounds.y - bounds.height * (1 - pivot.y), 0));
    }

    private static void FollowupVisibleImage(ApkEarningPageView view, string path)
    {
        Image image = FollowupNode(view, path).GetComponent<Image>();
        if (image == null) throw new InvalidOperationException("Reference panel needs its existing Image: " + path);
        image.gameObject.SetActive(true);
        image.enabled = true;
    }

    private static void FollowupFooter(ApkEarningPageView view, float top, float height)
    {
        TMP_Text text = view.GetText("TestMode");
        if (text == null) throw new InvalidOperationException("The test disclosure must remain configured: " + view.name);
        FollowupPosition(view, text.rectTransform, new Rect(160, top, 760, height), new Vector2(.5f, .5f));
        SetFontSize(text, 22);
        text.gameObject.SetActive(true);
        text.enabled = true;
        text.alignment = TextAlignmentOptions.Center;
        text.transform.SetAsLastSibling();
    }

    // Source: TXUserInfoSurePanel.prefab; top-left boxes at 1080 x 1920.
    private static readonly FollowupBox[] ConfirmationReference = {
        new FollowupBox("main/Close", 482f, 1637.5f, 116f, 117f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Image", 283.5f, 441.5f, 513f, 303f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Tip", 124.85f, 819.28f, 830.3f, 180.65f, 0.5f, 0.5f, 50f),
        new FollowupBox("main/Title", 311f, 212f, 476f, 84f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/bg", 46f, 189.5f, 988f, 1421f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/bg1", 77.5f, 792.49f, 925f, 235.02f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/btnParent", 490f, 1083.47f, 100f, 505.06f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Title/label_113", 331.21f, 212f, 440.54f, 84f, 0.5f, 0.5f, 60f),
    };

    // Source: LocalWithdrawalVerification.prefab; top-left boxes at 1080 x 1920.
    private static readonly FollowupBox[] VerificationReference = {
        new FollowupBox("main/Close", 919f, 497.5f, 116f, 117f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/GoldCount", 328f, 657.08f, 503.01f, 175.83f, 0.5f, 0.5f, 100f),
        new FollowupBox("main/Image", 48.5f, 499.16f, 983f, 1201.85f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Image (1)", 115f, 499f, 850f, 376f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Image (2)", 99.5f, 662f, 881f, 734f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Image (3)", 202.5f, 692.5f, 123f, 123f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step", 490f, 910f, 100f, 100f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Sure", 158f, 1439f, 764f, 192f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Title1", 220f, 546.1f, 640f, 96f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/TitleTip", 90f, 280f, 900f, 180f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/label_81", 168.96f, 876.68f, 733.37f, 82.64f, 0.5f, 0.5f, 40f),
        new FollowupBox("main/Step/1", 183f, 972f, 100f, 100f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/2", 183f, 1112f, 100f, 100f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/3", 183f, 1254f, 100f, 100f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Sure/label_93", 178f, 1439f, 724f, 181.69f, 0.5f, 0.5f, 70f),
        new FollowupBox("main/Title1/label_37", 255f, 557.1f, 570f, 74f, 0.5f, 0.5f, 60f),
        new FollowupBox("main/TitleTip/Tip", 215f, 314f, 650f, 112f, 0.5f, 0.5f, 60f),
        new FollowupBox("main/Step/1/Content", 183f, 972f, 100f, 100f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/1/Done", 196.49f, 984.49f, 73.02f, 73.02f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/1/Loading", 197.49f, 985.49f, 71.02f, 71.02f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/2/Content", 183f, 1112f, 100f, 100f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/2/Done", 196.49f, 1124.49f, 73.02f, 73.02f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/2/Image (1)", 230.5f, 1199f, 5f, 36f, 0.5f, 1f, 0f),
        new FollowupBox("main/Step/2/Loading", 197.49f, 1125.49f, 71.02f, 71.02f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/3/Content", 183f, 1254f, 100f, 100f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/3/Done", 196.49f, 1263.79f, 73.02f, 73.02f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/3/Loading", 197.49f, 1264.79f, 71.02f, 71.02f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/1/Content/Line", 230.18f, 1059.8f, 5f, 60f, 0.5f, 1f, 0f),
        new FollowupBox("main/Step/1/Content/label_90", 288.65f, 980.68f, 680.11f, 82.64f, 0.5f, 0.5f, 40f),
        new FollowupBox("main/Step/1/Loading/loading", 218.49f, 999.99f, 29.01f, 42.01f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/2/Content/Line", 230.5f, 1199.8f, 5f, 60.02f, 0.5f, 1f, 0f),
        new FollowupBox("main/Step/2/Content/label_91", 288.96f, 1120.68f, 680.11f, 82.64f, 0.5f, 0.5f, 40f),
        new FollowupBox("main/Step/2/Loading/loading", 218.49f, 1139.99f, 29.01f, 42.01f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/3/Content/label_92", 288.96f, 1262.68f, 680.11f, 82.64f, 0.5f, 0.5f, 40f),
        new FollowupBox("main/Step/3/Loading/loading", 218.49f, 1279.29f, 29.01f, 42.01f, 0.5f, 0.5f, 0f),
    };

    // Source: TXProgress0Panel.prefab; top-left boxes at 1080 x 1920.
    private static readonly FollowupBox[] StageReference = {
        new FollowupBox("main/Close", 888f, 134f, 126f, 126f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Image", 41f, 152.36f, 998f, 1550f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/KeFu", 63.81f, 559.2f, 403.39f, 64.01f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step", 490f, 1059f, 100f, 100f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Sure", 158f, 1460f, 764f, 192f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Title", 474f, 348f, 476f, 84f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/label_43", 507.66f, 437.38f, 442.33f, 159.24f, 0.5f, 0.5f, 51.51f),
        new FollowupBox("main/KeFu/Value", 77.24f, 561.77f, 376.52f, 56.9f, 0.5f, 0.5f, 37.65f),
        new FollowupBox("main/Step/1", 155f, 680f, 100f, 100f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/2", 155f, 847f, 100f, 100f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/3", 155f, 1059f, 100f, 100f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/4", 155f, 1283f, 100f, 100f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Sure/label_50", 158f, 1450f, 764f, 190.9f, 0.5f, 0.5f, 70f),
        new FollowupBox("main/Title/label_42", 476.48f, 348f, 471.03f, 84f, 0.5f, 0.5f, 60f),
        new FollowupBox("main/Step/1/Content", 155f, 680f, 100f, 100f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/1/Done", 168.5f, 693.5f, 73f, 73f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/1/Loading", 169.5f, 694.5f, 71f, 71f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/2/Content", 155f, 847f, 100f, 100f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/2/Done", 168f, 859.5f, 74f, 75f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/2/Loading", 169.5f, 861.9f, 71f, 71f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/3/Content", 155f, 1059f, 100f, 100f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/3/Loading", 168f, 1044f, 74f, 74f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/4/Loading", 168f, 1296f, 74f, 74f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/4/label_41", 260.97f, 1292.68f, 574.06f, 82.64f, 0.5f, 0.5f, 40f),
        new FollowupBox("main/Step/1/Content/Image (1)", 202.5f, 769.6f, 5f, 87.13f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/1/Content/Time", 260.97f, 748.78f, 574.06f, 58.44f, 0.5f, 0.5f, 30f),
        new FollowupBox("main/Step/1/Content/label_38", 260.97f, 680.68f, 574.06f, 82.64f, 0.5f, 0.5f, 40f),
        new FollowupBox("main/Step/1/Loading/loading", 190.5f, 709f, 29f, 42f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/2/Content/Image (1)", 202.5f, 937f, 5f, 99.53f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/2/Content/Time", 260.97f, 915.78f, 574.06f, 58.44f, 0.5f, 0.5f, 30f),
        new FollowupBox("main/Step/2/Content/label_39", 260.97f, 847.68f, 574.06f, 82.64f, 0.5f, 0.5f, 33.7f),
        new FollowupBox("main/Step/2/Loading/loading", 190.5f, 876.4f, 29f, 42f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/3/Content/Des", 306.91f, 1143.5f, 656.39f, 115.67f, 0.5f, 0.5f, 35f),
        new FollowupBox("main/Step/3/Content/Image (1)", 202f, 1123.08f, 6f, 169f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/3/Content/Image (2)", 246f, 1007f, 734f, 272.37f, 0.5f, 0.5f, 0f),
        new FollowupBox("main/Step/3/Content/Tip", 306.9f, 1029.08f, 632.23f, 105.23f, 0.5f, 0.5f, 30.45f),
        new FollowupBox("main/Step/3/Loading/loading", 190.5f, 1060f, 29f, 42f, 0.5f, 0.5f, 0f),
    };

    // Source: LocalEarningSequence.prefab; top-left boxes at 1080 x 1920.
    private static readonly FollowupBox[] SequenceReference = {
        new FollowupBox("Bonuses", 0f, 0f, 1080f, 1920f, 0.5f, 0.5f, 0f),
        new FollowupBox("CashFlight1", 468f, 833f, 88f, 70f, 0.5f, 0.5f, 0f),
        new FollowupBox("CashFlight2", 482f, 823f, 88f, 70f, 0.5f, 0.5f, 0f),
        new FollowupBox("CashFlight3", 496f, 813f, 88f, 70f, 0.5f, 0.5f, 0f),
        new FollowupBox("CashFlight4", 510f, 803f, 88f, 70f, 0.5f, 0.5f, 0f),
        new FollowupBox("CashFlight5", 524f, 793f, 88f, 70f, 0.5f, 0.5f, 0f),
        new FollowupBox("Message", 0f, 0f, 1080f, 1920f, 0.5f, 0.5f, 0f),
        new FollowupBox("Receipt", 40f, 757.99f, 1000f, 404.02f, 0.5f, 0.5f, 0f),
        new FollowupBox("Bonuses/AvatarMask", 417.5f, 87.5f, 245f, 245f, 0.5f, 0.5f, 0f),
        new FollowupBox("Bonuses/Bonus1", 80f, 529f, 920f, 222f, 0.5f, 0.5f, 0f),
        new FollowupBox("Bonuses/Bonus2", 80f, 779f, 920f, 222f, 0.5f, 0.5f, 0f),
        new FollowupBox("Bonuses/Bonus3", 80f, 1029f, 920f, 222f, 0.5f, 0.5f, 0f),
        new FollowupBox("Bonuses/Bonus4", 80f, 1279f, 920f, 222f, 0.5f, 0.5f, 0f),
        new FollowupBox("Bonuses/Bonus5", 80f, 1529f, 920f, 222f, 0.5f, 0.5f, 0f),
        new FollowupBox("Bonuses/CashDecorationLeft", 251f, 229f, 206f, 148f, 0.5f, 0.5f, 0f),
        new FollowupBox("Bonuses/CashDecorationRight", 623f, 229f, 206f, 148f, 0.5f, 0.5f, 0f),
        new FollowupBox("Bonuses/Heading", 66f, 406f, 948f, 78f, 0.5f, 0.5f, 0f),
        new FollowupBox("Bonuses/PlayerRibbon", 253f, 291f, 574f, 102f, 0.5f, 0.5f, 0f),
        new FollowupBox("Message/Activated", 20f, 191.4f, 1040f, 1157.21f, 0.5f, 0.5f, 0f),
        new FollowupBox("Message/Activating", 20f, 191.4f, 1040f, 1157.21f, 0.5f, 0.5f, 0f),
        new FollowupBox("Message/Assessment", 20f, 558.13f, 1040f, 623.74f, 0.5f, 0.5f, 0f),
        new FollowupBox("Message/Summary", 20f, 558.13f, 1040f, 623.74f, 0.5f, 0.5f, 0f),
        new FollowupBox("Receipt/Amount", 170f, 951f, 740f, 134f, 0.5f, 0.5f, 112f),
        new FollowupBox("Receipt/Banner", 40f, 757.99f, 1000f, 404.02f, 0.5f, 0.5f, 0f),
        new FollowupBox("Bonuses/AvatarMask/Avatar", 412f, 82f, 256f, 256f, 0.5f, 0.5f, 0f),
        new FollowupBox("Bonuses/Bonus1/BonusBadge", 325f, 500f, 430f, 70f, 0.5f, 0.5f, 0f),
        new FollowupBox("Bonuses/Bonus1/Caption", 130f, 692f, 820f, 52f, 0.5f, 0.5f, 38f),
        new FollowupBox("Bonuses/Bonus1/Icon", 436f, 575f, 208f, 112f, 0.5f, 0.5f, 0f),
        new FollowupBox("Bonuses/Bonus2/BonusBadge", 325f, 750f, 430f, 70f, 0.5f, 0.5f, 0f),
        new FollowupBox("Bonuses/Bonus2/Caption", 130f, 942f, 820f, 52f, 0.5f, 0.5f, 38f),
        new FollowupBox("Bonuses/Bonus2/Icon", 436f, 825f, 208f, 112f, 0.5f, 0.5f, 0f),
        new FollowupBox("Bonuses/Bonus3/BonusBadge", 325f, 1000f, 430f, 70f, 0.5f, 0.5f, 0f),
        new FollowupBox("Bonuses/Bonus3/Caption", 130f, 1192f, 820f, 52f, 0.5f, 0.5f, 38f),
        new FollowupBox("Bonuses/Bonus3/Icon", 436f, 1075f, 208f, 112f, 0.5f, 0.5f, 0f),
        new FollowupBox("Bonuses/Bonus4/BonusBadge", 325f, 1250f, 430f, 70f, 0.5f, 0.5f, 0f),
        new FollowupBox("Bonuses/Bonus4/Caption", 130f, 1442f, 820f, 52f, 0.5f, 0.5f, 38f),
        new FollowupBox("Bonuses/Bonus4/Icon", 436f, 1325f, 208f, 112f, 0.5f, 0.5f, 0f),
        new FollowupBox("Bonuses/Bonus5/BonusBadge", 325f, 1500f, 430f, 70f, 0.5f, 0.5f, 0f),
        new FollowupBox("Bonuses/Bonus5/Caption", 130f, 1692f, 820f, 52f, 0.5f, 0.5f, 38f),
        new FollowupBox("Bonuses/Bonus5/Icon", 492f, 1575f, 96f, 112f, 0.5f, 0.5f, 0f),
        new FollowupBox("Bonuses/Heading/Title", 116f, 414f, 848f, 62f, 0.5f, 0.5f, 43f),
        new FollowupBox("Bonuses/PlayerRibbon/PlayerCaption", 315f, 307f, 450f, 70f, 0.5f, 0.5f, 46f),
        new FollowupBox("Message/Activated/Artwork", 20f, 191.4f, 1040f, 1157.21f, 0.5f, 0.5f, 0f),
        new FollowupBox("Message/Activated/Body", 120f, 1110f, 840f, 100f, 0.5f, 0.5f, 40f),
        new FollowupBox("Message/Activated/Title", 215f, 850f, 650f, 248f, 0.5f, 0.5f, 100f),
        new FollowupBox("Message/Activated/You", 355f, 706f, 370f, 78f, 0.5f, 0.5f, 66f),
        new FollowupBox("Message/Activating/Artwork", 20f, 191.4f, 1040f, 1157.21f, 0.5f, 0.5f, 0f),
        new FollowupBox("Message/Activating/Body", 120f, 1110f, 840f, 100f, 0.5f, 0.5f, 40f),
        new FollowupBox("Message/Activating/Title", 215f, 850f, 650f, 248f, 0.5f, 0.5f, 100f),
        new FollowupBox("Message/Activating/You", 355f, 706f, 370f, 78f, 0.5f, 0.5f, 66f),
        new FollowupBox("Message/Assessment/Artwork", 20f, 558.13f, 1040f, 623.74f, 0.5f, 0.5f, 0f),
        new FollowupBox("Message/Assessment/Body", 135f, 876f, 810f, 200f, 0.5f, 0.5f, 64f),
        new FollowupBox("Message/Assessment/Title", 135f, 870f, 810f, 60f, 0.5f, 0.5f, 55f),
        new FollowupBox("Message/Summary/Artwork", 20f, 558.13f, 1040f, 623.74f, 0.5f, 0.5f, 0f),
        new FollowupBox("Message/Summary/Body", 135f, 946f, 810f, 126f, 0.5f, 0.5f, 42f),
        new FollowupBox("Message/Summary/Title", 135f, 870f, 810f, 60f, 0.5f, 0.5f, 55f),
        new FollowupBox("Bonuses/Bonus1/BonusBadge/Value", 372f, 507f, 336f, 56f, 0.5f, 0.5f, 37f),
        new FollowupBox("Bonuses/Bonus2/BonusBadge/Value", 372f, 757f, 336f, 56f, 0.5f, 0.5f, 37f),
        new FollowupBox("Bonuses/Bonus3/BonusBadge/Value", 372f, 1007f, 336f, 56f, 0.5f, 0.5f, 37f),
        new FollowupBox("Bonuses/Bonus4/BonusBadge/Value", 372f, 1257f, 336f, 56f, 0.5f, 0.5f, 37f),
        new FollowupBox("Bonuses/Bonus5/BonusBadge/Value", 372f, 1507f, 336f, 56f, 0.5f, 0.5f, 37f),
        new FollowupBox("Bonuses/Bonus5/Icon/LevelNumber", 507f, 1613f, 66f, 62f, 0.5f, 0.5f, 48f),
    };
}
