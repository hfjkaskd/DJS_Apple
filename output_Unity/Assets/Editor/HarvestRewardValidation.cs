using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Isolated asset audit/layout captures; never initializes the reward service or a player's save.</summary>
public static class HarvestRewardValidation
{
    private static readonly List<string> LayoutErrors = new List<string>();
    private static Vector2 ProductionReferenceResolution;
    private static List<LevelConfig> MainLevels;
    private static Dictionary<int, SingleEleImageConfig> FruitSizes;
    [Serializable] private sealed class MainLevelTable { public MainLevelConfigData[] array = null; }
    private static readonly string[] PageFields = { "walletRoot", "confirmationRoot", "progressRoot", "settlementRoot", "completedRoot", "historyRoot", "rulesRoot" };
    private static readonly string[] PageImages = { "harvest-wallet", "harvest-confirmation", "harvest-progress", "harvest-settlement", "harvest-completed", "harvest-history", "harvest-rules" };

    public static void ValidateAndRender()
    {
        LayoutErrors.Clear();
        ReadProductionCanvas();
        ReadBoardConfigurations();
        HarvestRewardConfig config = AssetDatabase.LoadAssetAtPath<HarvestRewardConfig>("Assets/Resources/HarvestRewardConfig.asset");
        if (config == null) throw new InvalidOperationException("Harvest config is missing.");
        config.Settings.Validate();
        GameObject wallet = LoadPrefab(HarvestRewardsUI.Path);
        HarvestRewardsUI controller = wallet.GetComponent<HarvestRewardsUI>();
        if (controller == null) throw new InvalidOperationException("Harvest wallet controller is missing.");
        AuditModal(wallet, controller);
        ValidatePages(controller, config.Settings);
        GameObject popup = LoadPrefab(HarvestRewardPopupUI.Path);
        HarvestRewardPopupUI popupController = popup.GetComponent<HarvestRewardPopupUI>();
        if (popupController == null) throw new InvalidOperationException("Harvest reward popup controller is missing.");
        AuditModal(popup, popupController);
        for (int i = 0; i < PageFields.Length; i++)
        {
            int page = i;
            Render(wallet, PageImages[i], 2340, view => PrepareWallet(view, config.Settings, page), true);
            Render(wallet, PageImages[i] + "-short", 1920, view => PrepareWallet(view, config.Settings, page), true);
        }
        Render(popup, "harvest-reward-welcome", 2340, view => PreparePopup(view, 0), true);
        Render(popup, "harvest-reward-fruit", 2340, view => PreparePopup(view, 1), true);
        Render(popup, "harvest-reward-win", 2340, view => PreparePopup(view, 2), true);
        Render(popup, "harvest-reward-fruit-short", 1920, view => PreparePopup(view, 1), true);
        ValidateEntry("home/Home", "harvest-home");
        ValidateEntry("coreplay/CorePlayUI", "harvest-gameplay");
        ValidateEntry("coreplaywin/WinUI", "harvest-win");
        GameObject gameplay = LoadPrefab("coreplay/CorePlayUI");
        Render(gameplay, "harvest-gameplay-extremes", 2340, PrepareGameplayBounds, false);
        Render(gameplay, "harvest-gameplay-extremes-short", 1920, PrepareGameplayBounds, false);
        if (LayoutErrors.Count > 0)
            throw new InvalidOperationException("Harvest layout issues (all captures were saved):\n" + string.Join("\n", LayoutErrors));
        Debug.Log("HARVEST_ASSETS_OK: seven wallet pages, reward popup variants, three HUD entries, mainline board bounds, reparent scaling, complete references and standard Buttons validated; 26 captures.");
    }

    private static void ReadProductionCanvas()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Game.unity", OpenSceneMode.Additive);
        try
        {
            CanvasScaler found = null;
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (CanvasScaler candidate in root.GetComponentsInChildren<CanvasScaler>(true))
                    if (candidate.name == "Canvas") found = candidate;
            if (found == null || found.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize ||
                found.screenMatchMode != CanvasScaler.ScreenMatchMode.Expand)
                throw new InvalidOperationException("Review capture sizing when the production CanvasScaler changes.");
            ProductionReferenceResolution = found.referenceResolution;
        }
        finally { EditorSceneManager.CloseScene(scene, true); }
    }

    private static void ReadBoardConfigurations()
    {
        string configRoot = Path.Combine(Application.dataPath, "res_server/server_configs");
        var table = JsonUtility.FromJson<MainLevelTable>("{\"array\":" + File.ReadAllText(Path.Combine(configRoot, "MainLevelConfig.json")) + "}");
        var selected = new HashSet<string>();
        foreach (MainLevelConfigData row in table.array)
        {
            foreach (string id in row.LevelList.Split(',')) selected.Add(id.Trim());
            if (!string.IsNullOrEmpty(row.Guaranteed_Level)) selected.Add(row.Guaranteed_Level.Trim());
        }
        MainLevels = new List<LevelConfig>();
        var found = new HashSet<string>();
        foreach (string path in Directory.GetFiles(Path.Combine(Application.dataPath, "res_server/server_levelconfigs/mainlevel"), "LevelConfigs.json", SearchOption.AllDirectories))
        {
            LevelConfigList data = JsonUtility.FromJson<LevelConfigList>(File.ReadAllText(path));
            foreach (LevelConfig level in data.Levels)
                if (selected.Contains(level.levelID) && found.Add(level.levelID)) MainLevels.Add(level);
        }
        if (MainLevels.Count == 0) throw new InvalidOperationException("No local mainline levels were available for board bounds validation.");
        FruitSizes = new Dictionary<int, SingleEleImageConfig>();
        foreach (SingleEleImageConfig item in JsonUtility.FromJson<AllEleImageConfig>(File.ReadAllText(Path.Combine(configRoot, "AllEleImageConfig.json"))).eleImageConfigs)
            FruitSizes[item.eleID] = item;
        Debug.Log("HARVEST_BOARD_COVERAGE: " + MainLevels.Count + " local levels selected by MainLevelConfig; " + (selected.Count - found.Count) + " referenced IDs have no local configuration and are outside this asset audit.");
    }

    private static void PrepareGameplayBounds(GameObject view)
    {
        PrepareHud(view, true);
        var ui = view.GetComponent<CorePlay.CorePlayUI>();
        if (ui == null || ui.m_BgTrans == null || ui.m_GameLayer == null || ui.m_CollectArea == null ||
            ui.m_BgTrans.Find("Layer") != ui.m_GameLayer || !ui.m_GameLayer.IsChildOf(ui.m_BgTrans))
            throw new InvalidOperationException("Gameplay board and occlusion coordinates must reference the same authored Map/Layer.");
        ui.SetGameAreaAlpha(1f);
        Canvas.ForceUpdateCanvases();
        float hudBottom = float.PositiveInfinity;
        foreach (Graphic graphic in view.GetComponentInChildren<HarvestRewardsHud>().GetComponentsInChildren<Graphic>())
            if (graphic.enabled && graphic.color.a > 0f) hudBottom = Mathf.Min(hudBottom, WorldRect(graphic.rectTransform).yMin);
        Transform basket = ui.m_CollectArea.parent.Find("BastetUp");
        if (basket == null) throw new InvalidOperationException("Basket upper edge is missing from the gameplay prefab.");
        float basketTop = WorldRect((RectTransform)basket).yMax;
        Rect screen = WorldRect((RectTransform)view.transform.parent);
        GameObject fruitPrefab = LoadPrefab("coreplay/prefab/CollectItem");
        GameObject probe = (GameObject)PrefabUtility.InstantiatePrefab(fruitPrefab, ui.m_GameLayer);
        CollectItem item = probe.GetComponent<CollectItem>();
        Transform scaleLayer = Reference<Transform>(new SerializedObject(item), "m_ScaleTrans");
        foreach (Graphic graphic in probe.GetComponentsInChildren<Graphic>(true))
            if (!graphic.transform.IsChildOf(scaleLayer)) throw new InvalidOperationException("Fruit visual escapes the configured scaling layer.");
        foreach (Renderer renderer in probe.GetComponentsInChildren<Renderer>(true))
            if (!renderer.transform.IsChildOf(scaleLayer)) throw new InvalidOperationException("Fruit renderer escapes the configured scaling layer.");
        var shapes = new Dictionary<int, Vector3[]>();
        foreach (SingleEleImageConfig size in FruitSizes.Values)
        {
            item.CollectImageRect.sizeDelta = new Vector2(size.sizeX, size.sizeY);
            Vector3[] corners = new Vector3[4];
            item.CollectImageRect.GetWorldCorners(corners);
            for (int i = 0; i < corners.Length; i++) corners[i] = probe.transform.InverseTransformPoint(corners[i]);
            shapes[size.eleID] = corners;
        }
        AuditFruitReparent(item, ui.m_CollectArea, ui.m_GameLayer);
        UnityEngine.Object.DestroyImmediate(probe);
        float[] edges = { float.PositiveInfinity, float.PositiveInfinity, float.NegativeInfinity, float.NegativeInfinity };
        SingleNormalTile[] extremes = new SingleNormalTile[4];
        string[] extremeLevels = new string[4];
        int tiles = 0;
        foreach (LevelConfig level in MainLevels)
            foreach (SingleNormalTile tile in level.normalTiles)
            {
                Vector3[] shape;
                if (!shapes.TryGetValue(tile.eleType, out shape)) throw new InvalidOperationException("Missing fruit geometry for type " + tile.eleType);
                tiles++;
                foreach (Vector3 corner in shape)
                {
                    Vector3 point = ui.m_GameLayer.TransformPoint(new Vector3(tile.posX, tile.posY, 0) + corner);
                    float[] values = { point.x, point.y, point.x, point.y };
                    for (int edge = 0; edge < 4; edge++)
                        if (edge < 2 ? values[edge] < edges[edge] : values[edge] > edges[edge])
                        { edges[edge] = values[edge]; extremes[edge] = tile; extremeLevels[edge] = level.levelID; }
                }
            }
        float topGap = hudBottom - edges[3], bottomGap = edges[1] - basketTop;
        if (topGap < 20f || bottomGap < 20f || edges[0] < screen.xMin + 10f || edges[2] > screen.xMax - 10f)
            LayoutErrors.Add(string.Format(CultureInfo.InvariantCulture,
                "Gameplay board overlaps reserved UI or screen margin: HUD gap {0:F1}, basket gap {1:F1}, x [{2:F1},{3:F1}], canvas x [{4:F1},{5:F1}]", topGap, bottomGap, edges[0], edges[2], screen.xMin, screen.xMax));
        var visible = new HashSet<SingleNormalTile>();
        for (int edge = 0; edge < 4; edge++)
        {
            SingleNormalTile tile = extremes[edge];
            if (!visible.Add(tile)) continue;
            GameObject fruit = (GameObject)PrefabUtility.InstantiatePrefab(fruitPrefab, ui.m_GameLayer);
            fruit.name = "Boundary_Level" + extremeLevels[edge] + "_Tile" + tile.tileID;
            ((RectTransform)fruit.transform).anchoredPosition = new Vector2(tile.posX, tile.posY);
            CollectItem boundary = fruit.GetComponent<CollectItem>();
            SingleEleImageConfig size = FruitSizes[tile.eleType];
            boundary.CollectImageRect.sizeDelta = new Vector2(size.sizeX, size.sizeY);
            Image fruitImage = boundary.CollectImageRect.GetComponent<Image>();
            fruitImage.sprite = CollectItem.GetItemSprite(tile.eleType);
            fruitImage.color = Color.white;
            if (fruitImage.sprite == null) throw new InvalidOperationException("Missing boundary fruit sprite: " + tile.eleType);
            foreach (Orange.DynamicCanvasLayer layer in fruit.GetComponentsInChildren<Orange.DynamicCanvasLayer>(true)) layer.SetLayer(0);
        }
        Debug.Log(string.Format(CultureInfo.InvariantCulture,
            "HARVEST_BOARD_OK: {0} tiles; world bounds [{1:F1},{2:F1}]..[{3:F1},{4:F1}], HUD gap {5:F1}, basket gap {6:F1}; extrema levels {7}",
            tiles, edges[0], edges[1], edges[2], edges[3], topGap, bottomGap, string.Join(",", extremeLevels)));
    }

    private static Rect WorldRect(RectTransform transform)
    {
        Vector3[] corners = new Vector3[4];
        transform.GetWorldCorners(corners);
        Vector2 min = corners[0], max = corners[0];
        foreach (Vector3 corner in corners) { min = Vector2.Min(min, corner); max = Vector2.Max(max, corner); }
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private static void AuditFruitReparent(CollectItem item, Transform basket, Transform board)
    {
        Rect before = WorldRect(item.CollectImageRect);
        item.transform.SetParent(basket, true);
        item.NormalizeScaleAfterReparent();
        AssertSameRect(before, WorldRect(item.CollectImageRect), "board to basket");
        item.SetScale(.55f);
        Rect landed = WorldRect(item.CollectImageRect);
        item.transform.SetParent(board, true);
        item.NormalizeScaleAfterReparent();
        AssertSameRect(landed, WorldRect(item.CollectImageRect), "basket to board");
        item.SetScale(1f);
        AssertSameRect(before, WorldRect(item.CollectImageRect), "undo final scale");
        item.transform.SetParent(basket, false);
        item.transform.localScale = Vector3.one;
        item.SetScale(.55f);
        if ((WorldRect(item.CollectImageRect).size - landed.size).sqrMagnitude > .001f)
            throw new InvalidOperationException("Restored basket fruit size differs from the landed fruit size.");
    }

    private static void AssertSameRect(Rect expected, Rect actual, string context)
    {
        if ((expected.position - actual.position).sqrMagnitude > .001f || (expected.size - actual.size).sqrMagnitude > .001f)
            throw new InvalidOperationException("Fruit changes visual bounds during " + context + ".");
    }

    private static GameObject LoadPrefab(string path)
    {
        GameObject result = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/res/local/" + path + ".prefab");
        if (result == null) throw new InvalidOperationException("Missing harvest prefab: " + path);
        return result;
    }

    private static void AuditModal(GameObject prefab, MonoBehaviour controller)
    {
        if (prefab.GetComponent<Canvas>() == null || prefab.GetComponent<GraphicRaycaster>() == null)
            throw new InvalidOperationException("Modal needs an authored Canvas and GraphicRaycaster: " + prefab.name);
        AuditHierarchy(prefab);
        AuditReferences(controller);
    }

    private static void AuditHierarchy(GameObject root)
    {
        Button[] buttons = root.GetComponentsInChildren<Button>(true);
        if (buttons.Length == 0) throw new InvalidOperationException("No standard Button in " + root.name);
        foreach (Button button in buttons)
        {
            if (button.targetGraphic == null || button.targetGraphic.gameObject != button.gameObject || !button.targetGraphic.raycastTarget)
                throw new InvalidOperationException("Button must own its visible raycastable target: " + button.name);
            if (button.onClick.GetPersistentEventCount() != 0)
                throw new InvalidOperationException("Use code-bound Button events: " + button.name);
        }
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) != 0)
                throw new InvalidOperationException("Missing script on " + child.name);
            if (child.GetComponent<InputMono>() != null || child.GetComponent<InputMonoNoDrag>() != null ||
                child.GetComponent<EventTrigger>() != null || child.GetComponent<Collider>() != null || child.GetComponent<Collider2D>() != null)
                throw new InvalidOperationException("Harvest interaction must use standard Buttons: " + child.name);
        }
    }

    // Unity's Editor resource API audits configured references; no runtime reflection is used.
    private static void AuditReferences(MonoBehaviour controller)
    {
        var serialized = new SerializedObject(controller);
        SerializedProperty property = serialized.GetIterator();
        while (property.NextVisible(true))
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference || property.name == "m_Script") continue;
            UnityEngine.Object reference = property.objectReferenceValue;
            if (reference == null) throw new InvalidOperationException("Unassigned harvest reference: " + controller.name + "." + property.propertyPath);
            GameObject target = reference as GameObject;
            Component component = reference as Component;
            if (component != null) target = component.gameObject;
            if (target != null && target != controller.gameObject && !target.transform.IsChildOf(controller.transform))
                throw new InvalidOperationException("Harvest UI reference escapes its hierarchy: " + property.propertyPath);
        }
    }

    private static void ValidatePages(HarvestRewardsUI controller, HarvestRewardSettings settings)
    {
        var serialized = new SerializedObject(controller);
        var roots = new HashSet<GameObject>();
        int active = 0;
        foreach (string field in PageFields)
        {
            GameObject page = Reference<GameObject>(serialized, field);
            if (!roots.Add(page) || page == controller.gameObject) throw new InvalidOperationException("Wallet pages need distinct child roots: " + field);
            if (page.activeSelf) active++;
        }
        if (active != 1 || !Reference<GameObject>(serialized, "walletRoot").activeSelf)
            throw new InvalidOperationException("Only the wallet page should be active in the authored prefab.");
        RequireArrayLength(serialized, "tierButtons", settings.ThresholdCents.Length);
        RequireArrayLength(serialized, "tierLabels", settings.ThresholdCents.Length);
        RequireArrayLength(serialized, "tierSelectedMarkers", settings.ThresholdCents.Length);
        RequireArrayLength(serialized, "stageCompleteMarks", settings.Stages.Length);
        RequireArrayLength(serialized, "stageCurrentMarks", settings.Stages.Length);
    }

    private static void ValidateEntry(string path, string imageName)
    {
        GameObject prefab = LoadPrefab(path);
        HarvestRewardsHud[] huds = prefab.GetComponentsInChildren<HarvestRewardsHud>(true);
        if (huds.Length != 1) throw new InvalidOperationException("Host requires exactly one harvest HUD: " + path);
        AuditHierarchy(huds[0].gameObject);
        AuditReferences(huds[0]);
        var serialized = new SerializedObject(huds[0]);
        Reference<Button>(serialized, "walletButton");
        Reference<TMP_Text>(serialized, "balanceText");
        Reference<TMP_Text>(serialized, "goalText");
        Slider progress = Reference<Slider>(serialized, "goalProgressBar");
        if (progress.fillRect == null || progress.interactable || progress.onValueChanged.GetPersistentEventCount() != 0)
            throw new InvalidOperationException("HUD progress requires a configured display-only Slider: " + path);
        RequireArrayLength(serialized, "completedMarks", 6);
        RequireArrayLength(serialized, "currentMarks", 6);
        Render(prefab, imageName, 2340, view => PrepareHud(view, false), false);
        Render(prefab, imageName + "-challenge", 2340, view => PrepareHud(view, true), false);
    }

    private static void PrepareWallet(GameObject view, HarvestRewardSettings settings, int page)
    {
        var serialized = new SerializedObject(view.GetComponent<HarvestRewardsUI>());
        for (int i = 0; i < PageFields.Length; i++) Reference<GameObject>(serialized, PageFields[i]).SetActive(i == page);
        Reference<Button>(serialized, "backButton").gameObject.SetActive(page != 0 && page != 4);
        Reference<Button>(serialized, "rulesButton").gameObject.SetActive(page != 6);
        Reference<Button>(serialized, "historyButton").gameObject.SetActive(page != 5 && page != 1 && page != 6);
        SetText(serialized, "availableText", "9,750.00");
        SetText(serialized, "frozenText", "In challenge: 0.00");
        SetText(serialized, "walletHintText", "250.00 more to reach this tier.");
        SetText(serialized, "applyText", "Keep harvesting");
        SetText(serialized, "confirmationAmountText", "10,000.00");
        SerializedProperty summaryFormat = serialized.FindProperty("confirmationSummaryFormat");
        SetText(serialized, "confirmationSummaryText", string.Format(CultureInfo.InvariantCulture, summaryFormat.stringValue, "10,000.00", 6, "168", "13"));
        SetText(serialized, "stageTitleText", "Harvest stage 6 / 6");
        SetText(serialized, "stageTaskText", HarvestRewardsUI.CreateTaskText(settings.Stages[5]));
        SetText(serialized, "stageCountText", "499 / 500");
        Reference<Slider>(serialized, "stageProgressBar").SetValueWithoutNotify(.998f);
        SetText(serialized, "stageTimerText", "0d 23:59:59");
        SetText(serialized, "stageHintText", "Finish the task and waiting time to unlock the next stage.");
        SetText(serialized, "settlementAmountText", "10,000.00");
        SetText(serialized, "settlementTimerText", "6d 23:59:59");
        SetText(serialized, "completedAmountText", "10,000.00");
        SetText(serialized, "completedDetailText", "All harvest withdrawal tiers are complete. No cash was paid.");
        SetText(serialized, "historyText", "10,000.00  -  Completed\n2026-09-15 23:59 UTC\n\n8,000.00  -  Completed\n2026-09-01 23:59 UTC\n\n5,000.00  -  Completed\n2026-08-15 23:59 UTC\n\n3,000.00  -  Completed\n2026-08-01 23:59 UTC\n\n1,500.00  -  Completed\n2026-07-15 23:59 UTC\n\n1,000.00  -  Completed\n2026-07-01 23:59 UTC");
        long minimumWait;
        SetText(serialized, "rulesText", HarvestRewardsUI.CreateRulesText(settings, out minimumWait));
        for (int i = 0; i < settings.ThresholdCents.Length; i++)
        {
            ArrayReference<TMP_Text>(serialized, "tierLabels", i).text = (settings.ThresholdCents[i] / 100m).ToString("N2", CultureInfo.InvariantCulture) + (i < 5 ? "\nCompleted" : "");
            ArrayReference<GameObject>(serialized, "tierSelectedMarkers", i).SetActive(i == 5);
            ArrayReference<GameObject>(serialized, "stageCompleteMarks", i).SetActive(i < 5);
            ArrayReference<GameObject>(serialized, "stageCurrentMarks", i).SetActive(i == 5);
        }
    }

    private static void PreparePopup(GameObject view, int kind)
    {
        var serialized = new SerializedObject(view.GetComponent<HarvestRewardPopupUI>());
        SetText(serialized, "titleText", serialized.FindProperty(kind == 0 ? "welcomeTitle" : kind == 1 ? "fruitTitle" : "winTitle").stringValue);
        SetText(serialized, "amountText", kind == 0 ? "+20.00" : kind == 1 ? "+5.00" : "+100.00");
        SetText(serialized, "balanceText", kind == 1 ? "Balance: 9,750.00" : "Balance: 20.00");
        SetText(serialized, "hintText", serialized.FindProperty(kind == 0 ? "welcomeHint" : "rewardHint").stringValue);
        SetText(serialized, "claimText", kind == 0 ? "Continue" : "Collect");
        SetText(serialized, "adText", "Ad unavailable");
        Reference<Button>(serialized, "adButton").gameObject.SetActive(kind == 1);
        Reference<Button>(serialized, "adButton").interactable = false;
    }

    private static void PrepareHud(GameObject view, bool challenge)
    {
        var serialized = new SerializedObject(view.GetComponentInChildren<HarvestRewardsHud>(true));
        SetText(serialized, "balanceText", "9,750.00");
        SetText(serialized, "goalText", challenge ? "Stage 3: 3 / 10 levels" : "Earn 250.00 more to apply");
        Reference<Slider>(serialized, "goalProgressBar").SetValueWithoutNotify(challenge ? .3f : .975f);
        Reference<GameObject>(serialized, "stageTrackRoot").SetActive(challenge);
        for (int i = 0; i < 6; i++)
        {
            ArrayReference<GameObject>(serialized, "completedMarks", i).SetActive(challenge && i < 2);
            ArrayReference<GameObject>(serialized, "currentMarks", i).SetActive(challenge && i == 2);
        }
    }

    private static void SetText(SerializedObject serialized, string field, string value) { Reference<TMP_Text>(serialized, field).text = value; }
    private static T Reference<T>(SerializedObject serialized, string field) where T : UnityEngine.Object
    {
        SerializedProperty property = serialized.FindProperty(field);
        T result = property == null ? null : property.objectReferenceValue as T;
        if (result == null) throw new InvalidOperationException("Missing or incompatible harvest field: " + field);
        return result;
    }
    private static T ArrayReference<T>(SerializedObject serialized, string field, int index) where T : UnityEngine.Object
    {
        SerializedProperty property = serialized.FindProperty(field);
        T result = property == null || !property.isArray || index >= property.arraySize ? null : property.GetArrayElementAtIndex(index).objectReferenceValue as T;
        if (result == null) throw new InvalidOperationException("Missing harvest array reference: " + field + "[" + index + "]");
        return result;
    }
    private static void RequireArrayLength(SerializedObject serialized, string field, int count)
    {
        SerializedProperty property = serialized.FindProperty(field);
        if (property == null || !property.isArray || property.arraySize != count) throw new InvalidOperationException("Harvest array requires " + count + " entries: " + field);
    }

    private static void Render(GameObject prefab, string imageName, int height, Action<GameObject> prepare, bool auditAllText)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        // CanvasScaler.Expand preserves the reference height on a wider/shorter phone.
        float productionScale = Mathf.Min(1080f / ProductionReferenceResolution.x, height / ProductionReferenceResolution.y);
        Vector2 logicalSize = new Vector2(1080f / productionScale, height / productionScale);
        GameObject cameraObject = new GameObject("ValidationCamera", typeof(Camera));
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.08f, .15f, .1f);
        camera.orthographic = true;
        camera.orthographicSize = logicalSize.y * .5f;
        camera.nearClipPlane = .1f;
        camera.farClipPlane = 500f;
        camera.transform.position = new Vector3(0, 0, -100);
        GameObject canvasObject = new GameObject("ValidationCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = camera;
        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = logicalSize;
        canvasRect.position = Vector3.zero;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = ProductionReferenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        GameObject view = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvasObject.transform);
        RectTransform viewRect = view.transform as RectTransform;
        viewRect.anchorMin = Vector2.zero; viewRect.anchorMax = Vector2.one;
        viewRect.offsetMin = Vector2.zero; viewRect.offsetMax = Vector2.zero; viewRect.localScale = Vector3.one;
        BaseUI controller = view.GetComponent<BaseUI>();
        if (controller != null)
        {
            controller.enabled = false;
            Orange.SafeAreaSim.InsetRect(viewRect);
        }
        // Invoke the same public layout operations as BaseUI and the authored adapters,
        // without initializing gameplay, rewards, ads or a player's save.
        foreach (Orange.ProtectedAreaAdapt inset in view.GetComponentsInChildren<Orange.ProtectedAreaAdapt>(true))
            Orange.SafeAreaSim.InsetRect(inset.transform as RectTransform, true);
        foreach (Orange.ProtectedAreaAdaptReverse reverse in view.GetComponentsInChildren<Orange.ProtectedAreaAdaptReverse>(true))
            Orange.SafeAreaSim.ExpandRect(reverse.transform as RectTransform, new SerializedObject(reverse).FindProperty("m_IsNeedChangeSizeDelta").boolValue);
        foreach (HarvestRewardsHud hud in view.GetComponentsInChildren<HarvestRewardsHud>(true)) hud.enabled = false;
        foreach (LevelBoxReward box in view.GetComponentsInChildren<LevelBoxReward>(true)) box.gameObject.SetActive(false);
        foreach (Orange.DynamicCanvasLayer layer in view.GetComponentsInChildren<Orange.DynamicCanvasLayer>(true)) layer.SetLayer(0);
        view.SetActive(true);
        prepare(view);
        RenderTexture render = new RenderTexture(1080, height, 24);
        Texture2D capture = null;
        try
        {
            camera.targetTexture = render;
            camera.aspect = 1080f / height;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(viewRect);
            foreach (TMP_Text text in view.GetComponentsInChildren<TMP_Text>(true))
            {
                text.ForceMeshUpdate(true);
                bool audit = auditAllText || text.GetComponentInParent<HarvestRewardsHud>() != null;
                if (audit && text.gameObject.activeInHierarchy && text.isTextOverflowing)
                    LayoutErrors.Add("Harvest text overflows in " + imageName + ": " + text.name);
            }
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture.active = render;
            capture = new Texture2D(render.width, render.height, TextureFormat.RGB24, false);
            capture.ReadPixels(new Rect(0, 0, render.width, render.height), 0, 0);
            capture.Apply();
            string output = Path.GetFullPath(Path.Combine(Application.dataPath, "../Tests/HarvestRewards/Artifacts"));
            Directory.CreateDirectory(output);
            File.WriteAllBytes(Path.Combine(output, imageName + ".png"), capture.EncodeToPNG());
            Debug.Log("HARVEST_RENDER_OK: " + imageName + " (1080x" + height + ")");
        }
        finally
        {
            RenderTexture.active = null;
            camera.targetTexture = null;
            if (capture != null) UnityEngine.Object.DestroyImmediate(capture);
            UnityEngine.Object.DestroyImmediate(render);
            UnityEngine.Object.DestroyImmediate(view);
            UnityEngine.Object.DestroyImmediate(canvasObject);
            UnityEngine.Object.DestroyImmediate(cameraObject);
        }
    }
}
