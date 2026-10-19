using System;
using System.Collections.Generic;
using FruitApkEarning;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Stores correctly proportioned progress caps and runtime sizing in the prefabs.</summary>
public static class RoundedEarningProgressAuthoring
{
    private const string ArtFolder = "Assets/OrchardEarningArt/CashWallet/";
    private const string PanelFolder = "Assets/ApkEarningReference/Resources/ApkEarningReference/Panels/";
    private static readonly string[] PrefabPaths =
    {
        PanelFolder + "TXPanel.prefab",
        PanelFolder + "TXCoinPanel.prefab",
        PanelFolder + "TXGuideLoadingPanel.prefab",
        PanelFolder + "TXRecordGuidePanel.prefab",
        "Assets/res/local/withdrawal/ApkEarningTop.prefab"
    };

    [MenuItem("Tools/Apk Earning/Apply Rounded Progress and Validate")]
    public static void ApplyAndValidate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before editing progress prefabs.");
        foreach (string path in PrefabPaths)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                Apply(root);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        AssetDatabase.SaveAssets();
        RoundedEarningProgressValidation.Render();
    }

    // Authoring callers pass true only while the rects still contain the newly
    // authored full sizes. Repairing existing prefabs preserves their saved sizes.
    public static void Apply(GameObject root, bool sizesWereAuthored = false)
    {
        if (root == null) throw new ArgumentNullException(nameof(root));
        ApkEarningPageView[] views = root.GetComponentsInChildren<ApkEarningPageView>(true);
        foreach (Image image in root.GetComponentsInChildren<Image>(true))
        {
            if (image.sprite == null) continue;
            string path = AssetDatabase.GetAssetPath(image.sprite);
            bool fill = path == ArtFolder + "CashWalletProgressFill.png";
            if (!fill && path != ArtFolder + "CashWalletProgressTrack.png") continue;
            if (fill) ApplyFill(image, views, sizesWereAuthored);
            else ConfigureImage(image, image.rectTransform.rect.height);
        }
    }

    private static void ApplyFill(Image image, ApkEarningPageView[] views, bool sizesWereAuthored)
    {
        var bindings = new List<ApkEarningPageView.ImageSlot>();
        ApkEarningPageView boundView = null;
        ApkEarningPageView.ImageSlot savedSize = null;
        foreach (ApkEarningPageView view in views)
        {
            foreach (ApkEarningPageView.ImageSlot slot in view.Images)
            {
                if (slot.Target != image) continue;
                if (boundView == null) boundView = view;
                bindings.Add(slot);
                if (sizesWereAuthored || !slot.ResizeWidthWithProgress || slot.FullProgressWidth <= 0) continue;
                // Existing normalized aliases carry the full size even at zero or tiny progress.
                if (savedSize == null || (savedSize.FullProgressHeight <= 0 && slot.FullProgressHeight > 0))
                    savedSize = slot;
            }
        }

        RectTransform rect = image.rectTransform;
        float width = savedSize != null ? savedSize.FullProgressWidth : rect.rect.width;
        float height = savedSize != null && savedSize.FullProgressHeight > 0
            ? savedSize.FullProgressHeight : rect.rect.height;
        if (width <= 0 || height <= 0)
            throw new InvalidOperationException("Progress needs a positive full size: " + image.name);
        float progress = savedSize != null ? Mathf.Clamp01(rect.rect.width / width)
            : image.type == Image.Type.Filled ? Mathf.Clamp01(image.fillAmount) : 1;

        // Reanchor around the existing left edge, not the current width's center.
        // This preserves the visible origin for both centered and previously resized fills.
        RectTransform parent = rect.parent as RectTransform;
        if (parent == null) throw new InvalidOperationException("Progress needs a RectTransform parent: " + image.name);
        Vector3 leftCenter = parent.InverseTransformPoint(rect.TransformPoint(
            new Vector3(rect.rect.xMin, rect.rect.center.y, 0)));
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, .5f);
        rect.anchoredPosition = new Vector2(leftCenter.x - parent.rect.xMin,
            leftCenter.y - parent.rect.center.y);

        float pixelsPerUnit = ConfigureImage(image, height);
        image.fillAmount = 1;
        foreach (ApkEarningPageView.ImageSlot slot in bindings)
        {
            slot.ResizeWidthWithProgress = true;
            slot.FullProgressWidth = width;
            slot.FullProgressHeight = height;
            slot.FullProgressPixelsPerUnit = pixelsPerUnit;
        }
        if (boundView != null) boundView.SetProgress(bindings[0].Key, progress);
        else rect.sizeDelta = new Vector2(width, height);
    }

    private static float ConfigureImage(Image image, float height)
    {
        if (height <= 0) throw new InvalidOperationException("Progress needs a positive height: " + image.name);
        image.type = Image.Type.Sliced;
        image.preserveAspect = false;
        // Image.pixelsPerUnit is the sprite/canvas conversion, excluding the multiplier.
        // Using the imported Sprite rect also accounts for the texture's platform size limit.
        float multiplier = image.sprite.rect.height / height / image.pixelsPerUnit;
        image.pixelsPerUnitMultiplier = multiplier;
        return multiplier;
    }
}
