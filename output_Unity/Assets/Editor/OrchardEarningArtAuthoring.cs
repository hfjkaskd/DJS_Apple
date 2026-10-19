using System;
using System.Collections.Generic;
using System.IO;
using FruitApkEarning;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Applies an Inspector-configured orchard skin to saved earning prefabs.</summary>
public static class OrchardEarningArtAuthoring
{
    public const string ThemePath = "Assets/OrchardEarningArt/OrchardEarningTheme.asset";
    private const string SourceArt = "Assets/ApkEarningReference/Visuals/atlas/";
    private const string Common = "Assets/res/local/common/sprite/";
    private const string ResourceArt = "Assets/ApkEarningReference/Resources/ApkEarningReference/Artwork/";
    private const string OrchardArt = "Assets/OrchardEarningArt/";
    private const string AccentName = "OrchardAppleAccent";

    [MenuItem("Tools/Apk Earning/Apply Orchard Art and Validate %&o")]
    public static void ApplyAndValidate()
    {
        Apply(GetOrCreateTheme());
        ApkEarningValidation.RenderFixtures();
    }

    [MenuItem("Tools/Apk Earning/Apply Orchard Art")]
    public static void ApplyConfiguredTheme() { Apply(GetOrCreateTheme()); }

    public static void ApplyIfConfigured()
    {
        var theme = AssetDatabase.LoadAssetAtPath<OrchardEarningTheme>(ThemePath);
        if (theme != null && theme.Enabled) Apply(theme);
    }

    private static OrchardEarningTheme GetOrCreateTheme()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before authoring orchard earning art.");
        var theme = AssetDatabase.LoadAssetAtPath<OrchardEarningTheme>(ThemePath);
        if (theme != null) return theme;
        EnsureFolder("Assets/OrchardEarningArt");
        theme = ScriptableObject.CreateInstance<OrchardEarningTheme>();
        SeedRules(theme);
        UpgradeTheme(theme);
        AssetDatabase.CreateAsset(theme, ThemePath);
        AssetDatabase.SaveAssets();
        return theme;
    }

    private static void SeedRules(OrchardEarningTheme theme)
    {
        // The replacement size is expressed through Image 9-slice PPU, not a
        // transformed RectTransform. This preserves every existing click target.
        string panel = Common + "pop/PopBg.asset";
        string green = Common + "btn/Btn_Normal.asset";
        string orange = Common + "btn/Btn_Orange.asset";
        string close = Common + "pop/PopClose_top.asset";
        foreach (string name in new[] { "panel", "panel-heading", "background", "diamond-panel", "wood-card", "aux-steps-panel" })
            Add(theme, "octobergarden/" + name, panel, 3.8f);
        foreach (string name in new[] { "cream-card", "cash-card", "stats-panel", "input", "aux-level-card", "aux-verification-card", "main-currency-card", "main-tip" })
            Add(theme, "octobergarden/" + name, panel, 5.5f);
        Add(theme, "approvedgarden/stats-card", panel, 5.5f);
        Add(theme, "localhistory/record-card", panel, 5.5f);
        Add(theme, "txrecordpanel/bg1", SourceArt + "txrecordpanel/bg1.png", 1, true);
        foreach (string name in new[] { "primary-button", "main-cash-button", "aux-guide-withdraw" })
            Add(theme, "octobergarden/" + name, green, 3.2f);
        foreach (string name in new[] { "title", "cash-heading", "wood-ribbon", "cream-ribbon", "success-title", "aux-guide-title", "aux-complete-title", "aux-level-title", "aux-level-band", "aux-loading-band" })
            Add(theme, "octobergarden/" + name, orange, 2.8f, false,
                name == "title" || name == "cash-heading" || name == "aux-guide-title");
        foreach (string name in new[] { "cash-title", "selection-title", "title" })
            Add(theme, "approvedgarden/" + name, orange, 2.8f, false, name == "title");
        foreach (string name in new[] { "close", "aux-blue-close" })
            Add(theme, "octobergarden/" + name, close, 1, true);
        Add(theme, "octobergarden/amount-normal", panel, theme.OptionPixelsPerUnitMultiplier);
        Add(theme, "octobergarden/amount-selected", green, theme.OptionPixelsPerUnitMultiplier);
        Add(theme, "octobergarden/main-milestone-pending", theme.NormalMilestonePath, 1, true);
        Add(theme, "octobergarden/main-milestone-normal", theme.PassedMilestonePath, 1, true);
        Add(theme, "octobergarden/main-milestone-active", theme.ActiveMilestonePath, 1, true);
        Add(theme, "octobergarden/aux-guide-node", theme.NormalMilestonePath, 1, true);
        Add(theme, "octobergarden/progress-track", Common + "ProgressBg.asset", 2);
        // Filled progress images retain their original long texture and fill mode.
        // Replacing it with the small native pill would stretch its specular ends.
        Add(theme, "octobergarden/progress-fill", SourceArt + "octobergarden/progress-fill.png", 1, false, false, true);
        Add(theme, "octobergarden/diamond-progress-fill", SourceArt + "octobergarden/progress-fill.png", 1, false, false, true);
        Add(theme, "locallucky/screw-decoration", theme.AppleCrestPath, 1, true);
        Add(theme, "newbieguidepanel/yellow-screw", theme.AppleAccentPath, 1, true);
        Add(theme, "localearningmessages/portrait-banner", panel, 4.5f);
        Add(theme, "localearningmessages/cash-banner", panel, 4.5f);
    }

    private static void UpgradeTheme(OrchardEarningTheme theme)
    {
        UpgradeDepthTheme(theme);
        if (theme.ThemeVersion >= 4) return;
        string panel = OrchardArt + "OrchardPanel.png";
        string button = OrchardArt + "OrchardPrimaryButton.png";
        string selected = OrchardArt + "OrchardSelectedButton.png";
        string title = OrchardArt + "OrchardWoodTitle.png";
        AddTextureRule(theme, panel, new Vector4(280, 280, 280, 280), 1024);
        AddTextureRule(theme, button, new Vector4(330, 250, 330, 330), 1024);
        AddTextureRule(theme, selected, new Vector4(330, 250, 330, 330), 1024);
        AddTextureRule(theme, title, new Vector4(150, 70, 150, 70), 1024);
        foreach (string name in new[] { "panel", "panel-heading", "background", "diamond-panel", "wood-card", "aux-steps-panel" })
            SetArtwork(theme, "octobergarden/" + name, panel, 2f);
        foreach (string name in new[] { "cream-card", "cash-card", "stats-panel" })
            SetArtwork(theme, "octobergarden/" + name, panel, 3.8f);
        SetArtwork(theme, "approvedgarden/stats-card", panel, 10f);
        SetArtwork(theme, "localhistory/record-card", panel, 3.8f);
        foreach (string name in new[] { "input", "aux-level-card", "aux-verification-card", "main-currency-card", "main-tip" })
            SetArtwork(theme, "octobergarden/" + name, panel, 6f);
        SetArtwork(theme, "localearningmessages/portrait-banner", panel, 3.8f);
        SetArtwork(theme, "localearningmessages/cash-banner", panel, 3.8f);
        SetArtwork(theme, "octobergarden/primary-button", button, 4f);
        SetArtwork(theme, "octobergarden/main-cash-button", button, 5.5f);
        SetArtwork(theme, "octobergarden/aux-guide-withdraw", button, 8f);
        foreach (string name in new[] { "title", "cash-heading", "wood-ribbon", "cream-ribbon", "success-title", "aux-guide-title", "aux-complete-title", "aux-level-title", "aux-level-band", "aux-loading-band" })
            SetArtwork(theme, "octobergarden/" + name, title, 3f);
        foreach (string name in new[] { "title", "cash-heading", "success-title", "aux-guide-title", "aux-complete-title", "aux-level-title" })
            SetArtwork(theme, "octobergarden/" + name, title, 1.8f);
        SetArtwork(theme, "approvedgarden/cash-title", title, 3f);
        SetArtwork(theme, "approvedgarden/title", title, 1.8f);
        SetArtwork(theme, "approvedgarden/selection-title", title, 3f);
        theme.NormalOptionTexture = panel;
        theme.NormalOptionBorder = new Vector4(280, 280, 280, 280);
        theme.SelectedOptionTexture = selected;
        theme.SelectedOptionBorder = new Vector4(330, 250, 330, 330);
        theme.OptionPixelsPerUnitMultiplier = 4f;
        SetArtwork(theme, "octobergarden/amount-normal", panel, theme.OptionPixelsPerUnitMultiplier);
        SetArtwork(theme, "octobergarden/amount-selected", selected, theme.OptionPixelsPerUnitMultiplier);
        theme.ThemeVersion = 4;
        EditorUtility.SetDirty(theme);
    }

    private static void AddTextureRule(OrchardEarningTheme theme, string path, Vector4 border, int maxSize)
    {
        foreach (var rule in theme.TextureRules) if (rule.Path == path) return;
        theme.TextureRules.Add(new OrchardEarningTheme.TextureRule { Path = path, Border = border, MaxTextureSize = maxSize });
    }

    private static void SetArtwork(OrchardEarningTheme theme, string source, string replacement, float ppu)
    {
        var rule = FindRule(theme, source);
        if (rule == null) return;
        rule.ReplacementPath = replacement;
        rule.PixelsPerUnitMultiplier = ppu;
        rule.ImageType = Image.Type.Sliced;
        rule.KeepImageType = false;
        rule.PreserveAspect = false;
        rule.Tint = Color.white;
    }

    private static OrchardEarningTheme.SpriteRule FindRule(OrchardEarningTheme theme, string source)
    {
        string path = SourceArt + source + ".png";
        foreach (var rule in theme.SpriteRules) if (rule.SourcePath == path) return rule;
        return null;
    }

    private static void UpgradeDepthTheme(OrchardEarningTheme theme)
    {
        if (theme.ThemeVersion >= 3) return;
        if (theme.ThemeVersion < 2)
        {
            foreach (var rule in theme.SpriteRules)
            {
                if (rule.SourcePath == SourceArt + "txrecordpanel/bg1.png")
                {
                    rule.ReplacementPath = rule.SourcePath;
                    rule.ImageType = Image.Type.Simple; rule.KeepImageType = false;
                    rule.PreserveAspect = true; rule.PixelsPerUnitMultiplier = 1;
                }
                if (rule.SourcePath == SourceArt + "approvedgarden/cash-title.png" ||
                    rule.SourcePath == SourceArt + "approvedgarden/selection-title.png")
                    rule.AddAppleAccent = false;
            }
            AddIfMissing(theme, "rewardactivation/reward2", theme.AppleCrestPath);
            var multiplier = AddIfMissing(theme, "rewardactivation/reward3", ResourceArt + "CashPile.asset");
            multiplier.OverlayText = "X10";
            AddIfMissing(theme, "rewardactivation/reward4", ResourceArt + "DiamondPile.asset");
            theme.ThemeVersion = 2;
        }
        // Version 3 adds restrained depth using the existing beveled sprites.
        // Exact source rules leave currencies, fruit, progress fills and gameplay
        // art alone. Values remain editable in the saved Inspector theme.
        foreach (string name in new[] { "panel", "panel-heading", "background", "diamond-panel", "wood-card", "aux-steps-panel" })
            SetDepth(theme, "octobergarden/" + name, 2.4f, .18f, -8);
        foreach (string name in new[] { "cream-card", "cash-card", "stats-panel" })
            SetDepth(theme, "octobergarden/" + name, 3.6f, .13f, -4, true);
        SetDepth(theme, "approvedgarden/stats-card", 3.6f, .13f, -4, true);
        SetDepth(theme, "localhistory/record-card", 3.6f, .13f, -4, true);
        foreach (string name in new[] { "input", "aux-level-card", "aux-verification-card" })
            SetDepth(theme, "octobergarden/" + name, 4.8f, .13f, -4);
        foreach (string name in new[] { "main-currency-card", "main-tip" })
            SetDepth(theme, "octobergarden/" + name, 4.5f, 0, 0);
        SetDepth(theme, "octobergarden/primary-button", 1.8f, .16f, -5);
        SetDepth(theme, "octobergarden/main-cash-button", 2.2f, 0, 0);
        SetDepth(theme, "octobergarden/aux-guide-withdraw", 2.8f, .16f, -5);
        foreach (string name in new[] { "title", "cash-heading", "wood-ribbon", "cream-ribbon", "success-title", "aux-guide-title", "aux-complete-title", "aux-level-title", "aux-level-band", "aux-loading-band" })
            SetDepth(theme, "octobergarden/" + name, 2.4f, .16f, -5);
        foreach (string name in new[] { "cash-title", "selection-title", "title" })
            SetDepth(theme, "approvedgarden/" + name, 2.4f, .16f, -5);
        theme.OptionPixelsPerUnitMultiplier = 2.8f;
        SetDepth(theme, "octobergarden/amount-normal", theme.OptionPixelsPerUnitMultiplier, .13f, -4);
        SetDepth(theme, "octobergarden/amount-selected", theme.OptionPixelsPerUnitMultiplier, .13f, -4);
        theme.ThemeVersion = 3;
        EditorUtility.SetDirty(theme);
    }

    private static void SetDepth(OrchardEarningTheme theme, string source, float ppu, float alpha, float offsetY, bool warmCard = false)
    {
        string path = SourceArt + source + ".png";
        foreach (var rule in theme.SpriteRules)
        {
            if (rule.SourcePath != path) continue;
            rule.PixelsPerUnitMultiplier = ppu;
            rule.AddShadow = alpha > 0;
            rule.ShadowColor = new Color(88f / 255, 45f / 255, 22f / 255, alpha);
            rule.ShadowOffset = new Vector2(0, offsetY);
            if (warmCard) rule.Tint = new Color32(255, 248, 232, 255);
            return;
        }
    }

    private static OrchardEarningTheme.SpriteRule AddIfMissing(OrchardEarningTheme theme, string source, string replacement)
    {
        string path = SourceArt + source + ".png";
        foreach (var rule in theme.SpriteRules) if (rule.SourcePath == path) return rule;
        Add(theme, source, replacement, 1, true);
        return theme.SpriteRules[theme.SpriteRules.Count - 1];
    }

    private static void Add(OrchardEarningTheme theme, string source, string replacement, float ppu,
        bool aspect = false, bool accent = false, bool keepType = false)
    {
        theme.SpriteRules.Add(new OrchardEarningTheme.SpriteRule {
            SourcePath = SourceArt + source + ".png", ReplacementPath = replacement,
            ImageType = aspect ? Image.Type.Simple : Image.Type.Sliced,
            KeepImageType = keepType, PreserveAspect = aspect,
            PixelsPerUnitMultiplier = ppu, AddAppleAccent = accent
        });
    }

    private static void Apply(OrchardEarningTheme theme)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before authoring orchard earning art.");
        if (!theme.Enabled) return;
        UpgradeTheme(theme);
        ConfigureThemeTextures(theme);
        ConfigureCrest(theme.AppleCrestPath);
        WriteOptionResource(theme.NormalOptionTexture, ResourceArt + "DiamondOptionNormal.png", theme.NormalOptionBorder,
            OptionTextureSize(theme, theme.NormalOptionTexture));
        WriteOptionResource(theme.SelectedOptionTexture, ResourceArt + "DiamondOptionSelected.png", theme.SelectedOptionBorder,
            OptionTextureSize(theme, theme.SelectedOptionTexture));
        var font = Require<TMP_FontAsset>(theme.DisplayFontPath);
        Material displayMaterial = GetDisplayMaterial(theme, font);
        var rules = new Dictionary<string, OrchardEarningTheme.SpriteRule>(StringComparer.Ordinal);
        foreach (var rule in theme.SpriteRules)
        {
            if (string.IsNullOrEmpty(rule.SourcePath) || string.IsNullOrEmpty(rule.ReplacementPath)) continue;
            if (rule.SourcePath == SourceArt + "octobergarden/amount-normal.png" ||
                rule.SourcePath == SourceArt + "octobergarden/amount-selected.png")
                rule.PixelsPerUnitMultiplier = theme.OptionPixelsPerUnitMultiplier;
            if (rules.ContainsKey(rule.SourcePath)) throw new InvalidOperationException("Duplicate orchard image rule: " + rule.SourcePath);
            rules.Add(rule.SourcePath, rule);
        }
        var remembered = new Dictionary<string, OrchardEarningTheme.AuthoredImage>(StringComparer.Ordinal);
        foreach (var binding in theme.AuthoredImages)
            remembered[binding.PrefabPath + "|" + binding.HierarchyIndices] = binding;
        var paths = new List<string>();
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { theme.PanelFolder }))
            paths.Add(AssetDatabase.GUIDToAssetPath(guid));
        foreach (string path in theme.AdditionalPrefabs) if (!paths.Contains(path)) paths.Add(path);
        paths.Sort(StringComparer.Ordinal);
        // LocalSuccess inherits this page. Author its decorations before loading
        // the variant so the variant reuses them instead of adding a second copy.
        string receiptBasePath = theme.PanelFolder.TrimEnd('/') + "/SuccessPanel.prefab";
        if (paths.Remove(receiptBasePath)) paths.Insert(0, receiptBasePath);
        int changedImages = 0;
        foreach (string path in paths)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach (Image image in root.GetComponentsInChildren<Image>(true))
                {
                    if (image == null || image.name == AccentName || image.sprite == null) continue;
                    string source = AssetDatabase.GetAssetPath(image.sprite);
                    // Runtime selection replaces only the sprite. Recognize its
                    // exact Resources images if a page has been saved with one.
                    if (source == ResourceArt + "DiamondOptionNormal.png") source = SourceArt + "octobergarden/amount-normal.png";
                    else if (source == ResourceArt + "DiamondOptionSelected.png") source = SourceArt + "octobergarden/amount-selected.png";
                    string hierarchy = HierarchyIndices(root.transform, image.transform);
                    string key = path + "|" + hierarchy;
                    OrchardEarningTheme.SpriteRule rule;
                    if (!rules.TryGetValue(source, out rule))
                    {
                        if (!remembered.TryGetValue(key, out var old) || !rules.TryGetValue(old.SourcePath, out rule)) continue;
                        // Source authoring can rebuild a page. Never re-skin a new
                        // business icon merely because it reused a sibling index.
                        string lastReplacement = string.IsNullOrEmpty(old.LastReplacementPath) ? rule.ReplacementPath : old.LastReplacementPath;
                        if (source != lastReplacement) continue;
                        source = old.SourcePath;
                    }
                    if (!remembered.TryGetValue(key, out var entry))
                    {
                        entry = new OrchardEarningTheme.AuthoredImage { PrefabPath = path, HierarchyIndices = hierarchy };
                        theme.AuthoredImages.Add(entry); remembered.Add(key, entry);
                    }
                    entry.SourcePath = source;
                    entry.LastReplacementPath = rule.ReplacementPath;
                    image.sprite = SpriteAt(rule.ReplacementPath);
                    if (!rule.KeepImageType) image.type = rule.ImageType;
                    image.pixelsPerUnitMultiplier = rule.PixelsPerUnitMultiplier;
                    image.preserveAspect = rule.PreserveAspect;
                    image.color = rule.Tint;
                    ApplyImageShadow(image, rule);
                    AddAppleAccent(theme, image, rule.AddAppleAccent);
                    AddRewardOverlay(theme, image, rule.OverlayText, font, displayMaterial);
                    changedImages++;
                }
                StyleSuccessLayout(root, path, theme);
                StyleStatsLayout(root, path, theme);
                StyleTestCaption(root, theme);
                StyleOptionImages(root, theme, rules);
                StyleTypography(root, theme, font, displayMaterial);
                StyleOptionLabels(root, theme);
                foreach (ApkEarningPageView page in root.GetComponentsInChildren<ApkEarningPageView>(true))
                    page.EmphasisColor = theme.AccentTextColor;
                foreach (ApkEarningTopView top in root.GetComponentsInChildren<ApkEarningTopView>(true))
                {
                    top.NormalMilestone = SpriteAt(theme.NormalMilestonePath);
                    top.PassedMilestone = SpriteAt(theme.PassedMilestonePath);
                    top.ActiveMilestone = SpriteAt(theme.ActiveMilestonePath);
                }
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        EditorUtility.SetDirty(theme);
        AssetDatabase.SaveAssets();
        CashWalletArtAuthoring.ApplyIfConfigured();
        CompactEarningArtAuthoring.ApplyIfConfigured();
        CompactHudAuthoring.ApplyIfConfigured();
        Debug.Log("[OrchardEarningArt] Applied Inspector theme to " + paths.Count + " saved prefabs / " + changedImages +
            " images. Currency icons, monetary values, conditions and test labels remain unchanged.");
    }

    private static void ApplyImageShadow(Image image, OrchardEarningTheme.SpriteRule rule)
    {
        // Reuse the same component, including an inherited one on a prefab
        // variant. An Outline is a different effect and must not be repurposed.
        Shadow shadow = null;
        foreach (Shadow effect in image.GetComponents<Shadow>())
            if (!(effect is Outline)) { shadow = effect; break; }
        if (shadow == null && rule.AddShadow) shadow = image.gameObject.AddComponent<Shadow>();
        if (shadow == null) return;
        shadow.enabled = rule.AddShadow;
        shadow.effectColor = rule.ShadowColor;
        shadow.effectDistance = rule.ShadowOffset;
        shadow.useGraphicAlpha = true;
    }

    private static void StyleOptionImages(GameObject root, OrchardEarningTheme theme,
        Dictionary<string, OrchardEarningTheme.SpriteRule> rules)
    {
        rules.TryGetValue(SourceArt + "octobergarden/amount-normal.png", out var sharedOptionRule);
        foreach (ApkEarningPageView page in root.GetComponentsInChildren<ApkEarningPageView>(true))
        {
            for (int i = 0; i < 4; i++)
            {
                Button option = page.GetButton("options[" + i + "]");
                Image image = option == null ? null : option.targetGraphic as Image;
                if (image == null) continue;
                image.type = Image.Type.Sliced;
                image.pixelsPerUnitMultiplier = theme.OptionPixelsPerUnitMultiplier;
                image.preserveAspect = false;
                // Keep all four shadows equal while runtime swaps normal and
                // selected Resources sprites; no runtime component work needed.
                if (sharedOptionRule != null) ApplyImageShadow(image, sharedOptionRule);
            }
        }
    }

    private static void StyleTypography(GameObject root, OrchardEarningTheme theme, TMP_FontAsset font, Material material)
    {
        var headingRule = FindRule(theme, "octobergarden/title");
        var buttonRule = FindRule(theme, "octobergarden/primary-button");
        foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
        {
            bool longBody = text.text != null && (text.text.Length > 75 || text.text.IndexOf('\n') >= 0);
            bool orangeHeading = false;
            for (Transform node = text.transform.parent; node != null && node != root.transform; node = node.parent)
            {
                var background = node.GetComponent<Image>();
                if (background != null && background.sprite != null)
                {
                    string path = AssetDatabase.GetAssetPath(background.sprite);
                    if (path == Common + "btn/Btn_Orange.asset" || (headingRule != null && path == headingRule.ReplacementPath))
                    { orangeHeading = true; break; }
                }
            }
            bool display = !longBody && (orangeHeading || text.fontSize >= theme.DisplayFontMinimumSize);
            if (display) { text.font = font; text.fontSharedMaterial = material; }
            Color current = text.color;
            // Preserve disabled labels, explicit red conditions, and light-on-dark
            // labels. Only imported dark brown lettering is unified here.
            if (current.a > .9f && current.r > current.g * 1.15f && current.r > current.b * 1.15f &&
                current.r < .8f && current.g > .08f)
                text.color = display ? theme.HeadingColor : theme.BodyColor;
            if (orangeHeading) text.color = theme.ButtonTextColor;
            Button button = text.GetComponentInParent<Button>();
            if (button != null)
            {
                Image buttonImage = button.targetGraphic as Image;
                string path = buttonImage != null && buttonImage.sprite != null ? AssetDatabase.GetAssetPath(buttonImage.sprite) : string.Empty;
                if (path == Common + "btn/Btn_Normal.asset" || path == Common + "btn/Btn_Orange.asset" ||
                    (buttonRule != null && path == buttonRule.ReplacementPath))
                    text.color = theme.ButtonTextColor;
            }
        }
    }

    private static void StyleOptionLabels(GameObject root, OrchardEarningTheme theme)
    {
        // Selection only swaps the option sprite at runtime. A single dark label
        // color remains readable on both the cream and bright green backgrounds.
        foreach (ApkEarningPageView page in root.GetComponentsInChildren<ApkEarningPageView>(true))
        {
            for (int i = 0; i < 4; i++)
            {
                Button option = page.GetButton("options[" + i + "]");
                if (option == null) continue;
                foreach (TMP_Text label in option.GetComponentsInChildren<TMP_Text>(true))
                    label.color = theme.BodyColor;
            }
        }
    }

    private static void AddAppleAccent(OrchardEarningTheme theme, Image title, bool requested)
    {
        Transform existing = title.transform.Find(AccentName);
        if (!theme.AddTitleApples || !requested)
        {
            if (existing != null) Object.DestroyImmediate(existing.gameObject);
            return;
        }
        Image image;
        if (existing == null)
        {
            var go = new GameObject(AccentName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = title.gameObject.layer; go.transform.SetParent(title.transform, false);
            image = go.GetComponent<Image>();
        }
        else image = existing.GetComponent<Image>();
        image.sprite = SpriteAt(theme.AppleAccentPath);
        image.type = Image.Type.Simple; image.preserveAspect = true; image.raycastTarget = false;
        var rect = image.rectTransform;
        rect.anchorMin = rect.anchorMax = theme.AppleAccentAnchor;
        rect.pivot = new Vector2(.5f, .5f); rect.anchoredPosition = theme.AppleAccentPosition;
        rect.sizeDelta = theme.AppleAccentSize;
        rect.localRotation = Quaternion.identity; rect.localScale = Vector3.one;
    }

    private static void StyleSuccessLayout(GameObject root, string path, OrchardEarningTheme theme)
    {
        string name = Path.GetFileNameWithoutExtension(path);
        if (!theme.RestyleSuccessLayout || (name != "SuccessPanel" && name != "LocalSuccess")) return;
        var view = root.GetComponent<ApkEarningPageView>();
        if (view == null) return;
        Transform title = root.transform.Find("main/Title");
        Transform panel = root.transform.Find("bg");
        if (title == null || panel == null)
            throw new InvalidOperationException("Receipt artwork hierarchy changed: " + path);
        SetCenteredRect((RectTransform)panel, theme.SuccessPanelPosition, theme.SuccessPanelSize);
        SetCenteredRect((RectTransform)title, theme.SuccessTitlePosition, theme.SuccessTitleSize);
        TMP_Text caption = title.GetComponentInChildren<TMP_Text>(true);
        if (caption != null) SetCenteredRect(caption.rectTransform, Vector2.zero, theme.SuccessLabelSize);
        Sprite crestSprite = SpriteAt(theme.AppleCrestPath);
        Image crest = name == "LocalSuccess"
            ? ReuseInheritedReceiptCrest(title, theme.PanelFolder.TrimEnd('/') + "/SuccessPanel.prefab", crestSprite)
            : null;
        if (crest == null) crest = FindOrCreateImage(title, "OrchardReceiptCrest");
        crest.sprite = crestSprite; crest.type = Image.Type.Simple;
        crest.preserveAspect = true; crest.raycastTarget = false;
        SetCenteredRect(crest.rectTransform, theme.SuccessCrestPosition, theme.SuccessCrestSize);
    }

    private static void StyleStatsLayout(GameObject root, string path, OrchardEarningTheme theme)
    {
        if (Path.GetFileNameWithoutExtension(path) != "PlayerInfo") return;
        RectTransform header = root.transform.Find("Time") as RectTransform;
        if (header != null) header.anchoredPosition = theme.StatsHeaderPosition;
        foreach (string labelPath in new[] { "1/label_32", "2/label_33", "3/label_34" })
        {
            Transform node = root.transform.Find(labelPath);
            TMP_Text label = node != null ? node.GetComponent<TMP_Text>() : null;
            if (label != null) label.margin = theme.StatsLabelMargin;
        }
    }

    private static Image ReuseInheritedReceiptCrest(Transform title, string basePath, Sprite crestSprite)
    {
        const string name = "OrchardReceiptCrest";
        Image inherited = null;
        for (int i = 0; i < title.childCount; i++)
        {
            Transform child = title.GetChild(i);
            if (child.name != name || PrefabUtility.GetCorrespondingObjectFromSourceAtPath(child.gameObject, basePath) == null) continue;
            inherited = child.GetComponent<Image>();
            if (inherited != null) break;
        }
        if (inherited == null) return null;

        // Repair the earlier variant-first authoring pass. Remove only an added
        // override with this tool's exact decorative shape; keep inherited and
        // business objects even if someone gave them the same name.
        for (int i = title.childCount - 1; i >= 0; i--)
        {
            Transform child = title.GetChild(i);
            if (child == inherited.transform || child.name != name || child.childCount != 0 ||
                !PrefabUtility.IsAddedGameObjectOverride(child.gameObject)) continue;
            Image image = child.GetComponent<Image>();
            if (image == null || image.raycastTarget || image.sprite != crestSprite ||
                child.GetComponent<CanvasRenderer>() == null || child.GetComponents<Component>().Length != 3) continue;
            Object.DestroyImmediate(child.gameObject);
        }
        return inherited;
    }

    private static void StyleTestCaption(GameObject root, OrchardEarningTheme theme)
    {
        foreach (ApkEarningTopView top in root.GetComponentsInChildren<ApkEarningTopView>(true))
        {
            Transform caption = top.transform.Find("TestModeCaption");
            if (caption == null) continue;
            var text = caption.GetComponent<TMP_Text>();
            var rect = (RectTransform)caption;
            if (text != null) text.color = theme.BodyColor;
            Image plate = FindOrCreateImage(caption.parent, "OrchardTestBackdrop");
            plate.sprite = SpriteAt(theme.TestBackdropPath); plate.type = Image.Type.Sliced;
            plate.pixelsPerUnitMultiplier = theme.TestBackdropPixelsPerUnitMultiplier;
            plate.color = theme.TestBackdropColor; plate.raycastTarget = false;
            var plateRect = plate.rectTransform;
            plateRect.anchorMin = rect.anchorMin; plateRect.anchorMax = rect.anchorMax;
            plateRect.pivot = rect.pivot; plateRect.anchoredPosition = rect.anchoredPosition;
            plateRect.sizeDelta = rect.sizeDelta + theme.TestBackdropPadding;
            plateRect.localScale = Vector3.one; plateRect.localRotation = Quaternion.identity;
            if (plate.transform.GetSiblingIndex() > caption.GetSiblingIndex())
                plate.transform.SetSiblingIndex(caption.GetSiblingIndex());
        }
    }

    private static void AddRewardOverlay(OrchardEarningTheme theme, Image icon, string value, TMP_FontAsset font, Material material)
    {
        const string name = "OrchardRewardMultiplier";
        Transform old = icon.transform.Find(name);
        if (string.IsNullOrEmpty(value))
        {
            if (old != null) Object.DestroyImmediate(old.gameObject);
            return;
        }
        TMP_Text label;
        if (old != null) label = old.GetComponent<TMP_Text>();
        else
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.layer = icon.gameObject.layer; go.transform.SetParent(icon.transform, false);
            label = go.GetComponent<TextMeshProUGUI>();
        }
        label.text = value; label.font = font; label.fontSharedMaterial = material;
        label.fontSize = theme.RewardOverlayFontSize; label.color = theme.ButtonTextColor;
        label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false;
        label.enableWordWrapping = false;
        SetCenteredRect(label.rectTransform, theme.RewardOverlayPosition, theme.RewardOverlaySize);
        label.rectTransform.anchorMin = label.rectTransform.anchorMax = new Vector2(.5f, 0);
    }

    private static Image FindOrCreateImage(Transform parent, string name)
    {
        Transform old = parent.Find(name);
        if (old != null) return old.GetComponent<Image>();
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = parent.gameObject.layer; go.transform.SetParent(parent, false);
        return go.GetComponent<Image>();
    }

    private static void SetCenteredRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = position; rect.sizeDelta = size;
        rect.localScale = Vector3.one; rect.localRotation = Quaternion.identity;
    }

    private static void ConfigureThemeTextures(OrchardEarningTheme theme)
    {
        foreach (var rule in theme.TextureRules)
        {
            if (string.IsNullOrEmpty(rule.Path) || !File.Exists(rule.Path))
                throw new InvalidOperationException("Missing orchard theme texture: " + rule.Path);
            if (rule.MaxTextureSize < 32 || rule.MaxTextureSize > 8192 || !Mathf.IsPowerOfTwo(rule.MaxTextureSize))
                throw new InvalidOperationException("Orchard texture size must be a power of two from 32 to 8192: " + rule.Path);
            var importer = AssetImporter.GetAtPath(rule.Path) as TextureImporter;
            if (importer == null)
            {
                AssetDatabase.ImportAsset(rule.Path, ImportAssetOptions.ForceUpdate);
                importer = AssetImporter.GetAtPath(rule.Path) as TextureImporter;
            }
            if (importer == null) throw new InvalidOperationException("Not an orchard theme texture: " + rule.Path);
            bool changed = importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single ||
                importer.spriteBorder != rule.Border || importer.spritePixelsPerUnit != 100 || importer.mipmapEnabled ||
                importer.maxTextureSize != rule.MaxTextureSize || !importer.alphaIsTransparency ||
                importer.textureCompression != TextureImporterCompression.Compressed;
            if (!changed) continue;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spriteBorder = rule.Border; importer.spritePixelsPerUnit = 100;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
            importer.maxTextureSize = rule.MaxTextureSize;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.SaveAndReimport();
        }
    }

    private static void ConfigureCrest(string path)
    {
        if (!File.Exists(path)) throw new InvalidOperationException("Missing orchard crest: " + path);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) throw new InvalidOperationException("Not a texture: " + path);
        bool changed = importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single ||
            importer.mipmapEnabled || importer.maxTextureSize != 1024 || !importer.alphaIsTransparency;
        if (!changed) return;
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.maxTextureSize = 1024;
        importer.spritePixelsPerUnit = 100; importer.spriteBorder = Vector4.zero;
        importer.textureCompression = TextureImporterCompression.Compressed; importer.SaveAndReimport();
    }

    private static int OptionTextureSize(OrchardEarningTheme theme, string source)
    {
        // Keep runtime copies at the same import resolution as their authored
        // source so swapping an option sprite retains its nine-slice geometry.
        foreach (var rule in theme.TextureRules) if (rule.Path == source) return rule.MaxTextureSize;
        return 512;
    }

    private static void WriteOptionResource(string source, string target, Vector4 border, int maxSize)
    {
        byte[] desired = File.ReadAllBytes(source);
        bool write = !File.Exists(target);
        if (!write)
        {
            byte[] current = File.ReadAllBytes(target);
            write = current.Length != desired.Length;
            for (int i = 0; !write && i < current.Length; i++) if (current[i] != desired[i]) write = true;
        }
        if (write) { File.WriteAllBytes(target, desired); AssetDatabase.ImportAsset(target, ImportAssetOptions.ForceUpdate); }
        var importer = AssetImporter.GetAtPath(target) as TextureImporter;
        if (importer == null) throw new InvalidOperationException("Missing runtime option texture: " + target);
        if (!write && importer.textureType == TextureImporterType.Sprite && importer.spriteBorder == border &&
            importer.spriteImportMode == SpriteImportMode.Single && importer.spritePixelsPerUnit == 100 &&
            !importer.mipmapEnabled && importer.alphaIsTransparency && importer.maxTextureSize == maxSize &&
            importer.textureCompression == TextureImporterCompression.Compressed) return;
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.spriteBorder = border; importer.spritePixelsPerUnit = 100;
        importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.maxTextureSize = maxSize;
        importer.textureCompression = TextureImporterCompression.Compressed; importer.SaveAndReimport();
    }

    private static Material GetDisplayMaterial(OrchardEarningTheme theme, TMP_FontAsset font)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(theme.DisplayMaterialPath);
        if (material == null)
        {
            material = new Material(font.material);
            AssetDatabase.CreateAsset(material, theme.DisplayMaterialPath);
        }
        material.shader = font.material.shader;
        material.CopyPropertiesFromMaterial(font.material);
        material.SetColor("_OutlineColor", theme.DisplayOutlineColor);
        material.SetFloat("_OutlineWidth", theme.DisplayOutlineWidth);
        material.SetColor("_FaceColor", Color.white);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static string HierarchyIndices(Transform root, Transform target)
    {
        string result = string.Empty;
        for (Transform node = target; node != root && node != null; node = node.parent)
            result = node.GetSiblingIndex() + "/" + result;
        return result;
    }

    private static Sprite SpriteAt(string path)
    {
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path)) if (asset is Sprite sprite) return sprite;
        throw new InvalidOperationException("Missing orchard sprite: " + path);
    }

    private static T Require<T>(string path) where T : Object
    {
        T result = AssetDatabase.LoadAssetAtPath<T>(path);
        if (result == null) throw new InvalidOperationException("Missing orchard asset: " + path);
        return result;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent); AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
