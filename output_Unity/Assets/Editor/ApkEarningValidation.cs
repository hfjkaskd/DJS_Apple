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

/// <summary>Saved-prefab fixtures and separately labelled captures of the actual running game.</summary>
public static class ApkEarningValidation
{
    private const string PanelFolder = "Assets/ApkEarningReference/Resources/ApkEarningReference/Panels/";
    private const string CorePath = "Assets/res/local/coreplay/CorePlayUI.prefab";
    private const string TopPath = "Assets/res/local/withdrawal/ApkEarningTop.prefab";
    private const string HostPath = "Assets/res/local/withdrawal/ApkEarningHost.prefab";
    private static string Output = "E:/CodexCacheBackups/DJS_Apple-apk-validation-20261014";
    private static readonly string[] Pages = {
        "TXPanel", "TXCoinPanel", "LocalWithdrawalConditions", "LocalCashHistory", "LocalEarningSequence",
        "LocalLuckyReward", "TXUserInfoPanel", "TXUserInfoSurePanel", "LocalSuccess", "LocalEarningRecovery",
        "TXRecordGuidePanel", "TXGuideLoadingPanel", "TXProgress0Panel", "TXChannelPanel",
        "LocalWithdrawalVerification", "TXGuideTargetCompletePanel", "LocalDiamondUnlockGuide", "Tip",
        // Retained source/alternate pages are also checked so a later route cannot restore old artwork.
        "NewbieGuidePanel", "RewardPanel", "SuccessPanel", "LocalUserLevel", "TXLevelPanel", "TXTooFastHintPanel"
    };
    private static string pendingCapture;
    private static double captureDeadline;

    [Serializable] private sealed class Report
    {
        public string generatedUtc, scope;
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
        public List<PageReport> pages = new List<PageReport>();
        public List<RenderReport> renders = new List<RenderReport>();
    }
    [Serializable] private sealed class PageReport
    {
        public string path;
        public int buttons, texts, images, textBindings, buttonBindings, imageBindings, objectBindings, inputBindings;
    }
    [Serializable] private sealed class RenderReport
    {
        public string page, state, image;
        public string dataSource = "Static editor preview data only; not player data or a running-game capture.";
        public int width, height, activeButtons, activeTexts, textOverlapChecks, buttonOverlapChecks, buttonTextOverlapChecks;
        public float canvasScale;
        public ModalBoundsReport modalBounds;
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
    }
    [Serializable] private sealed class ModalBoundsReport
    {
        public string measurement = "Screen-space rectangle union of enabled, non-transparent CashWalletPanel Images; not alpha-pixel bounds. Visible bounds are intersected with active ancestor masks and the viewport.";
        public int frameCount;
        public float referenceWidth, minimumReferenceHeight, artworkScale;
        public Rect frameBounds, visibleFrameBounds;
        public float widthRatio, heightRatio, visibleWidthRatio, visibleHeightRatio;
        public List<string> frames = new List<string>();
    }
    [Serializable] private sealed class RuntimeReport
    {
        public string generatedUtc, scope = "Actual running game capture; current real save, no fixture substitution or UI navigation.", image;
        public int width, height, activePages, activeButtons;
        public List<string> pageNames = new List<string>();
        public List<string> errors = new List<string>();
        public List<RuntimeButtonReport> buttons = new List<RuntimeButtonReport>();
        public List<RuntimeTextReport> statsTexts = new List<RuntimeTextReport>();
    }
    [Serializable] private sealed class RuntimeTextReport
    {
        public string alias, path, text, font, material, texture, overflow, alignment;
        public bool active, enabled, culled, autoSize, wordWrapping;
        public Color color;
        public float rendererAlpha, fontSize, fontSizeMin, fontSizeMax, preferredWidth, preferredHeight;
        public int characters, visibleCharacters, maxVisibleCharacters, materialQueue;
        public Vector2 rectSize, screenCenter;
        public List<string> canvasGroups = new List<string>();
    }
    [Serializable] private sealed class RuntimeButtonReport
    {
        public string path;
        public bool interactable, active, centerReachesButton;
        public Vector2 screenCenter;
        public List<string> canvasChain = new List<string>();
        public List<string> canvasGroups = new List<string>();
        public List<string> centerHits = new List<string>();
    }

    [MenuItem("Tools/Apk Earning/Build and Validate Fixtures %&j")]
    public static void BuildAndValidate()
    {
        ApkEarningUIAuthoring.Build();
        RenderFixtures();
    }

    [MenuItem("Tools/Apk Earning/Apply and Validate Flow Fixes")]
    public static void ApplyAndValidateFlowFixes()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before applying and rendering saved flow fixtures.");
        ApkEarningFlowAdditionsAuthoring.Apply();
        ApkEarningFlowPrefabValidation.Validate();
        Output = "C:/Projects/DJS_Apple/flow-parity-fix-20261020/static-fixtures";
        Directory.CreateDirectory(Output);
        var report = new Report {
            generatedUtc = DateTime.UtcNow.ToString("O"),
            scope = "Only the nine restored-flow pages are audited and rendered after localized prefab authoring. These are isolated static editor fixtures with explicitly synthetic preview data, not real players, live Host navigation, APK screenshots or device captures. No service, accounting, storage, ad network or payment operation is initialized. Test-video countdown and completion are separate authored presentation fixtures; animation and button behavior require the independent Play-mode regression."
        };
        string[] changedPages = {
            "TXPanel", "TXCoinPanel", "LocalWithdrawalConditions", "LocalUserLevel",
            "TXTooFastHintPanel", "LocalRewardVideoTest", "LocalWithdrawalVerification",
            "TXProgress0Panel", "LocalEarningSequence"
        };
        foreach (string name in changedPages)
        {
            string path = PanelFolder + name + ".prefab";
            AuditPrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path), path, report);
        }
        foreach (Vector2Int size in new[] { new Vector2Int(1080, 1920), new Vector2Int(1080, 2340), new Vector2Int(720, 1600) })
            foreach (string name in changedPages)
                foreach (string state in FixtureStates(name))
                    RenderFixture(name, PanelFolder + name + ".prefab", size, false, report, state);
        File.WriteAllText(Path.Combine(Output, "prefab-fixtures-report.json"), JsonUtility.ToJson(report, true));
        if (report.errors.Count > 0)
            throw new InvalidOperationException("Restored flow fixture validation found " + report.errors.Count + " errors. See " + Output);
        Debug.Log("[ApkEarningValidation] Restored flow passed " + report.pages.Count + " prefab audits and " + report.renders.Count + " static fixture renders. " + Output);
    }

    [MenuItem("Tools/Apk Earning/Validate Enlarged Modal Fixtures")]
    public static void ValidateEnlargedModalFixtures()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before rendering saved modal fixtures.");
        string output = Environment.GetEnvironmentVariable("DJS_APPLE_MODAL_VALIDATION_OUTPUT");
        if (string.IsNullOrWhiteSpace(output))
            output = "E:/CodexCacheBackups/DJS_Apple-modal-size-20261020/after";
        Directory.CreateDirectory(output);
        var report = new Report {
            generatedUtc = DateTime.UtcNow.ToString("O"),
            scope = "Read-only saved-prefab layout previews and cross-resolution validation. Every Panels prefab whose root PageView has FitWidth enabled, including variants and LocalRewardVideoTest, is rendered in all existing fixture states at 1080x1920, 1080x2400 and 720x1600. Nine representative default-state 1080x2400 PNGs are saved. At 1080x1920, cash, diamond, account, history, reward, withdrawal conditions, verification and account confirmation default states plus diamond withdrawal conditions are saved for equal-resolution layout comparisons. Other renders still run layout, camera rendering and all existing audits without GPU readback or PNG encoding. Modal coverage measures visible CashWalletPanel Image rectangles, not alpha pixels; absent frames are reported explicitly. Synthetic preview data and production RefreshLayout are reused unchanged. These are layout previews, not live gameplay captures or proof of reference-pixel parity. No authoring, save mutation, Service, Host, ad network or payment operation is run."
        };
        var paths = new List<string>();
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { PanelFolder.TrimEnd('/') }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            ApkEarningPageView view = prefab == null ? null : prefab.GetComponent<ApkEarningPageView>();
            if (view != null && view.FitWidth) paths.Add(path);
        }
        paths.Sort(StringComparer.Ordinal);
        if (paths.Count == 0) report.errors.Add("No FitWidth modal prefabs were found.");
        if (!paths.Contains(PanelFolder + "LocalRewardVideoTest.prefab"))
            report.errors.Add("LocalRewardVideoTest must be present and configured as a FitWidth modal.");
        var screenshots = new HashSet<string>(StringComparer.Ordinal) {
            "TXPanel", "TXCoinPanel", "TXUserInfoPanel", "LocalCashHistory", "LocalSuccess",
            "LocalWithdrawalConditions", "LocalEarningSequence", "LocalWithdrawalVerification", "LocalRewardVideoTest"
        };
        var standardScreenshots = new HashSet<string>(StringComparer.Ordinal) {
            "TXPanel", "TXCoinPanel", "TXUserInfoPanel", "LocalCashHistory", "LocalSuccess",
            "LocalWithdrawalConditions", "LocalWithdrawalVerification", "TXUserInfoSurePanel"
        };
        var requiredScreenshots = new HashSet<string>(screenshots, StringComparer.Ordinal);
        requiredScreenshots.UnionWith(standardScreenshots);
        foreach (string name in requiredScreenshots)
            if (!paths.Contains(PanelFolder + name + ".prefab"))
                report.errors.Add("Representative FitWidth modal is missing: " + name);
        foreach (string path in paths)
            AuditPrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path), path, report);
        foreach (Vector2Int size in new[] { new Vector2Int(1080, 1920), new Vector2Int(1080, 2400), new Vector2Int(720, 1600) })
            foreach (string path in paths)
            {
                string name = Path.GetFileNameWithoutExtension(path);
                foreach (string state in FixtureStates(name))
                    RenderFixture(name, path, size, false, report, state,
                        size.x == 1080 &&
                        ((size.y == 2400 && string.IsNullOrEmpty(state) && screenshots.Contains(name)) ||
                         (size.y == 1920 && ((string.IsNullOrEmpty(state) && standardScreenshots.Contains(name)) ||
                          (name == "LocalWithdrawalConditions" && state == "diamond") ||
                          (name == "LocalSuccess" && state == "early-reward")))),
                        true, output);
            }
        File.WriteAllText(Path.Combine(output, "enlarged-modal-fixtures-report.json"), JsonUtility.ToJson(report, true));
        if (report.errors.Count > 0)
            throw new InvalidOperationException("Modal layout validation found " + report.errors.Count + " errors. See " + output);
        Debug.Log("[ApkEarningValidation] Modal layouts passed " + report.pages.Count + " prefab audits and " +
            report.renders.Count + " static renders, with nineteen representative PNGs. " + output);
    }

    [MenuItem("Tools/Apk Earning/Validate and Render Saved Fixtures")]
    public static void RenderFixtures()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before rendering saved prefab fixtures.");
        Directory.CreateDirectory(Output);
        var report = new Report {
            generatedUtc = DateTime.UtcNow.ToString("O"),
            scope = "Saved prefab visual fixtures with explicitly synthetic preview data, not live gameplay or APK device screenshots. All active earning pages and retained alternate/source pages are rendered. Conditions, history records, reward items, account forms and every message/receipt/bonus sequence layout receive representative static data in an isolated preview scene. No Service, Host navigation, accounting, player storage or payment operation is initialized. Imported earning prefabs, earning children inside CorePlayUI, and the modal host are audited strictly. Unassigned fonts on legacy CorePlayUI text outside earning views are reported separately as warnings. Production PageView.RefreshLayout and Game.unity Expand scaling are reused."
        };
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { PanelFolder.TrimEnd('/') }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AuditPrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path), path, report);
        }
        AuditPrefab(AssetDatabase.LoadAssetAtPath<GameObject>(CorePath), CorePath, report, false, true);
        AuditPrefab(AssetDatabase.LoadAssetAtPath<GameObject>(HostPath), HostPath, report, false);
        foreach (Vector2Int size in new[] { new Vector2Int(1080, 1920), new Vector2Int(1080, 2340), new Vector2Int(720, 1600) })
        {
            RenderFixture("GameplayHUD", CorePath, size, true, report);
            foreach (string page in Pages)
                foreach (string state in FixtureStates(page))
                    RenderFixture(page, PanelFolder + page + ".prefab", size, false, report, state);
        }
        File.WriteAllText(Path.Combine(Output, "prefab-fixtures-report.json"), JsonUtility.ToJson(report, true));
        if (report.errors.Count > 0)
            throw new InvalidOperationException("APK earning fixture validation found " + report.errors.Count + " errors. See " + Output);
        Debug.Log("[ApkEarningValidation] Passed " + report.pages.Count + " prefab audits and " + report.renders.Count + " fixture renders. " + Output);
    }

    [MenuItem("Tools/Apk Earning/Validate Compact Gameplay HUD %#&y")]
    public static void ValidateCompactGameplayHud()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before rendering saved HUD fixtures.");
        Directory.CreateDirectory(Output);
        var report = new Report {
            generatedUtc = DateTime.UtcNow.ToString("O"),
            scope = "Saved CorePlayUI and ApkEarningTop prefab audit with three isolated gameplay HUD previews. The serialized $3.15 / 102 / 3-of-5 values are representative preview data, not live player data. The native normal level badge and completed header fade-in are selected explicitly because runtime controllers and animations are disabled. BaseUI safe-area insetting and ApkEarningTopView.RefreshLayout are reused. The disabled TopView never initializes the service; no accounting, player storage, navigation or payment operation is initialized. Visible HUD text overflow, Button viewport boundaries and a greater-than-three-pixel vertical center mismatch in the compact header row are errors. Legacy CorePlayUI fonts outside earning views remain separate warnings."
        };
        AuditPrefab(AssetDatabase.LoadAssetAtPath<GameObject>(CorePath), CorePath, report, false, true);
        AuditPrefab(AssetDatabase.LoadAssetAtPath<GameObject>(TopPath), TopPath, report);
        foreach (Vector2Int size in new[] { new Vector2Int(1080, 1920), new Vector2Int(1080, 2340), new Vector2Int(720, 1600) })
            RenderFixture("GameplayHUD", CorePath, size, true, report);
        File.WriteAllText(Path.Combine(Output, "compact-hud-report.json"), JsonUtility.ToJson(report, true));
        if (report.errors.Count > 0)
            throw new InvalidOperationException("Compact HUD validation found " + report.errors.Count + " errors. See " + Output);
        Debug.Log("[ApkEarningValidation] Compact HUD passed " + report.pages.Count + " prefab audits and " + report.renders.Count + " fixture renders. " + Output);
    }

    [MenuItem("Tools/Apk Earning/Capture Current Runtime %&k")]
    public static void CaptureCurrentRuntime()
    {
        if (!EditorApplication.isPlaying) throw new InvalidOperationException("Start the game and navigate to the desired page before capturing.");
        if (pendingCapture != null) throw new InvalidOperationException("Wait for the current runtime capture to finish.");
        Directory.CreateDirectory(Output);
        var report = new RuntimeReport { generatedUtc = DateTime.UtcNow.ToString("O"), width = Screen.width, height = Screen.height };
        var seenButtons = new HashSet<Button>();
        foreach (ApkEarningPageView view in Object.FindObjectsOfType<ApkEarningPageView>())
        {
            if (!view.gameObject.activeInHierarchy) continue;
            report.activePages++; report.pageNames.Add(view.name);
            foreach (ApkEarningPageView.TextSlot slot in view.Texts)
                if (slot.Target != null && slot.Key.StartsWith("Stats.", StringComparison.Ordinal))
                    report.statsTexts.Add(InspectRuntimeText(slot.Key, slot.Target));
            foreach (Button button in view.GetComponentsInChildren<Button>(false))
            {
                if (!seenButtons.Add(button)) continue;
                report.activeButtons++;
                AuditButton(button, report.errors);
                report.buttons.Add(InspectRuntimeButton(button));
            }
        }
        string stem = "runtime-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff");
        report.image = Path.Combine(Output, stem + ".png");
        File.WriteAllText(Path.Combine(Output, stem + ".json"), JsonUtility.ToJson(report, true));
        pendingCapture = report.image;
        captureDeadline = EditorApplication.timeSinceStartup + 15;
        ScreenCapture.CaptureScreenshot(pendingCapture);
        EditorApplication.update += AwaitCapture;
    }

    // Read-only EventSystem diagnostics: no pointer event is dispatched and no
    // flow state is changed. Covered gameplay buttons remain expected results.
    private static RuntimeButtonReport InspectRuntimeButton(Button button)
    {
        var result = new RuntimeButtonReport {
            path = Hierarchy(button.transform), active = button.isActiveAndEnabled,
            interactable = button.IsInteractable()
        };
        foreach (Canvas canvas in button.GetComponentsInParent<Canvas>(true))
            result.canvasChain.Add(Hierarchy(canvas.transform) + " mode=" + canvas.renderMode +
                " override=" + canvas.overrideSorting + " order=" + canvas.sortingOrder +
                " raycaster=" + (canvas.GetComponent<GraphicRaycaster>() != null));
        foreach (CanvasGroup group in button.GetComponentsInParent<CanvasGroup>(true))
            result.canvasGroups.Add(Hierarchy(group.transform) + " alpha=" + group.alpha +
                " interactable=" + group.interactable + " blocks=" + group.blocksRaycasts +
                " ignoreParents=" + group.ignoreParentGroups);
        var rect = (RectTransform)button.transform;
        Canvas nearest = button.GetComponentInParent<Canvas>();
        Camera camera = nearest == null || nearest.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null : nearest.rootCanvas.worldCamera;
        result.screenCenter = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
        if (EventSystem.current != null)
        {
            var pointer = new PointerEventData(EventSystem.current) { position = result.screenCenter };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            for (int i = 0; i < hits.Count && i < 8; i++)
                result.centerHits.Add(Hierarchy(hits[i].gameObject.transform) +
                    " order=" + hits[i].sortingOrder + " depth=" + hits[i].depth);
            result.centerReachesButton = hits.Count > 0 &&
                ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) == button.gameObject;
        }
        return result;
    }

    private static RuntimeTextReport InspectRuntimeText(string alias, TMP_Text text)
    {
        Material material = text.fontSharedMaterial;
        var result = new RuntimeTextReport {
            alias = alias, path = Hierarchy(text.transform), text = text.text,
            active = text.gameObject.activeInHierarchy, enabled = text.enabled,
            culled = text.canvasRenderer.cull, color = text.color,
            rendererAlpha = text.canvasRenderer.GetAlpha(), fontSize = text.fontSize,
            autoSize = text.enableAutoSizing, wordWrapping = text.enableWordWrapping,
            fontSizeMin = text.fontSizeMin, fontSizeMax = text.fontSizeMax,
            preferredWidth = text.preferredWidth, preferredHeight = text.preferredHeight,
            overflow = text.overflowMode.ToString(), alignment = text.alignment.ToString(),
            font = text.font == null ? "missing" : text.font.name,
            material = material == null ? "missing" : material.name,
            texture = material == null || material.mainTexture == null ? "missing" : material.mainTexture.name,
            materialQueue = material == null ? -1 : material.renderQueue,
            characters = text.textInfo.characterCount, maxVisibleCharacters = text.maxVisibleCharacters,
            rectSize = text.rectTransform.rect.size
        };
        for (int i = 0; i < text.textInfo.characterCount; i++)
            if (text.textInfo.characterInfo[i].isVisible) result.visibleCharacters++;
        Canvas nearest = text.GetComponentInParent<Canvas>();
        Camera camera = nearest == null || nearest.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null : nearest.rootCanvas.worldCamera;
        result.screenCenter = RectTransformUtility.WorldToScreenPoint(camera,
            text.rectTransform.TransformPoint(text.rectTransform.rect.center));
        foreach (CanvasGroup group in text.GetComponentsInParent<CanvasGroup>(true))
            result.canvasGroups.Add(Hierarchy(group.transform) + " alpha=" + group.alpha);
        return result;
    }

    private static void AwaitCapture()
    {
        if (pendingCapture != null && File.Exists(pendingCapture) && new FileInfo(pendingCapture).Length > 0)
        {
            string path = pendingCapture; pendingCapture = null; EditorApplication.update -= AwaitCapture;
            Debug.Log("[ApkEarningValidation] Actual runtime screenshot saved: " + path);
        }
        else if (EditorApplication.timeSinceStartup >= captureDeadline)
        {
            string path = pendingCapture; pendingCapture = null; EditorApplication.update -= AwaitCapture;
            Debug.LogError("[ApkEarningValidation] Runtime screenshot did not finish: " + path);
        }
    }

    private static void AuditPrefab(GameObject root, string path, Report report, bool requirePage = true, bool legacyCoreFonts = false)
    {
        if (root == null) { report.errors.Add("Missing prefab: " + path); return; }
        var page = new PageReport { path = path };
        report.pages.Add(page);
        foreach (Transform node in root.GetComponentsInChildren<Transform>(true))
            foreach (Component component in node.GetComponents<Component>())
                if (component == null) report.errors.Add(path + ": missing script at " + Hierarchy(node));
        foreach (Button button in root.GetComponentsInChildren<Button>(true)) { page.buttons++; AuditButton(button, report.errors); }
        foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
        {
            page.texts++;
            if (text.font == null || text.fontSharedMaterial == null)
            {
                string issue = path + ": missing text font/material at " + Hierarchy(text.transform);
                if (legacyCoreFonts && text.GetComponentInParent<ApkEarningPageView>(true) == null)
                    report.warnings.Add("Legacy CorePlayUI text (outside imported earning audit; verify runtime font assignment): " + issue);
                else report.errors.Add(issue);
            }
        }
        foreach (Image image in root.GetComponentsInChildren<Image>(true))
        {
            page.images++;
            SerializedProperty sprite = new SerializedObject(image).FindProperty("m_Sprite");
            if (sprite != null && sprite.objectReferenceValue == null && sprite.objectReferenceInstanceIDValue != 0)
                report.errors.Add(path + ": missing sprite at " + Hierarchy(image.transform));
        }
        if (!requirePage) return;
        ApkEarningPageView view = root.GetComponent<ApkEarningPageView>();
        if (view == null) { report.errors.Add(path + ": ApkEarningPageView has not been authored."); return; }
        page.textBindings = view.Texts.Length; page.buttonBindings = view.Buttons.Length; page.imageBindings = view.Images.Length;
        page.objectBindings = view.Objects.Length; page.inputBindings = view.Inputs.Length;
        AuditSlots(view.Texts, slot => slot.Key, slot => slot.Target, path + " text", report.errors);
        AuditSlots(view.Buttons, slot => slot.Key, slot => slot.Target, path + " button", report.errors);
        AuditSlots(view.Images, slot => slot.Key, slot => slot.Target, path + " image", report.errors);
        AuditSlots(view.Objects, slot => slot.Key, slot => slot.Target, path + " object", report.errors);
        AuditSlots(view.Inputs, slot => slot.Key, slot => slot.Target, path + " input", report.errors);
    }

    private static void AuditSlots<T>(T[] slots, Func<T, string> key, Func<T, Object> target, string label, List<string> errors) where T : class
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (T slot in slots)
        {
            if (slot == null || string.IsNullOrEmpty(key(slot)) || target(slot) == null) { errors.Add(label + ": null/empty binding."); continue; }
            if (!keys.Add(key(slot))) errors.Add(label + ": duplicate key " + key(slot));
        }
    }

    private static void AuditButton(Button button, List<string> errors)
    {
        if (button.onClick.GetPersistentEventCount() != 0) errors.Add(Hierarchy(button.transform) + ": Inspector event bindings remain.");
        Graphic graphic = button.targetGraphic;
        if (graphic == null || (graphic.transform != button.transform && !graphic.transform.IsChildOf(button.transform)))
            errors.Add(Hierarchy(button.transform) + ": Button does not own its target graphic.");
    }

    private static void RenderFixture(string name, string path, Vector2Int size, bool hudOnly, Report report,
        string state = "", bool saveImage = true, bool measureModal = false, string outputDirectory = null)
    {
        var item = new RenderReport { page = name, state = string.IsNullOrEmpty(state) ? "representative" : state, width = size.x, height = size.y };
        report.renders.Add(item);
        Scene scene = EditorSceneManager.NewPreviewScene();
        RenderTexture texture = null; Texture2D pixels = null;
        RenderTexture previous = RenderTexture.active;
        GameObject cameraObject = null, canvasObject = null;
        try
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (source == null) throw new InvalidOperationException("Missing " + path);
            cameraObject = new GameObject("ApkFixtureCamera"); SceneManager.MoveGameObjectToScene(cameraObject, scene);
            Camera camera = cameraObject.AddComponent<Camera>(); camera.enabled = false; camera.scene = scene;
            camera.orthographic = true; camera.orthographicSize = size.y * .5f; camera.aspect = (float)size.x / size.y;
            camera.transform.position = new Vector3(0, 0, -10); camera.nearClipPlane = .01f; camera.farClipPlane = 100;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black; camera.allowHDR = camera.allowMSAA = false;
            texture = new RenderTexture(size.x, size.y, 24, RenderTextureFormat.ARGB32); texture.Create(); camera.targetTexture = texture;
            canvasObject = new GameObject("ApkFixtureCanvas", typeof(RectTransform)); SceneManager.MoveGameObjectToScene(canvasObject, scene);
            canvasObject.SetActive(false);
            Canvas canvas = canvasObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera; canvas.planeDistance = 1; canvas.referencePixelsPerUnit = 100;
            item.canvasScale = Mathf.Min(size.x / 1080f, size.y / 2340f); canvas.scaleFactor = item.canvasScale;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;
            AddBackground(canvas.transform, size);
            GameObject instance = Object.Instantiate(source, canvas.transform, false); instance.name = source.name;
            if (!hudOnly) ApplyPreviewState(instance.GetComponent<ApkEarningPageView>(), name, state);
            // The inactive ancestor prevents runtime accounting/host initialization.
            // HUD previews retain a disabled controller solely to call its shared layout method.
            foreach (MonoBehaviour component in instance.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (hudOnly && component is ApkEarningTopView top) top.enabled = false;
                else if (component != null && !(component is UIBehaviour) && !(component is ApkEarningPageView)) Object.DestroyImmediate(component);
            }
            foreach (Animation animation in instance.GetComponentsInChildren<Animation>(true)) animation.enabled = false;
            foreach (Animator animator in instance.GetComponentsInChildren<Animator>(true)) animator.enabled = false;
            if (hudOnly)
            {
                KeepHeader(instance.transform);
                ApplyHeaderPreviewState(instance.transform);
                Orange.SafeAreaSim.InsetRect(instance.transform as RectTransform);
            }
            canvasObject.SetActive(true); instance.SetActive(true);
            Canvas.ForceUpdateCanvases();
            foreach (ApkEarningPageView view in instance.GetComponentsInChildren<ApkEarningPageView>(true)) view.RefreshLayout();
            if (hudOnly)
                foreach (ApkEarningTopView top in instance.GetComponentsInChildren<ApkEarningTopView>(true)) top.RefreshLayout();
            foreach (TMP_Text text in instance.GetComponentsInChildren<TMP_Text>(false)) text.ForceMeshUpdate(true, true);
            Canvas.ForceUpdateCanvases(); camera.Render();
            if (measureModal)
            {
                MeasureModalBounds(instance, camera, size, item);
                AuditModalTextOverlaps(instance.GetComponent<ApkEarningPageView>(), name, camera, size, item);
                if (name == "TXCoinPanel")
                    AuditDiamondOptionOverlaps(instance.GetComponent<ApkEarningPageView>(), camera, size, item);
            }
            if (hudOnly) AuditCompactHeaderAlignment(instance.transform, camera, item.errors);
            foreach (Button button in instance.GetComponentsInChildren<Button>(false))
            {
                item.activeButtons++; Rect bounds = ScreenBounds((RectTransform)button.transform, camera);
                if (bounds.width < .5f || bounds.height < .5f)
                    (hudOnly ? item.errors : item.warnings).Add(Hierarchy(button.transform) + ": static Button has no visible size.");
                else if (bounds.xMax < 0 || bounds.xMin > size.x || bounds.yMax < 0 || bounds.yMin > size.y)
                    (hudOnly ? item.errors : item.warnings).Add(Hierarchy(button.transform) + ": authored state keeps Button offscreen; runtime routing must hide or position it.");
                else if (bounds.xMin < -1 || bounds.yMin < -1 || bounds.xMax > size.x + 1 || bounds.yMax > size.y + 1)
                    item.errors.Add(Hierarchy(button.transform) + ": visible Button crosses viewport bounds: " + bounds);
                AuditMaskClipping((RectTransform)button.transform, camera, "Button", item.warnings);
            }
            foreach (TMP_Text text in instance.GetComponentsInChildren<TMP_Text>(false))
            {
                if (!text.isActiveAndEnabled) continue;
                item.activeTexts++;
                if (text.isTextOverflowing)
                    (hudOnly ? item.errors : item.warnings).Add(Hierarchy(text.transform) + ": representative preview text overflows its text area.");
                if (hudOnly && !string.IsNullOrWhiteSpace(text.text))
                {
                    Rect bounds = ScreenBounds(text.rectTransform, camera);
                    if (bounds.xMin < -1 || bounds.yMin < -1 || bounds.xMax > size.x + 1 || bounds.yMax > size.y + 1)
                        item.errors.Add(Hierarchy(text.transform) + ": visible HUD text crosses viewport bounds: " + bounds);
                }
                if (!string.IsNullOrWhiteSpace(text.text)) AuditMaskClipping(text.rectTransform, camera, "text", item.warnings);
            }
            if (hudOnly && item.activeTexts == 0) item.errors.Add("The earning HUD fixture contains no visible text.");
            if (name == "LocalEarningSequence" && item.activeTexts == 0)
                item.errors.Add("The requested sequence preview state contains no visible text.");
            if (saveImage)
            {
                RenderTexture.active = texture; pixels = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false);
                pixels.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0); pixels.Apply(false, false);
                string stateSuffix = string.IsNullOrEmpty(state) ? string.Empty : "--" + state;
                item.image = Path.Combine(outputDirectory ?? Output, "fixture-" + name + stateSuffix + "-" + size.x + "x" + size.y + ".png");
                File.WriteAllBytes(item.image, pixels.EncodeToPNG());
            }
        }
        catch (Exception exception) { item.errors.Add(exception.ToString()); }
        finally
        {
            RenderTexture.active = previous;
            if (cameraObject != null) cameraObject.GetComponent<Camera>().targetTexture = null;
            if (canvasObject != null) Object.DestroyImmediate(canvasObject);
            if (cameraObject != null) Object.DestroyImmediate(cameraObject);
            if (pixels != null) Object.DestroyImmediate(pixels);
            if (texture != null) { texture.Release(); Object.DestroyImmediate(texture); }
            EditorSceneManager.ClosePreviewScene(scene);
        }
        foreach (string error in item.errors) report.errors.Add(name + " [" + item.state + "] " + size + ": " + error);
        foreach (string warning in item.warnings) report.warnings.Add(name + " [" + item.state + "] " + size + ": " + warning);
    }

    private static void MeasureModalBounds(GameObject instance, Camera camera, Vector2Int size, RenderReport item)
    {
        var bounds = new ModalBoundsReport();
        item.modalBounds = bounds;
        ApkEarningPageView view = instance.GetComponent<ApkEarningPageView>();
        if (view != null)
        {
            bounds.referenceWidth = view.ReferenceWidth;
            bounds.minimumReferenceHeight = view.MinimumReferenceHeight;
            if (view.ArtworkRoot != null) bounds.artworkScale = view.ArtworkRoot.localScale.x;
        }
        Rect viewport = new Rect(0, 0, size.x, size.y);
        foreach (Image image in instance.GetComponentsInChildren<Image>(false))
        {
            Sprite sprite = image.overrideSprite != null ? image.overrideSprite : image.sprite;
            if (!image.isActiveAndEnabled || sprite == null || sprite.name != "CashWalletPanel" ||
                image.color.a * image.canvasRenderer.GetAlpha() * image.canvasRenderer.GetInheritedAlpha() <= .001f) continue;
            Rect frame = ScreenBounds(image.rectTransform, camera);
            Rect visible = IntersectBounds(frame, viewport);
            foreach (RectMask2D mask in image.GetComponentsInParent<RectMask2D>(false))
                if (mask.isActiveAndEnabled && mask.transform != image.transform)
                    visible = IntersectBounds(visible, ScreenBounds(mask.rectTransform, camera));
            foreach (Mask mask in image.GetComponentsInParent<Mask>(false))
                if (mask.isActiveAndEnabled && mask.transform != image.transform)
                    visible = IntersectBounds(visible, ScreenBounds(mask.rectTransform, camera));
            if (visible.width <= 0 || visible.height <= 0) continue;
            bounds.frameBounds = bounds.frameCount == 0 ? frame : UnionBounds(bounds.frameBounds, frame);
            bounds.visibleFrameBounds = bounds.frameCount == 0 ? visible : UnionBounds(bounds.visibleFrameBounds, visible);
            bounds.frameCount++;
            bounds.frames.Add(Hierarchy(image.transform));
            if (frame.xMin < -1 || frame.yMin < -1 || frame.xMax > size.x + 1 || frame.yMax > size.y + 1)
                item.errors.Add(Hierarchy(image.transform) + ": visible modal frame crosses viewport bounds: " + frame);
        }
        if (bounds.frameCount == 0)
            item.warnings.Add("No visible CashWalletPanel Image is available for modal coverage measurement.");
        bounds.widthRatio = bounds.frameBounds.width / size.x;
        bounds.heightRatio = bounds.frameBounds.height / size.y;
        bounds.visibleWidthRatio = bounds.visibleFrameBounds.width / size.x;
        bounds.visibleHeightRatio = bounds.visibleFrameBounds.height / size.y;
    }

    private static void AuditDiamondOptionOverlaps(ApkEarningPageView page, Camera camera,
        Vector2Int size, RenderReport item)
    {
        if (page == null) return;
        var bounds = new Rect[4];
        var visible = new bool[4];
        for (int i = 0; i < bounds.Length; i++)
        {
            Button button = page.GetButton("options[" + i + "]");
            if (button == null)
            {
                item.errors.Add("Cannot audit diamond option overlap; required Button is missing: options[" + i + "]");
                continue;
            }
            Graphic graphic = button.targetGraphic;
            if (!button.gameObject.activeInHierarchy || graphic == null || !graphic.isActiveAndEnabled ||
                graphic.canvasRenderer.cull ||
                graphic.color.a * graphic.canvasRenderer.GetAlpha() * graphic.canvasRenderer.GetInheritedAlpha() <= .001f) continue;
            // Measure the actual Button rectangle, never its label or a child
            // decoration. Disabled-but-visible options still occupy screen area.
            RectTransform rect = button.transform as RectTransform;
            if (rect == null)
            {
                item.errors.Add("Diamond option Button has no RectTransform: options[" + i + "]");
                continue;
            }
            bounds[i] = IntersectBounds(ScreenBounds(rect, camera), new Rect(0, 0, size.x, size.y));
            foreach (RectMask2D mask in button.GetComponentsInParent<RectMask2D>(false))
                if (mask.isActiveAndEnabled && mask.transform != button.transform)
                    bounds[i] = IntersectBounds(bounds[i], ScreenBounds(mask.rectTransform, camera));
            foreach (Mask mask in button.GetComponentsInParent<Mask>(false))
                if (mask.isActiveAndEnabled && mask.transform != button.transform)
                    bounds[i] = IntersectBounds(bounds[i], ScreenBounds(mask.rectTransform, camera));
            visible[i] = bounds[i].width > 0 && bounds[i].height > 0;
        }
        for (int first = 0; first < bounds.Length; first++)
            for (int second = first + 1; second < bounds.Length; second++)
            {
                if (!visible[first] || !visible[second]) continue;
                item.buttonOverlapChecks++;
                Rect overlap = IntersectBounds(bounds[first], bounds[second]);
                if (overlap.width > 0 && overlap.height > 0)
                    item.errors.Add("Visible diamond option Buttons overlap: options[" + first + "] / options[" + second +
                        "]; first=" + bounds[first] + "; second=" + bounds[second] + "; overlap=" + overlap +
                        ". Bounds use the actual Button RectTransforms.");
            }
    }

    private static void AuditModalTextOverlaps(ApkEarningPageView page, string name, Camera camera,
        Vector2Int size, RenderReport item)
    {
        if (page == null) return;
        if (name == "TXPanel")
        {
            AuditTextPair(page, "TestMode", "labels[0].Text", camera, size, item);
            foreach (string key in new[] { "Stats.PassCount", "Stats.ChallengeTimes", "Stats.GoldGetCount" })
                AuditTextPair(page, "Stats.Time", key, camera, size, item);
            AuditButtonTextPair(page, "TXBtn", "Stats.Time", camera, size, item);
        }
        else if (name == "LocalWithdrawalConditions")
        {
            foreach (ApkEarningPageView.TextSlot slot in page.Texts)
                if (slot != null && slot.Key != null && slot.Key.StartsWith("rows[", StringComparison.Ordinal) &&
                    slot.Key.EndsWith("].Name", StringComparison.Ordinal))
                    AuditTextPair(page, slot.Key, slot.Key.Substring(0, slot.Key.Length - 4) + "Value", camera, size, item);
        }
    }

    private static void AuditTextPair(ApkEarningPageView page, string firstKey, string secondKey,
        Camera camera, Vector2Int size, RenderReport item)
    {
        TMP_Text first = page.GetText(firstKey), second = page.GetText(secondKey);
        if (first == null || second == null)
        {
            item.errors.Add("Cannot audit modal text overlap; required bindings are missing: " + firstKey + " / " + secondKey);
            return;
        }
        if (!TryVisibleTextBounds(first, camera, size, out Rect firstBounds) ||
            !TryVisibleTextBounds(second, camera, size, out Rect secondBounds)) return;
        item.textOverlapChecks++;
        Rect overlap = IntersectBounds(firstBounds, secondBounds);
        if (overlap.width > 0 && overlap.height > 0)
            item.errors.Add("Visible TMP text overlaps: " + firstKey + " / " + secondKey +
                "; first=" + firstBounds + "; second=" + secondBounds + "; overlap=" + overlap +
                ". Bounds use rendered textBounds, not the RectTransform text areas.");
    }

    private static void AuditButtonTextPair(ApkEarningPageView page, string buttonKey, string textKey,
        Camera camera, Vector2Int size, RenderReport item)
    {
        Button button = page.GetButton(buttonKey);
        TMP_Text text = page.GetText(textKey);
        if (button == null || text == null)
        {
            item.errors.Add("Cannot audit modal Button/text overlap; required bindings are missing: " + buttonKey + " / " + textKey);
            return;
        }
        Graphic graphic = button.targetGraphic;
        if (!button.gameObject.activeInHierarchy || graphic == null || !graphic.isActiveAndEnabled ||
            graphic.canvasRenderer.cull ||
            graphic.color.a * graphic.canvasRenderer.GetAlpha() * graphic.canvasRenderer.GetInheritedAlpha() <= .001f ||
            !TryVisibleTextBounds(text, camera, size, out Rect textBounds)) return;
        RectTransform rect = button.transform as RectTransform;
        if (rect == null)
        {
            item.errors.Add("Modal Button has no RectTransform: " + buttonKey);
            return;
        }
        Rect buttonBounds = IntersectBounds(ScreenBounds(rect, camera), new Rect(0, 0, size.x, size.y));
        foreach (RectMask2D mask in button.GetComponentsInParent<RectMask2D>(false))
            if (mask.isActiveAndEnabled && mask.transform != button.transform)
                buttonBounds = IntersectBounds(buttonBounds, ScreenBounds(mask.rectTransform, camera));
        foreach (Mask mask in button.GetComponentsInParent<Mask>(false))
            if (mask.isActiveAndEnabled && mask.transform != button.transform)
                buttonBounds = IntersectBounds(buttonBounds, ScreenBounds(mask.rectTransform, camera));
        if (buttonBounds.width <= 0 || buttonBounds.height <= 0) return;
        item.buttonTextOverlapChecks++;
        Rect overlap = IntersectBounds(buttonBounds, textBounds);
        if (overlap.width > 0 && overlap.height > 0)
            item.errors.Add("Visible Button and TMP text overlap: " + buttonKey + " / " + textKey +
                "; button=" + buttonBounds + "; text=" + textBounds + "; overlap=" + overlap +
                ". Bounds use the Button RectTransform and rendered textBounds, not the text area.");
    }

    private static bool TryVisibleTextBounds(TMP_Text text, Camera camera, Vector2Int size, out Rect screenBounds)
    {
        screenBounds = default;
        if (!text.isActiveAndEnabled || string.IsNullOrWhiteSpace(text.text) || text.canvasRenderer.cull ||
            text.color.a * text.canvasRenderer.GetAlpha() * text.canvasRenderer.GetInheritedAlpha() <= .001f) return false;
        bool hasVisibleCharacter = false;
        for (int i = 0; i < text.textInfo.characterCount; i++)
            if (text.textInfo.characterInfo[i].isVisible) { hasVisibleCharacter = true; break; }
        if (!hasVisibleCharacter) return false;
        // ForceMeshUpdate already ran before camera.Render. TMP textBounds is the
        // actual local text extent; a large text RectTransform is not a collision.
        Bounds local = text.textBounds;
        if (local.size.x <= 0 || local.size.y <= 0) return false;
        Vector2 min = Vector2.zero, max = Vector2.zero;
        for (int corner = 0; corner < 4; corner++)
        {
            var point = new Vector3((corner & 1) == 0 ? local.min.x : local.max.x,
                (corner & 2) == 0 ? local.min.y : local.max.y, local.center.z);
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(camera, text.transform.TransformPoint(point));
            if (corner == 0) min = max = screen;
            else { min = Vector2.Min(min, screen); max = Vector2.Max(max, screen); }
        }
        screenBounds = IntersectBounds(Rect.MinMaxRect(min.x, min.y, max.x, max.y), new Rect(0, 0, size.x, size.y));
        foreach (RectMask2D mask in text.GetComponentsInParent<RectMask2D>(false))
            if (mask.isActiveAndEnabled && mask.transform != text.transform)
                screenBounds = IntersectBounds(screenBounds, ScreenBounds(mask.rectTransform, camera));
        foreach (Mask mask in text.GetComponentsInParent<Mask>(false))
            if (mask.isActiveAndEnabled && mask.transform != text.transform)
                screenBounds = IntersectBounds(screenBounds, ScreenBounds(mask.rectTransform, camera));
        return screenBounds.width > 0 && screenBounds.height > 0;
    }

    private static Rect IntersectBounds(Rect first, Rect second)
    {
        float left = Mathf.Max(first.xMin, second.xMin), bottom = Mathf.Max(first.yMin, second.yMin);
        return new Rect(left, bottom, Mathf.Max(0, Mathf.Min(first.xMax, second.xMax) - left),
            Mathf.Max(0, Mathf.Min(first.yMax, second.yMax) - bottom));
    }

    private static Rect UnionBounds(Rect first, Rect second)
    {
        return Rect.MinMaxRect(Mathf.Min(first.xMin, second.xMin), Mathf.Min(first.yMin, second.yMin),
            Mathf.Max(first.xMax, second.xMax), Mathf.Max(first.yMax, second.yMax));
    }

    private static string[] FixtureStates(string page)
    {
        switch (page)
        {
            case "LocalEarningSequence": return new[] { "", "activating", "activated", "summary", "receipt", "bonuses" };
            case "LocalWithdrawalConditions": return new[] { "", "diamond", "ready" };
            case "TXUserInfoPanel": return new[] { "", "email" };
            case "TXCoinPanel": return new[] { "", "target-reached" };
            case "LocalRewardVideoTest": return new[] { "", "complete" };
            case "LocalSuccess": return new[] { "", "early-reward" };
            default: return new[] { "" };
        }
    }

    // This is deliberately editor-only fixture setup. It fills existing saved views
    // and instantiates saved row/item prefabs, never the runtime service or a host.
    private static void ApplyPreviewState(ApkEarningPageView page, string name, string state)
    {
        if (page == null) throw new InvalidOperationException("Missing preview PageView: " + name);
        page.SetText("TestMode", "TEST MODE · NO CASH PAYMENT");
        switch (name)
        {
            case "TXPanel":
                page.SetText("GoldCount", "$500.79"); page.SetText("Title", "CASH WALLET");
                page.SetText("StageLabel", "Level 7"); page.SetText("labels[0].Text", "Available balance");
                page.SetText("Tip", "Clear level 22\nto review withdrawal"); page.SetText("TXText", "Withdraw");
                page.SetText("ProgressValue", "6 / 22"); page.SetText("ProgressTipValue", "16"); page.SetProgress("Progress", 6f / 22);
                page.SetText("Stats.Time", "Simulated stats");
                page.SetText("Stats.PassCount", "Players today\n13,110"); page.SetText("Stats.ChallengeTimes", "Avg. attempts\n8");
                page.SetText("Stats.GoldGetCount", "Avg. amount\n$32,500.00"); page.SetVisible("Level1", false); page.SetVisible("Level2", false);
                break;
            case "TXCoinPanel": PreviewDiamonds(page, state == "target-reached"); break;
            case "LocalWithdrawalConditions": PreviewConditions(page, state); break;
            case "LocalCashHistory": PreviewHistory(page); break;
            case "TXUserInfoPanel": PreviewAccount(page, state == "email"); break;
            case "TXUserInfoSurePanel":
                page.SetText("labels[0].Text", "Confirm account"); page.SetText("labels[1].Text", "Confirm");
                page.SetText("labels[2].Text", "Re-enter");
                page.SetText("Tip", "Preview Player\npreview@example.com\n\nTest request · no payment");
                PreviewSprite(page, "Image", "PayPalLarge"); break;
            case "TXChannelPanel":
                page.SetText("labels[0].Text", "Enter test payment account information. No payment is submitted.");
                page.SetText("labels[1].Text", "Withdrawal account"); page.SetText("labels[2].Text", "Confirm");
                PreviewInput(page, "InputInfo", "Preview Player\npreview@example.com"); break;
            case "LocalSuccess": case "SuccessPanel": case "LocalLuckyReward":
                PreviewReward(page, name == "LocalLuckyReward", state == "early-reward"); break;
            case "RewardPanel":
                var items = page.GetObject("items");
                if (items == null) items = page.GetObject("Items");
                if (items != null) { PreviewRewardItem(items.transform, false, false); PreviewRewardItem(items.transform, true, false); }
                break;
            case "LocalEarningSequence": PreviewSequence(page, state); break;
            case "TXGuideLoadingPanel":
                page.SetText("label", "Preparing withdrawal request..."); page.SetProgress("Progress", .65f); break;
            case "LocalWithdrawalVerification": case "TXLevelPanel":
                page.SetText("Tip", "Reward #1"); page.SetText("GoldCount", "$1.50");
                PreviewSteps(page, true); break;
            case "TXProgress0Panel": PreviewSteps(page, false); break;
            case "TXGuideTargetCompletePanel":
                page.SetText("labels[0].Text", "Withdraw"); page.SetText("labels[1].Text", "Withdrawal on Clear");
                page.SetText("tip", "$1.50"); break;
            case "LocalDiamondUnlockGuide":
                page.SetText("title", "Diamond rewards unlocked"); page.SetText("diamondAmount", "102");
                page.SetText("body", "Diamond rewards are unlocked. Tap here to review withdrawal requirements."); break;
            case "LocalEarningRecovery":
                page.SetText("title", "Local save unavailable");
                page.SetText("body", "Preview save error.\nYour last saved rewards are preserved. Retry saving, or return to the game and recover them on restart."); break;
            case "LocalRewardVideoTest":
                page.SetText("title", "Test reward video");
                page.SetText("body", state == "complete"
                    ? "Test video finished.\nTap Complete test video to collect 10X.\nCancel keeps the normal reward available."
                    : "Local test video · 5s\nNo ad network or payment is connected.\nComplete to collect 10X, or cancel without changing your reward.");
                page.SetText("actionCaption", "Complete test video"); page.SetText("cancelCaption", "Cancel");
                Button videoConfirm = page.GetButton("retry");
                if (videoConfirm != null) videoConfirm.interactable = state == "complete";
                break;
            case "LocalUserLevel":
                page.SetText("Title", "Account level"); page.SetText("levelLabel", "Level 0");
                page.SetText("progressLabel", "Awaiting account update"); page.SetText("ProgressCaption", "Account progress");
                page.SetText("Caption", "Continue"); page.SetProgress("progressFill", 0); break;
            case "TXTooFastHintPanel":
                page.SetText("labels[0].Text", "Complete the next level target to continue. Your current progress is shown below.");
                page.SetText("labels[1].Text", "Requirements review"); page.SetText("labels[2].Text", "Continue");
                page.SetText("LevelTip", "Complete level 51"); page.SetText("VideoTip", "Test reward videos completed");
                page.SetText("LevelPregressValue", "22 / 51"); page.SetText("VideoPregressValue", "0 / 10");
                page.SetProgress("LevelPregress", 22f / 51); page.SetProgress("VideoPregress", 0); break;
            case "Tip": page.SetText("content", "Reward video unavailable. Use Claim to collect."); break;
        }
    }

    private static void PreviewDiamonds(ApkEarningPageView page, bool reached)
    {
        page.SetText("labels[0].Text", "My Diamonds");
        page.SetText("CoinValue", reached ? "50,000" : "355.11"); page.SetText("GoldValue", reached ? "$500" : "$3.55");
        page.SetText("TipValue", "100=$1"); page.SetText("TXTip", "$500 Withdrawal Instructions:");
        page.SetText("IitemTip", reached ? "Balance target complete\nReview remaining withdrawal requirements" : "Balance reaches $500 to withdraw all\nStill $496.45 to go");
        page.SetText("TipBarValue", reached ? "$500 / $500" : "$3.55 / $500"); page.SetProgress("TipBar", reached ? 1 : 355.11f / 50000);
        string[] amounts = { "$500", "$1,000", "$1,500", "$2,000" };
        for (int i = 0; i < amounts.Length; i++)
        {
            page.SetText("amounts[" + i + "]", amounts[i]); page.SetVisible("selectionFrames[" + i + "]", i == 0);
            Button button = page.GetButton("options[" + i + "]");
            var graphic = button == null ? null : button.targetGraphic as Image;
            if (graphic != null)
            {
                if (page.TintDiamondOptions) graphic.color = i == 0 ? page.SelectedOptionColor : page.NormalOptionColor;
                else { Sprite sprite = LoadPreviewSprite(i == 0 ? "DiamondOptionSelected" : "DiamondOptionNormal"); if(sprite != null) graphic.sprite = sprite; }
            }
        }
    }

    private static void PreviewConditions(ApkEarningPageView page, string state)
    {
        bool diamond = state == "diamond", ready = state == "ready";
        page.SetText("title", "Withdrawal requirements"); page.SetText("subtitle", diamond ? "Diamond withdrawal" : "Cash withdrawal");
        page.SetText("notice", ready ? "Requirements complete. Confirm your payment details." : "Complete the requirements below to continue.");
        page.SetText("actionCaption", ready ? "Enter payment details" : "Continue playing");
        string[] names = diamond
            ? new[] { "Required balance", "Levels completed today", "Login days", "Account level" }
            : new[] { "Complete level 22", "Complete level 51", "Required balance", "Complete cash guidance", "Levels completed today", "Login days", "Account level" };
        string[] values = diamond ? new[] { "355.11 / 50,000", "6 / 10", "2 / 5", "0 / 31" }
            : new[] { "6 / 22 boards", "6 / 51 boards", "Awaiting account update", "0 / 1", "6 / 10", "2 / 5", "0 / 31" };
        for (int i = 0; i < 7; i++)
        {
            string key = "rows[" + i + "]"; page.SetVisible(key + ".Root", i < names.Length);
            if (i >= names.Length) continue;
            page.SetText(key + ".Name", names[i]); page.SetText(key + ".Value", ready ? "Complete" : values[i]);
            page.SetText(key + ".Status", ready ? "Complete" : "In progress");
        }
    }

    private static void PreviewHistory(ApkEarningPageView page)
    {
        page.SetText("titleLabel", "Withdrawal History"); page.SetText("balanceLabel", "$3.15");
        GameObject parent = page.GetObject("rowParent");
        if (parent == null) throw new InvalidOperationException("History preview has no rowParent.");
        for (int i = 0; i < 3; i++)
        {
            ApkEarningPageView row = InstantiatePreview("LocalCashHistoryRow", parent.transform);
            bool pending = i == 0;
            row.SetText("titleLabel", "Cash Reward #" + (i + 1));
            row.SetText("timeLabel", i < 2 ? "10/16/2026 09:30" : string.Empty);
            row.SetText("amountLabel", i == 2 ? "$3.15" : "$1.50"); row.SetText("actionLabel", "Withdraw");
            row.SetText("statusLabel", "Pending - not paid"); row.SetVisible("readyVisual", !pending);
            row.SetVisible("pendingVisual", pending); row.SetVisible("statusLabel", pending);
            row.SetVisible("actionButton", !pending);
            Button action = row.GetButton("actionButton");
            if (action != null) { action.interactable = !pending; action.gameObject.SetActive(!pending); }
        }
    }

    private static void PreviewAccount(ApkEarningPageView page, bool email)
    {
        page.SetText("labels[0].Text", "Use test account details. No payment is submitted.");
        page.SetText("labels[1].Text", "Withdrawal account"); page.SetText("labels[2].Text", "Confirm");
        PreviewInput(page, "InputName", "Preview Player"); PreviewInput(page, "InputEmail", "preview@example.com");
        PreviewInput(page, "InputAreaNumber", "+1"); PreviewInput(page, "InputNumber", "2025550123");
        foreach (string key in new[] { "InputEmail", "InputAreaNumber", "InputNumber" })
        {
            TMP_InputField input = page.GetInput(key);
            if (input != null) input.gameObject.SetActive(key == "InputEmail" ? email : !email);
        }
        string[] icons = { "VenmoSmall", "ZelleSmall", "PayPalSmall" };
        for (int i = 0; i < icons.Length; i++)
        {
            PreviewSprite(page, "channels[" + i + "].Icon", icons[i]);
            page.SetVisible("channels[" + i + "].Checkmark", i == (email ? 2 : 0));
        }
    }

    private static void PreviewReward(ApkEarningPageView page, bool lucky, bool early)
    {
        page.SetText("labels[2].Text", lucky ? "Lucky Reward" : "Level Complete");
        page.SetText("moreLabel", early ? "Withdraw" : "Claim 10X"); page.SetText("labels[1].Text", "Claim");
        page.SetVisible("ad", !early); page.SetVisible("MoreGetBtn", true); page.SetVisible("GetBtn", !early); page.SetVisible("Close", false);
        GameObject parent = page.GetObject("Items");
        if (parent == null) throw new InvalidOperationException("Reward preview has no Items binding.");
        PreviewRewardItem(parent.transform, false, lucky);
        if (!early) PreviewRewardItem(parent.transform, true, lucky);
    }

    private static void PreviewRewardItem(Transform parent, bool diamond, bool lucky)
    {
        ApkEarningPageView item = InstantiatePreview(lucky ? "LocalLuckyItem" : "Item_Big", parent);
        item.SetText("count", diamond ? "102" : "$1.50"); item.SetVisible("max", false);
        Image icon = item.GetImage("icon"); Sprite art = LoadPreviewSprite(diamond ? "DiamondPile" : "CashPile");
        if (icon != null && art != null) { icon.sprite = art; icon.preserveAspect = true; }
    }

    private static void PreviewSequence(ApkEarningPageView page, string state)
    {
        bool receipt = state == "receipt", bonuses = state == "bonuses";
        int index = state == "activating" ? 1 : state == "activated" ? 2 : state == "summary" ? 3 : 0;
        page.SetVisible("messageGroup", !receipt && !bonuses); page.SetVisible("receiptGroup", receipt); page.SetVisible("bonusesGroup", bonuses);
        string[] titles = { "Reward assessment", "Activating rewards", "Rewards activated", "Your withdrawal history" };
        string[] bodies = { "Your first two cash rewards are saved in withdrawal history.", "Preparing your cash rewards and level bonuses...", "Continue playing to collect cash and unlock more rewards.", "Your reward records are saved. Submitted requests remain pending; no payment has been made." };
        for (int i = 0; i < 4; i++)
        {
            page.SetVisible("messageLayouts[" + i + "].Root", !receipt && !bonuses && i == index);
            page.SetText("messageLayouts[" + i + "].Title", titles[i]); page.SetText("messageLayouts[" + i + "].Body", bodies[i]);
        }
        page.SetText("receiptAmount", "$3.15"); page.SetText("bonusesTitle", "Your reward bonuses");
        string[] captions = { "Extra cash reward", "Match three fruits to earn a reward", "Test reward video bonus", "Unlock diamond rewards", "Withdrawal requirements" };
        for (int i = 0; i < captions.Length; i++)
        {
            page.SetText("bonusCaptions[" + i + "]", captions[i]);
            GameObject row = page.GetObject("bonusRows[" + i + "]");
            if (row != null) { row.SetActive(true); CanvasGroup group = row.GetComponent<CanvasGroup>(); if (group != null) group.alpha = 1; }
            page.SetVisible("flightIcons[" + i + "]", false);
        }
    }

    private static void PreviewSteps(ApkEarningPageView page, bool verification)
    {
        page.SetText("labels[0].Text", verification ? "Withdrawal request" : "Withdrawal progress");
        page.SetText("labels[1].Text", verification ? "Confirm withdrawal" : "Back to withdrawal");
        page.SetText("labels[2].Text", verification ? "Current status" : "Complete the next stage requirements");
        page.SetText("labels[3].Text", verification ? "Reward recorded" : "First level target completed");
        page.SetText("labels[4].Text", verification ? "Account details entered" : "Next level target pending");
        page.SetText("labels[5].Text", verification ? "Ready to submit" : "Review remaining requirements before applying");
        if (!verification)
        {
            page.SetText("Tip", "Complete level 51 to continue the next stage");
            page.SetText("Des", "Complete the next level target to continue."); page.SetText("KeFu", "Application not submitted");
        }
        for (int i = 1; i <= (verification ? 3 : 4); i++)
        {
            string key = "path:main/Step/" + i;
            page.SetVisible(key + "/Done", verification || i == 1); page.SetVisible(key + "/Loading", !verification && i > 1);
            page.SetText(key + "/Content/Time", string.Empty);
        }
    }

    private static ApkEarningPageView InstantiatePreview(string prefab, Transform parent)
    {
        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(PanelFolder + prefab + ".prefab");
        if (source == null) throw new InvalidOperationException("Missing fixture child prefab: " + prefab);
        GameObject child = Object.Instantiate(source, parent, false); child.name = prefab + "Preview";
        ApkEarningPageView view = child.GetComponent<ApkEarningPageView>();
        if (view == null) throw new InvalidOperationException("Fixture child lacks PageView: " + prefab);
        return view;
    }

    private static void PreviewInput(ApkEarningPageView page, string key, string value)
    { TMP_InputField input = page.GetInput(key); if (input != null) input.SetTextWithoutNotify(value); }

    private static void PreviewSprite(ApkEarningPageView page, string key, string name)
    { Image image = page.GetImage(key); Sprite sprite = LoadPreviewSprite(name); if (image != null && sprite != null) image.sprite = sprite; }

    private static Sprite LoadPreviewSprite(string name)
    {
        string root = "Assets/ApkEarningReference/Resources/ApkEarningReference/Artwork/" + name;
        foreach (string extension in new[] { ".asset", ".png" })
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(root + extension))
                if (asset is Sprite sprite) return sprite;
        return null;
    }

    private static void AuditMaskClipping(RectTransform target, Camera camera, string kind, List<string> warnings)
    {
        Rect bounds = ScreenBounds(target, camera);
        foreach (RectMask2D mask in target.GetComponentsInParent<RectMask2D>(false))
            if (mask.isActiveAndEnabled && mask.transform != target)
                AuditClipBounds(target, bounds, ScreenBounds(mask.rectTransform, camera), kind, warnings);
        foreach (Mask mask in target.GetComponentsInParent<Mask>(false))
            if (mask.isActiveAndEnabled && mask.transform != target)
                AuditClipBounds(target, bounds, ScreenBounds(mask.rectTransform, camera), kind, warnings);
    }

    private static void AuditClipBounds(RectTransform target, Rect bounds, Rect clip, string kind, List<string> warnings)
    {
        if (bounds.xMax <= clip.xMin || bounds.xMin >= clip.xMax || bounds.yMax <= clip.yMin || bounds.yMin >= clip.yMax)
            warnings.Add(Hierarchy(target) + ": preview " + kind + " is completely outside an ancestor mask.");
        else if (bounds.xMin < clip.xMin - 1 || bounds.yMin < clip.yMin - 1 || bounds.xMax > clip.xMax + 1 || bounds.yMax > clip.yMax + 1)
            warnings.Add(Hierarchy(target) + ": preview " + kind + " rectangle crosses an ancestor mask; inspect intended clipping.");
    }

    private static void KeepHeader(Transform root)
    {
        Transform top = root.Find("UI/Top");
        if (top == null) throw new InvalidOperationException("CorePlayUI header is missing.");
        Transform hud = root.Find("ApkEarningTop");
        if (hud == null)
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                if (child.name == "ApkEarningTop" || child.name == "WithdrawalHud" || child.name == "ApkEarningHud") { hud = child; break; }
        if (hud == null) throw new InvalidOperationException("CorePlayUI earning HUD is missing.");
        hud.gameObject.SetActive(true);
        KeepBranches(root, top, hud);
    }
    private static void ApplyHeaderPreviewState(Transform root)
    {
        // CorePlayUI.UpdateText selects the normal badge at runtime. Its three
        // variants are saved inactive, so a controller-free fixture must select
        // that same representative variant explicitly. Opening animations also
        // fade these native controls; preview their settled visible state only.
        Transform level = root.Find("UI/Top/Level");
        if (level == null) throw new InvalidOperationException("CorePlayUI level badge is missing.");
        level.gameObject.SetActive(true);
        foreach (string name in new[] { "Bg_Normal", "Bg_Hard", "Bg_SuperHard" })
        {
            Transform variant = level.Find(name);
            if (variant == null) throw new InvalidOperationException("CorePlayUI level badge variant is missing: " + name);
            variant.gameObject.SetActive(name == "Bg_Normal");
        }
        CanvasGroup levelGroup = level.GetComponent<CanvasGroup>();
        if (levelGroup != null) levelGroup.alpha = 1;
        Transform settings = root.Find("UI/Top/SettingBtn");
        if (settings == null) throw new InvalidOperationException("CorePlayUI settings button is missing.");
        settings.gameObject.SetActive(true);
        CanvasGroup settingsGroup = settings.GetComponent<CanvasGroup>();
        if (settingsGroup != null) settingsGroup.alpha = 1;
    }
    private static void AuditCompactHeaderAlignment(Transform root, Camera camera, List<string> errors)
    {
        ApkEarningTopView controller = root.GetComponentInChildren<ApkEarningTopView>(true);
        if (controller == null || !controller.CompactHud) return;
        RectTransform level = root.Find("UI/Top/Level") as RectTransform;
        RectTransform settings = root.Find("UI/Top/SettingBtn") as RectTransform;
        RectTransform cash = controller.transform.Find("CompactHud/Cash") as RectTransform;
        RectTransform diamonds = controller.transform.Find("CompactHud/Diamonds") as RectTransform;
        if (level == null || settings == null || cash == null || diamonds == null)
        {
            errors.Add("Compact header alignment requires level, cash, diamonds and settings rectangles.");
            return;
        }
        float levelY = ScreenBounds(level, camera).center.y;
        float settingsY = ScreenBounds(settings, camera).center.y;
        float cashY = ScreenBounds(cash, camera).center.y;
        float diamondsY = ScreenBounds(diamonds, camera).center.y;
        float minY = Mathf.Min(Mathf.Min(levelY, settingsY), Mathf.Min(cashY, diamondsY));
        float maxY = Mathf.Max(Mathf.Max(levelY, settingsY), Mathf.Max(cashY, diamondsY));
        if (maxY - minY > 3)
            errors.Add("Compact header centers differ by more than 3 screen pixels: level=" + levelY +
                ", cash=" + cashY + ", diamonds=" + diamondsY + ", settings=" + settingsY +
                ". Check nested earning-prefab RectTransform anchor overrides.");
    }
    private static void KeepBranches(Transform parent, Transform top, Transform hud)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child == top || child == hud) continue;
            if (top.IsChildOf(child) || (hud != null && hud.IsChildOf(child))) KeepBranches(child, top, hud);
            else child.gameObject.SetActive(false);
        }
    }
    private static void AddBackground(Transform parent, Vector2Int size)
    {
        Texture2D background = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/res_server/server_maps/map1008/Map1008.png");
        if (background == null) throw new InvalidOperationException("Original fruit background is missing.");
        var node = new GameObject("FruitBackgroundFixture", typeof(RectTransform), typeof(RawImage)); node.transform.SetParent(parent, false);
        var rect = (RectTransform)node.transform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
        RawImage image = node.GetComponent<RawImage>(); image.texture = background; image.raycastTarget = false;
        float imageAspect = (float)background.width / background.height, viewportAspect = (float)size.x / size.y;
        float width = Mathf.Min(1, viewportAspect / imageAspect), height = Mathf.Min(1, imageAspect / viewportAspect);
        image.uvRect = new Rect((1 - width) * .5f, (1 - height) * .5f, width, height);
    }
    private static Rect ScreenBounds(RectTransform rect, Camera camera)
    {
        var corners = new Vector3[4]; rect.GetWorldCorners(corners);
        Vector2 min = camera.WorldToScreenPoint(corners[0]), max = min;
        for (int i = 1; i < 4; i++) { Vector2 point = camera.WorldToScreenPoint(corners[i]); min = Vector2.Min(min, point); max = Vector2.Max(max, point); }
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }
    private static string Hierarchy(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null) { transform = transform.parent; path = transform.name + "/" + path; }
        return path;
    }
}
