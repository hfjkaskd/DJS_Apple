using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Batch validation of the shipped assets, independent of any player save.</summary>
public static class HarvestRewardValidation
{
    public static void ValidateAndRender()
    {
        HarvestRewardConfig config = AssetDatabase.LoadAssetAtPath<HarvestRewardConfig>("Assets/Resources/HarvestRewardConfig.asset");
        if (config == null) throw new InvalidOperationException("Harvest config is missing.");
        config.Settings.Validate();
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/res/local/harvest/HarvestRewardsUI.prefab");
        if (prefab == null || prefab.GetComponent<HarvestRewardsUI>() == null)
            throw new InvalidOperationException("Harvest prefab/controller is missing.");
        Button[] buttons = prefab.GetComponentsInChildren<Button>(true);
        if (buttons.Length < 5) throw new InvalidOperationException("Harvest navigation/claim/apply buttons are missing.");
        foreach (Button button in buttons)
        {
            if (button.targetGraphic == null || button.targetGraphic.gameObject != button.gameObject)
                throw new InvalidOperationException("Button must own its visible target: " + button.name);
            if (button.onClick.GetPersistentEventCount() != 0)
                throw new InvalidOperationException("Use code-bound button events: " + button.name);
        }
        foreach (Transform child in prefab.GetComponentsInChildren<Transform>(true))
            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) != 0)
                throw new InvalidOperationException("Missing script on " + child.name);
        Render(prefab, "harvest-center");
        ValidateEntry("Assets/res/local/home/Home.prefab", "harvest-home");
        ValidateEntry("Assets/res/local/coreplay/CorePlayUI.prefab", "harvest-gameplay");
        ValidateEntry("Assets/res/local/coreplaywin/WinUI.prefab", "harvest-win");
        Debug.Log("HARVEST_ASSETS_OK: config, prefab, standard Buttons and render validated.");
    }

    private static void ValidateEntry(string path, string imageName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        bool found = false;
        foreach (Button button in prefab.GetComponentsInChildren<Button>(true))
            if (button.name == "HarvestRewardsEntry") found = true;
        if (!found) throw new InvalidOperationException("Missing harvest entry in " + path);
        Render(prefab, imageName);
    }

    private static void Render(GameObject prefab, string imageName)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject cameraObject = new GameObject("ValidationCamera", typeof(Camera));
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.08f, .15f, .1f);
        camera.orthographic = true;
        camera.orthographicSize = 1170;
        camera.nearClipPlane = .1f;
        camera.farClipPlane = 500f;
        camera.transform.position = new Vector3(0, 0, -100);
        GameObject canvasObject = new GameObject("ValidationCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = camera;
        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1080, 2340);
        canvasRect.position = Vector3.zero;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 2340);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        // Match MgrUI's instantiate-with-parent path, preventing a nested Canvas
        // from briefly becoming a standalone screen Canvas and resizing its RectTransform.
        GameObject view = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvasObject.transform);
        RectTransform viewRect = view.transform as RectTransform;
        viewRect.anchorMin = Vector2.zero;
        viewRect.anchorMax = Vector2.one;
        viewRect.offsetMin = Vector2.zero;
        viewRect.offsetMax = Vector2.zero;
        viewRect.localScale = Vector3.one;
        view.GetComponent<BaseUI>().enabled = false;
        // WinUI.Init normally hides the optional chest modal before showing the win page.
        if (imageName == "harvest-win")
            foreach (LevelBoxReward box in view.GetComponentsInChildren<LevelBoxReward>(true))
                box.gameObject.SetActive(false);
        foreach (Orange.DynamicCanvasLayer layer in view.GetComponentsInChildren<Orange.DynamicCanvasLayer>(true))
            layer.SetLayer(0);
        view.SetActive(true);
        if (imageName == "harvest-center")
        {
            HarvestRewardSettings settings = AssetDatabase.LoadAssetAtPath<HarvestRewardConfig>("Assets/Resources/HarvestRewardConfig.asset").Settings;
            long minimumWait;
            string fullRules = HarvestRewardsUI.CreateRulesText(settings, out minimumWait);
            foreach (TMP_Text text in view.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text.name == "Terms") text.text = fullRules;
                if (text.name == "AvailableValue") text.text = "10,000.00";
                if (text.name == "FrozenValue") text.text = "1,000.00";
                for (int i = 0; i < settings.ThresholdCents.Length; i++)
                    if (text.name == "Tier" + (i + 1) + "Label")
                        text.text = (settings.ThresholdCents[i] / 100m).ToString("N2", System.Globalization.CultureInfo.InvariantCulture)
                            + (i == 0 ? "\nSELECTED" : "");
            }
        }
        RenderTexture render = new RenderTexture(1080, 2340, 24);
        camera.targetTexture = render;
        camera.aspect = 1080f / 2340;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(view.transform as RectTransform);
        foreach (TMP_Text text in view.GetComponentsInChildren<TMP_Text>(true))
        {
            text.ForceMeshUpdate(true);
            if (imageName == "harvest-center" && text.gameObject.activeInHierarchy && text.isTextOverflowing)
                throw new InvalidOperationException("Harvest text overflows its configured bounds: " + text.name);
        }
        Canvas.ForceUpdateCanvases();
        camera.Render();
        RenderTexture.active = render;
        Texture2D capture = new Texture2D(render.width, render.height, TextureFormat.RGB24, false);
        capture.ReadPixels(new Rect(0, 0, render.width, render.height), 0, 0);
        capture.Apply();
        string output = Path.GetFullPath(Path.Combine(Application.dataPath, "../Tests/HarvestRewards/Artifacts"));
        Directory.CreateDirectory(output);
        File.WriteAllBytes(Path.Combine(output, imageName + ".png"), capture.EncodeToPNG());
        RenderTexture.active = null;
        camera.targetTexture = null;
        UnityEngine.Object.DestroyImmediate(capture);
        UnityEngine.Object.DestroyImmediate(render);
    }
}
