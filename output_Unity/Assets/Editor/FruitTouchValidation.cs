using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
/// Isolated saved-prefab touch regression checks. Uses UGUI's own touch press,
/// move, drag and release dispatch, without initializing gameplay or player saves.
/// </summary>
public static class FruitTouchValidation
{
    private const string PrefabPath = "Assets/res/local/coreplay/prefab/CollectItem.prefab";
    private const int Width = 1080;
    private const int Height = 2400;

    [Serializable]
    private sealed class Report
    {
        public string generatedUtc;
        public string scope = "Temporary instance of the saved fruit prefab in an isolated preview scene. " +
            "Real GraphicRaycaster and StandaloneInputModule touch dispatch; no player save, " +
            "gameplay initialization, scene saving or device interaction. Positive gesture cases " +
            "count Button events using a fixture listener. Guard cases retain the production fruit callback. " +
            "Does not claim device input, successful gameplay collection or runtime listener-count inspection.";
        public List<string> passed = new List<string>();
        public List<string> errors = new List<string>();
        public List<string> diagnostics = new List<string>();
    }

    [MenuItem("Tools/Validation/Fruit touch and pooled button bindings")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play mode before checking the saved fruit prefab.");
        if (MCCIJBJGMCK.IsLock())
            throw new InvalidOperationException("A live input lock exists. This check will not reset it.");

        var report = new Report { generatedUtc = DateTime.UtcNow.ToString("O") };
        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Require(source != null, "Saved fruit prefab is missing.");
        AuditPrefab(source);
        report.passed.Add("Saved prefab owns one standard Button on the visible fruit Image, with no drag handler or Inspector event.");

        Scene preview = EditorSceneManager.NewPreviewScene();
        EventSystem previousEventSystem = EventSystem.current;
        FruitTouchFixtureEventSystem fixtureEventSystem = null;
        RenderTexture target = null;
        Application.LogCallback collectErrors = (message, stack, type) =>
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                report.errors.Add(message);
        };
        Application.logMessageReceived += collectErrors;
        try
        {
            GameObject cameraObject = CreateObject("Fruit touch validation camera", preview);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false;
            camera.scene = preview;
            camera.orthographic = true;
            camera.orthographicSize = Height * .5f;
            camera.transform.position = new Vector3(0, 0, -100);
            camera.nearClipPlane = .1f;
            camera.farClipPlane = 200;
            target = new RenderTexture(Width, Height, 0);
            target.Create();
            camera.targetTexture = target;
            camera.aspect = Width / (float)Height;

            GameObject canvasObject = CreateObject("Fruit touch validation canvas", preview, typeof(RectTransform));
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = camera;
            ((RectTransform)canvas.transform).sizeDelta = new Vector2(Width, Height);
            GraphicRaycaster raycaster = canvasObject.AddComponent<GraphicRaycaster>();
            raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;

            GameObject events = CreateObject("Fruit touch validation events", preview);
            fixtureEventSystem = events.AddComponent<FruitTouchFixtureEventSystem>();
            fixtureEventSystem.InitializeFixture();
            EventSystem eventSystem = fixtureEventSystem;
            // Matches the saved Game.unity value that caused tiny movements to be treated as drag.
            eventSystem.pixelDragThreshold = 10;
            FruitTouchFixtureInputModule input = events.AddComponent<FruitTouchFixtureInputModule>();
            input.InitializeFixture();
            EventSystem.current = eventSystem;

            GameObject instance = Object.Instantiate(source, canvas.transform, false);
            instance.name = "Saved fruit fixture";
            instance.hideFlags = HideFlags.HideAndDontSave;
            CollectItem item = instance.GetComponent<CollectItem>();
            FruitCollectButton button = instance.GetComponentInChildren<FruitCollectButton>(true);
            Require(item != null && button != null, "Fruit fixture components are missing.");
            instance.SetActive(true);
            ((RectTransform)instance.transform).anchoredPosition = Vector2.zero;
            PrepareGraphics(canvas, camera);
            var fruitRect = (RectTransform)button.transform;
            Vector2 center = RectTransformUtility.WorldToScreenPoint(camera, fruitRect.TransformPoint(fruitRect.rect.center));
            Vector2 nearby = center + Vector2.right * 15;
            Vector2 outside = new Vector2(Width - 10, Height - 10);
            string centerDiagnostic = DescribeRaycast(input, raycaster, eventSystem, center, button.targetGraphic);
            report.diagnostics.Add(centerDiagnostic);
            Require(input.Hit(raycaster, eventSystem, center).gameObject == button.gameObject,
                "The real prefab fruit Image does not receive the center raycast. " + centerDiagnostic);
            Require(input.Hit(raycaster, eventSystem, nearby).gameObject == button.gameObject,
                "The 15-pixel endpoint is not inside the same visible fruit Image.");
            Require(ExecuteEvents.GetEventHandler<IPointerClickHandler>(
                input.Hit(raycaster, eventSystem, outside).gameObject) != button.gameObject,
                "The outside endpoint unexpectedly hits the fruit.");

            item.UnBindClick();
            int clicks = 0;
            button.onClick.AddListener(() => clicks++);
            input.Gesture(raycaster, eventSystem, center, center);
            Require(clicks == 1, "A stationary fruit touch must emit one click.");
            report.passed.Add("Stationary touch on the saved fruit Button emits exactly one click.");
            input.Gesture(raycaster, eventSystem, center, nearby);
            Require(clicks == 2, "A 15-pixel touch movement within the fruit must emit one further click.");
            report.passed.Add("15px movement inside the same real fruit Image emits exactly one standard Button click.");
            input.Gesture(raycaster, eventSystem, center, outside);
            Require(clicks == 2, "Releasing outside the pressed fruit must not click it.");
            report.passed.Add("Pressing fruit then releasing outside it emits no click.");

            // Baseline control uses the unchanged original InputMono and the same
            // UGUI module/10px threshold. It must reproduce the reported failure.
            GameObject legacy = CreateObject("Original InputMono baseline", preview, typeof(RectTransform));
            legacy.transform.SetParent(canvas.transform, false);
            var legacyRect = (RectTransform)legacy.transform;
            legacyRect.sizeDelta = new Vector2(160, 160);
            legacyRect.anchoredPosition = new Vector2(-300, 0);
            legacy.AddComponent<Image>();
            InputMono oldInput = legacy.AddComponent<InputMono>();
            oldInput.playClickSound = false;
            int oldClicks = 0;
            oldInput.onClick = ignored => oldClicks++;
            PrepareGraphics(canvas, camera);
            Vector2 oldCenter = RectTransformUtility.WorldToScreenPoint(camera, legacyRect.TransformPoint(legacyRect.rect.center));
            report.diagnostics.Add(DescribeRaycast(input, raycaster, eventSystem, oldCenter, legacy.GetComponent<Image>()));
            input.Gesture(raycaster, eventSystem, oldCenter, oldCenter, true);
            Require(oldClicks == 1, "The original InputMono stationary baseline did not click.");
            input.Gesture(raycaster, eventSystem, oldCenter, oldCenter + Vector2.right * 15, true);
            Require(oldClicks == 1, "The original InputMono did not reproduce the 15px cancelled click.");
            report.passed.Add("Original InputMono control: stationary touch clicks; the same 15px movement within its Image is discarded.");
            legacy.SetActive(false);

            // This observes reuse and unbinding, not private UnityEvent listener
            // counts. Bind's RemoveListener/AddListener symmetry is reviewed separately.
            button.onClick.RemoveAllListeners();
            for (int cycle = 0; cycle < 8; cycle++)
            {
                item.BindClick();
                item.BindClick();
                item.UnBindClick();
                instance.SetActive(false);
                instance.SetActive(true);
            }
            int pooledClicks = 0;
            button.onClick.AddListener(() => pooledClicks++);
            PrepareGraphics(canvas, camera);
            input.Gesture(raycaster, eventSystem, center, nearby);
            Require(pooledClicks == 1, "Pool-style reuse must emit a single fixture click after unbinding.");
            Require(instance.GetComponentsInChildren<InputMono>(true).Length == 0,
                "Binding recreated the legacy drag handler.");
            report.passed.Add("Eight bind/unbind and enable cycles preserve one Button click per gesture and do not recreate a legacy drag component. Listener idempotence is a separate static review.");

            button.onClick.RemoveAllListeners();
            item.BindClick();
            int deliveredGuardedClicks = 0;
            button.onClick.AddListener(() => deliveredGuardedClicks++);
            item.Status = LECIONHKEJL.InCollect;
            input.Gesture(raycaster, eventSystem, center, nearby);
            Require(deliveredGuardedClicks == 1 && item.Status == LECIONHKEJL.InCollect,
                "A non-field fruit changed state or aborted guarded event dispatch.");
            report.passed.Add("Non-OnField production callback returns without entering gameplay.");

            item.Status = LECIONHKEJL.OnField;
            MCCIJBJGMCK.Lock();
            try
            {
                input.Gesture(raycaster, eventSystem, center, nearby);
                Require(item.Status == LECIONHKEJL.OnField,
                    "A globally locked fruit entered gameplay.");
            }
            finally { MCCIJBJGMCK.UnlockOnce(); }
            Require(deliveredGuardedClicks == 2, "Locked guard case did not receive its Button event.");
            report.passed.Add("Global lock at press prevents the production callback from entering gameplay.");

            // A popup/transition can acquire a lock between touch down and release.
            PointerEventData pending = input.Press(raycaster, eventSystem, center);
            MCCIJBJGMCK.Lock();
            try
            {
                input.Release(raycaster, pending, nearby);
                Require(item.Status == LECIONHKEJL.OnField,
                    "A lock acquired during a touch did not block collection.");
            }
            finally { MCCIJBJGMCK.UnlockOnce(); }
            Require(deliveredGuardedClicks == 3, "Mid-touch lock guard did not receive its Button event.");
            report.passed.Add("Global lock acquired after press also prevents collection on release.");
            item.UnBindClick();
        }
        catch (Exception exception) { report.errors.Add(exception.ToString()); }
        finally
        {
            Application.logMessageReceived -= collectErrors;
            if (fixtureEventSystem != null) fixtureEventSystem.ReleaseFixture();
            EditorSceneManager.ClosePreviewScene(preview);
            if (previousEventSystem != null) EventSystem.current = previousEventSystem;
            if (target != null)
            {
                target.Release();
                Object.DestroyImmediate(target);
            }
        }

        string output = Environment.GetEnvironmentVariable("DJS_APPLE_VALIDATION_OUTPUT");
        if (!string.IsNullOrEmpty(output))
        {
            Directory.CreateDirectory(output);
            File.WriteAllText(Path.Combine(output, "fruit-touch-results.json"), JsonUtility.ToJson(report, true));
        }
        if (report.errors.Count != 0)
            throw new InvalidOperationException("Fruit touch validation failed: " + string.Join("\n", report.errors));
        Debug.Log("FRUIT_TOUCH_PASS cases=" + report.passed.Count + " real_prefab=true real_ugui_touch_dispatch=true no_player_storage=true");
    }

    private static void AuditPrefab(GameObject source)
    {
        FruitCollectButton[] buttons = source.GetComponentsInChildren<FruitCollectButton>(true);
        Require(buttons.Length == 1, "Fruit prefab must contain exactly one FruitCollectButton.");
        FruitCollectButton button = buttons[0];
        Image graphic = button.GetComponent<Image>();
        Require(graphic != null && button.targetGraphic == graphic && graphic.raycastTarget,
            "Fruit Button must use its own visible Image as the raycast target.");
        Require(button.enabled && button.interactable, "Saved fruit Button is disabled.");
        Require(button.onClick.GetPersistentEventCount() == 0, "Bind fruit events in code, not the Inspector.");
        Require(source.GetComponentsInChildren<InputMono>(true).Length == 0,
            "Legacy InputMono remains in the fruit prefab.");
        foreach (MonoBehaviour component in source.GetComponentsInChildren<MonoBehaviour>(true))
            Require(!(component is IDragHandler) && !(component is IBeginDragHandler),
                "The fruit prefab must not register a drag handler.");
    }

    private static GameObject CreateObject(string name, Scene scene, params Type[] components)
    {
        var value = new GameObject(name, components) { hideFlags = HideFlags.HideAndDontSave };
        SceneManager.MoveGameObjectToScene(value, scene);
        return value;
    }

    private static void PrepareGraphics(Canvas canvas, Camera camera)
    {
        ((RectTransform)canvas.transform).ForceUpdateRectTransforms();
        Canvas.ForceUpdateCanvases();
        // GraphicRaycaster rejects depth == -1. A real render initializes the
        // preview's CanvasRenderer depths; do not fake a hit or bypass that filter.
        camera.Render();
        Canvas.ForceUpdateCanvases();
    }

    private static string DescribeRaycast(FruitTouchFixtureInputModule input, GraphicRaycaster raycaster,
        EventSystem owner, Vector2 position, Graphic target)
    {
        Camera camera = raycaster.eventCamera;
        Canvas canvas = raycaster.GetComponent<Canvas>();
        var description = new StringBuilder();
        description.Append("position=").Append(position)
            .Append(" screen=").Append(Screen.width).Append('x').Append(Screen.height)
            .Append(" graphicsDevice=").Append(SystemInfo.graphicsDeviceType)
            .Append(" raycaster=").Append(raycaster.name)
            .Append(" enabled=").Append(raycaster.isActiveAndEnabled)
            .Append(" registeredGraphics=").Append(GraphicRegistry.GetRaycastableGraphicsForCanvas(canvas).Count)
            .Append(" eventCamera=").Append(camera != null ? camera.name : "<null>");
        if (camera != null)
            description.Append(" pixelRect=").Append(camera.pixelRect)
                .Append(" viewportPoint=").Append(camera.ScreenToViewportPoint(position))
                .Append(" nearFar=").Append(camera.nearClipPlane).Append('/').Append(camera.farClipPlane);
        if (target != null)
        {
            var rect = target.rectTransform;
            description.Append(" target=").Append(target.name)
                .Append(" active=").Append(target.isActiveAndEnabled)
                .Append(" raycastTarget=").Append(target.raycastTarget)
                .Append(" depth=").Append(target.depth)
                .Append(" cull=").Append(target.canvasRenderer.cull)
                .Append(" canvas=").Append(target.canvas != null ? target.canvas.name : "<null>")
                .Append(" rect=").Append(rect.rect)
                .Append(" pivot=").Append(rect.pivot)
                .Append(" worldCenter=").Append(rect.TransformPoint(rect.rect.center))
                .Append(" lossyScale=").Append(rect.lossyScale)
                .Append(" contains=").Append(RectTransformUtility.RectangleContainsScreenPoint(rect, position, camera))
                .Append(" graphicFilterPass=").Append(target.Raycast(position, camera));
        }
        description.Append(" hits=").Append(input.DescribeHits(raycaster, owner, position));
        return description.ToString();
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

/// <summary>Transient input driver, never attached to or serialized in a project asset.</summary>
public sealed class FruitTouchFixtureInputModule : StandaloneInputModule
{
    private readonly List<RaycastResult> hits = new List<RaycastResult>();
    public bool UseEventSystemRaycasts { get; set; }
    public Scene RaycastScene { get; set; }

    // Editor-only fixture input is driven explicitly by the cases above.
    public override void Process() { }

    public void InitializeFixture()
    {
        // Runtime EventSystems/modules do not receive normal Play-mode lifecycle
        // callbacks in Edit mode. Invoke their protected initialization directly.
        base.OnEnable();
    }

    public RaycastResult Hit(GraphicRaycaster raycaster, EventSystem owner, Vector2 position)
    {
        var pointer = new PointerEventData(owner) { position = position };
        hits.Clear();
        if (UseEventSystemRaycasts)
        {
            // Preserve Unity's cross-Canvas sorting. The original touch-only
            // fixture intentionally uses one canvas; return fixtures must not.
            owner.RaycastAll(pointer, hits);
            if (RaycastScene.IsValid())
                hits.RemoveAll(hit => hit.gameObject == null || hit.gameObject.scene != RaycastScene);
        }
        else raycaster.Raycast(pointer, hits);
        return hits.Count > 0 ? hits[0] : default(RaycastResult);
    }

    public string DescribeHits(GraphicRaycaster raycaster, EventSystem owner, Vector2 position)
    {
        Hit(raycaster, owner, position);
        if (hits.Count == 0) return "<none>";
        var description = new StringBuilder();
        foreach (RaycastResult hit in hits)
        {
            if (description.Length > 0) description.Append(", ");
            description.Append(hit.gameObject != null ? hit.gameObject.name : "<null>")
                .Append("(fruit=").Append(hit.gameObject.GetComponentInParent<CollectItem>() != null
                    ? hit.gameObject.GetComponentInParent<CollectItem>().name : "<none>")
                .Append(", module=").Append(hit.module != null ? hit.module.name : "<none>")
                .Append(", order=").Append(hit.sortingOrder)
                .Append(", depth=").Append(hit.depth).Append(", distance=").Append(hit.distance).Append(')');
        }
        return description.ToString();
    }

    public PointerEventData Press(GraphicRaycaster raycaster, EventSystem owner, Vector2 position, bool allowDrag = false)
    {
        var pointer = new PointerEventData(owner)
        {
            pointerId = 0,
            button = PointerEventData.InputButton.Left,
            position = position,
            pointerCurrentRaycast = Hit(raycaster, owner, position)
        };
        ProcessTouchPress(pointer, true, false);
        if (!allowDrag) Require(pointer.pointerDrag == null, "Fruit touch unexpectedly registered a drag handler.");
        else Require(pointer.pointerDrag != null, "Original InputMono baseline has no drag handler.");
        return pointer;
    }

    public void Release(GraphicRaycaster raycaster, PointerEventData pointer, Vector2 position, bool allowDrag = false)
    {
        pointer.delta = position - pointer.position;
        pointer.position = position;
        pointer.pointerCurrentRaycast = Hit(raycaster, eventSystem, position);
        ProcessMove(pointer);
        ProcessDrag(pointer);
        if (!allowDrag) Require(!pointer.dragging, "An ordinary fruit touch became a drag.");
        ProcessTouchPress(pointer, false, true);
    }

    public void Gesture(GraphicRaycaster raycaster, EventSystem owner, Vector2 start, Vector2 end, bool allowDrag = false)
    {
        Release(raycaster, Press(raycaster, owner, start, allowDrag), end, allowDrag);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

/// <summary>Explicit Edit-mode lifecycle for the isolated fixture EventSystem.</summary>
public sealed class FruitTouchFixtureEventSystem : EventSystem
{
    private bool registered;

    public void InitializeFixture()
    {
        if (!registered) OnEnable();
    }

    public void ReleaseFixture()
    {
        if (registered) OnDisable();
    }

    protected override void OnEnable()
    {
        if (registered) return;
        registered = true;
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        if (!registered) return;
        registered = false;
        base.OnDisable();
    }
}
