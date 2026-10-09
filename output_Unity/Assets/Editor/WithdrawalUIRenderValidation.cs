using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using FruitWithdrawal;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
/// Renders saved UI with labelled validation fixtures in disposable preview scenes. Does not construct a
/// WithdrawalHost, access WithdrawalService or modify prefabs. Button callbacks are replaced
/// with a local event recorder, so dispatch checks cannot reach game state or player storage.
/// </summary>
public static class WithdrawalUIRenderValidation
{
    private const string PanelFolder = "Assets/Resources/WithdrawalImported/Panels";
    private const string OutputFolder = "C:/Projects/DJS_Apple/withdrawal-fruit-ui-validation";
    private const string BackgroundPath = "Assets/res_server/server_maps/map1008/Map1008.png";
    private static readonly Vector2Int[] Sizes = { new Vector2Int(1080, 1920), new Vector2Int(1080, 2340), new Vector2Int(720, 1600) };
    private static readonly string[] RequiredPages = { "TXPanel", "TXCoinPanel", "TXChannelPanel", "TXUserInfoSurePanel",
        "TXProgress0Panel", "TXProgress1Panel", "TXProgress2Panel", "TXProgress3Panel", "TXRecordPanel", "RewardGetPanel", "RulesPanel" };

    [Serializable]
    private sealed class Report
    {
        public string generatedUtc;
        public string scope = "Synthetic presentation fixtures over original Map1008. A plain RectTransform receives the same SafeAreaSim.InsetRect call as BaseUI; saved modal shades receive the production ExpandRect call. Button dispatch is exercised only against a local recorder. No WithdrawalHost is instantiated, no service initialization or player storage access.";
        public string outputDirectory;
        public List<string> errors = new List<string>();
        public List<string> skippedUnboundPrefabs = new List<string>();
        public List<string> runtimePageResources = new List<string>();
        public string gameEntryPrefab;
        public string gameEntryButton;
        public List<PageReport> pages = new List<PageReport>();
    }

    [Serializable]
    private sealed class PageReport
    {
        public string prefab;
        public int width;
        public int height;
        public string image;
        public string fixture;
        public bool recordsBottom;
        public bool matchesReferenceAspect;
        public string backgroundAsset;
        public int verifiedButtonDispatches;
        public Vector3 actualContentScale;
        public float recordsNormalizedPosition;
        public Vector2 safeAreaTopBottom;
        public Vector2 safeAreaHostSize;
        public bool shadeCoversCanvas;
        public List<string> fixtureNotes = new List<string>();
        public Vector2 actualCanvasSize;
        public Vector2 configuredDesignSize;
        public int gameObjects;
        public int buttons;
        public int textComponents;
        public int images;
        public int distinctSampledColors;
        public List<string> missingComponents = new List<string>();
        public List<string> missingFonts = new List<string>();
        public List<string> missingTextures = new List<string>();
        public List<string> brokenObjectReferences = new List<string>();
        public List<string> invalidBindings = new List<string>();
        public List<string> plainImagesWithoutSprite = new List<string>();
        public List<string> disabledNonUiComponents = new List<string>();
        public List<string> renderErrors = new List<string>();
        public List<TextReport> texts = new List<TextReport>();
        public List<string> outsideCanvas = new List<string>();
        public List<string> textOverflows = new List<string>();
    }

    [Serializable]
    private sealed class TextReport
    {
        public string path;
        public bool active;
        public string text;
        public string fontAsset;
        public string material;
        public Vector2 rectSize;
        public float fontSize;
        public bool overflowing;
        public int visibleCharacters;
        public string missingCharacters;
        public bool scrollingContent;
    }

    [MenuItem("Tools/Withdrawal/Validate Static Page Renders")]
    public static void Run()
    {
        if (EditorApplication.isPlaying)
            throw new InvalidOperationException("Stop Play Mode before running the isolated static UI renderer.");

        Directory.CreateDirectory(OutputFolder);
        var report = new Report
        {
            generatedUtc = DateTime.UtcNow.ToString("O"),
            outputDirectory = OutputFolder
        };
        AuditHostResources(report);
        string[] ids = AssetDatabase.FindAssets("t:Prefab", new[] { PanelFolder });
        var paths = new List<string>();
        for (int i = 0; i < ids.Length; i++) paths.Add(AssetDatabase.GUIDToAssetPath(ids[i]));
        paths.Sort(StringComparer.Ordinal);
        var foundPages = new HashSet<string>(StringComparer.Ordinal);
        int boundPages = 0;
        foreach (string path in paths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                report.errors.Add("Cannot load prefab: " + path);
                continue;
            }
            WithdrawalPageView sourceView = prefab.GetComponent<WithdrawalPageView>();
            if (sourceView == null)
            {
                report.skippedUnboundPrefabs.Add(path);
                continue;
            }
            boundPages++;
            foundPages.Add(Path.GetFileNameWithoutExtension(path));
            var requestedSizes = new List<Vector2Int>(Sizes);
            var referenceSize = new Vector2Int(1080, Mathf.RoundToInt(1080 * sourceView.DesignSize.y / sourceView.DesignSize.x));
            if (referenceSize.y > 0 && !requestedSizes.Contains(referenceSize)) requestedSizes.Add(referenceSize);
            foreach (Vector2Int size in requestedSizes)
            {
                var page = new PageReport { prefab = path, width = size.x, height = size.y, matchesReferenceAspect = size == referenceSize };
                report.pages.Add(page);
                try { Render(prefab, page); }
                catch (Exception exception) { page.renderErrors.Add(exception.ToString()); }
                if (prefab.name == "TXRecordPanel")
                {
                    var bottom = new PageReport { prefab = path, width = size.x, height = size.y, recordsBottom = true, matchesReferenceAspect = size == referenceSize };
                    report.pages.Add(bottom);
                    try { Render(prefab, bottom); }
                    catch (Exception exception) { bottom.renderErrors.Add(exception.ToString()); }
                }
            }
        }
        if (boundPages == 0) report.errors.Add("No bound WithdrawalPageView prefabs found. Run authoring first.");
        foreach (string required in RequiredPages)
            if (!foundPages.Contains(required)) report.errors.Add("Bound " + required + ".prefab is missing.");
        foreach (PageReport page in report.pages)
        {
            int failures = page.missingComponents.Count + page.missingFonts.Count + page.missingTextures.Count
                + page.brokenObjectReferences.Count + page.invalidBindings.Count + page.renderErrors.Count + page.textOverflows.Count;
            if (failures > 0) report.errors.Add(Path.GetFileNameWithoutExtension(page.prefab) + " " + page.width + "x" + page.height + ": " + failures + " structural/render issues.");
        }
        string reportPath = Path.Combine(OutputFolder, "withdrawal-ui-report.json");
        File.WriteAllText(reportPath, JsonUtility.ToJson(report, true));
        Debug.Log("[WithdrawalUIRenderValidation] " + report.pages.Count + " page renders; report: " + reportPath);
        if (report.errors.Count != 0)
            throw new InvalidOperationException("Static withdrawal UI validation found " + report.errors.Count + " failing page/global checks. See " + reportPath);
    }

    private static void AuditHostResources(Report report)
    {
        const string hostPath = "Assets/res/local/withdrawal/WithdrawalHost.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(hostPath);
        WithdrawalHost host = prefab == null ? null : prefab.GetComponent<WithdrawalHost>();
        if (host == null || host.PageRoot == null || host.Screens == null)
        {
            report.errors.Add("Saved WithdrawalHost is missing its component, page root or screen definitions.");
            return;
        }
        var screens = new HashSet<WithdrawalScreen>();
        var pageNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (WithdrawalScreenDefinition definition in host.Screens)
        {
            if (definition == null || string.IsNullOrEmpty(definition.ResourcePath))
            {
                report.errors.Add("WithdrawalHost has an empty screen resource definition.");
                continue;
            }
            if (!screens.Add(definition.Screen)) report.errors.Add("WithdrawalHost has a duplicate screen definition.");
            string path = "Assets/Resources/" + definition.ResourcePath + ".prefab";
            int screenIndex = (int)definition.Screen;
            if (screenIndex < 0 || screenIndex >= RequiredPages.Length
                || Path.GetFileNameWithoutExtension(path) != RequiredPages[screenIndex])
                report.errors.Add("WithdrawalHost screen-to-page association is incorrect: " + definition.ResourcePath);
            GameObject page = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (page == null || page.GetComponent<WithdrawalPageView>() == null)
                report.errors.Add("WithdrawalHost cannot load a bound page at " + path);
            else
            {
                pageNames.Add(Path.GetFileNameWithoutExtension(path));
                report.runtimePageResources.Add(definition.ResourcePath);
            }
        }
        foreach (string required in RequiredPages)
            if (!pageNames.Contains(required)) report.errors.Add("WithdrawalHost does not reference " + required + ".");

        const string corePath = "Assets/res/local/coreplay/CorePlayUI.prefab";
        GameObject core = AssetDatabase.LoadAssetAtPath<GameObject>(corePath);
        var catalog = AssetDatabase.LoadAssetAtPath<GameResCatalog>("Assets/Resources/GameResCatalog.asset");
        if (catalog == null || catalog.entries == null)
            report.errors.Add("Runtime GameResCatalog is missing.");
        else
        {
            bool hostMapped = false, coreMapped = false;
            foreach (GameResCatalog.Entry entry in catalog.entries)
            {
                if (entry == null) continue;
                if (entry.key == "res/local/withdrawal/withdrawalhost" && entry.asset == prefab) hostMapped = true;
                if (entry.key == "res/local/coreplay/coreplayui" && entry.asset == core) coreMapped = true;
            }
            if (!hostMapped) report.errors.Add("Runtime resource catalog does not map the withdrawal host entry to this saved host.");
            if (!coreMapped) report.errors.Add("Runtime resource catalog does not map CorePlayUI to this saved gameplay prefab.");
        }
        WithdrawalHud[] huds = core == null ? Array.Empty<WithdrawalHud>() : core.GetComponentsInChildren<WithdrawalHud>(true);
        if (huds.Length != 1) report.errors.Add("CorePlayUI must contain one configured WithdrawalHud entry.");
        else
        {
            WithdrawalHud hud = huds[0];
            if (hud.CashButton == null || hud.DiamondButton == null || hud.RewardsButton == null
                || hud.Cash == null || hud.Diamonds == null || hud.Rewards == null)
                report.errors.Add("CorePlayUI withdrawal entry has incomplete serialized bindings.");
            else
            {
                bool active = hud.enabled && hud.CashButton.enabled && hud.CashButton.interactable;
                for (Transform current = hud.CashButton.transform; current != null; current = current.parent)
                    active &= current.gameObject.activeSelf;
                if (!active) report.errors.Add("CorePlayUI withdrawal cash entry is disabled or inactive.");
                if (hud.CashButton.targetGraphic == null) report.errors.Add("CorePlayUI withdrawal cash entry has no Button visual.");
                if (hud.CashButton.onClick.GetPersistentEventCount() != 0)
                    report.errors.Add("CorePlayUI withdrawal entry has a serialized event instead of code binding.");
                report.gameEntryPrefab = corePath;
                report.gameEntryButton = Hierarchy(hud.CashButton.transform, core.transform);
            }
        }
    }

    private static void Render(GameObject prefab, PageReport report)
    {
        Scene scene = EditorSceneManager.NewPreviewScene();
        GameObject cameraObject = null;
        GameObject canvasObject = null;
        RenderTexture target = null;
        Texture2D output = null;
        RenderTexture previousActive = RenderTexture.active;
        Application.LogCallback log = (message, trace, type) =>
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                report.renderErrors.Add(message);
        };
        Application.logMessageReceived += log;
        try
        {
            cameraObject = new GameObject("WithdrawalStaticRenderCamera");
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.scene = scene;
            camera.enabled = false;
            camera.orthographic = true;
            camera.orthographicSize = report.height * .5f;
            camera.transform.position = new Vector3(0, 0, -10);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.12f, .18f, .25f, 1);
            camera.nearClipPlane = .01f;
            camera.farClipPlane = 100;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            target = new RenderTexture(report.width, report.height, 24, RenderTextureFormat.ARGB32);
            target.antiAliasing = 1;
            target.Create();
            camera.targetTexture = target;
            camera.aspect = (float)report.width / report.height;

            canvasObject = new GameObject("WithdrawalStaticRenderCanvas", typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(canvasObject, scene);
            canvasObject.SetActive(false);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1;
            // A fixed scale exposes the requested physical dimensions to production Fit(),
            // independently of the batch editor's unrelated Screen size.
            canvas.scaleFactor = 1;
            canvas.referencePixelsPerUnit = 100;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1
                | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;

            AddBackground(canvasObject.transform, report);

            var safeHostObject = new GameObject("BaseUISafeAreaRectFixture", typeof(RectTransform));
            var safeHost = (RectTransform)safeHostObject.transform;
            safeHost.SetParent(canvasObject.transform, false);
            safeHost.anchorMin = Vector2.zero;
            safeHost.anchorMax = Vector2.one;
            safeHost.offsetMin = safeHost.offsetMax = Vector2.zero;
            Orange.SafeAreaSim.InsetRect(safeHost);
            report.safeAreaTopBottom = new Vector2(Orange.SafeAreaSim.Top, Orange.SafeAreaSim.Bottom);

            GameObject instance = Object.Instantiate(prefab, safeHost, false);
            instance.name = prefab.name;
            var page = instance.GetComponent<WithdrawalPageView>();
            report.configuredDesignSize = page.DesignSize;
            foreach (MonoBehaviour behaviour in instance.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour == null || behaviour is UIBehaviour || behaviour is WithdrawalPageView) continue;
                behaviour.enabled = false;
                report.disabledNonUiComponents.Add(Hierarchy(behaviour.transform, instance.transform));
            }
            Transform shadeTransform = instance.transform.Find("ModalShade");
            RectTransform shade = shadeTransform as RectTransform;
            var shadeImage = shadeTransform == null ? null : shadeTransform.GetComponent<Image>();
            var reverse = shadeTransform == null ? null : shadeTransform.GetComponent<Orange.ProtectedAreaAdaptReverse>();
            if (shade == null || shadeImage == null || reverse == null || !shadeImage.raycastTarget)
                report.invalidBindings.Add("ModalShade requires a raycast-blocking Image and ProtectedAreaAdaptReverse.");
            else
            {
                // The saved component is disabled above to prevent Start from applying twice.
                // This is exactly the public layout operation called by its runtime Start.
                Orange.SafeAreaSim.ExpandRect(shade, false);
            }
            AuditComponents(instance, report);
            AuditBindings(page, report);
            ApplyFixture(page, report, prefab.name);
            canvasObject.SetActive(true);
            instance.SetActive(true);
            Canvas.ForceUpdateCanvases();
            AuditButtonDispatch(page, report);
            Canvas.ForceUpdateCanvases();
            AuditFit(page, report);
            AuditSafeArea(page, safeHost, shade, (RectTransform)canvas.transform, report);
            if (prefab.name == "TXRecordPanel")
            {
                ScrollRect scroll = instance.GetComponentInChildren<ScrollRect>(true);
                if (scroll != null)
                {
                    scroll.StopMovement();
                    scroll.verticalNormalizedPosition = report.recordsBottom ? 0 : 1;
                    Canvas.ForceUpdateCanvases();
                    report.recordsNormalizedPosition = scroll.verticalNormalizedPosition;
                    if (scroll.viewport == null || scroll.content == null)
                        report.invalidBindings.Add("Records ScrollRect is missing its viewport or content.");
                    else
                    {
                        if (scroll.content.rect.height <= scroll.viewport.rect.height)
                            report.invalidBindings.Add("Eight record cards do not create a scrollable content area.");
                        if (scroll.viewport.GetComponent<RectMask2D>() == null && scroll.viewport.GetComponent<Mask>() == null)
                            report.invalidBindings.Add("Records ScrollRect viewport has no clipping mask.");
                        if (Mathf.Abs(report.recordsNormalizedPosition - (report.recordsBottom ? 0 : 1)) > .01f)
                            report.invalidBindings.Add("Records ScrollRect did not reach its requested endpoint.");
                    }
                    int recordButtons = 0;
                    for (int i = 0; i < page.Buttons.Length; i++)
                        if (page.Buttons[i] != null && page.Buttons[i].Action == WithdrawalAction.SelectRecord)
                            recordButtons++;
                    if (recordButtons != 8) report.invalidBindings.Add("Records page must bind all eight record cards.");
                }
                else report.invalidBindings.Add("Records fixture cannot find its saved ScrollRect.");
            }
            foreach (TMP_Text text in instance.GetComponentsInChildren<TMP_Text>(true))
                if (text.gameObject.activeInHierarchy) text.ForceMeshUpdate(true, true);
            Canvas.ForceUpdateCanvases();
            camera.Render();
            report.actualCanvasSize = ((RectTransform)canvas.transform).rect.size;
            if (Mathf.Abs(report.actualCanvasSize.x - report.width) > 1 || Mathf.Abs(report.actualCanvasSize.y - report.height) > 1)
                report.renderErrors.Add("Canvas dimensions differ from requested render dimensions: " + report.actualCanvasSize);
            AuditTextAndBounds(instance, canvas, camera, report);

            RenderTexture.active = target;
            output = new Texture2D(report.width, report.height, TextureFormat.RGBA32, false);
            output.ReadPixels(new Rect(0, 0, report.width, report.height), 0, 0);
            output.Apply(false, false);
            Color32[] pixels = output.GetPixels32();
            var colors = new HashSet<uint>();
            for (int y = 0; y < report.height; y += 17)
                for (int x = 0; x < report.width; x += 17)
                {
                    Color32 color = pixels[y * report.width + x];
                    colors.Add((uint)color.r | (uint)color.g << 8 | (uint)color.b << 16);
                }
            report.distinctSampledColors = colors.Count;
            if (colors.Count < 8) report.renderErrors.Add("Render appears blank or contains too few distinct colors.");
            report.image = Path.Combine(OutputFolder, prefab.name + "-" + report.width + "x" + report.height + (report.recordsBottom ? "-bottom" : "") + ".png");
            File.WriteAllBytes(report.image, output.EncodeToPNG());
        }
        finally
        {
            Application.logMessageReceived -= log;
            RenderTexture.active = previousActive;
            if (cameraObject != null) cameraObject.GetComponent<Camera>().targetTexture = null;
            if (canvasObject != null) Object.DestroyImmediate(canvasObject);
            if (cameraObject != null) Object.DestroyImmediate(cameraObject);
            if (target != null) { target.Release(); Object.DestroyImmediate(target); }
            if (output != null) Object.DestroyImmediate(output);
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }

    private static void AuditSafeArea(WithdrawalPageView page, RectTransform safeHost, RectTransform shade, RectTransform canvas, PageReport report)
    {
        report.safeAreaHostSize = safeHost.rect.size;
        var corners = new Vector3[4];
        var canvasCorners = new Vector3[4];
        if (shade != null)
        {
            shade.GetWorldCorners(corners);
            canvas.GetWorldCorners(canvasCorners);
            report.shadeCoversCanvas = true;
            for (int i = 0; i < 4; i++)
                if (Vector3.Distance(corners[i], canvasCorners[i]) > .1f) report.shadeCoversCanvas = false;
            if (!report.shadeCoversCanvas) report.renderErrors.Add("ModalShade does not exactly cover the full canvas after native safe-area expansion.");
        }
        foreach (Graphic graphic in page.GetComponentsInChildren<Graphic>(false))
        {
            if (!graphic.enabled || graphic.color.a <= 0 || graphic.rectTransform == shade) continue;
            if (graphic is TMP_Text label && string.IsNullOrEmpty(label.text)) continue;
            if (graphic.GetComponentInParent<ScrollRect>() != null) continue;
            graphic.rectTransform.GetWorldCorners(corners);
            bool outside = false;
            for (int i = 0; i < 4; i++)
            {
                Vector3 local = safeHost.InverseTransformPoint(corners[i]);
                Rect area = safeHost.rect;
                if (local.x < area.xMin - .1f || local.x > area.xMax + .1f || local.y < area.yMin - .1f || local.y > area.yMax + .1f)
                    outside = true;
            }
            if (outside) report.renderErrors.Add(Hierarchy(graphic.transform, page.transform) + ": active content extends outside BaseUI's safe area.");
        }
        report.fixtureNotes.Add("Applied the production BaseUI safe-area inset to a plain RectTransform and production modal expansion to the disabled saved reverse adapter; no WithdrawalHost was constructed.");
    }

    private static void AddBackground(Transform parent, PageReport report)
    {
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(BackgroundPath);
        if (texture == null)
        {
            report.renderErrors.Add("The original Map1008 background could not be loaded.");
            return;
        }
        var background = new GameObject("OriginalMap1008ValidationBackdrop", typeof(RectTransform), typeof(RawImage));
        background.transform.SetParent(parent, false);
        var rect = (RectTransform)background.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        RawImage image = background.GetComponent<RawImage>();
        image.texture = texture;
        image.raycastTarget = false;
        float imageAspect = (float)texture.width / texture.height;
        float renderAspect = (float)report.width / report.height;
        float cropWidth = Mathf.Min(1, renderAspect / imageAspect);
        float cropHeight = Mathf.Min(1, imageAspect / renderAspect);
        image.uvRect = new Rect((1 - cropWidth) * .5f, (1 - cropHeight) * .5f, cropWidth, cropHeight);
        report.backgroundAsset = BackgroundPath;
        report.fixtureNotes.Add("Backdrop is the game's original Map1008 texture with centered aspect-fill cropping; no gameplay objects or game HUD are synthesized.");
    }

    private static void AuditFit(WithdrawalPageView page, PageReport report)
    {
        if (page.Content == null || page.DesignSize.x <= 0 || page.DesignSize.y <= 0) return;
        var root = (RectTransform)page.transform;
        if (page.UseCanvasFrame)
        {
            Canvas canvas = page.GetComponentInParent<Canvas>().rootCanvas;
            var canvasRect = (RectTransform)canvas.transform;
            float expectedScale = Mathf.Min(canvasRect.rect.width / page.DesignSize.x, canvasRect.rect.height / page.DesignSize.y, 1);
            bool bounded = page.ArtworkBounds.width > 0 && page.ArtworkBounds.height > 0;
            if (bounded) expectedScale = Mathf.Min(expectedScale, root.rect.width / page.ArtworkBounds.width, root.rect.height / page.ArtworkBounds.height);
            Vector3 origin = root.InverseTransformPoint(canvasRect.TransformPoint(canvasRect.rect.center));
            origin -= (Vector3)(page.DesignCenter * expectedScale);
            if (bounded)
            {
                origin.x = Mathf.Clamp(origin.x, root.rect.xMin - page.ArtworkBounds.xMin * expectedScale, root.rect.xMax - page.ArtworkBounds.xMax * expectedScale);
                origin.y = Mathf.Clamp(origin.y, root.rect.yMin - page.ArtworkBounds.yMin * expectedScale, root.rect.yMax - page.ArtworkBounds.yMax * expectedScale);
            }
            report.actualContentScale = page.Content.localScale;
            if (Vector3.Distance(page.Content.localScale, Vector3.one * expectedScale) > .001f)
                report.invalidBindings.Add("Content scale does not preserve the reference canvas and artwork safe bounds.");
            if (Vector3.Distance(root.InverseTransformPoint(page.Content.position), origin) > .1f)
                report.invalidBindings.Add("Content position differs from the centered reference frame with minimal safe-area correction.");
            Vector3 previousPosition = page.Content.position;
            page.RefreshLayout();
            if (Vector3.Distance(previousPosition, page.Content.position) > .1f)
                report.invalidBindings.Add("Refreshing the reference layout accumulates an unexpected position offset.");
            return;
        }
        float expected = Mathf.Min(root.rect.width / page.DesignSize.x, root.rect.height / page.DesignSize.y, 1);
        report.actualContentScale = page.Content.localScale;
        if (Vector3.Distance(page.Content.localScale, Vector3.one * expected) > .001f)
            report.invalidBindings.Add("Content scale does not match production Fit at this viewport size.");
        if (Vector2.Distance(page.Content.anchoredPosition, -page.DesignCenter * expected) > .1f)
            report.invalidBindings.Add("Content center does not match production Fit at this viewport size.");
    }

    private static void AuditButtonDispatch(WithdrawalPageView page, PageReport report)
    {
        if (page.Buttons == null) return;
        int dispatchCount = 0;
        WithdrawalAction lastAction = WithdrawalAction.Close;
        int lastIndex = -1;
        Action<WithdrawalAction, int> record = (action, index) =>
        {
            dispatchCount++;
            lastAction = action;
            lastIndex = index;
        };
        page.Bind(record);
        // Rebinding must replace the callback without installing duplicate listeners.
        page.Bind(record);
        foreach (WithdrawalButtonBinding binding in page.Buttons)
        {
            if (binding == null || binding.Button == null) continue;
            if (binding.Button.onClick.GetPersistentEventCount() != 0)
            {
                report.invalidBindings.Add(binding.Button.name + ": serialized Button event; not invoked by isolated validation.");
                continue;
            }
            int before = dispatchCount;
            binding.Button.onClick.Invoke();
            if (dispatchCount - before != 1 || lastAction != binding.Action || lastIndex != binding.Index)
                report.invalidBindings.Add(binding.Button.name + ": Button dispatch failed or invoked duplicate listeners.");
            else report.verifiedButtonDispatches++;
        }
        page.Bind(null);
        report.fixtureNotes.Add("Every saved Button was invoked against a local callback recorder after a repeat Bind; no host callback was installed.");
    }

    private static void ApplyFixture(WithdrawalPageView page, PageReport report, string prefabName)
    {
        // These strings intentionally reproduce the Host's display formats,
        // while their values are fixed synthetic data. No state engine or
        // service is created, and only this disposable instance is changed.
        report.fixtureNotes.Add("Synthetic validation content applied to an isolated instance; no actual application or payout.");
        page.SetText("test", "TEST MODE \u2022 NO CASH VALUE");
        page.SetText("notice", "");
        page.SetText("cash", "$500.00");
        page.SetText("diamonds", "50,600");
        page.SetText("frozen", "In application: $0.00");
        switch (prefabName)
        {
            case "TXPanel":
                report.fixture = "cash-main-wallet";
                page.SetText("tier0", WithdrawalPresentation.TierAmount(40)); page.SetText("tier1", WithdrawalPresentation.TierAmount(60)); page.SetText("tier2", WithdrawalPresentation.TierAmount(50000));
                for (int i = 0; i < 4; i++) { page.SetVisible("tier" + i, i < 3); page.SetVisible("selected" + i, i == 2); page.SetVisible("selectedAmount" + i, i == 2); }
                page.SetText("tierCost0", "$0.40"); page.SetText("tierCost1", "$0.60"); page.SetText("tierCost2", "$500.00");
                page.SetText("amount", "$500.00"); page.SetText("balance", "$500.00");
                page.SetText("target", "Balance target reached"); page.SetText("ratio", "100%");
                page.SetText("apply", "CHOOSE CHANNEL"); page.SetButton(WithdrawalAction.Apply, true);
                page.SetText("rewards", "Rewards");
                page.SetText("summary", WithdrawalPresentation.WalletSummary(WithdrawalRules.CreateDefault().Tiers[2]));
                page.SetProgress(1);
                break;
            case "TXCoinPanel":
                report.fixture = "reference-diamond-wallet-500-tier";
                long[] tiers = { 50000, 100000, 150000, 200000 };
                for (int i = 0; i < tiers.Length; i++)
                {
                    page.SetText("tier" + i, WithdrawalPresentation.TierAmount(tiers[i])); page.SetVisible("tier" + i, true);
                    page.SetText("tierCost" + i, ((i + 1) * 50000).ToString("N0", System.Globalization.CultureInfo.InvariantCulture));
                    page.SetVisible("selected" + i, i == 0);
                    page.SetVisible("selectedAmount" + i, i == 0);
                }
                page.SetText("frozen", "In application: 0 diamonds");
                page.SetText("amount", "$500.00"); page.SetText("balance", "50,600");
                page.SetText("target", "Balance target reached"); page.SetText("ratio", "100%");
                page.SetText("apply", "CHOOSE CHANNEL"); page.SetButton(WithdrawalAction.Apply, true);
                page.SetText("rewards", "Rewards");
                page.SetText("summary", WithdrawalPresentation.WalletSummary(WithdrawalRules.CreateDefault().Tiers[3])); page.SetProgress(1);
                break;
            case "TXChannelPanel":
                report.fixture = "channel-with-full-destination-label";
                page.SetText("amount", "$500.00");
                page.SetText("channel0", "Test wallet");
                page.SetText("channel1", "Test bank");
                page.SetVisible("channelSelected0", true); page.SetVisible("channelSelected1", false);
                page.SetText("hint", "No account details required.");
                break;
            case "TXUserInfoSurePanel":
                report.fixture = "full-confirmation-details";
                page.SetText("amount", "$500.00");
                page.SetText("channel", "Test wallet"); page.SetText("reserve", "$500.00"); page.SetText("afterBalance", "$0.00");
                page.SetText("hint", "Your test balance will be reserved.");
                page.SetText("details", "No payment account is collected. Read all conditions before applying.");
                break;
            case "TXProgress0Panel":
                report.fixture = "cash-main-500-all-seven-conditions";
                SetRequestFixture(page, "Conditions in progress");
                var conditions = new[] {
                    new WithdrawalConditionProgress { Kind = WithdrawalConditionKind.PassLevel, Current = 51, Target = 2, Complete = true },
                    new WithdrawalConditionProgress { Kind = WithdrawalConditionKind.PassHideLevel, Current = 51, Target = 22, Complete = true },
                    new WithdrawalConditionProgress { Kind = WithdrawalConditionKind.PassStage2Level, Current = 51, Target = 51, Complete = true },
                    new WithdrawalConditionProgress { Kind = WithdrawalConditionKind.GoldTarget, Current = 50000, Target = 50000, Complete = true },
                    new WithdrawalConditionProgress { Kind = WithdrawalConditionKind.LoginPassLevel, Current = 8, Target = 10 },
                    new WithdrawalConditionProgress { Kind = WithdrawalConditionKind.LoginDay, Current = 4, Target = 5 },
                    new WithdrawalConditionProgress { Kind = WithdrawalConditionKind.UserLevel, Current = 49, Target = 31, Complete = true }
                };
                for (int i = 0; i < 7; i++)
                {
                    page.SetVisible("condition" + i, true);
                    page.SetVisible("done" + i, conditions[i].Complete);
                    page.SetText("cond" + i, WithdrawalPresentation.ConditionText(conditions[i], WithdrawalCurrency.Cash));
                    page.SetProgress("condition" + i, WithdrawalPresentation.Ratio(conditions[i].Current, conditions[i].Target));
                }
                page.SetVisible("levelsRow", true); page.SetVisible("dailyRow", true); page.SetVisible("daysRow", true); page.SetVisible("rankRow", true);
                for (int i = 0; i < 3; i++) { page.SetVisible("levelDone" + i, true); page.SetVisible("levelPending" + i, false); }
                page.SetVisible("rankDone", true);
                page.SetText("milestones", "Level milestones " + WithdrawalPresentation.Milestones(WithdrawalRules.CreateDefault().Tiers[2]));
                page.SetText("daily", WithdrawalPresentation.GroupedProgress(conditions[4]));
                page.SetText("days", WithdrawalPresentation.GroupedProgress(conditions[5]));
                page.SetText("rank", WithdrawalPresentation.GroupedProgress(conditions[6]));
                page.SetText("dailyPercent", "80%"); page.SetText("daysPercent", "80%");
                page.SetProgress("daily", .8f); page.SetProgress("days", .8f);
                page.SetText("conditions", "[OK] Clear fruit levels  2 / 2\n"
                    + "[OK] First level milestone  22 / 22\n"
                    + "[OK] Final level milestone  51 / 51\n"
                    + "[ ] Win fruit levels in one UTC day  8 / 10\n"
                    + "[ ] Days with the published daily win goal  4 / 5\n"
                    + "[OK] Fruit rank from completed levels  49 / 31\n");
                page.SetText("step", "5 / 7 CONDITIONS COMPLETE");
                page.SetText("summary", "Earlier recorded progress counts. Keep playing to finish the remaining goals."); page.SetProgress(5f / 7);
                break;
            case "TXProgress1Panel":
                report.fixture = "simulated-review-countdown";
                SetRequestFixture(page, "Simulated review");
                page.SetText("countdown", "00:42");
                page.SetText("reviewHint", "All conditions completed.\nReview completes automatically.");
                page.SetText("conditions", "Application saved locally.\nNo account information is needed.\n\nSimulated review: 42s remaining");
                page.SetText("step", "SIMULATED REVIEW"); page.SetProgress(18f / 60);
                break;
            case "TXProgress2Panel":
                report.fixture = "long-simulated-completion-status";
                SetRequestFixture(page, "SIMULATED COMPLETE - NO PAYMENT");
                page.SetText("resultTitle", "Simulation completed");
                page.SetText("resultDetail", "No real money was sent");
                page.SetText("conditions", "Simulation completed.\n\nNo money was sent.\nThis result is saved in Records.");
                page.SetText("step", "SIMULATED COMPLETE - NO PAYMENT"); page.SetProgress(1);
                page.SetButton(WithdrawalAction.Cancel, false);
                break;
            case "TXProgress3Panel":
                report.fixture = "rejected-with-returned-units";
                SetRequestFixture(page, "Test rejected - units returned");
                page.SetText("resultTitle", "Test request closed");
                page.SetText("resultDetail", "Reserved units returned to wallet");
                page.SetText("afterBalance", "$500.00");
                page.SetText("conditions", "This test application was closed.\nThe reserved reward units were returned.\nYou can start another test.");
                page.SetText("step", "Test rejected - units returned"); page.SetButton(WithdrawalAction.Cancel, false);
                break;
            case "TXRecordPanel":
                report.fixture = report.recordsBottom ? "eight-records-scroll-bottom" : "eight-records-scroll-top";
                report.fixtureNotes.Add("All eight configured record rows are active. Top and bottom screenshots exercise ScrollRect clipping; offscreen rows are expected.");
                page.SetText("empty", "Your latest simulated applications");
                string[] amounts = { "$500.00", "$0.60", "$0.40", "$500.00", "$1,000.00", "$1,500.00", "$500.00", "$2,000.00" };
                WithdrawalRequestStatus[] statuses = { WithdrawalRequestStatus.SimulatedCompleted, WithdrawalRequestStatus.Cancelled,
                    WithdrawalRequestStatus.SimulatedCompleted, WithdrawalRequestStatus.Rejected, WithdrawalRequestStatus.ConditionsPending,
                    WithdrawalRequestStatus.ReviewPending, WithdrawalRequestStatus.Cancelled, WithdrawalRequestStatus.SimulatedCompleted };
                string[] dates = { "2026-10-09", "2026-10-08", "2026-10-08", "2026-10-07", "2026-10-06", "2026-10-05", "2026-10-04", "2026-10-03" };
                for (int i = 0; i < 8; i++)
                {
                    page.SetVisible("record" + i, true); page.SetText("recordAmount" + i, amounts[i]);
                    page.SetVisible("recordComplete" + i, statuses[i] == WithdrawalRequestStatus.SimulatedCompleted);
                    page.SetVisible("recordOther" + i, statuses[i] != WithdrawalRequestStatus.SimulatedCompleted);
                    page.SetText("recordStatus" + i, WithdrawalPresentation.RecordStatus(statuses[i]));
                    long created = DateTimeOffset.Parse(dates[i] + "T12:30:00Z", System.Globalization.CultureInfo.InvariantCulture).ToUnixTimeSeconds();
                    page.SetText("recordTime" + i, WithdrawalPresentation.RecordDate(created));
                }
                break;
            case "RewardGetPanel":
                report.fixture = "accumulated-fruit-rewards";
                page.SetText("rewardCash", "$1.30");
                page.SetText("rewardDiamonds", "+1,300 diamonds");
                page.SetText("afterCash", "$500.00"); page.SetText("afterDiamonds", "50,600");
                page.SetText("rewardDetail", "Fruit matches + level clear");
                page.SetButton(WithdrawalAction.Claim, true);
                break;
            case "RulesPanel":
                report.fixture = "cash-main-rules-all-published-condition-types";
                page.SetText("amount", "$500.00");
                WithdrawalRules fixtureRules = WithdrawalRules.CreateDefault();
                string[] rules = WithdrawalPresentation.RuleRows(fixtureRules.Tiers[2], fixtureRules);
                for (int i = 0; i < 8; i++) { page.SetVisible("rule" + i, true); page.SetText("rules" + i, rules[i]); }
                string[] summaries = WithdrawalPresentation.RuleSummaryRows(fixtureRules.Tiers[2]);
                for (int i = 0; i < summaries.Length; i++)
                {
                    page.SetVisible("ruleSummaryRow" + i, !string.IsNullOrEmpty(summaries[i]));
                    page.SetText("ruleSummary" + i, summaries[i]);
                }
                page.SetText("ruleFootnote", WithdrawalPresentation.RuleFootnote(fixtureRules.Tiers[2], fixtureRules));
                page.SetText("rules", "$500.00 - ALL CONDITIONS\n\n"
                    + "Balance required: $500.00\nClear 2 fruit levels.\n"
                    + "Clear fruit level 22.\n"
                    + "Clear fruit level 51.\n"
                    + "Clear 10 levels in one UTC day.\nQualify on 5 separate UTC days (each needs 10 wins).\nReach reward rank 31.\n"
                    + "\nSimulated review: 60 seconds after conditions complete.\n\n"
                    + "One active application. Its amount is reserved. Cancelling or rejecting returns it.\n\n"
                    + "All rewards and results are simulated, with no cash value.\n\n"
                    + "Progress is saved on this device. Review all conditions before applying.");
                break;
            default:
                report.fixture = "saved-defaults-no-specific-fixture";
                break;
        }
    }

    private static void SetRequestFixture(WithdrawalPageView page, string status)
    {
        page.SetText("amount", "$500.00"); page.SetText("status", status);
        page.SetText("channel", "Test wallet");
        page.SetText("requestId", "ID: 0123456789ab");
    }

    private static void AuditBindings(WithdrawalPageView view, PageReport report)
    {
        if (view.Content == null) report.invalidBindings.Add("Content is null.");
        if (view.DesignSize.x <= 0 || view.DesignSize.y <= 0) report.invalidBindings.Add("DesignSize is not positive.");
        if (view.Texts == null || view.Buttons == null || view.Objects == null || view.Progress == null)
        {
            report.invalidBindings.Add("A serialized binding array is null.");
            return;
        }
        foreach (WithdrawalTextBinding binding in view.Texts)
            if (binding == null || binding.Text == null || string.IsNullOrEmpty(binding.Key))
                report.invalidBindings.Add("Missing Text binding: " + (binding == null ? "null" : binding.Key));
        foreach (WithdrawalButtonBinding binding in view.Buttons)
            if (binding == null || binding.Button == null)
                report.invalidBindings.Add("Missing Button binding.");
        foreach (WithdrawalObjectBinding binding in view.Objects)
            if (binding == null || binding.Object == null || string.IsNullOrEmpty(binding.Key))
                report.invalidBindings.Add("Missing Object binding: " + (binding == null ? "null" : binding.Key));
        foreach (WithdrawalProgressBinding binding in view.Progress)
            if (binding == null || binding.Fill == null || string.IsNullOrEmpty(binding.Key))
                report.invalidBindings.Add("Missing Progress binding: " + (binding == null ? "null" : binding.Key));
        var boundButtons = new HashSet<Button>();
        foreach (WithdrawalButtonBinding binding in view.Buttons)
        {
            if (binding == null || binding.Button == null) continue;
            if (!boundButtons.Add(binding.Button)) report.invalidBindings.Add(binding.Button.name + ": duplicate Button binding.");
            if (binding.Button.targetGraphic == null) report.invalidBindings.Add(binding.Button.name + ": Button has no visual targetGraphic.");
            else if (binding.Button.targetGraphic.transform != binding.Button.transform
                && !binding.Button.targetGraphic.transform.IsChildOf(binding.Button.transform))
                report.invalidBindings.Add(binding.Button.name + ": Button visual is outside its hierarchy.");
        }
        foreach (Button button in view.GetComponentsInChildren<Button>(true))
            if (!boundButtons.Contains(button)) report.invalidBindings.Add(button.name + ": clickable Button has no dispatch binding.");
    }

    private static void AuditComponents(GameObject root, PageReport report)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        report.gameObjects = transforms.Length;
        report.buttons = root.GetComponentsInChildren<Button>(true).Length;
        report.textComponents = root.GetComponentsInChildren<TMP_Text>(true).Length;
        foreach (Transform transform in transforms)
        {
            string path = Hierarchy(transform, root.transform);
            int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject);
            if (missing != 0) report.missingComponents.Add(path + ": " + missing);
            foreach (Component component in transform.GetComponents<Component>())
            {
                if (component == null) continue;
                var serialized = new SerializedObject(component);
                SerializedProperty property = serialized.GetIterator();
                while (property.NextVisible(true))
                    if (property.propertyType == SerializedPropertyType.ObjectReference
                        && property.objectReferenceValue == null && property.objectReferenceInstanceIDValue != 0)
                        report.brokenObjectReferences.Add(path + "/" + property.propertyPath);
            }
        }
        Image[] images = root.GetComponentsInChildren<Image>(true);
        report.images = images.Length;
        foreach (Image image in images)
        {
            string path = Hierarchy(image.transform, root.transform);
            if (image.sprite == null) report.plainImagesWithoutSprite.Add(path);
            else if (image.sprite.texture == null || image.sprite.texture.width < 1 || image.sprite.texture.height < 1)
                report.missingTextures.Add(path + ": sprite texture is missing or empty.");
            if (image.material != null && (image.material.shader == null || !image.material.shader.isSupported))
                report.missingTextures.Add(path + ": material shader is missing/unsupported.");
        }
        foreach (RawImage image in root.GetComponentsInChildren<RawImage>(true))
            if (image.texture == null) report.missingTextures.Add(Hierarchy(image.transform, root.transform) + ": RawImage texture is null.");
    }

    private static void AuditTextAndBounds(GameObject root, Canvas canvas, Camera camera, PageReport report)
    {
        foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
        {
            string path = Hierarchy(text.transform, root.transform);
            var item = new TextReport
            {
                path = path, active = text.gameObject.activeInHierarchy, text = text.text,
                rectSize = text.rectTransform.rect.size, fontSize = text.fontSize,
                overflowing = text.gameObject.activeInHierarchy && text.isTextOverflowing,
                visibleCharacters = text.textInfo == null ? 0 : text.textInfo.characterCount,
                fontAsset = AssetDatabase.GetAssetPath(text.font),
                material = AssetDatabase.GetAssetPath(text.fontSharedMaterial)
            };
            item.scrollingContent = text.GetComponentInParent<ScrollRect>() != null;
            report.texts.Add(item);
            if (item.active && !string.IsNullOrWhiteSpace(item.text) && item.visibleCharacters == 0)
                report.renderErrors.Add(path + ": non-empty active text generated no characters.");
            if (item.overflowing) report.textOverflows.Add(path);
            if (text.font == null) { report.missingFonts.Add(path + ": font is null."); continue; }
            Texture2D[] atlases = text.font.atlasTextures;
            if (atlases == null || atlases.Length == 0) report.missingFonts.Add(path + ": no atlas textures.");
            else for (int i = 0; i < atlases.Length; i++)
                if (atlases[i] == null || atlases[i].width < 1 || atlases[i].height < 1)
                    report.missingFonts.Add(path + ": missing/empty atlas " + i);
            Material material = text.fontSharedMaterial;
            if (material == null || material.shader == null || !material.shader.isSupported || material.mainTexture == null)
                report.missingFonts.Add(path + ": missing font material, supported shader or atlas binding.");
            var missing = new HashSet<char>();
            string plain = Regex.Replace(text.text ?? "", "<[^>]+>", "");
            foreach (char character in plain)
                if (!char.IsWhiteSpace(character) && !char.IsControl(character)
                    && !text.font.HasCharacter(character, true, false)) missing.Add(character);
            if (missing.Count > 0)
            {
                var chars = new char[missing.Count]; missing.CopyTo(chars); Array.Sort(chars);
                item.missingCharacters = new string(chars);
                report.missingFonts.Add(path + ": unavailable glyphs " + item.missingCharacters);
            }
        }
        var corners = new Vector3[4];
        foreach (Graphic graphic in root.GetComponentsInChildren<Graphic>(false))
        {
            if (!graphic.enabled || graphic.color.a <= 0) continue;
            if (graphic is TMP_Text label && string.IsNullOrEmpty(label.text)) continue;
            graphic.rectTransform.GetWorldCorners(corners);
            bool outside = false;
            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 position = camera.WorldToViewportPoint(corners[i]);
                if (position.x < -.001f || position.x > 1.001f || position.y < -.001f || position.y > 1.001f) outside = true;
            }
            if (outside)
            {
                string path = Hierarchy(graphic.transform, root.transform);
                report.outsideCanvas.Add(path);
                if (graphic.GetComponentInParent<ScrollRect>() == null)
                    report.renderErrors.Add(path + ": active visual extends outside the rendered canvas.");
            }
        }
    }

    private static string Hierarchy(Transform transform, Transform root)
    {
        string path = transform.name;
        while (transform != root && transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }
}
