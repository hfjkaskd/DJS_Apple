using System;
using System.Collections.Generic;
using System.IO;
using FruitWithdrawal;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Read-only saved-prefab checks. Renders disabled HUD fixtures without a service or player storage.</summary>
public static class WithdrawalHudValidation
{
    private const string CorePath = "Assets/res/local/coreplay/CorePlayUI.prefab";
    private const string HudPath = "Assets/res/local/withdrawal/WithdrawalHud.prefab";
    private const string BackgroundPath = "Assets/res_server/server_maps/map1008/Map1008.png";
    private const string OutputPath = "C:/Projects/DJS_Apple/withdrawal-hud-validation";

    [Serializable]
    private sealed class Report
    {
        public string generatedUtc;
        public string scope = "Saved CorePlayUI and nested WithdrawalHud prefab validation. Original Map1008 background and fixed balance/Level 4 fixtures; not a player session. Runtime components never run, and no withdrawal service or player storage is accessed. Uses Game.unity Expand canvas scaling (1080x2340) and the production BaseUI safe-area helper.";
        public List<string> errors = new List<string>();
        public List<RenderReport> renders = new List<RenderReport>();
    }

    [Serializable]
    private sealed class RenderReport
    {
        public int width;
        public int height;
        public float canvasScale;
        public string image;
        public string cash = "$500.00";
        public string diamonds = "50,600";
        public Rect cashBounds;
        public Rect diamondBounds;
        public Vector2 safeAreaTopBottom;
        public List<string> errors = new List<string>();
    }

    [MenuItem("Tools/Withdrawal/Validate Top Currency HUD")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play mode before validating saved HUD prefabs.");
        Directory.CreateDirectory(OutputPath);
        var report = new Report { generatedUtc = DateTime.UtcNow.ToString("O") };
        GameObject core = AssetDatabase.LoadAssetAtPath<GameObject>(CorePath);
        GameObject hud = AssetDatabase.LoadAssetAtPath<GameObject>(HudPath);
        AuditSource(core, true, report.errors);
        AuditSource(hud, false, report.errors);
        if (core != null && core.GetComponentsInChildren<WithdrawalHud>(true).Length == 1)
        {
            foreach (Vector2Int size in new[] { new Vector2Int(1080, 1920), new Vector2Int(1080, 2340), new Vector2Int(720, 1600) })
            {
                var item = new RenderReport { width = size.x, height = size.y };
                report.renders.Add(item);
                try { Render(core, item); }
                catch (Exception exception) { item.errors.Add(exception.ToString()); }
                foreach (string error in item.errors) report.errors.Add(size.x + "x" + size.y + ": " + error);
            }
        }
        string path = Path.Combine(OutputPath, "withdrawal-hud-report.json");
        File.WriteAllText(path, JsonUtility.ToJson(report, true));
        if (report.errors.Count != 0)
            throw new InvalidOperationException("HUD validation failed with " + report.errors.Count + " errors. See " + path);
        Debug.Log("[WithdrawalHudValidation] Passed " + report.renders.Count + " native-background HUD renders. " + path);
    }

    private static void AuditSource(GameObject source, bool gameplay, List<string> errors)
    {
        if (source == null) { errors.Add("Missing saved " + (gameplay ? CorePath : HudPath)); return; }
        WithdrawalHud[] huds = source.GetComponentsInChildren<WithdrawalHud>(true);
        if (huds.Length != 1) { errors.Add(source.name + " must contain exactly one WithdrawalHud."); return; }
        WithdrawalHud hud = huds[0];
        if (!hud.enabled || !ActiveUnder(hud.transform, source.transform)) errors.Add(source.name + ": HUD is disabled.");
        AuditButton(hud.CashButton, "Cash", source.transform, errors);
        AuditButton(hud.DiamondButton, "Diamonds", source.transform, errors);
        if (hud.RewardsButton == null || hud.Rewards == null) errors.Add(source.name + ": rewards bindings are incomplete.");
        if (hud.Cash == null || hud.Diamonds == null) errors.Add(source.name + ": currency text bindings are incomplete.");
        else
        {
            if (!ActiveUnder(hud.Cash.transform, source.transform) || !ActiveUnder(hud.Diamonds.transform, source.transform))
                errors.Add(source.name + ": a currency balance label is hidden.");
            if (hud.Cash == hud.Diamonds) errors.Add(source.name + ": currencies share the same label.");
        }
        if (!gameplay) return;
        CorePlay.CorePlayUI controller = source.GetComponent<CorePlay.CorePlayUI>();
        if (controller == null) errors.Add("CorePlayUI controller is missing.");
        else
        {
            SerializedProperty legacy = new SerializedObject(controller).FindProperty("m_CommonCoinBtn");
            if (legacy == null || legacy.objectReferenceValue != null)
                errors.Add("CorePlayUI must have no serialized m_CommonCoinBtn reference that can reactivate the old balance.");
        }
        foreach (CommonCoinBtn coin in source.GetComponentsInChildren<CommonCoinBtn>(true))
            if (ActiveUnder(coin.transform, source.transform)) errors.Add("Old CommonCoinBtn remains active: " + Hierarchy(coin.transform));
    }

    private static void AuditButton(Button button, string currency, Transform root, List<string> errors)
    {
        if (button == null) { errors.Add(root.name + ": missing " + currency + " Button."); return; }
        if (!button.enabled || !button.interactable || !ActiveUnder(button.transform, root))
            errors.Add(root.name + ": " + currency + " Button is disabled or hidden.");
        Graphic graphic = button.targetGraphic;
        if (graphic == null || !graphic.raycastTarget || !graphic.enabled
            || (graphic.transform != button.transform && !graphic.transform.IsChildOf(button.transform)))
            errors.Add(root.name + ": " + currency + " Button must own its visible raycast target.");
        if (button.onClick.GetPersistentEventCount() != 0)
            errors.Add(root.name + ": " + currency + " Button has Inspector event bindings.");
    }

    private static bool ActiveUnder(Transform value, Transform root)
    {
        for (Transform current = value; current != null; current = current.parent)
        {
            if (!current.gameObject.activeSelf) return false;
            if (current == root) return true;
        }
        return false;
    }

    private static void Render(GameObject source, RenderReport report)
    {
        Scene scene = EditorSceneManager.NewPreviewScene();
        GameObject cameraObject = null, canvasObject = null;
        RenderTexture target = null;
        Texture2D output = null;
        RenderTexture previous = RenderTexture.active;
        Application.LogCallback log = (message, trace, type) =>
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message);
        };
        Application.logMessageReceived += log;
        try
        {
            cameraObject = new GameObject("HudValidationCamera");
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false; camera.scene = scene; camera.orthographic = true;
            camera.orthographicSize = report.height * .5f;
            camera.transform.position = new Vector3(0, 0, -10);
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black;
            camera.nearClipPlane = .01f; camera.farClipPlane = 100;
            camera.allowHDR = camera.allowMSAA = false;
            target = new RenderTexture(report.width, report.height, 24, RenderTextureFormat.ARGB32);
            target.antiAliasing = 1; target.Create(); camera.targetTexture = target;
            camera.aspect = (float)report.width / report.height;

            canvasObject = new GameObject("HudValidationCanvas", typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(canvasObject, scene);
            canvasObject.SetActive(false);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
            // Game.unity: ScaleWithScreenSize, reference 1080x2340, Expand. Explicit scale avoids batch Screen size.
            report.canvasScale = Mathf.Min(report.width / 1080f, report.height / 2340f);
            canvas.scaleFactor = report.canvasScale; canvas.referencePixelsPerUnit = 100;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;
            AddBackground(canvas.transform, report);

            GameObject instance = Object.Instantiate(source, canvas.transform, false);
            instance.name = source.name;
            WithdrawalHud hud = instance.GetComponentInChildren<WithdrawalHud>(true);
            // Parent stays inactive until all runtime behaviour has been removed or disabled.
            // WithdrawalHud has no Awake initialization; its OnEnable/OnDisable service binding remains disabled.
            hud.enabled = false;
            foreach (MonoBehaviour component in instance.GetComponentsInChildren<MonoBehaviour>(true))
                if (component != null && component != hud && !(component is UIBehaviour)) Object.DestroyImmediate(component);
            foreach (Animation animation in instance.GetComponentsInChildren<Animation>(true)) animation.enabled = false;
            foreach (Animator animator in instance.GetComponentsInChildren<Animator>(true)) animator.enabled = false;
            Transform top = instance.transform.Find("UI/Top");
            if (top == null) throw new InvalidOperationException("CorePlayUI/UI/Top is missing.");
            KeepHeaderBranches(instance.transform, top, hud.transform);
            Transform level = top.Find("Level");
            if (level != null)
            {
                for (int i = 0; i < level.childCount; i++) level.GetChild(i).gameObject.SetActive(level.GetChild(i).name == "Bg_Normal");
                foreach (TMP_Text text in level.GetComponentsInChildren<TMP_Text>(true)) text.text = "4";
            }
            Orange.SafeAreaSim.InsetRect((RectTransform)instance.transform);
            report.safeAreaTopBottom = new Vector2(Orange.SafeAreaSim.Top, Orange.SafeAreaSim.Bottom);
            hud.Cash.text = report.cash; hud.Diamonds.text = report.diamonds;
            if (hud.Rewards != null) hud.Rewards.text = "Rewards";
            canvasObject.SetActive(true); instance.SetActive(true);
            Canvas.ForceUpdateCanvases();
            foreach (TMP_Text text in instance.GetComponentsInChildren<TMP_Text>(false)) text.ForceMeshUpdate(true, true);
            Canvas.ForceUpdateCanvases();
            camera.Render();
            AuditRender(hud, top, canvas, camera, report);

            RenderTexture.active = target;
            output = new Texture2D(report.width, report.height, TextureFormat.RGBA32, false);
            output.ReadPixels(new Rect(0, 0, report.width, report.height), 0, 0); output.Apply(false, false);
            report.image = Path.Combine(OutputPath, "TopCurrencyHUD-" + report.width + "x" + report.height + ".png");
            File.WriteAllBytes(report.image, output.EncodeToPNG());
            // Existing highest configured withdrawal amounts exercise practical balance widths without altering screenshots.
            CheckAmount(hud.Cash, "$2,000.00", report);
            CheckAmount(hud.Diamonds, "200,000", report);
            CheckAmount(hud.Cash, "$0.00", report);
            CheckAmount(hud.Diamonds, "0", report);
        }
        finally
        {
            Application.logMessageReceived -= log;
            RenderTexture.active = previous;
            if (cameraObject != null) cameraObject.GetComponent<Camera>().targetTexture = null;
            if (canvasObject != null) Object.DestroyImmediate(canvasObject);
            if (cameraObject != null) Object.DestroyImmediate(cameraObject);
            if (target != null) { target.Release(); Object.DestroyImmediate(target); }
            if (output != null) Object.DestroyImmediate(output);
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }

    private static void KeepHeaderBranches(Transform parent, Transform top, Transform hud)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child == top || child == hud) continue;
            if (top.IsChildOf(child) || hud.IsChildOf(child)) KeepHeaderBranches(child, top, hud);
            else child.gameObject.SetActive(false);
        }
    }

    private static void AddBackground(Transform parent, RenderReport report)
    {
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(BackgroundPath);
        if (texture == null) throw new InvalidOperationException("Original Map1008 background is missing.");
        var background = new GameObject("OriginalMap1008ValidationBackground", typeof(RectTransform), typeof(RawImage));
        background.transform.SetParent(parent, false);
        var rect = (RectTransform)background.transform;
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
        RawImage image = background.GetComponent<RawImage>(); image.texture = texture; image.raycastTarget = false;
        float imageAspect = (float)texture.width / texture.height, renderAspect = (float)report.width / report.height;
        float width = Mathf.Min(1, renderAspect / imageAspect), height = Mathf.Min(1, imageAspect / renderAspect);
        image.uvRect = new Rect((1 - width) * .5f, (1 - height) * .5f, width, height);
    }

    private static void AuditRender(WithdrawalHud hud, Transform top, Canvas canvas, Camera camera, RenderReport report)
    {
        if (hud.enabled) report.errors.Add("Static fixture unexpectedly enabled WithdrawalHud.");
        report.cashBounds = ScreenBounds((RectTransform)hud.CashButton.transform, camera);
        report.diamondBounds = ScreenBounds((RectTransform)hud.DiamondButton.transform, camera);
        if (report.cashBounds.Overlaps(report.diamondBounds)) report.errors.Add("Cash and diamond Button rectangles overlap.");
        foreach (Rect bounds in new[] { report.cashBounds, report.diamondBounds })
        {
            if (bounds.xMin < -.5f || bounds.xMax > report.width + .5f || bounds.yMin < -.5f || bounds.yMax > report.height + .5f)
                report.errors.Add("A currency button extends outside the canvas.");
            if (bounds.yMax > report.height - Orange.SafeAreaSim.Top * report.canvasScale + 1)
                report.errors.Add("A currency button extends into the top safe area.");
        }
        foreach (string name in new[] { "SettingBtn", "Level/Bg_Normal" })
        {
            Transform native = top.Find(name);
            if (native == null || !native.gameObject.activeInHierarchy) continue;
            Rect bounds = ScreenBounds((RectTransform)native, camera);
            if (report.cashBounds.Overlaps(bounds) || report.diamondBounds.Overlaps(bounds))
                report.errors.Add("Currency button overlaps native " + name + ".");
        }
        foreach (TMP_Text text in hud.GetComponentsInChildren<TMP_Text>(false)) CheckText(text, report);
        if (Mathf.Abs(((RectTransform)canvas.transform).rect.width * canvas.scaleFactor - report.width) > 1)
            report.errors.Add("Canvas scale or dimensions differ from the requested output.");
    }

    private static void CheckAmount(TMP_Text text, string amount, RenderReport report)
    {
        text.text = amount; text.ForceMeshUpdate(true, true); CheckText(text, report);
    }

    private static void CheckText(TMP_Text text, RenderReport report)
    {
        if (text.font == null || text.fontSharedMaterial == null) { report.errors.Add(Hierarchy(text.transform) + ": missing font/material."); return; }
        if (text.isTextOverflowing) report.errors.Add(Hierarchy(text.transform) + ": overflowing text " + text.text);
        if (!string.IsNullOrEmpty(text.text) && (text.textInfo == null || text.textInfo.characterCount == 0))
            report.errors.Add(Hierarchy(text.transform) + ": nonempty label generated no text.");
    }

    private static Rect ScreenBounds(RectTransform transform, Camera camera)
    {
        var corners = new Vector3[4]; transform.GetWorldCorners(corners);
        Vector2 min = camera.WorldToScreenPoint(corners[0]), max = min;
        for (int i = 1; i < corners.Length; i++)
        {
            Vector2 point = camera.WorldToScreenPoint(corners[i]); min = Vector2.Min(min, point); max = Vector2.Max(max, point);
        }
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private static string Hierarchy(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null) { transform = transform.parent; path = transform.name + "/" + path; }
        return path;
    }
}
