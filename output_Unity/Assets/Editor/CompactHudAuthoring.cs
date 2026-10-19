using System;
using System.Collections.Generic;
using FruitApkEarning;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Builds the approved compact HUD into saved Prefabs, never at runtime.</summary>
public static class CompactHudAuthoring
{
    public const string ThemePath = "Assets/OrchardEarningArt/CompactHud/CompactHudTheme.asset";
    const string RootName = "CompactHud";
    static CompactHudTheme theme;
    static TMP_FontAsset font;
    static Material plain, outlined;
    static Sprite levelSprite, settingsSprite, chevronSprite;

    [MenuItem("Tools/Apk Earning/Apply Approved Compact HUD %#&h")]
    public static void ApplyApproved()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play before authoring.");
        theme = AssetDatabase.LoadAssetAtPath<CompactHudTheme>(ThemePath);
        if (theme == null) { theme = ScriptableObject.CreateInstance<CompactHudTheme>(); AssetDatabase.CreateAsset(theme, ThemePath); }
        ApplyIfConfigured();
        Debug.Log("[CompactHud] Saved approved gameplay HUD and native level/settings row.");
    }

    public static void ApplyAndValidate()
    {
        ApplyApproved();
        ApkEarningValidation.ValidateCompactGameplayHud();
    }

    public static void ApplyIfConfigured()
    {
        theme = AssetDatabase.LoadAssetAtPath<CompactHudTheme>(ThemePath);
        if (theme == null || !theme.Enabled) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play before authoring.");
        font = Require<TMP_FontAsset>(theme.Font); plain = Require<Material>(theme.PlainMaterial); outlined = Require<Material>(theme.OutlinedMaterial);
        ConfigureControls();
        var root = PrefabUtility.LoadPrefabContents(theme.TopPrefab);
        try
        {
            var view = root.GetComponent<ApkEarningPageView>();
            var controller = root.GetComponent<ApkEarningTopView>();
            Sprite cashIcon = root.transform.Find("GoldItem/Icon").GetComponent<Image>().sprite;
            Sprite diamondIcon = root.transform.Find("CoinItem/Icon").GetComponent<Image>().sprite;
            // Preserve source bindings for rebuilding/reference, but no source visual
            // is enabled by the compact runtime path.
            foreach (Transform child in root.transform) if (child.name != RootName) child.gameObject.SetActive(false);
            RectTransform top = (RectTransform)root.transform;
            top.anchorMin = new Vector2(0, 1); top.anchorMax = Vector2.one; top.pivot = new Vector2(.5f, 1);
            top.anchoredPosition = Vector2.zero; top.sizeDelta = Vector2.zero; top.localScale = Vector3.one;
            RectTransform content = Node(root.transform, RootName, Vector2.zero, new Vector2(theme.ReferenceWidth, 0));
            content.anchorMin = content.anchorMax = new Vector2(.5f, 1);
            content.gameObject.SetActive(true);
            view.FitWidth = false;
            ConfigureCurrency(content, view, "Cash", theme.CashPosition, theme.CashSize, cashIcon, "$3.15", "OriginalGoldItem");
            ConfigureCurrency(content, view, "Diamonds", theme.DiamondPosition, theme.DiamondSize, diamondIcon, "102", "OriginalCoinItem");
            BindObject(view, "CoinItem", content.Find("Diamonds").gameObject);
            var task = Picture(content, "Task", theme.TaskPosition, theme.TaskSize, SpriteAt(theme.Card), theme.TaskPixelsPerUnit);
            var label = Label(task.transform, "TaskLabel", theme.TaskLabelPosition, theme.TaskLabelSize, "Clear level 5", theme.TaskFontSize, false);
            label.alignment = TextAlignmentOptions.MidlineLeft;
            BindText(view, "CompactTaskLabel", label);
            Picture(task.transform, "ProgressTrack", theme.ProgressPosition, theme.ProgressSize, SpriteAt(theme.Track), 5);
            var fill = Picture(task.transform, "ProgressFill", theme.ProgressPosition, theme.ProgressSize, SpriteAt(theme.Fill), 6);
            fill.rectTransform.pivot = new Vector2(0, .5f);
            fill.rectTransform.anchoredPosition = theme.ProgressPosition - new Vector2(theme.ProgressSize.x / 2, 0);
            BindProgress(view, "CompactTaskProgress", fill, theme.ProgressSize.x);
            view.SetProgress("CompactTaskProgress", .6f);
            BindText(view, "CompactTaskRatio", Label(task.transform, "Ratio", theme.ProgressPosition, theme.ProgressSize, "3 / 5", theme.RatioFontSize, true));
            var action = Picture(task.transform, "View", theme.ViewPosition, theme.ViewSize, SpriteAt(theme.Action), theme.ButtonPixelsPerUnit);
            BindButton(view, "CompactTaskView", Clickable(action));
            Label(action.transform, "Label", Vector2.zero, theme.ViewSize - new Vector2(14, 12), "View", 40, true);
            // The wallet keeps the complete task details; gameplay can use a single row.
            task.gameObject.SetActive(theme.ShowTaskStrip);
            // Keep the test-state notice in the wallet, without a gameplay banner.
            foreach (string path in new[] { RootName + "/Disclosure", "TestModeCaption", "OrchardTestBackdrop" })
            {
                var notice = root.transform.Find(path);
                if (notice != null) Object.DestroyImmediate(notice.gameObject);
            }
            controller.CompactHud = true;
            controller.ReferenceWidth = theme.ReferenceWidth; controller.ReferencePosition = Vector2.zero;
            controller.ReferenceSettingsY = theme.RowY; controller.ReferenceViewportTop = theme.ViewportTop;
            PrefabUtility.SaveAsPrefabAsset(root, theme.TopPrefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        ApplyNativeControls();
        AssetDatabase.SaveAssets();
    }

    static void ConfigureCurrency(Transform parent, ApkEarningPageView view, string name, Vector2 pos, Vector2 size, Sprite icon, string value, string alias)
    {
        Image card = Picture(parent, name, pos, size, SpriteAt(theme.Card), theme.CardPixelsPerUnit);
        BindButton(view, alias + ".click", Clickable(card));
        Picture(card.transform, "Icon", new Vector2(-size.x / 2 + 57, 0), theme.CurrencyIconSize, icon, 1, true);
        var text = Label(card.transform, "Value", new Vector2(8, 0), theme.CurrencyValueSize, value, theme.BalanceFontSize, false);
        text.fontSizeMin = 28;
        BindText(view, alias + ".value", text);
        Picture(card.transform, "Chevron", new Vector2(size.x / 2 - 41, 0), theme.ChevronSize, chevronSprite, 1, true);
    }

    static void ApplyNativeControls()
    {
        var core = PrefabUtility.LoadPrefabContents(theme.CorePrefab);
        try
        {
            var frame = (RectTransform)core.transform.Find("ApkEarningFrame");
            var expand = frame.GetComponent<Orange.ProtectedAreaAdaptReverse>();
            if (expand != null) Object.DestroyImmediate(expand);
            frame.anchorMin = Vector2.zero; frame.anchorMax = Vector2.one;
            frame.offsetMin = frame.offsetMax = Vector2.zero;
            var row = (RectTransform)core.transform.Find("UI/Top");
            row.anchorMin = row.anchorMax = new Vector2(.5f, 1); row.pivot = new Vector2(.5f, .5f);
            row.anchoredPosition = new Vector2(0, theme.RowY); row.sizeDelta = new Vector2(theme.ReferenceWidth, 0); row.localScale = Vector3.one;
            var level = (RectTransform)row.Find("Level"); SetRect(level, theme.LevelPosition, theme.LevelSize);
            foreach (string name in new[] { "Bg_Normal", "Bg_Hard", "Bg_SuperHard" })
            {
                var bg = level.Find(name).GetComponent<Image>(); SetRect(bg.rectTransform, Vector2.zero, theme.LevelSize);
                Style(bg, levelSprite, 1, true);
                var number = bg.transform.Find("Text (TMP)").GetComponent<TMP_Text>();
                SetRect(number.rectTransform, theme.LevelNumberPosition, theme.LevelNumberSize); StyleText(number, theme.LevelNumberFontSize, true); number.text = "4";
                Transform ornament = bg.transform.Find("Image"); if (ornament != null) ornament.gameObject.SetActive(false);
                Label(bg.transform, "CompactLevelCaption", theme.LevelCaptionPosition, theme.LevelCaptionSize, "LEVEL", theme.LevelCaptionFontSize, true);
            }
            var setting = row.Find("SettingBtn"); SetRect((RectTransform)setting, theme.SettingsPosition, theme.SettingsSize);
            // Native CorePlayUI still binds this same standard Button in code.
            var visual = setting.GetComponent<Image>(); if (visual == null) visual = setting.gameObject.AddComponent<Image>();
            Style(visual, settingsSprite, 1, true); Clickable(visual);
            foreach (Transform child in setting) child.gameObject.SetActive(false);
            row.Find("CommonCoinBtn").gameObject.SetActive(false);
            var controller = core.GetComponentInChildren<ApkEarningTopView>(true);
            // Unity stores root RectTransform overrides when a prefab is nested.
            // Update those too, otherwise the source's new top anchors are ignored.
            var hudRect = (RectTransform)controller.transform;
            hudRect.anchorMin = new Vector2(0, 1); hudRect.anchorMax = Vector2.one;
            hudRect.pivot = new Vector2(.5f, 1); hudRect.anchoredPosition = Vector2.zero;
            hudRect.sizeDelta = Vector2.zero; hudRect.localScale = Vector3.one;
            PrefabUtility.RecordPrefabInstancePropertyModifications(hudRect);
            controller.LevelSettingsRow = row;
            controller.ReferenceSettingsY = theme.RowY; controller.ReferenceViewportTop = theme.ViewportTop; controller.ReferencePosition = Vector2.zero;
            if (controller.GameplayViewport != null) controller.GameplayViewport.offsetMax = new Vector2(controller.GameplayViewport.offsetMax.x, -theme.ViewportTop);
            PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
            PrefabUtility.SaveAsPrefabAsset(core, theme.CorePrefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(core); }
    }

    static void ConfigureControls()
    {
        var importer = AssetImporter.GetAtPath(theme.Controls) as TextureImporter;
        if (importer == null) throw new InvalidOperationException("Missing HUD controls texture.");
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.mipmapEnabled = false; importer.alphaIsTransparency = true; importer.maxTextureSize = 1024;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.spritesheet = new[] {
            new SpriteMetaData { name="HudLevel", rect=new Rect(118, 64, 609, 600), pivot=new Vector2(.5f,.5f), alignment=(int)SpriteAlignment.Center },
            new SpriteMetaData { name="HudSettings", rect=new Rect(838, 77, 570, 574), pivot=new Vector2(.5f,.5f), alignment=(int)SpriteAlignment.Center },
            new SpriteMetaData { name="HudChevron", rect=new Rect(1586, 129, 454, 446), pivot=new Vector2(.5f,.5f), alignment=(int)SpriteAlignment.Center }
        };
        importer.SaveAndReimport();
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(theme.Controls))
        {
            if (!(asset is Sprite sprite)) continue;
            if (sprite.name == "HudLevel") levelSprite = sprite;
            else if (sprite.name == "HudSettings") settingsSprite = sprite;
            else if (sprite.name == "HudChevron") chevronSprite = sprite;
        }
        if (levelSprite == null || settingsSprite == null || chevronSprite == null) throw new InvalidOperationException("HUD sprite import failed.");
    }
    static RectTransform Node(Transform parent, string name, Vector2 position, Vector2 size)
    {
        var old = parent.Find(name); RectTransform rect;
        if (old == null) { var go = new GameObject(name, typeof(RectTransform)); go.layer = 5; go.transform.SetParent(parent, false); rect = (RectTransform)go.transform; }
        else rect = (RectTransform)old;
        SetRect(rect, position, size); return rect;
    }
    static void SetRect(RectTransform rect, Vector2 position, Vector2 size)
    { rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f,.5f); rect.anchoredPosition = position; rect.sizeDelta = size; rect.localScale = Vector3.one; }
    static Image Picture(Transform parent, string name, Vector2 pos, Vector2 size, Sprite sprite, float ppu, bool aspect=false)
    { var rect=Node(parent,name,pos,size); var image=rect.GetComponent<Image>(); if(image==null)image=rect.gameObject.AddComponent<Image>(); Style(image,sprite,ppu,aspect); return image; }
    static void Style(Image image, Sprite sprite, float ppu, bool aspect)
    { image.sprite=sprite; image.type=aspect?Image.Type.Simple:Image.Type.Sliced; image.pixelsPerUnitMultiplier=ppu; image.preserveAspect=aspect; image.color=Color.white; image.raycastTarget=false; }
    static Button Clickable(Image image)
    { var button=image.GetComponent<Button>(); if(button==null)button=image.gameObject.AddComponent<Button>(); button.targetGraphic=image; image.raycastTarget=true; button.onClick=new Button.ButtonClickedEvent(); button.navigation=new Navigation{mode=Navigation.Mode.None}; return button; }
    static TMP_Text Label(Transform parent,string name,Vector2 pos,Vector2 size,string value,float fontSize,bool outline)
    { var rect=Node(parent,name,pos,size); var text=rect.GetComponent<TextMeshProUGUI>(); if(text==null)text=rect.gameObject.AddComponent<TextMeshProUGUI>(); StyleText(text,fontSize,outline); text.text=value; return text; }
    static void StyleText(TMP_Text text,float size,bool outline)
    { text.font=font; text.fontSharedMaterial=outline?outlined:plain; text.fontSize=text.fontSizeMax=size; text.fontSizeMin=size*.65f; text.enableAutoSizing=true; text.enableWordWrapping=false; text.alignment=TextAlignmentOptions.Center; text.color=outline?Color.white:theme.Ink; text.raycastTarget=false; text.fontStyle=FontStyles.Normal; text.margin=Vector4.zero; }
    static Sprite SpriteAt(string path)
    { foreach(Object asset in AssetDatabase.LoadAllAssetsAtPath(path))if(asset is Sprite sprite)return sprite; throw new InvalidOperationException("Missing Sprite: "+path); }
    static T Require<T>(string path) where T:Object
    { var asset=AssetDatabase.LoadAssetAtPath<T>(path); if(asset==null)throw new InvalidOperationException("Missing asset: "+path); return asset; }
    static void BindText(ApkEarningPageView view,string key,TMP_Text target)
    { foreach(var slot in view.Texts)if(slot.Key==key){slot.Target=target;return;} var list=new List<ApkEarningPageView.TextSlot>(view.Texts);list.Add(new ApkEarningPageView.TextSlot{Key=key,Target=target});view.Texts=list.ToArray(); }
    static void BindButton(ApkEarningPageView view,string key,Button target)
    { foreach(var slot in view.Buttons)if(slot.Key==key){slot.Target=target;return;}var list=new List<ApkEarningPageView.ButtonSlot>(view.Buttons);list.Add(new ApkEarningPageView.ButtonSlot{Key=key,Target=target});view.Buttons=list.ToArray(); }
    static void BindObject(ApkEarningPageView view,string key,GameObject target)
    { foreach(var slot in view.Objects)if(slot.Key==key){slot.Target=target;return;}var list=new List<ApkEarningPageView.ObjectSlot>(view.Objects);list.Add(new ApkEarningPageView.ObjectSlot{Key=key,Target=target});view.Objects=list.ToArray(); }
    static void BindProgress(ApkEarningPageView view,string key,Image target,float width)
    {
        foreach(var slot in view.Images)if(slot.Key==key){slot.Target=target;slot.ResizeWidthWithProgress=true;slot.FullProgressWidth=width;return;}
        var list=new List<ApkEarningPageView.ImageSlot>(view.Images);list.Add(new ApkEarningPageView.ImageSlot{Key=key,Target=target,ResizeWidthWithProgress=true,FullProgressWidth=width});view.Images=list.ToArray();
    }
}
