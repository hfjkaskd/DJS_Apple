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
    private const string HostPath = "Assets/res/local/withdrawal/ApkEarningHost.prefab";
    private const string Output = "E:/CodexCacheBackups/DJS_Apple-apk-validation-20261014";
    private static readonly string[] Pages = { "TXPanel", "TXCoinPanel", "LocalWithdrawalConditions", "LocalCashHistory", "LocalEarningSequence" };
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
        public string page, image;
        public int width, height, activeButtons, activeTexts;
        public float canvasScale;
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
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

    [MenuItem("Tools/Apk Earning/Validate and Render Saved Fixtures")]
    public static void RenderFixtures()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before rendering saved prefab fixtures.");
        Directory.CreateDirectory(Output);
        var report = new Report {
            generatedUtc = DateTime.UtcNow.ToString("O"),
            scope = "Saved prefab visual fixtures, not live gameplay or APK device screenshots. Imported earning prefabs, earning children inside CorePlayUI, and the modal host are audited strictly. Unassigned fonts on legacy CorePlayUI text outside earning views are reported separately as warnings; their runtime assignment is outside this fixture. Static authored labels/states are preserved. Runtime accounting, host navigation and player storage are never initialized. Production PageView.RefreshLayout and Game.unity Expand scaling are reused."
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
            foreach (string page in Pages) RenderFixture(page, PanelFolder + page + ".prefab", size, false, report);
        }
        File.WriteAllText(Path.Combine(Output, "prefab-fixtures-report.json"), JsonUtility.ToJson(report, true));
        if (report.errors.Count > 0)
            throw new InvalidOperationException("APK earning fixture validation found " + report.errors.Count + " errors. See " + Output);
        Debug.Log("[ApkEarningValidation] Passed " + report.pages.Count + " prefab audits and " + report.renders.Count + " fixture renders. " + Output);
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

    private static void RenderFixture(string name, string path, Vector2Int size, bool hudOnly, Report report)
    {
        var item = new RenderReport { page = name, width = size.x, height = size.y };
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
            // The inactive ancestor prevents runtime accounting/host initialization. Only the shared layout view survives.
            foreach (MonoBehaviour component in instance.GetComponentsInChildren<MonoBehaviour>(true))
                if (component != null && !(component is UIBehaviour) && !(component is ApkEarningPageView)) Object.DestroyImmediate(component);
            foreach (Animation animation in instance.GetComponentsInChildren<Animation>(true)) animation.enabled = false;
            foreach (Animator animator in instance.GetComponentsInChildren<Animator>(true)) animator.enabled = false;
            if (hudOnly) KeepHeader(instance.transform);
            canvasObject.SetActive(true); instance.SetActive(true);
            Canvas.ForceUpdateCanvases();
            foreach (ApkEarningPageView view in instance.GetComponentsInChildren<ApkEarningPageView>(true)) view.RefreshLayout();
            foreach (TMP_Text text in instance.GetComponentsInChildren<TMP_Text>(false)) text.ForceMeshUpdate(true, true);
            Canvas.ForceUpdateCanvases(); camera.Render();
            foreach (Button button in instance.GetComponentsInChildren<Button>(false))
            {
                item.activeButtons++; Rect bounds = ScreenBounds((RectTransform)button.transform, camera);
                if (bounds.width < .5f || bounds.height < .5f) item.warnings.Add(Hierarchy(button.transform) + ": static Button has no visible size.");
                else if (bounds.xMax < 0 || bounds.xMin > size.x || bounds.yMax < 0 || bounds.yMin > size.y)
                    item.warnings.Add(Hierarchy(button.transform) + ": authored state keeps Button offscreen; runtime routing must hide or position it.");
                else if (bounds.xMin < -1 || bounds.yMin < -1 || bounds.xMax > size.x + 1 || bounds.yMax > size.y + 1)
                    item.errors.Add(Hierarchy(button.transform) + ": visible Button crosses viewport bounds: " + bounds);
            }
            foreach (TMP_Text text in instance.GetComponentsInChildren<TMP_Text>(false))
            {
                item.activeTexts++;
                if (text.isTextOverflowing) item.warnings.Add(Hierarchy(text.transform) + ": authored placeholder overflows; inspect live text separately.");
            }
            if (hudOnly && item.activeTexts == 0) item.errors.Add("The earning HUD fixture contains no visible text.");
            if (name == "LocalEarningSequence" && item.activeTexts == 0)
                item.warnings.Add("Sequence groups are inactive in the saved prefab. This fixture audits their references only; actual sequence appearance requires a runtime capture.");
            RenderTexture.active = texture; pixels = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false);
            pixels.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0); pixels.Apply(false, false);
            item.image = Path.Combine(Output, "fixture-" + name + "-" + size.x + "x" + size.y + ".png");
            File.WriteAllBytes(item.image, pixels.EncodeToPNG());
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
        foreach (string error in item.errors) report.errors.Add(name + " " + size + ": " + error);
        foreach (string warning in item.warnings) report.warnings.Add(name + " " + size + ": " + warning);
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
