using System;
using System.Collections.Generic;
using FruitApkEarning;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Applies only the restored flow pages and the compact statistics footer.</summary>
public static class ApkEarningFlowAdditionsAuthoring
{
    private const string Panels = "Assets/ApkEarningReference/Resources/ApkEarningReference/Panels/";
    private const string Art = "Assets/OrchardEarningArt/CashWallet/";
    private static TMP_FontAsset font;
    private static Material plain, outlined;

    [MenuItem("Tools/Apk Earning/Apply Restored Flow Pages")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before authoring saved flow pages.");
        var theme = AssetDatabase.LoadAssetAtPath<CompactEarningTheme>(CompactEarningArtAuthoring.ThemePath);
        if (theme == null) throw new InvalidOperationException("Missing compact earning theme.");
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(theme.FontPath);
        plain = AssetDatabase.LoadAssetAtPath<Material>(theme.PlainMaterialPath);
        outlined = AssetDatabase.LoadAssetAtPath<Material>(theme.OutlinedMaterialPath);
        Edit("LocalUserLevel", Rank);
        Edit("TXTooFastHintPanel", Review);
        Edit("TXPanel", Stats);
        Edit("LocalRewardVideoTest", Video);
        AssetDatabase.SaveAssets();
        ApkEarningFlowPrefabValidation.Validate();
        Debug.Log("[ApkEarningFlowAdditionsAuthoring] Saved four targeted pages; other approved pages were not rebuilt.");
    }

    private static void Edit(string name, Action<ApkEarningPageView> edit)
    {
        string path = Panels + name + ".prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var view = root.GetComponent<ApkEarningPageView>();
            if (view == null || view.ArtworkRoot == null) throw new InvalidOperationException("Unbound page " + name);
            edit(view);
            if (name == "LocalUserLevel" || name == "TXTooFastHintPanel")
                RoundedEarningProgressAuthoring.Apply(root, true);
            if (name == "LocalUserLevel") view.SetProgress("progressFill", 0);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static void Rank(ApkEarningPageView v)
    {
        SetImage(v, "Card", "CashWalletPanel", 0, 0, 936, 1000);
        SetImage(v, "Heading", "CashWalletHeader", 20, 478, 620, 148);
        SetImage(v, "Close", "CashWalletClose", 430, 450, 112, 112);
        Text(v, "Title", 20, 478, 560, 112, 56, true);
        Text(v, "levelLabel", 0, 230, 750, 110, 70);
        Text(v, "ProgressCaption", 0, 85, 750, 70, 40);
        SetImage(v, "ProgressTrack", "CashWalletProgressTrack", 0, -20, 750, 76);
        SetImage(v, "progressFill", "CashWalletProgressFill", 0, -20, 720, 52);
        Text(v, "progressLabel", 0, -20, 650, 60, 36, true);
        SetImage(v, "Continue", "CashWalletButton", 0, -325, 676, 160);
        Text(v, "Caption", 0, -325, 606, 116, 62, true);
    }

    private static void Review(ApkEarningPageView v)
    {
        SetImage(v, "bg", "CashWalletPanel", 0, 0, 936, 1200);
        foreach (string key in new[] { "bg (1)", "bg (2)", "bg (3)" }) v.GetImage(key).enabled = false;
        SetImage(v, "Title", "CashWalletHeader", 20, 578, 620, 148);
        var accent = v.GetImage("Title").transform.Find("OrchardAppleAccent");
        if (accent != null) accent.gameObject.SetActive(false);
        Text(v, "labels[1].Text", 20, 578, 560, 112, 48, true);
        Text(v, "labels[0].Text", 0, 350, 760, 180, 38);
        Text(v, "LevelTip", 0, 160, 740, 70, 38);
        var levelFill = v.GetImage("LevelPregress");
        SetProgressPair(v, levelFill, 70);
        Text(v, "LevelPregressValue", 0, 70, 650, 60, 36, true);
        Text(v, "VideoTip", 0, -80, 740, 70, 38);
        var videoFill = v.GetImage("VideoPregress");
        SetProgressPair(v, videoFill, -170);
        Text(v, "VideoPregressValue", 0, -170, 650, 60, 36, true);
        StyleImage(v.GetButton("IKnowBtn").GetComponent<Image>(), "CashWalletButton");
        Rect(v, v.GetButton("IKnowBtn").transform as RectTransform, 0, -430, 676, 160);
        Text(v, "labels[2].Text", 0, -430, 606, 116, 62, true);
    }

    private static void SetProgressPair(ApkEarningPageView v, Image fill, float y)
    {
        var track = fill.transform.parent.GetComponent<Image>();
        if (track == null) throw new InvalidOperationException("Progress track is missing.");
        StyleImage(track, "CashWalletProgressTrack");
        Rect(v, track.rectTransform, 0, y, 750, 76);
        StyleImage(fill, "CashWalletProgressFill");
        Rect(v, fill.rectTransform, 0, y, 720, 52);
    }

    private static void Stats(ApkEarningPageView v)
    {
        Text(v, "Stats.Time", 0, -382, 720, 34, 24);
        v.SetText("Stats.Time", "Simulated stats");
        Text(v, "Stats.PassCount", -238, -432, 224, 76, 27);
        Text(v, "Stats.ChallengeTimes", 0, -432, 224, 76, 27);
        Text(v, "Stats.GoldGetCount", 238, -432, 224, 76, 27);
        v.SetText("Stats.PassCount", "Players today\nâ€”");
        v.SetText("Stats.ChallengeTimes", "Avg. attempts\nâ€”");
        v.SetText("Stats.GoldGetCount", "Avg. amount\nâ€”");
        var divider = v.ArtworkRoot.Find("CashStatsDivider") as RectTransform;
        if (divider != null) Rect(v, divider, 0, -484, 704, 2);
        var history = v.GetButton("HistoryButton");
        if (history != null) Rect(v, history.transform as RectTransform, 0, -532, 760, 66);
        Text(v, "HistoryCaption", 0, -532, 760, 66, 32);
    }

    private static void Video(ApkEarningPageView v)
    {
        v.SetText("title", "Test reward video");
        v.SetText("body", "This is a local test. No advertisement or payment service is connected.");
        v.SetText("actionCaption", "Complete test video");
        var cancel = v.GetButton("close").GetComponentInChildren<TMP_Text>(true);
        cancel.text = "Cancel";
        if (v.GetText("cancelCaption") == null)
        {
            var texts = new List<ApkEarningPageView.TextSlot>(v.Texts);
            texts.Add(new ApkEarningPageView.TextSlot { Key = "cancelCaption", Target = cancel });
            v.Texts = texts.ToArray();
        }
    }

    private static void SetImage(ApkEarningPageView v, string key, string sprite, float x, float y, float w, float h)
    {
        var image = v.GetImage(key);
        if (image == null) throw new InvalidOperationException(v.name + " missing image " + key);
        StyleImage(image, sprite);
        Rect(v, image.rectTransform, x, y, w, h);
    }

    private static void StyleImage(Image image, string name)
    {
        image.enabled = true;
        image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + name + ".png");
        if (image.sprite == null) throw new InvalidOperationException("Missing artwork " + name);
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
        image.color = Color.white;
        foreach (Shadow shadow in image.GetComponents<Shadow>()) shadow.enabled = false;
    }

    private static void Text(ApkEarningPageView v, string key, float x, float y, float w, float h, float size, bool white = false)
    {
        var text = v.GetText(key);
        if (text == null) throw new InvalidOperationException(v.name + " missing text " + key);
        text.gameObject.SetActive(true);
        text.font = font;
        text.fontSharedMaterial = white ? outlined : plain;
        text.fontSize = text.fontSizeMax = size;
        text.fontSizeMin = size * .8f;
        text.enableAutoSizing = true;
        text.enableWordWrapping = true;
        text.fontStyle = FontStyles.Normal;
        text.fontWeight = FontWeight.Regular;
        text.alignment = TextAlignmentOptions.Center;
        text.color = white ? new Color32(255, 255, 244, 255) : new Color32(113, 57, 27, 255);
        text.enableVertexGradient = false;
        text.margin = Vector4.zero;
        text.raycastTarget = false;
        Rect(v, text.rectTransform, x, y, w, h);
    }

    private static void Rect(ApkEarningPageView v, RectTransform rect, float x, float y, float w, float h)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.sizeDelta = new Vector2(w, h);
        rect.position = v.ArtworkRoot.TransformPoint(new Vector3(x, y, 0));
    }
}
