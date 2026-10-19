using System;
using System.Collections.Generic;
using System.IO;
using FruitApkEarning;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Edits the saved cash prefab; no artwork is constructed by the player.</summary>
public static class CashWalletArtAuthoring
{
    public const string ThemePath = "Assets/OrchardEarningArt/CashWallet/CashWalletArtTheme.asset";
    private const string HostPath = "Assets/res/local/withdrawal/ApkEarningHost.prefab";
    private const string MaterialFolder = "Assets/OrchardEarningArt/CashWallet/";

    [MenuItem("Tools/Apk Earning/Apply Approved Cash Wallet and Validate %&u")]
    public static void ApplyAndValidate()
    {
        ApplyIfConfigured();
        ApkEarningValidation.RenderFixtures();
    }

    public static void ApplyIfConfigured()
    {
        var theme = AssetDatabase.LoadAssetAtPath<CashWalletArtTheme>(ThemePath);
        if (theme == null || !theme.Enabled) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before editing the cash wallet prefab.");
        foreach (var sprite in theme.Sprites) ImportSprite(sprite);
        var font = Require<TMP_FontAsset>(theme.FontPath);
        Material plain = TextMaterial(font, "CashWalletPlain", false, theme);
        Material outlined = TextMaterial(font, "CashWalletOutlined", true, theme);
        GameObject root = PrefabUtility.LoadPrefabContents(theme.PrefabPath);
        try
        {
            var view = root.GetComponent<ApkEarningPageView>();
            if (view == null || view.ArtworkRoot == null) throw new InvalidOperationException("Cash page bindings are missing.");
            RectTransform artwork = view.ArtworkRoot;
            // These three small support nodes are serialized once alongside the
            // original bound objects. Every balance, action and progress binding survives.
            AddCaption(view, "StageLabel", "CashStageLabel", view.GetText("Tip"));
            AddCaption(view, "TestMode", "CashTestMode", view.GetText("Tip"));
            Transform divider = artwork.Find("CashStatsDivider");
            if (divider == null)
            {
                var go = new GameObject("CashStatsDivider", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.layer = root.layer; go.transform.SetParent(artwork, false); divider = go.transform;
            }
            var dividerImage = divider.GetComponent<Image>();
            dividerImage.raycastTarget = false; dividerImage.color = theme.DividerColor;
            Image backdrop = view.GetImage("backdrop");
            if (backdrop != null) { backdrop.enabled = true; backdrop.color = theme.ScrimColor; }
            ConfigureHistory(view, theme);
            foreach (var rule in theme.Elements)
            {
                RectTransform rect = Resolve(view, rule.Target);
                if (rect == null) throw new InvalidOperationException("Cash layout target missing: " + rule.Target);
                rect.gameObject.SetActive(rule.Active);
                if (rule.SetRect) SetArtworkRect(rect, artwork, rule.Position, rule.Size);
                if (rule.StyleImage)
                {
                    Image image = rect.GetComponent<Image>();
                    if (image == null) throw new InvalidOperationException("Cash Image missing: " + rule.Target);
                    image.enabled = rule.ImageEnabled;
                    if (!string.IsNullOrEmpty(rule.SpritePath)) image.sprite = Require<Sprite>(rule.SpritePath);
                    image.type = rule.ImageType; image.pixelsPerUnitMultiplier = rule.PixelsPerUnit;
                    image.preserveAspect = rule.PreserveAspect; image.color = rule.ImageColor;
                    foreach (Shadow shadow in image.GetComponents<Shadow>()) shadow.enabled = false;
                }
                if (rule.StyleText)
                {
                    TMP_Text text = rect.GetComponent<TMP_Text>();
                    if (text == null) throw new InvalidOperationException("Cash text missing: " + rule.Target);
                    text.enabled = true; text.font = font; text.fontSharedMaterial = rule.Outlined ? outlined : plain;
                    text.fontSize = rule.FontSize; text.enableAutoSizing = true;
                    text.fontSizeMax = rule.FontSize; text.fontSizeMin = rule.FontSize * .7f;
                    text.fontStyle = rule.FontStyle; text.color = rule.TextColor;
                    text.fontWeight = FontWeight.Regular; text.enableVertexGradient = false;
                    text.alignment = TextAlignmentOptions.Center; text.margin = Vector4.zero;
                    text.enableWordWrapping = rule.Wrap; text.overflowMode = TextOverflowModes.Ellipsis;
                    text.raycastTarget = false;
                    if (rule.Text != null && rule.Text.Length > 0) text.text = rule.Text;
                }
            }
            // Capture freshly authored full sizes before applying the preview value.
            RoundedEarningProgressAuthoring.Apply(root, true);
            view.SetProgress("Progress", theme.PreviewProgress);
            // Draw the compact title, its apple and close control over the card.
            view.GetText("Title").transform.parent.SetAsLastSibling();
            view.GetButton("Close").transform.SetAsLastSibling();
            Button history = view.GetButton("HistoryButton");
            TMP_Text historyText = history.GetComponentInChildren<TMP_Text>(true);
            history.targetGraphic = historyText; historyText.raycastTarget = true;
            PrefabUtility.SaveAsPrefabAsset(root, theme.PrefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        root = PrefabUtility.LoadPrefabContents(HostPath);
        try
        {
            var host = root.GetComponent<ApkEarningHost>();
            host.GlobalTestModeCaption = root.transform.Find("TestModeCaption").GetComponent<TMP_Text>();
            PrefabUtility.SaveAsPrefabAsset(root, HostPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
        Debug.Log("[CashWalletArt] Applied approved compact layout, artwork and preserved runtime bindings.");
    }

    private static void AddCaption(ApkEarningPageView view, string alias, string name, TMP_Text source)
    {
        TMP_Text text = view.GetText(alias);
        if (text == null)
        {
            Transform existing = view.ArtworkRoot.Find(name);
            text = existing != null ? existing.GetComponent<TMP_Text>() : Object.Instantiate(source, view.ArtworkRoot, false);
            text.name = name;
            var slots = new List<ApkEarningPageView.TextSlot>(view.Texts);
            slots.Add(new ApkEarningPageView.TextSlot { Key = alias, Target = text }); view.Texts = slots.ToArray();
        }
    }

    private static void ConfigureHistory(ApkEarningPageView view, CashWalletArtTheme theme)
    {
        Button button = view.GetButton("HistoryButton");
        if (button == null) throw new InvalidOperationException("Cash history Button is missing.");
        ApkEarningPageView entry = button.GetComponentInParent<ApkEarningPageView>();
        if (entry == view) throw new InvalidOperationException("Cash history prefab structure changed.");
        // The imported marquee becomes a static, visible text button. Removing
        // scroll/mask behavior avoids moving or clipping its actual click target.
        foreach (ScrollRect scroll in entry.GetComponentsInChildren<ScrollRect>(true)) scroll.enabled = false;
        foreach (Mask mask in entry.GetComponentsInChildren<Mask>(true)) mask.enabled = false;
        foreach (RectMask2D mask in entry.GetComponentsInChildren<RectMask2D>(true)) mask.enabled = false;
        foreach (LayoutGroup layout in entry.GetComponentsInChildren<LayoutGroup>(true)) layout.enabled = false;
        foreach (ContentSizeFitter fit in entry.GetComponentsInChildren<ContentSizeFitter>(true)) fit.enabled = false;
        foreach (RectTransform rect in entry.GetComponentsInChildren<RectTransform>(true))
            SetArtworkRect(rect, view.ArtworkRoot, theme.HistoryPosition, theme.HistorySize);
        foreach (Image image in entry.GetComponentsInChildren<Image>(true)) image.enabled = false;
        button.interactable = true;
        var texts = new List<ApkEarningPageView.TextSlot>(view.Texts);
        if (view.GetText("HistoryCaption") == null)
            texts.Add(new ApkEarningPageView.TextSlot { Key = "HistoryCaption", Target = button.GetComponentInChildren<TMP_Text>(true) });
        view.Texts = texts.ToArray();
    }

    private static RectTransform Resolve(ApkEarningPageView view, string key)
    {
        string name = key.Substring(2);
        switch (key.Substring(0, 2))
        {
            case "t:": return view.GetText(name)?.rectTransform;
            case "l:": return view.GetText(name)?.transform.parent as RectTransform;
            case "i:": return view.GetImage(name)?.rectTransform;
            case "b:": return view.GetButton(name)?.transform as RectTransform;
            case "p:": return view.transform.Find(name) as RectTransform;
            default: throw new InvalidOperationException("Invalid cash layout selector: " + key);
        }
    }

    private static void SetArtworkRect(RectTransform rect, RectTransform artwork, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.localScale = Vector3.one; rect.localRotation = Quaternion.identity; rect.sizeDelta = size;
        rect.position = artwork.TransformPoint(position);
    }

    private static Material TextMaterial(TMP_FontAsset font, string name, bool outlined, CashWalletArtTheme theme)
    {
        string path = MaterialFolder + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null) { material = new Material(font.material); AssetDatabase.CreateAsset(material, path); }
        else { material.shader = font.material.shader; material.CopyPropertiesFromMaterial(font.material); }
        material.SetColor(ShaderUtilities.ID_FaceColor, Color.white);
        material.SetFloat(ShaderUtilities.ID_FaceDilate, 0);
        material.SetFloat(ShaderUtilities.ID_OutlineWidth, outlined ? theme.OutlineWidth : 0);
        material.SetColor(ShaderUtilities.ID_OutlineColor, theme.OutlineColor);
        if (outlined) material.EnableKeyword("OUTLINE_ON"); else material.DisableKeyword("OUTLINE_ON");
        material.DisableKeyword("UNDERLAY_ON"); material.DisableKeyword("GLOW_ON");
        EditorUtility.SetDirty(material); return material;
    }

    private static void ImportSprite(CashWalletArtTheme.SpriteImport spec)
    {
        if (spec.SpriteRect.width <= 0 || spec.SpriteRect.height <= 0)
            throw new InvalidOperationException("Cash sprite crop must have a positive size: " + spec.Path);
        var importer = AssetImporter.GetAtPath(spec.Path) as TextureImporter;
        if (importer == null) throw new InvalidOperationException("Missing cash sprite: " + spec.Path);
        SpriteMetaData[] current = importer.spritesheet;
        if (importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == SpriteImportMode.Multiple &&
            current.Length == 1 && current[0].rect == spec.SpriteRect && current[0].border == spec.Border &&
            importer.spritePixelsPerUnit == 100 && !importer.mipmapEnabled && importer.alphaIsTransparency &&
            importer.maxTextureSize == spec.MaxSize && importer.textureCompression == TextureImporterCompression.Compressed &&
            !importer.isReadable) return;
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 100; importer.mipmapEnabled = false; importer.alphaIsTransparency = true;
        importer.maxTextureSize = spec.MaxSize; importer.textureCompression = TextureImporterCompression.Compressed;
        importer.isReadable = false;
        var sprite = new SpriteMetaData { name = Path.GetFileNameWithoutExtension(spec.Path), rect = spec.SpriteRect,
            alignment = (int)SpriteAlignment.Center, pivot = new Vector2(.5f, .5f), border = spec.Border };
        // Serialized SpriteRect removes the generator's transparent padding;
        // original PNG pixels remain unchanged.
        importer.spritesheet = new[] { sprite };
        importer.SaveAndReimport();
    }

    private static T Require<T>(string path) where T : Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null) throw new InvalidOperationException("Missing cash artwork asset: " + path);
        return asset;
    }
}
