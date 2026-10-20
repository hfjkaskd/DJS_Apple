using System;
using System.Collections.Generic;
using System.IO;
using FruitApkEarning;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Authors responsive anchors into saved prefabs; the player creates no layout nodes.</summary>
public static class ApkEarningModalSizingAuthoring
{
    private const string Folder = "Assets/ApkEarningReference/Resources/ApkEarningReference/Panels";
    private const string BaselinePath = "Assets/Editor/ApkEarningModalSizingBaseline.json";
    [Serializable] private sealed class Baseline { public List<Page> pages = new List<Page>(); }
    [Serializable] private sealed class Page
    {
        public string path;
        public float width, height;
        public List<Node> nodes = new List<Node>();
    }
    [Serializable] private sealed class Node
    {
        public string path, name;
        public Vector2 min, max, pivot, position, size;
        public Vector3 scale;
        public float fontSize, fontMin, fontMax, ppu;
        public int imageType = -1;
    }
    private sealed class Geometry
    {
        public RectTransform rect;
        public float height, parentHeight, position, parentPivot;
        public bool stretch;
    }

    [MenuItem("Tools/Apk Earning/Enlarge Modal Layouts and Validate")]
    public static void ApplyAndValidate()
    {
        Apply();
        ApkEarningValidation.ValidateEnlargedModalFixtures();
    }

    public static void ApplyValidateAndAnimate()
    {
        ApplyAndValidate();
        ApkEarningFlowAnimationValidation.Run();
    }

    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before changing saved modal layouts.");
        Baseline baseline = File.Exists(BaselinePath)
            ? JsonUtility.FromJson<Baseline>(File.ReadAllText(BaselinePath))
            : CaptureBaseline();
        foreach (Page saved in baseline.pages)
        {
            var root = PrefabUtility.LoadPrefabContents(saved.path);
            try
            {
                var view = root.GetComponent<ApkEarningPageView>();
                Restore(root.transform, saved);
                view.ReferenceWidth = saved.width;
                view.MinimumReferenceHeight = saved.height;
                view.RefreshLayout();
                Configure(view, Path.GetFileNameWithoutExtension(saved.path));
                PrefabUtility.SaveAsPrefabAsset(root, saved.path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        EnlargeRewardItems("Item_Big");
        EnlargeRewardItems("LocalLuckyItem");
        EnlargeHistoryRows();
        AssetDatabase.SaveAssets();
        Debug.Log("[ApkEarningModalSizing] Saved " + baseline.pages.Count +
            " responsive modal prefabs. Gameplay HUD, gameplay objects and player data are unchanged.");
    }

    private static Baseline CaptureBaseline()
    {
        var baseline = new Baseline();
        var paths = new List<string>();
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { Folder }))
            paths.Add(AssetDatabase.GUIDToAssetPath(guid));
        // Save bases before variants both when capturing and when applying.
        paths.Sort(StringComparer.Ordinal);
        paths.Remove(Folder + "/SuccessPanel.prefab");
        paths.Insert(0, Folder + "/SuccessPanel.prefab");
        foreach (string path in paths)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var sourceView = source == null ? null : source.GetComponent<ApkEarningPageView>();
            if (sourceView == null || !sourceView.FitWidth || sourceView.ArtworkRoot == null) continue;
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var view = root.GetComponent<ApkEarningPageView>();
                var page = new Page { path = path, width = view.ReferenceWidth, height = view.MinimumReferenceHeight };
                foreach (var rect in root.GetComponentsInChildren<RectTransform>(true))
                {
                    var node = new Node {
                        path = IndexPath(root.transform, rect), name = rect.name,
                        min = rect.anchorMin, max = rect.anchorMax, pivot = rect.pivot,
                        position = rect.anchoredPosition, size = rect.sizeDelta, scale = rect.localScale
                    };
                    var text = rect.GetComponent<TMP_Text>();
                    if (text != null) { node.fontSize = text.fontSize; node.fontMin = text.fontSizeMin; node.fontMax = text.fontSizeMax; }
                    var image = rect.GetComponent<Image>();
                    if (image != null) { node.imageType = (int)image.type; node.ppu = image.pixelsPerUnitMultiplier; }
                    page.nodes.Add(node);
                }
                baseline.pages.Add(page);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        File.WriteAllText(BaselinePath, JsonUtility.ToJson(baseline, true));
        AssetDatabase.ImportAsset(BaselinePath);
        return baseline;
    }

    private static void Restore(Transform root, Page page)
    {
        if (root.GetComponentsInChildren<RectTransform>(true).Length != page.nodes.Count)
            throw new InvalidOperationException("Modal hierarchy changed; update its authoring baseline: " + page.path);
        foreach (Node node in page.nodes)
        {
            Transform target = root;
            if (!string.IsNullOrEmpty(node.path))
                foreach (string part in node.path.Split('/'))
                {
                    int index = int.Parse(part, System.Globalization.CultureInfo.InvariantCulture);
                    if (index >= target.childCount) throw new InvalidOperationException("Modal hierarchy changed: " + page.path);
                    target = target.GetChild(index);
                }
            if (target.name != node.name) throw new InvalidOperationException("Modal baseline no longer matches " + page.path + "/" + node.path);
            var rect = (RectTransform)target;
            rect.anchorMin = node.min; rect.anchorMax = node.max; rect.pivot = node.pivot;
            rect.sizeDelta = node.size; rect.anchoredPosition = node.position; rect.localScale = node.scale;
            var text = rect.GetComponent<TMP_Text>();
            if (text != null) { text.fontSize = node.fontSize; text.fontSizeMin = node.fontMin; text.fontSizeMax = node.fontMax; }
            var image = rect.GetComponent<Image>();
            if (image != null && node.imageType >= 0) { image.type = (Image.Type)node.imageType; image.pixelsPerUnitMultiplier = node.ppu; }
        }
    }

    private static void Configure(ApkEarningPageView view, string name)
    {
        RectTransform artwork = view.ArtworkRoot;
        float referenceHeight = name == "TXCoinPanel" ? 1420 : 1550;
        if (name == "TXPanel") referenceHeight = 1500;
        // Short loading/receipt messages keep a short-card shape.
        if (name == "TXGuideLoadingPanel") referenceHeight = 1920;
        var fixedGroups = new HashSet<RectTransform>();
        foreach (var button in artwork.GetComponentsInChildren<Button>(true)) fixedGroups.Add((RectTransform)button.transform);
        foreach (var input in artwork.GetComponentsInChildren<TMP_InputField>(true)) fixedGroups.Add((RectTransform)input.transform);
        foreach (var layout in artwork.GetComponentsInChildren<LayoutGroup>(true))
            if (layout.enabled) foreach (RectTransform child in layout.transform) fixedGroups.Add(child);
        for (int i = 1; i <= 4; i++)
        {
            var row = view.GetObject("path:main/Step/" + i);
            if (row != null) fixedGroups.Add((RectTransform)row.transform);
        }
        for (int i = 0; i < 7; i++)
        {
            var row = view.GetObject("rows[" + i + "].Root");
            if (row != null) fixedGroups.Add((RectTransform)row.transform);
            var bonus = view.GetObject("bonusRows[" + i + "]");
            if (bonus != null) fixedGroups.Add((RectTransform)bonus.transform);
        }
        // Capture local parent-relative geometry before changing any ancestor.
        var geometry = new List<Geometry>();
        foreach (RectTransform rect in artwork.GetComponentsInChildren<RectTransform>(true))
        {
            if (rect == artwork || InsideFixedGroup(rect, artwork, fixedGroups)) continue;
            var parent = rect.parent as RectTransform;
            if (parent == null) continue;
            float parentHeight = parent == artwork ? referenceHeight : parent.rect.height;
            if (parentHeight <= .01f) continue;
            var image = rect.GetComponent<Image>();
            bool panel = image != null && image.sprite != null &&
                (image.sprite.name == "CashWalletPanel" || image.sprite.name == "CashWalletInset");
            bool container = image == null && rect.GetComponent<TMP_Text>() == null &&
                rect.GetComponent<Button>() == null && !fixedGroups.Contains(rect);
            bool stretch = (container || panel) && !fixedGroups.Contains(rect) && rect.rect.height > 180;
            geometry.Add(new Geometry {
                rect = rect, height = rect.rect.height, position = rect.localPosition.y,
                parentHeight = parentHeight, parentPivot = parent.pivot.y, stretch = stretch
            });
            if (panel && stretch && image.sprite.border.sqrMagnitude > 0 && image.type == Image.Type.Simple)
            {
                image.type = Image.Type.Sliced;
                image.pixelsPerUnitMultiplier = image.sprite.rect.width / Mathf.Max(1, rect.rect.width);
            }
        }
        foreach (Geometry entry in geometry)
        {
            var rect = entry.rect;
            float center = entry.parentPivot + entry.position / entry.parentHeight;
            Vector2 min = rect.anchorMin, max = rect.anchorMax, size = rect.sizeDelta, position = rect.anchoredPosition;
            min.y = max.y = center;
            if (entry.stretch)
            {
                min.y -= entry.height * rect.pivot.y / entry.parentHeight;
                max.y += entry.height * (1 - rect.pivot.y) / entry.parentHeight;
                size.y = 0;
            }
            else size.y = entry.height;
            position.y = 0;
            rect.anchorMin = min; rect.anchorMax = max; rect.sizeDelta = size; rect.anchoredPosition = position;
        }
        var testCaption = view.GetText("TestMode");
        foreach (TMP_Text text in artwork.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text != testCaption && text.fontSize > 0 && text.fontSize < 36)
            {
                text.fontSize = 36;
                text.fontSizeMax = Mathf.Max(text.fontSizeMax, 36);
                var rect = text.rectTransform;
                if (rect.rect.height < 44) rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 44);
            }
        }
        view.ReferenceWidth = 1000;
        view.MinimumReferenceHeight = referenceHeight;
        view.RefreshLayout();
        foreach (Button button in artwork.GetComponentsInChildren<Button>(true))
        {
            var rect = (RectTransform)button.transform;
            if (rect.rect.width < 500 || rect.rect.height < 110) continue;
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(720, rect.rect.width));
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(200, rect.rect.height));
            foreach (TMP_Text text in button.GetComponentsInChildren<TMP_Text>(true))
                if (text.fontSize >= 48)
                {
                    text.fontSize = Mathf.Max(72, text.fontSize);
                    text.fontSizeMax = Mathf.Max(72, text.fontSizeMax);
                    text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                        Mathf.Max(116, text.rectTransform.rect.height));
                }
        }
        if (name == "TXPanel")
        {
            PlaceAtArtworkY(view.GetText("labels[0].Text").rectTransform, artwork, .75f);
            // This secondary caption sits between the fixed-height CTA and statistics.
            // Keep breathing room on the shortest supported portrait aspect ratio.
            var statsCaption = view.GetText("Stats.Time");
            statsCaption.fontSize = statsCaption.fontSizeMax = 30;
            PlaceAtArtworkY(statsCaption.rectTransform, artwork, .243f);
        }
        if (name == "TXCoinPanel")
            for (int i = 0; i < 4; i++)
            {
                var option = view.GetButton("options[" + i + "]");
                ((RectTransform)option.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 180);
                var amount = view.GetText("amounts[" + i + "]");
                amount.fontSize = amount.fontSizeMax = 76;
                amount.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 112);
            }
        for (int i = 0; i < 7; i++)
        {
            string key = "rows[" + i + "]";
            var row = view.GetObject(key + ".Root");
            if (row == null) continue;
            var rowRect = (RectTransform)row.transform;
            rowRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 108);
            var title = view.GetText(key + ".Name");
            var value = view.GetText(key + ".Value");
            if (title != null) PlaceRelativeToRow(title.rectTransform, rowRect, 28);
            if (value != null) PlaceRelativeToRow(value.rectTransform, rowRect, -28);
        }
    }

    private static void PlaceAtArtworkY(RectTransform rect, RectTransform artwork, float normalizedY)
    {
        var parent = (RectTransform)rect.parent;
        var world = rect.position;
        var relative = artwork.InverseTransformPoint(world);
        relative.y = (normalizedY - artwork.pivot.y) * artwork.rect.height;
        float localY = parent.InverseTransformPoint(artwork.TransformPoint(relative)).y;
        var min = rect.anchorMin; var max = rect.anchorMax; var position = rect.anchoredPosition;
        min.y = max.y = parent.pivot.y + localY / parent.rect.height;
        position.y = 0;
        rect.anchorMin = min; rect.anchorMax = max; rect.anchoredPosition = position;
    }

    private static void PlaceRelativeToRow(RectTransform text, RectTransform row, float offset)
    {
        Vector3 point = row.TransformPoint(new Vector3(0, offset, 0));
        var position = text.position; position.y = point.y; text.position = position;
        text.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 44);
    }

    private static void EnlargeRewardItems(string name)
    {
        string path = Folder + "/" + name + ".prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var view = root.GetComponent<ApkEarningPageView>();
            var amount = view.GetText("count");
            amount.fontSize = amount.fontSizeMax = 88;
            var amountPosition = amount.rectTransform.anchoredPosition;
            amountPosition.y = -135; amount.rectTransform.anchoredPosition = amountPosition;
            var icon = view.GetImage("icon").rectTransform;
            icon.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 315);
            icon.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 260);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static void EnlargeHistoryRows()
    {
        string path = Folder + "/LocalCashHistoryRow.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var view = root.GetComponent<ApkEarningPageView>();
            var amount = view.GetText("amountLabel");
            amount.fontSize = amount.fontSizeMax = 64;
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static bool InsideFixedGroup(RectTransform rect, RectTransform artwork, HashSet<RectTransform> fixedGroups)
    {
        for (var parent = rect.parent; parent != null && parent != artwork; parent = parent.parent)
            if (fixedGroups.Contains(parent as RectTransform)) return true;
        return false;
    }

    private static string IndexPath(Transform root, Transform rect)
    {
        string path = "";
        for (Transform current = rect; current != root; current = current.parent)
            path = current.GetSiblingIndex().ToString(System.Globalization.CultureInfo.InvariantCulture) +
                (path.Length == 0 ? "" : "/" + path);
        return path;
    }
}
