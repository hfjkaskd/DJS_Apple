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
    private const string CashSpritePath = "Assets/res/local/harvest/sprite/HarvestCash.png";
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
        AuditRewardSymbols(controller);
        GameObject popup = LoadPrefab(HarvestRewardPopupUI.Path);
        HarvestRewardPopupUI popupController = popup.GetComponent<HarvestRewardPopupUI>();
        if (popupController == null) throw new InvalidOperationException("Harvest reward popup controller is missing.");
        AuditModal(popup, popupController);
        RequireCashIcons(popup, 1);
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
        Render(wallet, "harvest-wallet-large-short", 1920, view => PrepareWallet(view, config.Settings, 0, true), true);
        Render(wallet, "harvest-progress-levels-short", 1920, view => PrepareWallet(view, config.Settings, 2, false, true), true);
        ValidateEntry("home/Home", "harvest-home");
        ValidateEntry("coreplay/CorePlayUI", "harvest-gameplay");
        ValidateEntry("coreplaywin/WinUI", "harvest-win");
        GameObject gameplay = LoadPrefab("coreplay/CorePlayUI");
        Render(gameplay, "harvest-gameplay-extremes", 2340, PrepareGameplayBounds, false);
        Render(gameplay, "harvest-gameplay-extremes-short", 1920, PrepareGameplayBounds, false);
        if (LayoutErrors.Count > 0)
            throw new InvalidOperationException("Harvest layout issues (all captures were saved):\n" + string.Join("\n", LayoutErrors));
        Debug.Log("HARVEST_ASSETS_OK: v3 wallet grid and currency/task symbols, seven wallet pages, reward popup variants, three HUD entries, mainline board bounds, reparent scaling, complete references and standard Buttons validated; 28 captures.");
    }

    /// <summary>
    /// Art-review captures at the concept's 373:756 viewport, enlarged exactly 3x.
    /// This is a visual comparison fixture, not a claim of pixel-perfect acceptance.
    /// </summary>
    public static void RenderReferenceComparison()
    {
        LayoutErrors.Clear();
        ReadProductionCanvas();
        ReadBoardConfigurations();
        HarvestRewardConfig config = AssetDatabase.LoadAssetAtPath<HarvestRewardConfig>("Assets/Resources/HarvestRewardConfig.asset");
        if (config == null) throw new InvalidOperationException("Harvest config is missing.");
        config.Settings.Validate();
        GameObject wallet = LoadPrefab(HarvestRewardsUI.Path);
        Render(wallet, "wallet-reference", 2268, view => PrepareWallet(view, config.Settings, 0), true, 1119, true);
        Render(wallet, "progress-reference", 2268, view => PrepareWallet(view, config.Settings, 2), true, 1119, true);
        Render(LoadPrefab(HarvestRewardPopupUI.Path), "popup-reference", 2268, PrepareReferencePopup, true, 1119, true);
        Render(LoadPrefab("coreplay/CorePlayUI"), "hud-reference", 2268, PrepareReferenceGameplay, false, 1119);
        if (LayoutErrors.Count > 0)
            throw new InvalidOperationException("Reference capture layout issues (all four images were saved):\n" + string.Join("\n", LayoutErrors));
        Debug.Log("HARVEST_REFERENCE_RENDERED: four 1119x2268 comparison frames with actual Map1008, level 10004 and authored UI; visual acceptance requires review against the v3 concept.");
    }

    private static void PrepareReferenceGameplay(GameObject view)
    {
        PrepareHud(view, false);
        var ui = view.GetComponent<CorePlay.CorePlayUI>();
        Animation animation = view.GetComponent<Animation>();
        AnimationClip opening = animation != null && ui != null ? animation.GetClip(ui.m_OpenAniName) : null;
        if (opening != null) opening.SampleAnimation(view, opening.length);
        Sprite backdrop = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/res_server/server_maps/map1008/Map1008.asset");
        if (ui == null || ui.m_BgImg == null || backdrop == null)
            throw new InvalidOperationException("Reference capture requires the actual CorePlayUI background and Map1008 Sprite.");
        ui.m_BgImg.sprite = backdrop;
        ui.m_BgImg.color = Color.white;
        ui.SetGameAreaAlpha(1f);
        var serialized = new SerializedObject(ui);
        int levelLabelCount = serialized.FindProperty("m_LevelTexts").arraySize;
        for (int i = 0; i < levelLabelCount; i++)
            ArrayReference<TMP_Text>(serialized, "m_LevelTexts", i).text = "Level 4";
        int levelRootCount = serialized.FindProperty("m_LevelTextGos").arraySize;
        for (int i = 0; i < levelRootCount; i++)
            ArrayReference<GameObject>(serialized, "m_LevelTextGos", i).SetActive(i == 0);
        CommonCoinBtn coin = Reference<CommonCoinBtn>(serialized, "m_CommonCoinBtn");
        SetText(new SerializedObject(coin), "m_CoinCount", "100");

        LevelConfig level = MainLevels.Find(candidate => candidate.levelID == "10004");
        if (level == null || level.normalTiles == null || level.normalTiles.Count == 0)
            throw new InvalidOperationException("The actual reference level 10004 is unavailable.");
        GameObject fruitPrefab = LoadPrefab("coreplay/prefab/CollectItem");
        var tiles = new List<SingleNormalTile>(level.normalTiles);
        tiles.Sort((left, right) => left.layerIndex != right.layerIndex
            ? left.layerIndex.CompareTo(right.layerIndex) : left.indexInLayer.CompareTo(right.indexInLayer));
        foreach (SingleNormalTile tile in tiles)
        {
            GameObject fruit = (GameObject)PrefabUtility.InstantiatePrefab(fruitPrefab, ui.m_GameLayer);
            fruit.name = "Reference_Level10004_Tile" + tile.tileID;
            ((RectTransform)fruit.transform).anchoredPosition = new Vector2(tile.posX, tile.posY);
            CollectItem item = fruit.GetComponent<CollectItem>();
            SingleEleImageConfig size;
            if (!FruitSizes.TryGetValue(tile.eleType, out size)) throw new InvalidOperationException("Missing reference fruit geometry: " + tile.eleType);
            item.CollectImageRect.sizeDelta = new Vector2(size.sizeX, size.sizeY);
            Image graphic = item.CollectImageRect.GetComponent<Image>();
            graphic.sprite = CollectItem.GetItemSprite(tile.eleType);
            graphic.color = Color.white;
            if (graphic.sprite == null) throw new InvalidOperationException("Missing reference fruit Sprite: " + tile.eleType);
            foreach (Orange.DynamicCanvasLayer layer in fruit.GetComponentsInChildren<Orange.DynamicCanvasLayer>(true)) layer.SetLayer(0);
        }
    }

    private static void PrepareReferencePopup(GameObject view)
    {
        // An ordinary level reward keeps the unavailable-ad option visible. The concept's
        // 247.61 -> 257.61 balance is a +10 reward, not its inconsistent +100 lettering.
        PreparePopup(view, 1);
        var serialized = new SerializedObject(view.GetComponent<HarvestRewardPopupUI>());
        SetText(serialized, "titleText", serialized.FindProperty("winTitle").stringValue);
        SetText(serialized, "amountText", "+10.00");
        SetText(serialized, "balanceText", string.Format(CultureInfo.InvariantCulture,
            serialized.FindProperty("balanceAfterClaimFormat").stringValue, "257.61"));
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
        HarvestReferenceLayout layout = prefab.GetComponent<HarvestReferenceLayout>();
        if (layout == null) throw new InvalidOperationException("Modal needs its authored reference layout: " + prefab.name);
        AuditReferences(layout);
        RectTransform dimmer = Reference<RectTransform>(new SerializedObject(layout), "dimmerRoot");
        Image blocker = dimmer.GetComponent<Image>();
        if (dimmer.parent != prefab.transform || dimmer.GetSiblingIndex() != 0 ||
            blocker == null || !blocker.raycastTarget || blocker.color.a <= 0f)
            throw new InvalidOperationException("Modal needs a visible, raycastable Dimmer as its first child: " + prefab.name);
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
        RequireArrayLength(serialized, "pageTitles", PageFields.Length);
        for (int i = 0; i < PageFields.Length; i++)
            if (string.IsNullOrWhiteSpace(serialized.FindProperty("pageTitles").GetArrayElementAtIndex(i).stringValue))
                throw new InvalidOperationException("Every wallet page needs a configured title: " + PageFields[i]);
        AuditDisplaySlider(Reference<Slider>(serialized, "walletProgressBar"), "wallet progress");
        AuditDisplaySlider(Reference<Slider>(serialized, "stageProgressBar"), "stage progress");
        if (Reference<GameObject>(serialized, "stageTaskFruitsRoot") == Reference<GameObject>(serialized, "stageTaskLevelsRoot"))
            throw new InvalidOperationException("Triple and level tasks require distinct authored visual roots.");
    }

    private static void AuditDisplaySlider(Slider slider, string context)
    {
        if (slider.fillRect == null || slider.interactable || slider.onValueChanged.GetPersistentEventCount() != 0)
            throw new InvalidOperationException("A configured display-only Slider is required for " + context);
    }

    private static void RequireCashIcons(GameObject root, int minimum)
    {
        Sprite cash = AssetDatabase.LoadAssetAtPath<Sprite>(CashSpritePath);
        if (cash == null) throw new InvalidOperationException("The v3 cash Sprite is missing or not imported as a Sprite: " + CashSpritePath);
        int found = 0;
        foreach (Image image in root.GetComponentsInChildren<Image>(true))
        {
            if (image.name != "HarvestCash" && image.name != "CashIcon") continue;
            if (image.sprite != cash || image.color.a <= 0f)
                throw new InvalidOperationException("Reward amount icon must display the dedicated cash Sprite: " + root.name + "/" + image.name);
            found++;
        }
        if (found < minimum) throw new InvalidOperationException("Missing v3 cash symbols in " + root.name + ": expected at least " + minimum + ", found " + found);
    }

    private static void AuditRewardSymbols(HarvestRewardsUI controller)
    {
        var serialized = new SerializedObject(controller);
        RequireCashIcons(Reference<GameObject>(serialized, "walletRoot"), 7);
        foreach (string field in new[] { "confirmationRoot", "progressRoot", "settlementRoot", "completedRoot", "historyRoot" })
            RequireCashIcons(Reference<GameObject>(serialized, field), 1);
        Sprite fruit = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Fruit_4.asset");
        if (fruit == null) throw new InvalidOperationException("The existing fruit-task Sprite is missing.");
        GameObject fruitRoot = Reference<GameObject>(serialized, "stageTaskFruitsRoot");
        int fruitCount = 0;
        foreach (Image image in fruitRoot.GetComponentsInChildren<Image>(true))
        {
            if (!image.name.StartsWith("TaskFruit", StringComparison.Ordinal)) continue;
            if (image.sprite != fruit) throw new InvalidOperationException("Triple-task symbols must keep the existing fruit Sprite: " + image.name);
            fruitCount++;
        }
        if (fruitCount != 3) throw new InvalidOperationException("The triple task must visibly retain its three fruit symbols.");
    }

    private static void AuditWalletGrid(GameObject view, string imageName)
    {
        HarvestRewardsUI controller = view.GetComponent<HarvestRewardsUI>();
        if (controller == null) return;
        var serialized = new SerializedObject(controller);
        GameObject wallet = Reference<GameObject>(serialized, "walletRoot");
        if (!wallet.activeInHierarchy) return;
        RequireArrayLength(serialized, "tierButtons", 6);
        Rect page = WorldRect((RectTransform)wallet.transform);
        Rect[] cards = new Rect[6];
        for (int i = 0; i < cards.Length; i++)
        {
            Button button = ArrayReference<Button>(serialized, "tierButtons", i);
            TMP_Text label = ArrayReference<TMP_Text>(serialized, "tierLabels", i);
            if (!label.transform.IsChildOf(button.transform)) throw new InvalidOperationException("A tier label must belong to its visible Button: " + label.name);
            cards[i] = WorldRect((RectTransform)button.transform);
            if (!ContainsRect(page, cards[i], 1f)) LayoutErrors.Add(imageName + ": tier card escapes the wallet page: " + i);
            for (int previous = 0; previous < i; previous++)
                if (cards[i].Overlaps(cards[previous])) LayoutErrors.Add(imageName + ": tier cards overlap: " + previous + "/" + i);
        }
        for (int row = 0; row < 3; row++)
        {
            Rect left = cards[row * 2], right = cards[row * 2 + 1];
            if (left.xMax >= right.xMin || Mathf.Abs(left.center.y - right.center.y) > 1f ||
                Mathf.Abs(left.center.x - cards[0].center.x) > 1f || Mathf.Abs(right.center.x - cards[1].center.x) > 1f ||
                (row > 0 && left.yMax >= cards[(row - 1) * 2].yMin))
                LayoutErrors.Add(imageName + ": withdrawal tiers must form two aligned columns and three separate rows in configured order.");
        }
    }

    private static bool ContainsRect(Rect outer, Rect inner, float tolerance)
    {
        return inner.xMin >= outer.xMin - tolerance && inner.xMax <= outer.xMax + tolerance &&
            inner.yMin >= outer.yMin - tolerance && inner.yMax <= outer.yMax + tolerance;
    }

    private static void ValidateEntry(string path, string imageName)
    {
        GameObject prefab = LoadPrefab(path);
        HarvestRewardsHud[] huds = prefab.GetComponentsInChildren<HarvestRewardsHud>(true);
        if (huds.Length != 1) throw new InvalidOperationException("Host requires exactly one harvest HUD: " + path);
        AuditHierarchy(huds[0].gameObject);
        AuditReferences(huds[0]);
        RequireCashIcons(huds[0].gameObject, 1);
        var serialized = new SerializedObject(huds[0]);
        Reference<Button>(serialized, "walletButton");
        Reference<TMP_Text>(serialized, "balanceText");
        Reference<TMP_Text>(serialized, "goalText");
        Reference<TMP_Text>(serialized, "goalFractionText");
        Slider progress = Reference<Slider>(serialized, "goalProgressBar");
        if (progress.fillRect == null || progress.interactable || progress.onValueChanged.GetPersistentEventCount() != 0)
            throw new InvalidOperationException("HUD progress requires a configured display-only Slider: " + path);
        RequireArrayLength(serialized, "completedMarks", 6);
        RequireArrayLength(serialized, "currentMarks", 6);
        Render(prefab, imageName, 2340, view => PrepareHud(view, false), false);
        Render(prefab, imageName + "-challenge", 2340, view => PrepareHud(view, true), false);
    }

    private static void PrepareWallet(GameObject view, HarvestRewardSettings settings, int page, bool largeAmount = false, bool levelTask = false)
    {
        var serialized = new SerializedObject(view.GetComponent<HarvestRewardsUI>());
        for (int i = 0; i < PageFields.Length; i++) Reference<GameObject>(serialized, PageFields[i]).SetActive(i == page);
        Reference<Button>(serialized, "backButton").gameObject.SetActive(page != 0 && page != 4);
        Reference<Button>(serialized, "rulesButton").gameObject.SetActive(page != 6);
        Reference<Button>(serialized, "historyButton").gameObject.SetActive(page != 5 && page != 1 && page != 6);
        Reference<Button>(serialized, "walletRulesButton").gameObject.SetActive(page == 0);
        Reference<Button>(serialized, "walletHistoryButton").gameObject.SetActive(page == 0);
        Reference<GameObject>(serialized, "detailFooterRoot").SetActive(page != 0);
        Reference<GameObject>(serialized, "walletJourneyHintRoot").SetActive(true);
        Reference<TMP_Text>(serialized, "feedbackText").gameObject.SetActive(page == 0);
        SetText(serialized, "feedbackText", "");
        SetText(serialized, "titleText", serialized.FindProperty("pageTitles").GetArrayElementAtIndex(page).stringValue);
        Reference<Button>(serialized, "pendingRewardButton").gameObject.SetActive(false);
        SetText(serialized, "availableText", largeAmount ? "9,750.00" : "247.61");
        SetText(serialized, "frozenText", "In challenge: 0.00");
        SetText(serialized, "walletHintText", largeAmount ? "250.00 more to apply" : "752.39 more to apply");
        SetText(serialized, "applyText", "KEEP HARVESTING");
        Reference<Slider>(serialized, "walletProgressBar").gameObject.SetActive(true);
        Reference<TMP_Text>(serialized, "percentageText").gameObject.SetActive(true);
        Reference<Slider>(serialized, "walletProgressBar").SetValueWithoutNotify(largeAmount ? .975f : .24761f);
        SetText(serialized, "percentageText", largeAmount ? "98%" : "25%");
        SetText(serialized, "confirmationAmountText", "1,000.00");
        SerializedProperty summaryFormat = serialized.FindProperty("confirmationSummaryFormat");
        SetText(serialized, "confirmationSummaryText", string.Format(CultureInfo.InvariantCulture, summaryFormat.stringValue, "1,000.00", 6, "168", "13"));
        int stage = levelTask ? 0 : 1;
        SetText(serialized, "stageTitleText", "Stage " + (stage + 1) + " of " + settings.Stages.Length);
        SetText(serialized, "stageRequestAmountText", string.Format(CultureInfo.InvariantCulture, serialized.FindProperty("stageRequestAmountFormat").stringValue, "1,000.00"));
        SetText(serialized, "stageTaskText", HarvestRewardsUI.CreateTaskText(settings.Stages[stage]));
        SetText(serialized, "stageCountText", levelTask ? "3 / 10" : "86 / 260");
        Reference<Slider>(serialized, "stageProgressBar").SetValueWithoutNotify(levelTask ? .3f : 86f / 260f);
        SetText(serialized, "stagePercentageText", levelTask ? "30%" : "33%");
        Reference<GameObject>(serialized, "stageTaskFruitsRoot").SetActive(!levelTask);
        Reference<GameObject>(serialized, "stageTaskLevelsRoot").SetActive(levelTask);
        SetText(serialized, "stageTimerText", HarvestRewardsUI.FormatDuration(18 * 3600 + 42 * 60 + 10));
        SetText(serialized, "stageHintText", "Complete both the task and timer");
        SetText(serialized, "settlementAmountText", "10,000.00");
        SetText(serialized, "settlementTimerText", HarvestRewardsUI.FormatDuration(7 * 86400 - 1));
        SetText(serialized, "completedAmountText", "10,000.00");
        SetText(serialized, "completedDetailText", "All harvest withdrawal tiers are complete. No cash was paid.");
        SetText(serialized, "historyText", "10,000.00  -  Completed\n2026-09-15 23:59 UTC\n\n8,000.00  -  Completed\n2026-09-01 23:59 UTC\n\n5,000.00  -  Completed\n2026-08-15 23:59 UTC\n\n3,000.00  -  Completed\n2026-08-01 23:59 UTC\n\n1,500.00  -  Completed\n2026-07-15 23:59 UTC\n\n1,000.00  -  Completed\n2026-07-01 23:59 UTC");
        long minimumWait;
        SetText(serialized, "rulesText", HarvestRewardsUI.CreateRulesText(settings, out minimumWait));
        for (int i = 0; i < settings.ThresholdCents.Length; i++)
        {
            ArrayReference<TMP_Text>(serialized, "tierLabels", i).text = HarvestRewardsUI.FormatTierAmount(settings.ThresholdCents[i])
                + (largeAmount && i < 5 ? "\n" + serialized.FindProperty("completedTierLabel").stringValue : "");
            ArrayReference<GameObject>(serialized, "tierSelectedMarkers", i).SetActive(i == (largeAmount ? 5 : 0));
            ArrayReference<GameObject>(serialized, "stageCompleteMarks", i).SetActive(i < stage);
            ArrayReference<GameObject>(serialized, "stageCurrentMarks", i).SetActive(i == stage);
        }
    }

    private static void PreparePopup(GameObject view, int kind)
    {
        var serialized = new SerializedObject(view.GetComponent<HarvestRewardPopupUI>());
        SetText(serialized, "titleText", serialized.FindProperty(kind == 0 ? "welcomeTitle" : kind == 1 ? "fruitTitle" : "winTitle").stringValue);
        SetText(serialized, "amountText", kind == 0 ? "+20.00" : kind == 1 ? "+5.00" : "+100.00");
        SetText(serialized, "balanceText", string.Format(CultureInfo.InvariantCulture,
            serialized.FindProperty(kind == 0 ? "currentBalanceFormat" : "balanceAfterClaimFormat").stringValue,
            kind == 0 ? "20.00" : kind == 1 ? "252.61" : "347.61"));
        SetText(serialized, "hintText", serialized.FindProperty(kind == 0 ? "welcomeHint" : "rewardHint").stringValue);
        SetText(serialized, "claimText", kind == 0 ? "CONTINUE" : "COLLECT");
        SetText(serialized, "adText", "Ad unavailable");
        Reference<Button>(serialized, "adButton").gameObject.SetActive(kind == 1);
        Reference<Button>(serialized, "adButton").interactable = false;
        Reference<Button>(serialized, "claimButton").interactable = true;
        Reference<Button>(serialized, "closeButton").interactable = true;
    }

    private static void PrepareHud(GameObject view, bool challenge)
    {
        var serialized = new SerializedObject(view.GetComponentInChildren<HarvestRewardsHud>(true));
        SetText(serialized, "balanceText", "247.61");
        SetText(serialized, "goalText", challenge ? "Stage 2: 86 / 260 triples" : "Earn 752.39 more to apply");
        SetText(serialized, "goalFractionText", challenge ? "86 / 260" : "247.61 / 1,000");
        Reference<Slider>(serialized, "goalProgressBar").SetValueWithoutNotify(challenge ? 86f / 260f : .24761f);
        Reference<Slider>(serialized, "goalProgressBar").gameObject.SetActive(!challenge);
        Reference<TMP_Text>(serialized, "goalFractionText").gameObject.SetActive(!challenge);
        Reference<GameObject>(serialized, "stageTrackRoot").SetActive(challenge);
        for (int i = 0; i < 6; i++)
        {
            ArrayReference<GameObject>(serialized, "completedMarks", i).SetActive(challenge && i < 1);
            ArrayReference<GameObject>(serialized, "currentMarks", i).SetActive(challenge && i == 1);
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

    private static GameObject CreateCaptureView(GameObject prefab, Transform canvasParent)
    {
        GameObject view = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvasParent);
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
        return view;
    }

    private static void Render(GameObject prefab, string imageName, int height, Action<GameObject> prepare, bool auditAllText,
        int width = 1080, bool gameplayBackdrop = false)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        // Use the production Expand policy and the requested viewport, not a new UI scale.
        float productionScale = Mathf.Min(width / ProductionReferenceResolution.x, height / ProductionReferenceResolution.y);
        Vector2 logicalSize = new Vector2(width / productionScale, height / productionScale);
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
        GameObject backdrop = null;
        if (gameplayBackdrop)
        {
            backdrop = CreateCaptureView(LoadPrefab("coreplay/CorePlayUI"), canvasObject.transform);
            PrepareReferenceGameplay(backdrop);
        }
        GameObject view = CreateCaptureView(prefab, canvasObject.transform);
        RectTransform viewRect = (RectTransform)view.transform;
        prepare(view);
        HarvestRewardsUI walletController = view.GetComponent<HarvestRewardsUI>();
        if (backdrop != null && walletController != null &&
            Reference<GameObject>(new SerializedObject(walletController), "progressRoot").activeSelf)
            PrepareHud(backdrop, true);
        foreach (HarvestReferenceLayout layout in canvasObject.GetComponentsInChildren<HarvestReferenceLayout>(true))
            layout.RefreshLayout();
        RenderTexture render = new RenderTexture(width, height, 24);
        Texture2D capture = null;
        try
        {
            camera.targetTexture = render;
            camera.aspect = (float)width / height;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(viewRect);
            foreach (HarvestReferenceLayout layout in view.GetComponentsInChildren<HarvestReferenceLayout>(true))
            {
                RectTransform dimmer = Reference<RectTransform>(new SerializedObject(layout), "dimmerRoot");
                Rect fullCanvas = WorldRect(canvasRect), covered = WorldRect(dimmer);
                // The modal backdrop intentionally covers the full viewport, including safe-area margins.
                if (!ContainsRect(covered, fullCanvas, 1f) || !ContainsRect(fullCanvas, covered, 1f))
                    LayoutErrors.Add(imageName + ": dimmer must cover the full canvas, including safe-area margins.");
            }
            AuditWalletGrid(view, imageName);
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
            Debug.Log("HARVEST_RENDER_OK: " + imageName + " (" + width + "x" + height + "); logical canvas "
                + logicalSize.x.ToString("F2", CultureInfo.InvariantCulture) + "x" + logicalSize.y.ToString("F2", CultureInfo.InvariantCulture)
                + "; safe area top/bottom " + Orange.SafeAreaSim.Top + "/" + Orange.SafeAreaSim.Bottom);
        }
        finally
        {
            RenderTexture.active = null;
            camera.targetTexture = null;
            if (capture != null) UnityEngine.Object.DestroyImmediate(capture);
            UnityEngine.Object.DestroyImmediate(render);
            UnityEngine.Object.DestroyImmediate(view);
            if (backdrop != null) UnityEngine.Object.DestroyImmediate(backdrop);
            UnityEngine.Object.DestroyImmediate(canvasObject);
            UnityEngine.Object.DestroyImmediate(cameraObject);
        }
    }
}
