using System;
using System.Collections.Generic;
using System.IO;
using FruitApkEarning;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Isolated saved-prefab progress fixtures, never a live-game or device capture.</summary>
public static class RoundedEarningProgressValidation
{
    private const string Panels = "Assets/ApkEarningReference/Resources/ApkEarningReference/Panels/";
    private const string Output = "C:/Projects/DJS_Apple/progress-bar-fix-20261019/after";
    private static readonly float[] Values = { 0f, .00204f, .01f, .1f, .6f, 1f };

    private sealed class Spec
    {
        public string Name, Path, Key;
        public bool SiblingTrack;
        public Spec(string name, string path, string key, bool siblingTrack = false)
        { Name = name; Path = path; Key = key; SiblingTrack = siblingTrack; }
    }

    [Serializable] private sealed class Report
    {
        public string generatedUtc, scope, activeSceneBefore, activeSceneAfter, walletPreview;
        public List<string> errors = new List<string>();
        public List<BarReport> bars = new List<BarReport>();
    }

    [Serializable] private sealed class BarReport
    {
        public string name, prefab, key, trackSprite, fillSprite, image;
        public float fullWidth, fullHeight, fullPixelsPerUnit;
        public bool hiddenInGamePrefab;
        public List<StateReport> states = new List<StateReport>();
        public List<string> errors = new List<string>();
    }

    [Serializable] private sealed class StateReport
    {
        public string sequence;
        public float requested, clamped, width, height, expectedWidth, expectedHeight;
        public float left, centerY, pixelsPerUnitMultiplier, leftCapWidth, rightCapWidth;
        public bool enabled, sliced;
    }

    private sealed class Preview : IDisposable
    {
        public readonly Scene Scene;
        public readonly GameObject CanvasObject;
        public readonly Canvas Canvas;
        public readonly Camera Camera;
        private readonly RenderTexture texture;
        private readonly RenderTexture previous;
        private readonly int width, height;

        public Preview(int width, int height, float scale = 1)
        {
            this.width = width; this.height = height;
            previous = RenderTexture.active;
            Scene = EditorSceneManager.NewPreviewScene();
            var cameraObject = new GameObject("RoundedProgressFixtureCamera");
            SceneManager.MoveGameObjectToScene(cameraObject, Scene);
            Camera = cameraObject.AddComponent<Camera>(); Camera.enabled = false; Camera.scene = Scene;
            Camera.orthographic = true; Camera.orthographicSize = height * .5f;
            Camera.aspect = (float)width / height; Camera.transform.position = new Vector3(0, 0, -10);
            Camera.nearClipPlane = .01f; Camera.farClipPlane = 100;
            Camera.clearFlags = CameraClearFlags.SolidColor; Camera.backgroundColor = new Color32(245, 235, 211, 255);
            Camera.allowHDR = Camera.allowMSAA = false;
            texture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            texture.Create(); Camera.targetTexture = texture;
            CanvasObject = new GameObject("RoundedProgressFixtureCanvas", typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(CanvasObject, Scene); CanvasObject.SetActive(false);
            Canvas = CanvasObject.AddComponent<Canvas>(); Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            Canvas.worldCamera = Camera; Canvas.planeDistance = 1; Canvas.referencePixelsPerUnit = 100;
            Canvas.scaleFactor = scale;
            Canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 |
                AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;
        }

        public void Save(string path)
        {
            CanvasObject.SetActive(true); UnityEngine.Canvas.ForceUpdateCanvases();
            foreach (TMP_Text text in CanvasObject.GetComponentsInChildren<TMP_Text>(false)) text.ForceMeshUpdate(true, true);
            UnityEngine.Canvas.ForceUpdateCanvases(); Camera.Render();
            RenderTexture.active = texture;
            var pixels = new Texture2D(width, height, TextureFormat.RGBA32, false);
            try
            {
                pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0); pixels.Apply(false, false);
                File.WriteAllBytes(path, pixels.EncodeToPNG());
            }
            finally { Object.DestroyImmediate(pixels); RenderTexture.active = previous; }
        }

        public void Dispose()
        {
            RenderTexture.active = previous; Camera.targetTexture = null;
            Object.DestroyImmediate(CanvasObject); Object.DestroyImmediate(Camera.gameObject);
            texture.Release(); Object.DestroyImmediate(texture); EditorSceneManager.ClosePreviewScene(Scene);
        }
    }

    [MenuItem("Tools/Apk Earning/Validate Rounded Progress Bars")]
    public static void Render()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before rendering isolated saved-prefab fixtures.");
        Directory.CreateDirectory(Output);
        var report = new Report {
            generatedUtc = DateTime.UtcNow.ToString("O"), activeSceneBefore = SceneManager.GetActiveScene().path,
            scope = "ISOLATED EDITOR FIXTURES, NOT LIVE GAME OR APK CAPTURES. Saved prefab tracks and fills are rendered by Unity at their authored dimensions on a Canvas with referencePixelsPerUnit=100. All game controllers and animations are disabled or removed before activation; no host, service, save or payment flow runs. Only fixture labels are generated. Progress values are synthetic and drive the production ApkEarningPageView.SetProgress method. The normally hidden compact HUD task is exposed only in its isolated fixture; no prefab or game visibility is changed."
        };
        var specs = new[] {
            new Spec("cash-wallet", Panels + "TXPanel.prefab", "Progress"),
            new Spec("diamond-wallet", Panels + "TXCoinPanel.prefab", "TipBar"),
            new Spec("request-loading", Panels + "TXGuideLoadingPanel.prefab", "Progress"),
            new Spec("record-guide", Panels + "TXRecordGuidePanel.prefab", "main/HideLevel/Progress/Bar"),
            new Spec("hidden-compact-hud", "Assets/res/local/withdrawal/ApkEarningTop.prefab", "CompactTaskProgress", true)
        };
        foreach (Spec spec in specs) RenderBar(spec, report);
        try { report.walletPreview = RenderCashWallet(); }
        catch (Exception exception) { report.errors.Add("Cash wallet preview: " + exception); }
        report.activeSceneAfter = SceneManager.GetActiveScene().path;
        if (report.activeSceneBefore != report.activeSceneAfter) report.errors.Add("Active scene changed during fixture validation.");
        string reportPath = Path.Combine(Output, "rounded-progress-report.json");
        File.WriteAllText(reportPath, JsonUtility.ToJson(report, true));
        if (report.errors.Count > 0)
            throw new InvalidOperationException("Rounded progress validation found " + report.errors.Count + " errors: " + reportPath);
        Debug.Log("[RoundedEarningProgressValidation] Passed 5 saved-prefab bars, 30 visible fixture states plus reverse/clamp recovery checks. " + reportPath);
    }

    private static void RenderBar(Spec spec, Report report)
    {
        var item = new BarReport { name = spec.Name, prefab = spec.Path, key = spec.Key };
        report.bars.Add(item);
        try
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(spec.Path);
            if (source == null) throw new InvalidOperationException("Missing prefab " + spec.Path);
            ApkEarningPageView sourcePage = source.GetComponent<ApkEarningPageView>();
            ApkEarningPageView.ImageSlot sourceSlot = FindSlot(sourcePage, spec.Key);
            Image originalFill = sourceSlot.Target;
            if (originalFill == null) throw new InvalidOperationException("Missing progress fill " + spec.Key);
            var sourceRoot = originalFill.transform.parent as RectTransform;
            Image originalTrack = spec.SiblingTrack ? sourceRoot.Find("ProgressTrack").GetComponent<Image>() : sourceRoot.GetComponent<Image>();
            if (originalTrack == null) throw new InvalidOperationException("Missing track for " + spec.Key);
            item.trackSprite = AssetDatabase.GetAssetPath(originalTrack.sprite);
            item.fillSprite = AssetDatabase.GetAssetPath(originalFill.sprite);
            item.fullWidth = sourceSlot.FullProgressWidth; item.fullHeight = sourceSlot.FullProgressHeight;
            item.fullPixelsPerUnit = sourceSlot.FullProgressPixelsPerUnit;
            item.hiddenInGamePrefab = !AllAncestorsActive(originalFill.transform);
            if (!sourceSlot.ResizeWidthWithProgress) item.errors.Add("Saved binding does not resize width with progress.");
            if (item.fullWidth <= 0 || item.fullHeight <= 0 || item.fullPixelsPerUnit <= 0)
                item.errors.Add("Saved rounded progress dimensions/PPU must be positive.");
            if (originalTrack.type != Image.Type.Sliced) item.errors.Add("Track is not Sliced.");
            if (originalFill.type != Image.Type.Sliced) item.errors.Add("Fill is not Sliced.");
            string fillPath = RelativePath(sourceRoot, originalFill.transform);
            string trackPath = RelativePath(sourceRoot, originalTrack.transform);
            TMP_Text sourceText = source.GetComponentInChildren<TMP_Text>(true);
            using (var preview = new Preview(1040, 930))
            {
                AddLabel(preview.Canvas.transform, sourceText.font, spec.Name, 0, 415, 990, 48, 31);
                AddLabel(preview.Canvas.transform, sourceText.font, "SAVED PREFAB FIXTURE - NOT LIVE GAME / APK", 0, 369, 990, 36, 20);
                for (int i = 0; i < Values.Length; i++)
                {
                    float y = 265 - i * 120;
                    GameObject instance = Object.Instantiate(sourceRoot.gameObject, preview.Canvas.transform, false);
                    instance.name = "Fixture_" + spec.Name + "_" + i;
                    StripControllers(instance);
                    var rect = (RectTransform)instance.transform;
                    rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
                    rect.localScale = Vector3.one; rect.localRotation = Quaternion.identity;
                    Image fill = At<Image>(rect, fillPath); Image track = At<Image>(rect, trackPath);
                    foreach (Graphic graphic in instance.GetComponentsInChildren<Graphic>(true))
                        graphic.enabled = graphic == fill || graphic == track;
                    ActivateAncestors(fill.transform, instance.transform); ActivateAncestors(track.transform, instance.transform);
                    rect.anchoredPosition = new Vector2(0, y);
                    // Recenter the actual track/fill pair without changing their authored relative geometry.
                    Vector3 trackCenter = rect.InverseTransformPoint(track.rectTransform.TransformPoint(track.rectTransform.rect.center));
                    rect.anchoredPosition -= new Vector2(trackCenter.x, trackCenter.y);
                    var page = instance.AddComponent<ApkEarningPageView>();
                    page.Images = new[] { CopySlot(sourceSlot, fill) };
                    page.SetProgress(spec.Key, 1);
                    StateReport baseline = Inspect(page, spec.Key, 1, "baseline", sourceSlot);
                    page.SetProgress(spec.Key, Values[i]);
                    StateReport state = Inspect(page, spec.Key, Values[i], "forward", sourceSlot);
                    item.states.Add(state); Validate(state, baseline, sourceSlot, item.errors);
                    AddLabel(preview.Canvas.transform, sourceText.font, Values[i].ToString("0.###%", System.Globalization.CultureInfo.InvariantCulture),
                        0, y + 54, 990, 34, 22);
                    if (i == Values.Length - 1)
                    {
                        foreach (float value in new[] { .6f, .1f, .01f, .00204f, 0f, 1f, -1f, 2f, .6f })
                        {
                            page.SetProgress(spec.Key, value);
                            StateReport reverse = Inspect(page, spec.Key, value, "reverse-and-clamp", sourceSlot);
                            item.states.Add(reverse); Validate(reverse, baseline, sourceSlot, item.errors);
                        }
                        page.SetProgress(spec.Key, 1);
                    }
                }
                item.image = Path.Combine(Output, "fixture-progress-" + spec.Name + ".png");
                preview.Save(item.image);
            }
        }
        catch (Exception exception) { item.errors.Add(exception.ToString()); }
        foreach (string error in item.errors) report.errors.Add(spec.Name + ": " + error);
    }

    private static StateReport Inspect(ApkEarningPageView page, string key, float requested, string sequence, ApkEarningPageView.ImageSlot authored)
    {
        Image fill = page.GetImage(key); RectTransform rect = fill.rectTransform;
        float ppu = fill.pixelsPerUnit * fill.pixelsPerUnitMultiplier;
        Vector3 center = rect.TransformPoint(rect.rect.center);
        Vector3 left = rect.TransformPoint(new Vector3(rect.rect.xMin, rect.rect.center.y));
        float progress = Mathf.Clamp01(requested), width = authored.FullProgressWidth * progress;
        return new StateReport {
            sequence = sequence, requested = requested, clamped = progress, width = rect.rect.width, height = rect.rect.height,
            expectedWidth = width, expectedHeight = progress > 0 ? Mathf.Min(authored.FullProgressHeight, width) : authored.FullProgressHeight,
            left = left.x, centerY = center.y, pixelsPerUnitMultiplier = fill.pixelsPerUnitMultiplier,
            enabled = fill.enabled, sliced = fill.type == Image.Type.Sliced,
            leftCapWidth = fill.sprite == null || ppu <= 0 ? 0 : fill.sprite.border.x / ppu,
            rightCapWidth = fill.sprite == null || ppu <= 0 ? 0 : fill.sprite.border.z / ppu
        };
    }

    private static void Validate(StateReport state, StateReport baseline, ApkEarningPageView.ImageSlot authored, List<string> errors)
    {
        string label = state.sequence + " " + state.requested + ": ";
        if (Mathf.Abs(state.width - state.expectedWidth) > .01f) errors.Add(label + "Fill width does not equal fullWidth * clampedProgress.");
        if (authored.FullProgressHeight > 0 && Mathf.Abs(state.height - state.expectedHeight) > .01f)
            errors.Add(label + "Visible height must equal min(fullHeight, width); hidden zero must restore fullHeight.");
        if (state.enabled && authored.FullProgressHeight > 0 && state.height > state.width + .01f) errors.Add(label + "Fill is taller than its width.");
        if (state.enabled != (state.clamped > 0)) errors.Add(label + "Zero progress must hide the fill; positive progress must show it.");
        if (!state.sliced) errors.Add(label + "Fill is not Sliced.");
        if (float.IsNaN(state.pixelsPerUnitMultiplier) || float.IsInfinity(state.pixelsPerUnitMultiplier) || state.pixelsPerUnitMultiplier <= 0)
            errors.Add(label + "Invalid pixelsPerUnitMultiplier.");
        if (Mathf.Abs(state.left - baseline.left) > .01f) errors.Add(label + "Left edge moved.");
        if (Mathf.Abs(state.centerY - baseline.centerY) > .01f) errors.Add(label + "Vertical center moved.");
        if (state.height > 0 && authored.FullProgressHeight > 0)
        {
            float expectedPPU = authored.FullProgressPixelsPerUnit * authored.FullProgressHeight / state.height;
            if (Mathf.Abs(state.pixelsPerUnitMultiplier - expectedPPU) > Mathf.Max(.01f, expectedPPU * .001f))
                errors.Add(label + "End-cap PPU does not scale proportionally with fill height.");
            float ratio = state.height / authored.FullProgressHeight;
            if (Mathf.Abs(state.leftCapWidth - baseline.leftCapWidth * ratio) > .01f ||
                Mathf.Abs(state.rightCapWidth - baseline.rightCapWidth * ratio) > .01f)
                errors.Add(label + "Left/right end caps changed scale relative to fill height.");
        }
    }

    /// <summary>A complete cash-wallet prefab preview, using synthetic 3-of-5 fixture data.</summary>
    public static string RenderCashWallet()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before rendering isolated saved-prefab fixtures.");
        Directory.CreateDirectory(Output);
        using (var preview = new Preview(1080, 1920))
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(Panels + "TXPanel.prefab");
            GameObject instance = Object.Instantiate(source, preview.Canvas.transform, false);
            StripControllers(instance, true);
            ApkEarningPageView page = instance.GetComponent<ApkEarningPageView>();
            page.SetText("GoldCount", "$3.15"); page.SetText("Title", "CASH WALLET");
            page.SetText("StageLabel", "Level 4"); page.SetText("labels[0].Text", "Available balance");
            page.SetText("Tip", "Clear level 5\nto review withdrawal"); page.SetText("TXText", "Withdraw");
            page.SetText("ProgressValue", "3 / 5"); page.SetText("ProgressTipValue", "2"); page.SetProgress("Progress", .6f);
            page.SetText("Stats.PassCount", "Today: 3"); page.SetText("Stats.ChallengeTimes", "Total runs: 12");
            page.SetText("Stats.GoldGetCount", "$3.15"); page.SetVisible("Level1", false); page.SetVisible("Level2", false);
            preview.CanvasObject.SetActive(true); instance.SetActive(true); Canvas.ForceUpdateCanvases(); page.RefreshLayout();
            TMP_Text sourceText = source.GetComponentInChildren<TMP_Text>(true);
            AddLabel(preview.Canvas.transform, sourceText.font, "SAVED PREFAB FIXTURE - NOT LIVE GAME / APK", 0, 925, 1040, 45, 25);
            string path = Path.Combine(Output, "fixture-cash-wallet-60-percent.png"); preview.Save(path); return path;
        }
    }

    private static ApkEarningPageView.ImageSlot FindSlot(ApkEarningPageView page, string key)
    {
        if (page == null) throw new InvalidOperationException("Missing PageView.");
        foreach (ApkEarningPageView.ImageSlot slot in page.Images) if (slot.Key == key) return slot;
        throw new InvalidOperationException("Missing ImageSlot " + key);
    }

    private static ApkEarningPageView.ImageSlot CopySlot(ApkEarningPageView.ImageSlot source, Image target)
    {
        return new ApkEarningPageView.ImageSlot { Key = source.Key, Target = target,
            ResizeWidthWithProgress = source.ResizeWidthWithProgress, FullProgressWidth = source.FullProgressWidth,
            FullProgressHeight = source.FullProgressHeight, FullProgressPixelsPerUnit = source.FullProgressPixelsPerUnit };
    }

    private static void StripControllers(GameObject instance, bool preservePage = false)
    {
        foreach (MonoBehaviour component in instance.GetComponentsInChildren<MonoBehaviour>(true))
            if (component != null && !(component is UIBehaviour) && !(preservePage && component is ApkEarningPageView))
                Object.DestroyImmediate(component);
        foreach (Animator animator in instance.GetComponentsInChildren<Animator>(true)) animator.enabled = false;
        foreach (Animation animation in instance.GetComponentsInChildren<Animation>(true)) animation.enabled = false;
    }

    private static string RelativePath(Transform root, Transform target)
    {
        var parts = new List<string>();
        while (target != root) { parts.Add(target.name); target = target.parent; }
        parts.Reverse(); return string.Join("/", parts);
    }

    private static T At<T>(Transform root, string path) where T : Component
    { return (string.IsNullOrEmpty(path) ? root : root.Find(path)).GetComponent<T>(); }

    private static bool AllAncestorsActive(Transform target)
    {
        for (Transform node = target; node != null; node = node.parent) if (!node.gameObject.activeSelf) return false;
        return true;
    }

    private static void ActivateAncestors(Transform target, Transform root)
    {
        while (target != null) { target.gameObject.SetActive(true); if (target == root) break; target = target.parent; }
    }

    private static void AddLabel(Transform parent, TMP_FontAsset font, string value, float x, float y, float width, float height, float fontSize)
    {
        var go = new GameObject("FixtureLabel", typeof(RectTransform)); go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = new Vector2(x, y); rect.sizeDelta = new Vector2(width, height);
        var text = go.AddComponent<TextMeshProUGUI>(); text.font = font; text.text = value; text.fontSize = fontSize;
        text.color = new Color32(89, 49, 26, 255); text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = false; text.raycastTarget = false;
    }
}
