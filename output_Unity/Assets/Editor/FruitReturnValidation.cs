using System;
using System.Collections.Generic;
using System.IO;
using CorePlay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Saved-prefab regression fixtures for returning fruit order and membership.</summary>
public static class FruitReturnValidation
{
    private const string PrefabPath = "Assets/res/local/coreplay/prefab/CollectItem.prefab";
    private const string LevelPath = "Assets/res_server/server_levelconfigs/mainlevel/300/LevelConfigs.json";

    [Serializable] private sealed class LevelTable { public List<LevelConfig> Levels; }
    [Serializable] private sealed class Report
    {
        public string generatedUtc;
        public string scope = "Isolated saved CollectItem prefab fixtures, production CollectItemFieldOrder.Restore, " +
            "production occlusion query/calculation, and real GraphicRaycaster/UGUI touch dispatch. " +
            "Uses level 30005 tile geometry and artwork. Button-event observations do not invoke gameplay collection. " +
            "Returning snapshot serialization is a production-data-contract fixture, not a call to private SaveSnapshot " +
            "or a device persistence test. No player services, saves, user scenes, or phone are modified.";
        public List<string> passed = new List<string>();
        public List<string> diagnostics = new List<string>();
        public List<string> errors = new List<string>();
    }

    [MenuItem("Tools/Validation/Fruit undo return order")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play mode before validating fruit return fixtures.");
        var report = new Report { generatedUtc = DateTime.UtcNow.ToString("O") };
        Application.LogCallback collectErrors = (message, stack, type) =>
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                report.errors.Add(message);
        };
        Application.logMessageReceived += collectErrors;
        try
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Require(source != null, "Saved CollectItem prefab is missing.");
            LevelTable table = JsonUtility.FromJson<LevelTable>(File.ReadAllText(LevelPath));
            LevelConfig level = table.Levels.Find(value => value.levelID == "30005");
            Require(level != null, "Level 30005 is missing.");
            using (var fixture = new Fixture(source))
            {
                ValidateHistoryIndex(fixture, report);
                fixture.ClearField();
                ValidatePair(fixture, level, 1, 32, report);
                fixture.ClearField();
                ValidatePair(fixture, level, 8, 27, report);
                fixture.ClearField();
                ValidateReturningMembership(fixture, level, report);
            }
        }
        catch (Exception exception) { report.errors.Add(exception.ToString()); }
        finally { Application.logMessageReceived -= collectErrors; }
        string output = Environment.GetEnvironmentVariable("DJS_APPLE_VALIDATION_OUTPUT");
        if (!string.IsNullOrEmpty(output))
        {
            Directory.CreateDirectory(output);
            File.WriteAllText(Path.Combine(output, "fruit-return-results.json"), JsonUtility.ToJson(report, true));
        }
        if (report.errors.Count != 0)
            throw new InvalidOperationException("Fruit return validation failed: " + string.Join("\n", report.errors));
        Debug.Log("FRUIT_RETURN_PASS cases=" + report.passed.Count + " saved_prefab=true real_ugui=true no_player_storage=true");
    }

    private static void ValidateHistoryIndex(Fixture f, Report report)
    {
        CollectItem a = f.Create(0, 10, 0, Vector2.zero);
        CollectItem b = f.Create(1, 10, 1, Vector2.zero);
        CollectItem c = f.Create(2, 10, 2, Vector2.zero);
        CollectItem d = f.Create(3, 10, 3, Vector2.zero);
        int oldIndex = c.transform.GetSiblingIndex();
        c.transform.SetParent(f.Tray, false);
        Object.DestroyImmediate(a.gameObject);
        Object.DestroyImmediate(b.gameObject);
        c.transform.SetParent(f.Field, false);
        c.transform.SetSiblingIndex(Mathf.Min(oldIndex, f.Field.childCount - 1));
        var field = new List<CollectItem> { d, c };
        Require(d.transform.GetSiblingIndex() == 0 && c.transform.GetSiblingIndex() == 1,
            "Historical-index baseline did not reproduce [D,C].");
        f.Render();
        Require(f.Hit(c) == f.Button(c).gameObject, "Historical baseline should draw/hit lower-priority C in front.");
        Require(f.Occlusion(c, field) > CollectItem.CLICKABLE_OCCLUSION_THRESHOLD,
            "Historical baseline C must simultaneously be logically blocked by D.");
        report.passed.Add("Original history-index rule reproduces [A0,B1,C2,D3] -> [D3,C2]: C receives the raycast while D makes its occlusion exceed 80%.");

        CollectItemFieldOrder.Restore(f.Field, field);
        Require(c.transform.GetSiblingIndex() == 0 && d.transform.GetSiblingIndex() == 1,
            "Production return order did not restore [C,D].");
        f.Render();
        Require(f.Hit(d) == f.Button(d).gameObject, "Production return order does not hit high-priority D first.");
        Require(Mathf.Approximately(f.Occlusion(d, field), 0), "Top D should have no blocking candidate.");
        int clicks = 0;
        f.Button(d).onClick.AddListener(() => clicks++);
        f.Tap(d);
        Require(clicks == 1, "Correctly ordered D did not receive one real UGUI click.");
        report.passed.Add("Production helper restores [C2,D3]; D receives one UGUI click and has zero production occlusion.");
    }

    private static void ValidatePair(Fixture f, LevelConfig level, int lowerId, int upperId, Report report)
    {
        SingleNormalTile lowerTile = FindTile(level, lowerId);
        SingleNormalTile upperTile = FindTile(level, upperId);
        CollectItem lower = f.Create(lowerTile);
        CollectItem upper = f.Create(upperTile);
        var field = new List<CollectItem> { upper, lower };
        // Exact failure shape: returned lower-priority fruit became the last child.
        lower.transform.SetAsLastSibling();
        f.Render();
        float lowerRatio = f.Occlusion(lower, field);
        Require(lowerRatio > CollectItem.CLICKABLE_OCCLUSION_THRESHOLD,
            "Level 30005 lower tile " + lowerId + " did not reproduce the blocked overlap.");
        Require(f.Hit(lower) == f.Button(lower).gameObject,
            "Level 30005 historical wrong order did not hit the lower fruit.");
        CollectItemFieldOrder.Restore(f.Field, field);
        f.Render();
        Require(upper.transform.GetSiblingIndex() > lower.transform.GetSiblingIndex(),
            "Higher-priority tile " + upperId + " was not restored above " + lowerId + ".");
        Require(f.Hit(lower) == f.Button(upper).gameObject && f.Hit(upper) == f.Button(upper).gameObject,
            "The actual overlapping fruit centers do not both hit the front high-priority fruit.");
        Require(Mathf.Approximately(f.Occlusion(upper, field), 0),
            "High-priority fruit has an unexpected occlusion candidate.");
        int upperClicks = 0, lowerClicks = 0;
        f.Button(upper).onClick.AddListener(() => upperClicks++);
        f.Button(lower).onClick.AddListener(() => lowerClicks++);
        f.Tap(upper);
        Require(upperClicks == 1 && lowerClicks == 0, "A lower layer intercepted the front fruit click.");
        report.diagnostics.Add("30005 tiles " + lowerId + "/" + upperId + ": lowerOcclusion=" + lowerRatio.ToString("F4") +
            "; upperOcclusion=0; frontHit=" + upper.name + "; original tile coordinates, types and authored image sizes used.");
        report.passed.Add("30005 tiles " + lowerId + "/" + upperId + ": production order and real UGUI agree; high-priority fruit receives exactly one click.");

        // The actual occlusion index must stop considering the collected upper fruit.
        upper.Status = LECIONHKEJL.InCollect;
        upper.transform.SetParent(f.Tray, false);
        upper.gameObject.SetActive(false);
        var index = new OPIBKKOCCIA(f.Field);
        index.Build(field);
        var candidates = new List<CollectItem>();
        index.GetHigherPriorityItems(lower, candidates);
        Require(candidates.Count == 0, "A collected fruit remained an occlusion candidate.");
        index.Remove(upper);
        field.Remove(upper);
        f.Render();
        Require(f.Hit(lower) == f.Button(lower).gameObject && Mathf.Approximately(f.Occlusion(lower, field), 0),
            "Removing the top fruit did not expose the lower fruit to raycasts.");
        f.Tap(lower);
        Require(lowerClicks == 1, "Exposed lower fruit did not receive one click.");
        report.passed.Add("30005 tiles " + lowerId + "/" + upperId + ": InCollect fruit is excluded from candidates; after removal the lower fruit receives one UGUI click.");
    }

    private static void ValidateReturningMembership(Fixture f, LevelConfig level, Report report)
    {
        CollectItem lower = f.Create(FindTile(level, 1));
        CollectItem upper = f.Create(FindTile(level, 32));
        lower.Status = LECIONHKEJL.Returning;
        var field = new List<CollectItem> { upper, lower };
        for (int repeat = 0; repeat < 4; repeat++) CollectItemFieldOrder.Restore(f.Field, field);
        Require(field.Count == 2 && field[0] == lower && field[1] == upper,
            "Repeated ordering lost, duplicated or excluded the Returning fruit.");
        var snapshot = new GameSnapshot
        {
            levelConfig = level,
            totalItems = new List<GameItemData>(),
            collectItems = new List<CollectAreaData>(),
            undoRecords = new List<UndoRecord>()
        };
        foreach (CollectItem item in field)
            snapshot.totalItems.Add(new GameItemData { tileId = item.Id, type = item.Type, priority = item.Priority });
        GameSnapshot roundTrip = JsonUtility.FromJson<GameSnapshot>(JsonUtility.ToJson(snapshot));
        Require(roundTrip.totalItems.Count == 2 && roundTrip.totalItems[0].tileId == 1 && roundTrip.totalItems[1].tileId == 32,
            "Returning fruit membership was lost in the production snapshot data contract.");
        // Use an independent plain controller instance; Init allocates collections
        // only. This does not replace or mutate the gameplay singleton.
        var controller = new EDLHEMMBABM();
        controller.Init();
        controller.GetTotalItems().Add(lower);
        controller.GetTotalItems().Add(upper);
        Require(!controller.CheckCanShuffle(),
            "Shuffle remains enabled while a fruit is returning and could cancel its completion callback.");
        lower.Status = LECIONHKEJL.OnField;
        Require(controller.CheckCanShuffle(), "Shuffle did not become available after return completed.");
        CollectItemFieldOrder.Restore(f.Field, field);
        Require(field.Count == 2 && field[0] != field[1],
            "Completing the return duplicated a field member.");
        report.passed.Add("Ordering retains Returning membership across repeated calls; a GameSnapshot JSON round trip preserves both IDs before return completes. This is a data-contract fixture, not private SaveSnapshot execution.");
        report.passed.Add("Independent production controller disables shuffle while Returning exists and enables it once return completes; no singleton/player storage involved.");
    }

    private static SingleNormalTile FindTile(LevelConfig level, int id)
    {
        SingleNormalTile tile = level.normalTiles.Find(value => value.tileID == id);
        Require(tile != null, "Missing level tile " + id);
        return tile;
    }

    private sealed class Fixture : IDisposable
    {
        private readonly Scene scene;
        private readonly EventSystem previousEventSystem;
        private readonly FruitTouchFixtureEventSystem events;
        private readonly FruitTouchFixtureInputModule input;
        private readonly GameObject source;
        private readonly Camera camera;
        private readonly Canvas canvas;
        private readonly GraphicRaycaster raycaster;
        private readonly RenderTexture texture;
        private readonly Dictionary<int, SingleEleImageConfig> sizes = new Dictionary<int, SingleEleImageConfig>();
        public readonly RectTransform Field;
        public readonly RectTransform Tray;

        public Fixture(GameObject prefab)
        {
            source = prefab;
            scene = EditorSceneManager.NewPreviewScene();
            previousEventSystem = EventSystem.current;
            GameObject cameraObject = Create("Return fixture camera");
            camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false; camera.scene = scene; camera.orthographic = true;
            camera.orthographicSize = 1200; camera.transform.position = new Vector3(0, 0, -100);
            camera.nearClipPlane = .1f; camera.farClipPlane = 200;
            texture = new RenderTexture(1080, 2400, 0); texture.Create();
            camera.targetTexture = texture; camera.aspect = 1080f / 2400;
            GameObject canvasObject = Create("Return fixture canvas", typeof(RectTransform));
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace; canvas.worldCamera = camera;
            ((RectTransform)canvas.transform).sizeDelta = new Vector2(1080, 2400);
            raycaster = canvasObject.AddComponent<GraphicRaycaster>();
            Field = (RectTransform)Create("Field", typeof(RectTransform)).transform;
            Field.SetParent(canvas.transform, false);
            Field.sizeDelta = new Vector2(1080, 2400);
            Tray = (RectTransform)Create("Tray", typeof(RectTransform)).transform;
            Tray.SetParent(canvas.transform, false);
            Tray.anchoredPosition = new Vector2(0, -900);
            GameObject eventObject = Create("Return fixture EventSystem");
            events = eventObject.AddComponent<FruitTouchFixtureEventSystem>(); events.InitializeFixture();
            events.pixelDragThreshold = 10;
            input = eventObject.AddComponent<FruitTouchFixtureInputModule>(); input.InitializeFixture();
            EventSystem.current = events;
            var config = JsonUtility.FromJson<AllEleImageConfig>(File.ReadAllText("Assets/res_server/server_configs/AllEleImageConfig.json"));
            foreach (SingleEleImageConfig size in config.eleImageConfigs) sizes.Add(size.eleID, size);
        }

        private GameObject Create(string name, params Type[] components)
        {
            var result = new GameObject(name, components) { hideFlags = HideFlags.HideAndDontSave };
            SceneManager.MoveGameObjectToScene(result, scene);
            return result;
        }

        public CollectItem Create(SingleNormalTile tile) =>
            Create(tile.tileID, tile.eleType, tile.layerIndex * 10000 + tile.indexInLayer, new Vector2(tile.posX, tile.posY));

        public CollectItem Create(int id, int type, int priority, Vector2 position)
        {
            GameObject instance = Object.Instantiate(source, Field, false);
            instance.name = "Tile_" + id + "_Type_" + type;
            var item = instance.GetComponent<CollectItem>();
            item.Id = id; item.Type = type; item.Priority = priority; item.Status = LECIONHKEJL.OnField;
            item.UnBindClick();
            ((RectTransform)instance.transform).anchoredPosition = position;
            SingleEleImageConfig size = sizes[type];
            item.CollectImageRect.sizeDelta = new Vector2(size.sizeX, size.sizeY);
            Image graphic = item.CollectImageRect.GetComponent<Image>();
            graphic.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/res_server/server_images/fruit/Fruit_" + type + ".png");
            Require(graphic.sprite != null, "Actual fruit artwork is missing for type " + type);
            return item;
        }

        public FruitCollectButton Button(CollectItem item) => item.GetComponentInChildren<FruitCollectButton>(true);
        private Vector2 Center(CollectItem item) =>
            RectTransformUtility.WorldToScreenPoint(camera, item.CollectImageRect.TransformPoint(item.CollectImageRect.rect.center));
        public GameObject Hit(CollectItem item) => input.Hit(raycaster, events, Center(item)).gameObject;
        public void Tap(CollectItem item) => input.Gesture(raycaster, events, Center(item), Center(item));
        public void Render() { Canvas.ForceUpdateCanvases(); camera.Render(); Canvas.ForceUpdateCanvases(); }
        public float Occlusion(CollectItem item, List<CollectItem> field)
        {
            var index = new OPIBKKOCCIA(Field); index.Build(field);
            var candidates = new List<CollectItem>(); index.GetHigherPriorityItems(item, candidates);
            return item.CalculatePreciseOcclusionRatio(candidates);
        }
        public void ClearField()
        {
            while (Field.childCount > 0) Object.DestroyImmediate(Field.GetChild(0).gameObject);
            while (Tray.childCount > 0) Object.DestroyImmediate(Tray.GetChild(0).gameObject);
        }
        public void Dispose()
        {
            events.ReleaseFixture();
            EditorSceneManager.ClosePreviewScene(scene);
            if (previousEventSystem != null) EventSystem.current = previousEventSystem;
            texture.Release(); Object.DestroyImmediate(texture);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
