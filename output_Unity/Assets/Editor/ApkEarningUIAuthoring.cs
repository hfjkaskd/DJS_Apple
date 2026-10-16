using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using FruitApkEarning;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Editor authoring only. Binds imported artwork without reconstructing its layout.</summary>
public static class ApkEarningUIAuthoring
{
    private const string ManifestPath = "Assets/ApkEarningReference/apk-earning-ui-import.json";
    private const string OldHostPath = "Assets/res/local/withdrawal/WithdrawalHost.prefab";
    private const string HostPath = "Assets/res/local/withdrawal/ApkEarningHost.prefab";
    private const string ConfigPath = "Assets/Resources/ApkEarningConfig.asset";
    private const string PanelRoot = "Assets/ApkEarningReference/Resources/ApkEarningReference/Panels/";
    private const string TopPath = "Assets/res/local/withdrawal/ApkEarningTop.prefab";
    private const string CorePlayPath = "Assets/res/local/coreplay/CorePlayUI.prefab";
    private const string TextCatalogPath = "Assets/ApkEarningReference/apk-ui-text-and-levels.json";
    [Serializable] private sealed class TextCatalog { public LabelRecord[] labels; public ApkEarningTopView.LevelDisplay[] levels; }
    [Serializable] private sealed class LabelRecord { public int id; public string text; }
    private static TextCatalog textCatalog;
    private static readonly Dictionary<int, string> englishLabels = new Dictionary<int, string>();

    [Serializable] private sealed class Manifest { public PrefabRecord[] prefabs; }
    [Serializable] private sealed class PrefabRecord
    {
        public string source, target, guid, resourcePath;
        public NodeRecord[] hierarchy;
        public ControllerRecord[] removedControllers;
    }
    [Serializable] private sealed class NodeRecord
    {
        public long fileId, gameObjectFileId, transformFileId, sourceFileId;
        public int classId, siblingIndex;
        public string path, name, componentType, sourcePrefab;
    }
    [Serializable] private sealed class ControllerRecord
    {
        public long fileId;
        public string path, script;
        public string serializedComponent;
        public BindingRecord[] bindings;
    }
    [Serializable] private sealed class BindingRecord
    {
        public string field, path, guid, sourceAsset, componentType;
        public long fileId, gameObjectFileId;
        public int classId;
    }

    [MenuItem("Tools/Apk Earning/Build")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before authoring APK earning prefabs.");
        textCatalog = JsonUtility.FromJson<TextCatalog>(File.ReadAllText(TextCatalogPath));
        englishLabels.Clear();
        foreach (LabelRecord label in textCatalog.labels) if (!englishLabels.ContainsKey(label.id)) englishLabels.Add(label.id, label.text);
        var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(ManifestPath));
        if (manifest == null || manifest.prefabs == null || manifest.prefabs.Length == 0)
            throw new InvalidOperationException("Import APK earning visual assets before running authoring.");
        var byTarget = new Dictionary<string, PrefabRecord>(StringComparer.Ordinal);
        var bySource = new Dictionary<string, PrefabRecord>(StringComparer.Ordinal);
        foreach (PrefabRecord record in manifest.prefabs)
        {
            byTarget.Add(record.target, record);
            bySource.Add(record.source, record);
        }
        int totalBindings = 0;
        var authored = new HashSet<string>(StringComparer.Ordinal);
        foreach (PrefabRecord record in manifest.prefabs)
            BuildPage(record, byTarget, bySource, authored, ref totalBindings);
        BuildCashComposition();
        BuildArtworkResources();
        BuildGameplayTop();
        BuildDiamondGuide();
        BuildRecoveryPage();
        BuildHost();
        BuildConfig();
        OrchardEarningArtAuthoring.ApplyIfConfigured();
        AssetDatabase.SaveAssets();
        GameResCatalogBuilder.Build();
        Debug.Log("[ApkEarningUIAuthoring] Saved " + authored.Count + " imported pages and " + totalBindings +
                  " serialized bindings, plus ApkEarningHost and ApkEarningConfig. Original artwork layouts preserved.");
    }

    private static void BuildPage(PrefabRecord record, Dictionary<string, PrefabRecord> byTarget,
        Dictionary<string, PrefabRecord> bySource, HashSet<string> authored, ref int totalBindings)
    {
        if (authored.Contains(record.target)) return;
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(record.target);
        if (asset == null) throw new InvalidOperationException("Missing imported prefab: " + record.target);
        var records = new List<PrefabRecord>();
        GameObject baseAsset = PrefabUtility.GetCorrespondingObjectFromSource(asset);
        if (baseAsset != null)
        {
            string basePath = AssetDatabase.GetAssetPath(baseAsset);
            if (byTarget.TryGetValue(basePath, out PrefabRecord baseRecord))
            {
                BuildPage(baseRecord, byTarget, bySource, authored, ref totalBindings);
                records.Add(baseRecord);
                asset = AssetDatabase.LoadAssetAtPath<GameObject>(record.target);
            }
        }
        records.Add(record);
        GameObject root = PrefabUtility.LoadPrefabContents(record.target);
        try
        {
            // Source fileIDs identify duplicate sibling names without relying on Transform.Find.
            var objects = BuildFileIdMap(asset, root);
            var slots = new SlotBuilder();
            foreach (PrefabRecord owner in records)
            {
                if (owner.removedControllers == null) continue;
                var controllers = new List<ControllerRecord>(owner.removedControllers);
                controllers.Sort((left, right) => ControllerPriority(left).CompareTo(ControllerPriority(right)));
                foreach (ControllerRecord controller in controllers)
                {
                    string componentName = string.IsNullOrEmpty(controller.script) ? "Controller" : Path.GetFileNameWithoutExtension(controller.script);
                    if (controller.bindings == null) continue;
                    foreach (BindingRecord binding in controller.bindings)
                    {
                        if (binding.fileId == 0 || string.IsNullOrEmpty(binding.field)) continue;
                        Object target = Resolve(binding, owner.guid, root, objects, bySource);
                        if (target == null)
                        {
                            // Settings and source-only business components have no visual counterpart.
                            if (!string.IsNullOrEmpty(binding.path))
                                throw new InvalidOperationException(record.target + ": cannot resolve " + componentName + "." + binding.field + " at " + binding.path);
                            continue;
                        }
                        AuditResolvedPath(target, binding, root, record.target);
                        slots.Add(binding.field, target);
                        slots.Add(componentName + "." + binding.field, target);
                        // Repeat helper types (localized labels, animation targets) retain distinct bindings.
                        slots.Add("source:" + controller.fileId + ":" + binding.field, target);
                        if (target is RectTransform animated && (binding.field == "title" || binding.field == "startContainer") && animated.localScale.sqrMagnitude < .000001f)
                            animated.localScale = Vector3.one;
                    }
                }
            }
            // Stable generic aliases remain available for source-owned decorative text and buttons.
            foreach (Transform node in root.GetComponentsInChildren<Transform>(true))
            {
                string path = RelativePath(node, root.transform);
                string key = path.Length == 0 ? "$root" : path;
                slots.Add(key, node.gameObject);
                slots.Add("path:" + key, node.gameObject);
                slots.Add(node.name, node.gameObject);
                foreach (Component component in node.GetComponents<Component>())
                {
                    if (component == null) throw new InvalidOperationException("Missing script in imported prefab: " + record.target);
                    if (!(component is TMP_Text) && !(component is Button) && !(component is Image) && !(component is TMP_InputField)) continue;
                    slots.Add(key, component);
                    slots.Add("path:" + key, component);
                    slots.Add(node.name, component);
                }
            }
            var view = root.GetComponent<ApkEarningPageView>();
            if (view == null) view = root.AddComponent<ApkEarningPageView>();
            slots.Apply(view);
            ConfigurePageArtwork(root, view, record.source);
            root.SetActive(true);
            if (view.ArtworkRoot != null)
            {
                view.ArtworkRoot.gameObject.SetActive(true);
                if (view.ArtworkRoot.localScale.sqrMagnitude < .000001f) view.ArtworkRoot.localScale = Vector3.one;
                var group = view.ArtworkRoot.GetComponent<CanvasGroup>();
                if (group != null && group.alpha <= 0) group.alpha = 1;
            }
            foreach (Button button in root.GetComponentsInChildren<Button>(true))
            {
                while (button.onClick.GetPersistentEventCount() > 0)
                    UnityEventTools.RemovePersistentListener(button.onClick, 0);
                if (button.targetGraphic == null)
                {
                    Graphic graphic = button.GetComponent<Graphic>();
                    if (graphic == null) throw new InvalidOperationException("Button has no owned graphic: " + record.target + "/" + RelativePath(button.transform, root.transform));
                    button.targetGraphic = graphic;
                }
            }
            ApplyEnglishLabels(view, records);
            if (record.target.EndsWith("/TXRecordGuidePanel.prefab", StringComparison.Ordinal))
            {
                GameObject currency = view.GetObject("currencyParticles");
                var renderer = currency == null ? null : currency.GetComponent<ParticleSystemRenderer>();
                if (renderer != null && renderer.sharedMaterial != null)
                {
                    const string materialPath = "Assets/ApkEarningReference/Visuals/CurrencyParticlesUS.mat";
                    Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                    if (material == null) { material = new Material(renderer.sharedMaterial); AssetDatabase.CreateAsset(material, materialPath); }
                    material.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ApkEarningReference/Visuals/atlas/Golds/US1.png"));
                    renderer.sharedMaterial = material; EditorUtility.SetDirty(material);
                }
            }
            if (record.target.EndsWith("/LocalEarningSequence.prefab", StringComparison.Ordinal))
            {
                for (int i = 0; i < 4; i++)
                {
                    Image artwork = view.GetImage("messageLayouts[" + i + "].Artwork");
                    string name = i == 0 || i == 3 ? "cash-banner" : "portrait-banner";
                    if (artwork != null) artwork.sprite = LoadSpriteAt("Assets/ApkEarningReference/Visuals/atlas/localearningmessages/" + name + ".png");
                }
            }
            if (record.target.EndsWith("/Item_Big.prefab", StringComparison.Ordinal) || record.target.EndsWith("/LocalLuckyItem.prefab", StringComparison.Ordinal))
            {
                AppendText(view, "value", view.GetText("count"));
                AppendImage(view, "image", view.GetImage("icon"));
                view.SetVisible("max", false);
            }
            PrefabUtility.SaveAsPrefabAsset(root, record.target);
            totalBindings += slots.Count;
            authored.Add(record.target);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static int ControllerPriority(ControllerRecord controller)
    {
        bool atRoot = !string.IsNullOrEmpty(controller.path) && controller.path.IndexOf('/') < 0;
        bool panel = !string.IsNullOrEmpty(controller.script) && (controller.script.EndsWith("Panel.cs", StringComparison.Ordinal) || controller.script.EndsWith("Row.cs", StringComparison.Ordinal));
        return atRoot ? panel ? 0 : 1 : 2;
    }

    private static void ConfigurePageArtwork(GameObject root, ApkEarningPageView view, string source)
    {
        bool panel = source.StartsWith("Resources/prefabs/panels/", StringComparison.Ordinal);
        string name = Path.GetFileNameWithoutExtension(source);
        bool fullPage = panel && name != "LocalMainPanelTop" && name != "LocalCashHistoryEntry" && name != "LocalCashHistoryRow";
        RectTransform artwork = root.transform.Find("main") as RectTransform;
        if (fullPage && artwork == null)
        {
            artwork = root.transform.Find("ApkArtwork") as RectTransform;
            if (artwork == null)
            {
                var children = new List<Transform>();
                foreach (Transform child in root.transform) children.Add(child);
                var go = new GameObject("ApkArtwork", typeof(RectTransform)); go.layer = 5;
                go.transform.SetParent(root.transform, false); artwork = (RectTransform)go.transform;
                foreach (Transform child in children)
                {
                    RectTransform rect = child as RectTransform;
                    Vector2 anchored = rect == null ? Vector2.zero : rect.anchoredPosition;
                    child.SetParent(artwork, false);
                    if (rect != null) rect.anchoredPosition = anchored;
                }
            }
        }
        view.ArtworkRoot = artwork == null ? (RectTransform)root.transform : artwork;
        view.FitWidth = fullPage;
        view.ReferenceWidth = 1080; view.MinimumReferenceHeight = 1800;
        if (fullPage)
        {
            artwork.anchorMin = artwork.anchorMax = new Vector2(.5f, .5f);
            artwork.pivot = new Vector2(.5f, .5f); artwork.anchoredPosition = Vector2.zero;
            artwork.sizeDelta = new Vector2(1080, 1920); artwork.localScale = Vector3.one;
        }
    }

    private static Dictionary<string, Object> BuildFileIdMap(GameObject asset, GameObject contents)
    {
        var result = new Dictionary<string, Object>(StringComparer.Ordinal);
        foreach (Transform sourceNode in asset.GetComponentsInChildren<Transform>(true))
        {
            Transform destination = BySiblingIndices(sourceNode, asset.transform, contents.transform);
            if (destination == null) throw new InvalidOperationException("Prefab content hierarchy differs from its asset.");
            RegisterIds(sourceNode.gameObject, destination.gameObject, result);
            Component[] source = sourceNode.GetComponents<Component>();
            Component[] target = destination.GetComponents<Component>();
            if (source.Length != target.Length) throw new InvalidOperationException("Prefab component lists differ while binding.");
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] == null || target[i] == null) throw new InvalidOperationException("Imported prefab contains a missing script.");
                RegisterIds(source[i], target[i], result);
            }
        }
        return result;
    }

    private static string ObjectKey(string guid, long fileId)
    { return guid + ":" + fileId.ToString(System.Globalization.CultureInfo.InvariantCulture); }

    private static void RegisterIds(Object source, Object destination, Dictionary<string, Object> map)
    {
        // A local fileID is only unique within its own asset. PlayerInfo and
        // TXPanel reuse several IDs, so nested prefab originals must never
        // overwrite the containing page's bindings.
        if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(source, out string guid, out long id))
            map[ObjectKey(guid, id)] = destination;
        Object original = PrefabUtility.GetCorrespondingObjectFromOriginalSource(source);
        if (original != null && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(original, out guid, out id))
            map[ObjectKey(guid, id)] = destination;
    }

    private static Transform BySiblingIndices(Transform source, Transform sourceRoot, Transform destinationRoot)
    {
        var indices = new List<int>();
        for (Transform node = source; node != sourceRoot; node = node.parent)
        {
            if (node == null) return null;
            indices.Add(node.GetSiblingIndex());
        }
        Transform current = destinationRoot;
        for (int i = indices.Count - 1; i >= 0; i--)
        {
            if (indices[i] >= current.childCount) return null;
            current = current.GetChild(indices[i]);
        }
        return current;
    }

    private static Object Resolve(BindingRecord binding, string ownerGuid, GameObject root, Dictionary<string, Object> objects,
        Dictionary<string, PrefabRecord> bySource)
    {
        if (!string.IsNullOrEmpty(binding.sourceAsset))
        {
            if (bySource.TryGetValue(binding.sourceAsset, out PrefabRecord asset))
                return AssetDatabase.LoadAssetAtPath<GameObject>(asset.target);
            return null;
        }
        string guid = string.IsNullOrEmpty(binding.guid) ? ownerGuid : binding.guid;
        if (objects.TryGetValue(ObjectKey(guid, binding.fileId), out Object exact)) return exact;
        if (binding.gameObjectFileId != 0 && objects.TryGetValue(ObjectKey(guid, binding.gameObjectFileId), out Object owner))
            return VisualOn(owner as GameObject, binding);
        if (string.IsNullOrEmpty(binding.path)) return null;
        int slash = binding.path.IndexOf('/');
        string relative = slash < 0 ? string.Empty : binding.path.Substring(slash + 1);
        Transform target = relative.Length == 0 ? root.transform : root.transform.Find(relative);
        return target == null ? null : VisualOn(target.gameObject, binding);
    }

    private static Object VisualOn(GameObject owner, BindingRecord binding)
    {
        if (owner == null) return null;
        switch (binding.componentType)
        {
            case "TextMeshProUGUI": case "TMP_Text": return owner.GetComponent<TMP_Text>();
            case "Image": case "OriginalGoldImage": return owner.GetComponent<Image>();
            case "Button": return owner.GetComponent<Button>();
            case "TMP_InputField": return owner.GetComponent<TMP_InputField>();
        }
        return owner;
    }

    private static void AuditResolvedPath(Object target, BindingRecord binding, GameObject root, string assetPath)
    {
        if (!string.IsNullOrEmpty(binding.sourceAsset) || string.IsNullOrEmpty(binding.path)) return;
        GameObject go = target as GameObject;
        if (target is Component component) go = component.gameObject;
        if (go == null) return;
        int slash = binding.path.IndexOf('/');
        string expected = slash < 0 ? string.Empty : binding.path.Substring(slash + 1);
        string actual = RelativePath(go.transform, root.transform);
        if (actual == "ApkArtwork") actual = string.Empty;
        else if (actual.StartsWith("ApkArtwork/", StringComparison.Ordinal)) actual = actual.Substring("ApkArtwork/".Length);
        if (!string.Equals(actual, expected, StringComparison.Ordinal))
            throw new InvalidOperationException(assetPath + ": " + binding.field + " resolved to " + actual + " instead of source path " + expected);
    }

    private static string RelativePath(Transform node, Transform root)
    {
        if (node == root) return string.Empty;
        var names = new List<string>();
        for (Transform current = node; current != null && current != root; current = current.parent) names.Add(current.name);
        names.Reverse();
        return string.Join("/", names);
    }

    private sealed class SlotBuilder
    {
        private readonly Dictionary<string, TMP_Text> texts = new Dictionary<string, TMP_Text>(StringComparer.Ordinal);
        private readonly Dictionary<string, Button> buttons = new Dictionary<string, Button>(StringComparer.Ordinal);
        private readonly Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>(StringComparer.Ordinal);
        private readonly Dictionary<string, Image> images = new Dictionary<string, Image>(StringComparer.Ordinal);
        private readonly Dictionary<string, TMP_InputField> inputs = new Dictionary<string, TMP_InputField>(StringComparer.Ordinal);
        public int Count => texts.Count + buttons.Count + objects.Count + images.Count + inputs.Count;
        public void Add(string key, Object value)
        {
            if (string.IsNullOrEmpty(key) || value == null) return;
            if (value is TMP_Text text) { if (!texts.ContainsKey(key)) texts.Add(key, text); }
            else if (value is Button button) { if (!buttons.ContainsKey(key)) buttons.Add(key, button); }
            else if (value is Image image) { if (!images.ContainsKey(key)) images.Add(key, image); }
            else if (value is TMP_InputField input) { if (!inputs.ContainsKey(key)) inputs.Add(key, input); }
            else if (value is GameObject go) { if (!objects.ContainsKey(key)) objects.Add(key, go); }
            else if (value is Component component) { if (!objects.ContainsKey(key)) objects.Add(key, component.gameObject); }
        }
        public void Apply(ApkEarningPageView view)
        {
            var textSlots = new List<ApkEarningPageView.TextSlot>();
            foreach (var slot in texts) textSlots.Add(new ApkEarningPageView.TextSlot { Key = slot.Key, Target = slot.Value });
            var buttonSlots = new List<ApkEarningPageView.ButtonSlot>();
            foreach (var slot in buttons) buttonSlots.Add(new ApkEarningPageView.ButtonSlot { Key = slot.Key, Target = slot.Value });
            var objectSlots = new List<ApkEarningPageView.ObjectSlot>();
            foreach (var slot in objects) objectSlots.Add(new ApkEarningPageView.ObjectSlot { Key = slot.Key, Target = slot.Value });
            var imageSlots = new List<ApkEarningPageView.ImageSlot>();
            foreach (var slot in images) imageSlots.Add(new ApkEarningPageView.ImageSlot { Key = slot.Key, Target = slot.Value });
            var inputSlots = new List<ApkEarningPageView.InputSlot>();
            foreach (var slot in inputs) inputSlots.Add(new ApkEarningPageView.InputSlot { Key = slot.Key, Target = slot.Value });
            view.Texts = textSlots.ToArray(); view.Buttons = buttonSlots.ToArray(); view.Objects = objectSlots.ToArray();
            view.Images = imageSlots.ToArray(); view.Inputs = inputSlots.ToArray();
        }
    }

    private static void BuildHost()
    {
        if (!File.Exists(HostPath) && !AssetDatabase.CopyAsset(OldHostPath, HostPath))
            throw new InvalidOperationException("Cannot create APK earning host from existing host layout.");
        GameObject root = PrefabUtility.LoadPrefabContents(HostPath);
        try
        {
            root.name = "ApkEarningHost";
            var old = root.GetComponent<FruitWithdrawal.WithdrawalHost>();
            if (old != null) Object.DestroyImmediate(old);
            var host = root.GetComponent<ApkEarningHost>();
            if (host == null) host = root.AddComponent<ApkEarningHost>();
            var blocker = root.GetComponent<Image>();
            if (blocker == null) blocker = root.AddComponent<Image>();
            blocker.color = Color.clear; blocker.raycastTarget = true;
            // BaseUI applies the game's simulated safe area. The imported APK's
            // modal pages use the full width-matched canvas, including the shade.
            if (root.GetComponent<Orange.ProtectedAreaAdaptReverse>() == null)
                root.AddComponent<Orange.ProtectedAreaAdaptReverse>();
            RectTransform pageRoot = root.transform.Find("PageRoot") as RectTransform;
            if (pageRoot == null)
            {
                var child = new GameObject("PageRoot", typeof(RectTransform)); child.layer = 5;
                child.transform.SetParent(root.transform, false); pageRoot = (RectTransform)child.transform;
            }
            pageRoot.anchorMin = Vector2.zero; pageRoot.anchorMax = Vector2.one;
            pageRoot.anchoredPosition = Vector2.zero; pageRoot.sizeDelta = Vector2.zero;
            pageRoot.SetAsFirstSibling(); host.PageRoot = pageRoot;
            Transform existingCaption = root.transform.Find("TestModeCaption");
            TextMeshProUGUI caption;
            if (existingCaption == null)
            {
                var child = new GameObject("TestModeCaption", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI)); child.layer = 5;
                child.transform.SetParent(root.transform, false); caption = child.GetComponent<TextMeshProUGUI>();
            }
            else caption = existingCaption.GetComponent<TextMeshProUGUI>();
            var captionRect = (RectTransform)caption.transform;
            captionRect.anchorMin = new Vector2(0, 0); captionRect.anchorMax = new Vector2(1, 0);
            captionRect.anchoredPosition = new Vector2(0, 30); captionRect.sizeDelta = new Vector2(-40, 32);
            captionRect.SetAsLastSibling();
            var fontSource = AssetDatabase.LoadAssetAtPath<GameObject>(PanelRoot + "LocalMainPanelTop.prefab").GetComponent<ApkEarningPageView>().GetText("GoldItem/Value");
            caption.font = fontSource.font; caption.fontSharedMaterial = fontSource.fontSharedMaterial;
            caption.fontSize = 22; caption.alignment = TextAlignmentOptions.Center;
            caption.color = Color.white; caption.raycastTarget = false; caption.enableWordWrapping = false;
            caption.text = "TEST MODE • NO CASH PAYMENT";
            host.GlobalTestModeCaption = caption;
            PrefabUtility.SaveAsPrefabAsset(root, HostPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static void BuildConfig()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        var config = AssetDatabase.LoadAssetAtPath<ApkEarningConfig>(ConfigPath);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<ApkEarningConfig>();
            config.Rules = ApkEarningRules.CreateDefault();
            AssetDatabase.CreateAsset(config, ConfigPath);
        }
        else if (config.Rules == null)
        {
            config.Rules = ApkEarningRules.CreateDefault();
            EditorUtility.SetDirty(config);
        }
        int maximum = 0;
        foreach (var level in textCatalog.levels) maximum = Math.Max(maximum, level.level);
        config.DisplayLevels = new int[maximum + 1];
        foreach (var level in textCatalog.levels) config.DisplayLevels[level.level] = level.showLevel;
        config.PaymentChannels = new[] {
            new ApkPaymentChannel { Name = "Venmo", RequiresName = true, SmallIconPath = "ApkEarningReference/Artwork/VenmoSmall", LargeIconPath = "ApkEarningReference/Artwork/VenmoLarge" },
            new ApkPaymentChannel { Name = "Zelle", RequiresName = true, SmallIconPath = "ApkEarningReference/Artwork/ZelleSmall", LargeIconPath = "ApkEarningReference/Artwork/ZelleLarge" },
            new ApkPaymentChannel { Name = "PayPal", UsesEmail = true, RequiresName = true, SmallIconPath = "ApkEarningReference/Artwork/PayPalSmall", LargeIconPath = "ApkEarningReference/Artwork/PayPalLarge" }
        };
        config.OtherChannelIconPath = "ApkEarningReference/Artwork/OtherChannel";
        config.DiamondPreviewHundredths = 3000000; config.DiamondPreviewSeconds = 2;
        EditorUtility.SetDirty(config);
    }

    private static void ApplyEnglishLabels(ApkEarningPageView view, List<PrefabRecord> records)
    {
        foreach (PrefabRecord record in records)
        foreach (ControllerRecord controller in record.removedControllers ?? Array.Empty<ControllerRecord>())
        {
            string serialized = controller.serializedComponent ?? "";
            MatchCollection labels = Regex.Matches(serialized, @"(?m)^  - Text: \{fileID: \d+\}\r?\n    Id: (\d+)");
            for (int i = 0; i < labels.Count; i++)
                if (int.TryParse(labels[i].Groups[1].Value, out int id) && englishLabels.TryGetValue(id, out string english))
                    view.SetText("source:" + controller.fileId + ":labels[" + i + "].Text", english);
            if (string.IsNullOrEmpty(controller.script)) continue;
            if (controller.script.EndsWith("LocalLocalizedText.cs", StringComparison.Ordinal))
            {
                Match source = Regex.Match(serialized, @"(?m)^  englishSource: (.*)$");
                if (source.Success && source.Groups[1].Value.Length > 0)
                    view.SetText("source:" + controller.fileId + ":label", source.Groups[1].Value.Trim('"', '\r'));
            }
            if (controller.script.EndsWith("OriginalGoldImage.cs", StringComparison.Ordinal))
            {
                Match type = Regex.Match(serialized, @"(?m)^  goldType: (\d+)");
                Image image = view.GetImage("source:" + controller.fileId + ":image");
                if (image != null && type.Success)
                {
                    string path = "Assets/ApkEarningReference/Visuals/atlas/Golds/US" + type.Groups[1].Value + ".asset";
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (sprite == null) throw new InvalidOperationException("Missing USD currency art: " + path);
                    image.sprite = sprite; image.SetNativeSize();
                }
            }
        }
        switch (view.gameObject.name)
        {
            case "TXPanel": view.SetText("labels[0].Text", "Withdrawal amount"); view.SetText("TXText", "Withdraw"); break;
            case "TXCoinPanel": view.SetText("labels[0].Text", "My Diamonds"); view.SetText("labels[1].Text", "Select withdrawal amount"); view.SetText("labels[2].Text", "Withdraw"); break;
            case "TXUserInfoPanel":
                view.SetText("title", "Withdrawal account");
                view.SetText("GetBtn/label_1", "Confirm"); break;
            case "TXRecordGuidePanel":
                if (englishLabels.TryGetValue(163, out string tip)) view.SetText("tip", tip);
                break;
            case "LocalMainPanelTop":
                view.SetText("GoldItem/Click/label_185", "Cash"); view.SetText("CoinItem/Click/label_185", "Cash");
                for (int i = 1; i <= 4; i++) view.SetText("HideLevel/Levels/" + i + "/Get/label_1", "Withdraw");
                view.SetText("HideLevel/Levels/5/Get/label_160", "Withdraw All"); break;
        }
    }

    private static void BuildCashComposition()
    {
        string path = PanelRoot + "TXPanel.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var page = root.GetComponent<ApkEarningPageView>();
            foreach (string level in new[] { "1", "2" })
            {
                Transform expected = root.transform.Find("main/" + level);
                if (expected == null || page.GetObject("Level" + level) != expected.gameObject)
                    throw new InvalidOperationException("Cash Level" + level + " must refer to its own stage, not nested stats.");
            }
            GameObject parent = page.GetObject("PlayerInfoPos");
            if (parent == null) throw new InvalidOperationException("Cash page PlayerInfoPos is missing.");
            Transform existing = parent.transform.Find("PlayerInfo");
            GameObject stats = existing == null ? (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PanelRoot + "PlayerInfo.prefab"), parent.transform) : existing.gameObject;
            stats.name = "PlayerInfo"; stats.transform.localPosition = Vector3.zero; stats.SetActive(true);
            var statsView = stats.GetComponent<ApkEarningPageView>();
            statsView.SetText("labels[0].Text", "Total runs");
            statsView.SetText("labels[1].Text", "Today levels");
            statsView.SetText("labels[2].Text", "Available balance");
            statsView.SetText("Time", "Test stats");
            foreach (string key in new[] { "Time", "PassCount", "ChallengeTimes", "GoldGetCount" })
            {
                TMP_Text value = statsView.GetText(key);
                if (value == null) throw new InvalidOperationException("PlayerInfo text missing: " + key);
                value.gameObject.SetActive(true); value.enabled = true;
                Color color = value.color; color.a = 1; value.color = color;
                AppendText(page, "Stats." + key, value);
            }
            GameObject marqueeParent = statsView.GetObject("PMDPos");
            if (marqueeParent == null) throw new InvalidOperationException("PlayerInfo PMDPos is missing.");
            Transform old = marqueeParent.transform.Find("LocalCashHistoryEntry");
            GameObject entry = old == null ? (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PanelRoot + "LocalCashHistoryEntry.prefab"), marqueeParent.transform) : old.gameObject;
            entry.name = "LocalCashHistoryEntry"; entry.transform.localPosition = Vector3.zero; entry.SetActive(true);
            var entryView = entry.GetComponent<ApkEarningPageView>();
            foreach (TMP_Text text in entry.GetComponentsInChildren<TMP_Text>(true)) text.text = "View withdrawal history";
            GameObject head = entryView.GetObject("Image"); if (head != null) head.SetActive(false);
            Button history = entry.GetComponentInChildren<Button>(true);
            if (history == null) throw new InvalidOperationException("History entry requires its original Button.");
            history.interactable = true;
            AppendButton(page, "HistoryButton", history);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static void BuildArtworkResources()
    {
        string folder = "Assets/ApkEarningReference/Resources/ApkEarningReference/Artwork";
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/ApkEarningReference/Resources/ApkEarningReference", "Artwork");
        CopyAssetOnce("Assets/ApkEarningReference/Visuals/atlas/Golds/US3.asset", folder + "/CashPile.asset");
        CopyAssetOnce("Assets/ApkEarningReference/Visuals/atlas/items/Coin2.asset", folder + "/DiamondPile.asset");
        CopyAssetOnce("Assets/ApkEarningReference/Visuals/atlas/octobergarden/amount-selected.png", folder + "/DiamondOptionSelected.png");
        CopyAssetOnce("Assets/ApkEarningReference/Visuals/atlas/octobergarden/amount-normal.png", folder + "/DiamondOptionNormal.png");
        foreach (string name in new[] { "Venmo", "Zelle", "PayPal" })
        {
            CopyAssetOnce("Assets/ApkEarningReference/Visuals/atlas/paysimple/" + name + ".asset", folder + "/" + name + "Small.asset");
            CopyAssetOnce("Assets/ApkEarningReference/Visuals/atlas/pay/" + (name == "PayPal" ? "paypal" : name) + ".asset", folder + "/" + name + "Large.asset");
        }
        CopyAssetOnce("Assets/ApkEarningReference/Visuals/atlas/pay/Other.asset", folder + "/OtherChannel.asset");
    }

    private static void CopyAssetOnce(string source, string destination)
    {
        if (!File.Exists(destination) && !AssetDatabase.CopyAsset(source, destination))
            throw new InvalidOperationException("Cannot author shared artwork resource: " + destination);
    }

    private static void BuildGameplayTop()
    {
        // This remains a separately authored prefab. The APK reference asset stays unchanged.
        CopyAssetOnce(PanelRoot + "LocalMainPanelTop.prefab", TopPath);
        GameObject top = PrefabUtility.LoadPrefabContents(PanelRoot + "LocalMainPanelTop.prefab");
        try
        {
            top.name = "ApkEarningTop";
            var view = top.GetComponent<ApkEarningPageView>();
            var rect = (RectTransform)top.transform;
            rect.anchorMin = new Vector2(0, .9f); rect.anchorMax = new Vector2(1, .9f);
            rect.anchoredPosition = new Vector2(0, 182); rect.sizeDelta = new Vector2(0, 100);
            rect.localScale = Vector3.one; view.FitWidth = false;
            foreach (string name in new[] { "GM", "PlayerGoldGetHint", "PMDBullet", "Level", "TX_JD", "HideLevel/SubRounds" })
            { Transform node = top.transform.Find(name); if (node != null) node.gameObject.SetActive(false); }
            Transform caption = top.transform.Find("TestModeCaption");
            TextMeshProUGUI text;
            if (caption == null)
            {
                var go = new GameObject("TestModeCaption", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                go.layer = 5; go.transform.SetParent(top.transform, false); text = go.GetComponent<TextMeshProUGUI>();
            }
            else text = caption.GetComponent<TextMeshProUGUI>();
            var textRect = (RectTransform)text.transform;
            textRect.anchorMin = textRect.anchorMax = new Vector2(.5f, .5f);
            textRect.anchoredPosition = new Vector2(0, -450); textRect.sizeDelta = new Vector2(660, 30);
            text.font = view.GetText("GoldItem/Value").font;
            text.fontSize = 22; text.alignment = TextAlignmentOptions.Center;
            text.color = new Color32(83, 34, 14, 255); text.raycastTarget = false;
            text.text = "TEST MODE • NO CASH VALUE";
            var controller = top.GetComponent<ApkEarningTopView>();
            if (controller == null) controller = top.AddComponent<ApkEarningTopView>();
            controller.View = view;
            controller.ReferencePosition = new Vector2(0, 182);
            controller.Levels = textCatalog.levels;
            controller.NormalMilestone = LoadSprite("main-milestone-pending");
            controller.PassedMilestone = LoadSprite("main-milestone-normal");
            controller.ActiveMilestone = LoadSprite("main-milestone-active");
            controller.PendingDot = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ApkEarningReference/Visuals/atlas/newbieguidepanel/dian.asset");
            controller.PassedDot = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ApkEarningReference/Visuals/atlas/newbieguidepanel/dian1.asset");
            GameObject template = view.GetObject("subRoundTemplate");
            if (template == null) throw new InvalidOperationException("Source subround template missing.");
            template.SetActive(false);
            int dotCount = 0;
            foreach (var level in textCatalog.levels) dotCount = Math.Max(dotCount, level.subTotalRound);
            var dots = new List<ApkEarningTopView.DotSlot>();
            for (int i = 0; i < dotCount; i++)
            {
                string name = "ApkSubRound" + i;
                Transform oldDot = template.transform.parent.Find(name);
                GameObject dot = oldDot == null ? Object.Instantiate(template, template.transform.parent) : oldDot.gameObject;
                dot.name = name; dot.SetActive(false);
                dots.Add(new ApkEarningTopView.DotSlot { Root = dot, Left = dot.transform.Find("Line0").gameObject,
                    Right = dot.transform.Find("Line1").gameObject, Icon = dot.transform.Find("Icon").GetComponent<Image>() });
            }
            controller.SubRoundDots = dots.ToArray();
            for (int i = 1; i <= 5; i++)
            {
                GameObject get = view.GetObject("HideLevel/Levels/" + i + "/Get");
                Button button = get.GetComponent<Button>();
                if (button == null) button = get.AddComponent<Button>();
                button.targetGraphic = get.GetComponent<Image>();
                AppendButton(view, "MilestoneButton[" + (i - 1) + "]", button);
            }
            PrefabUtility.SaveAsPrefabAsset(top, TopPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(top); }
        GameObject core = PrefabUtility.LoadPrefabContents(CorePlayPath);
        try
        {
            foreach (FruitWithdrawal.WithdrawalHud old in core.GetComponentsInChildren<FruitWithdrawal.WithdrawalHud>(true))
                Object.DestroyImmediate(old.gameObject);
            Transform oldTop = core.transform.Find("ApkEarningTop");
            if (oldTop != null) Object.DestroyImmediate(oldTop.gameObject);
            RectTransform frame = core.transform.Find("ApkEarningFrame") as RectTransform;
            if (frame == null)
            {
                var go = new GameObject("ApkEarningFrame", typeof(RectTransform), typeof(Orange.ProtectedAreaAdaptReverse));
                go.layer = 5; go.transform.SetParent(core.transform, false); frame = (RectTransform)go.transform;
            }
            frame.anchorMin = Vector2.zero; frame.anchorMax = Vector2.one;
            frame.anchoredPosition = Vector2.zero; frame.sizeDelta = Vector2.zero;
            Transform previous = frame.Find("ApkEarningTop");
            if (previous != null) Object.DestroyImmediate(previous.gameObject);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(TopPath), frame);
            instance.name = "ApkEarningTop";
            Transform levelSettings = core.transform.Find("UI/Top");
            if (levelSettings == null) throw new InvalidOperationException("CorePlayUI/UI/Top row is missing.");
            var row = (RectTransform)levelSettings;
            // Source earnings occupy y=15..430; controls remain on one row beneath it.
            row.anchoredPosition = new Vector2(0, -520);
            var gameplay = core.GetComponent<CorePlay.CorePlayUI>();
            RectTransform map = core.transform.Find("Game/Map") as RectTransform;
            RectTransform ui = core.transform.Find("UI") as RectTransform;
            RectTransform tray = core.transform.Find("UI/Bottom/Box_Root") as RectTransform;
            if (gameplay == null || map == null || ui == null || tray == null)
                throw new InvalidOperationException("CorePlayUI gameplay/viewport references are missing.");
            RectTransform viewport = ui.Find("FieldViewport") as RectTransform;
            if (viewport == null)
            {
                var go = new GameObject("FieldViewport", typeof(RectTransform)); go.layer = 5;
                go.transform.SetParent(ui, false); viewport = (RectTransform)go.transform;
            }
            viewport.anchorMin = Vector2.zero; viewport.anchorMax = Vector2.one;
            viewport.offsetMin = new Vector2(40, 0); viewport.offsetMax = new Vector2(-40, -600);
            gameplay.m_BgTrans = map;
            gameplay.m_FieldViewport = viewport; gameplay.m_FieldTrayRect = tray;
            gameplay.m_FieldItemPadding = new Vector2(110, 120); gameplay.m_FieldMaximumScale = 1; gameplay.m_FieldTrayGap = 40;
            var topController = instance.GetComponent<ApkEarningTopView>();
            topController.LevelSettingsRow = row; topController.GameplayViewport = viewport;
            PrefabUtility.RecordPrefabInstancePropertyModifications(topController);
            PrefabUtility.SaveAsPrefabAsset(core, CorePlayPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(core); }
    }

    private static Sprite LoadSprite(string name)
    {
        string path = "Assets/ApkEarningReference/Visuals/atlas/octobergarden/" + name + ".png";
        return LoadSpriteAt(path);
    }

    private static void BuildDiamondGuide()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(PanelRoot + "NewbieGuidePanel.prefab");
        try
        {
            root.name = "LocalDiamondUnlockGuide";
            var view = root.GetComponent<ApkEarningPageView>();
            foreach (string key in new[] { "Mask", "Teach", "ButtonFull", "Close", "Arraw" }) view.SetVisible(key, false);
            var header = AssetDatabase.LoadAssetAtPath<GameObject>(TopPath);
            var headerRect = (RectTransform)header.transform;
            GameObject sourceCard = header.GetComponent<ApkEarningPageView>().GetObject("CoinItem");
            var sourceRect = (RectTransform)sourceCard.transform;
            Vector2 center = sourceRect.anchoredPosition + headerRect.anchoredPosition + new Vector2(0, (.5f - headerRect.pivot.y) * headerRect.sizeDelta.y);
            Vector2 size = sourceRect.sizeDelta;
            RectTransform position = (RectTransform)view.GetObject("Pos").transform;
            // The source tutorial draws this node on an absolute order-1200
            // canvas. This game's modal host starts at order 2000, so retaining
            // that canvas puts its Button behind the host's input blocker.
            // The saved sibling order already places the card above the shade.
            var sourceRaycaster = position.GetComponent<GraphicRaycaster>();
            if (sourceRaycaster != null) Object.DestroyImmediate(sourceRaycaster);
            var sourceCanvas = position.GetComponent<Canvas>();
            if (sourceCanvas != null) Object.DestroyImmediate(sourceCanvas);
            float targetAnchorY = headerRect.anchorMin.y;
            position.anchorMin = position.anchorMax = new Vector2(.5f, targetAnchorY);
            position.anchoredPosition = center; position.sizeDelta = size;
            Button continueButton = view.GetButton("Button"); continueButton.gameObject.SetActive(true);
            var buttonRect = (RectTransform)continueButton.transform;
            buttonRect.anchorMin = Vector2.zero; buttonRect.anchorMax = Vector2.one;
            buttonRect.offsetMin = buttonRect.offsetMax = Vector2.zero;
            Image hitGraphic = continueButton.GetComponent<Image>();
            hitGraphic.color = Color.clear; hitGraphic.raycastTarget = true;
            GameObject card = Object.Instantiate(sourceCard, continueButton.transform, false); card.name = "DiamondCard";
            var cardRect = (RectTransform)card.transform;
            cardRect.anchorMin = Vector2.zero; cardRect.anchorMax = Vector2.one;
            cardRect.offsetMin = cardRect.offsetMax = Vector2.zero; card.SetActive(true);
            Transform hint = card.transform.Find("Hint"); if (hint != null) hint.gameObject.SetActive(false);
            foreach (Button nested in card.GetComponentsInChildren<Button>(true)) Object.DestroyImmediate(nested);
            foreach (Graphic graphic in card.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
            Image cardBackground = card.transform.Find("BG").GetComponent<Image>();
            cardBackground.raycastTarget = true;
            continueButton.targetGraphic = cardBackground;
            AppendButton(view, "continueButton", continueButton);
            AppendText(view, "diamondAmount", card.transform.Find("Value").GetComponent<TMP_Text>());
            TMP_Text body = view.GetText("TipValue");
            var tip = (RectTransform)body.transform.parent;
            tip.SetParent(view.ArtworkRoot, false); tip.anchorMin = tip.anchorMax = new Vector2(.5f, .5f);
            tip.anchoredPosition = new Vector2(0, 450); tip.gameObject.SetActive(true);
            body.text = "Diamond rewards are unlocked. Tap here to review withdrawal requirements.";
            AppendText(view, "title", body); AppendText(view, "body", body);
            var hand = view.GetObject("Hand"); hand.SetActive(true); hand.transform.SetAsLastSibling();
            foreach (Graphic graphic in hand.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
            foreach (Graphic graphic in tip.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
            float left = center.x - size.x * .5f, right = center.x + size.x * .5f;
            float top = center.y + size.y * .5f, bottom = center.y - size.y * .5f;
            GuideShade(view.ArtworkRoot, "ShadeAbove", new Vector2(0, targetAnchorY), Vector2.one, new Vector2(0, top), Vector2.zero);
            GuideShade(view.ArtworkRoot, "ShadeBelow", Vector2.zero, new Vector2(1, targetAnchorY), Vector2.zero, new Vector2(0, bottom));
            GuideShade(view.ArtworkRoot, "ShadeLeft", new Vector2(0, targetAnchorY), new Vector2(.5f, targetAnchorY), new Vector2(0, bottom), new Vector2(left, top));
            GuideShade(view.ArtworkRoot, "ShadeRight", new Vector2(.5f, targetAnchorY), new Vector2(1, targetAnchorY), new Vector2(right, bottom), new Vector2(0, top));
            position.SetAsLastSibling(); tip.SetAsLastSibling();
            PrefabUtility.SaveAsPrefabAsset(root, PanelRoot + "LocalDiamondUnlockGuide.prefab");
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static void GuideShade(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); go.layer = 5; go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform; rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.offsetMin = offsetMin; rect.offsetMax = offsetMax;
        var image = go.GetComponent<Image>(); image.color = new Color(0, 0, 0, .7647059f); image.raycastTarget = true;
    }

    private static void BuildRecoveryPage()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(PanelRoot + "LocalWithdrawalConditions.prefab");
        try
        {
            root.name = "LocalEarningRecovery";
            var view = root.GetComponent<ApkEarningPageView>();
            for (int i = 0; i < 7; i++) view.SetVisible("rows[" + i + "].Root", false);
            TMP_Text notice = view.GetText("notice"); if (notice != null) notice.gameObject.SetActive(false);
            Button originalClose = view.GetButton("close"); if (originalClose != null) originalClose.gameObject.SetActive(false);
            TMP_Text body = view.GetText("subtitle");
            body.rectTransform.anchoredPosition = new Vector2(0, 160); body.rectTransform.sizeDelta = new Vector2(800, 300);
            body.fontSize = 38; body.enableWordWrapping = true;
            body.text = "Your rewards could not be saved. Retry to continue, or return to the game.";
            AppendText(view, "body", body); view.SetText("title", "Save not completed");
            Button retry = view.GetButton("action");
            ((RectTransform)retry.transform).anchoredPosition = new Vector2(0, -50);
            view.SetText("actionCaption", "Retry"); AppendButton(view, "retry", retry);
            GameObject returnObject = Object.Instantiate(retry.gameObject, retry.transform.parent, false); returnObject.name = "ReturnToGame";
            ((RectTransform)returnObject.transform).anchoredPosition = new Vector2(0, -220);
            foreach (TMP_Text text in returnObject.GetComponentsInChildren<TMP_Text>(true)) text.text = "Return to game";
            Button close = returnObject.GetComponent<Button>();
            AppendButton(view, "close", close);
            PrefabUtility.SaveAsPrefabAsset(root, PanelRoot + "LocalEarningRecovery.prefab");
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
    private static Sprite LoadSpriteAt(string path)
    {
        foreach (Object item in AssetDatabase.LoadAllAssetsAtPath(path)) if (item is Sprite sprite) return sprite;
        throw new InvalidOperationException("Missing source sprite: " + path);
    }

    private static void AppendText(ApkEarningPageView view, string key, TMP_Text value)
    {
        if (value == null) return;
        var slots = new List<ApkEarningPageView.TextSlot>(view.Texts);
        slots.RemoveAll(slot => slot.Key == key); slots.Add(new ApkEarningPageView.TextSlot { Key = key, Target = value }); view.Texts = slots.ToArray();
    }
    private static void AppendButton(ApkEarningPageView view, string key, Button value)
    {
        var slots = new List<ApkEarningPageView.ButtonSlot>(view.Buttons);
        slots.RemoveAll(slot => slot.Key == key); slots.Add(new ApkEarningPageView.ButtonSlot { Key = key, Target = value }); view.Buttons = slots.ToArray();
    }
    private static void AppendImage(ApkEarningPageView view, string key, Image value)
    {
        if (value == null) return;
        var slots = new List<ApkEarningPageView.ImageSlot>(view.Images);
        slots.RemoveAll(slot => slot.Key == key); slots.Add(new ApkEarningPageView.ImageSlot { Key = key, Target = value }); view.Images = slots.ToArray();
    }
}
