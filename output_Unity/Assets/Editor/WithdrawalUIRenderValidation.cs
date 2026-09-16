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
/// WithdrawalHost, access WithdrawalService, invoke Buttons or modify prefabs.
/// </summary>
public static class WithdrawalUIRenderValidation
{
    private const string PanelFolder = "Assets/Resources/WithdrawalImported/Panels";
    private const string OutputFolder = "C:/Projects/DJS_Apple/withdrawal-port-validation";
    private static readonly int[] Heights = { 1920, 2340 };

    [Serializable]
    private sealed class Report
    {
        public string generatedUtc;
        public string scope = "Synthetic presentation fixtures matching runtime text formats; not gameplay evidence. No service initialization, Button invocation or player storage access.";
        public string outputDirectory;
        public List<string> errors = new List<string>();
        public List<string> skippedUnboundPrefabs = new List<string>();
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
        string[] ids = AssetDatabase.FindAssets("t:Prefab", new[] { PanelFolder });
        var paths = new List<string>();
        for (int i = 0; i < ids.Length; i++) paths.Add(AssetDatabase.GUIDToAssetPath(ids[i]));
        paths.Sort(StringComparer.Ordinal);
        bool rulesFound = false;
        int boundPages = 0;
        foreach (string path in paths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                report.errors.Add("Cannot load prefab: " + path);
                continue;
            }
            if (prefab.GetComponent<WithdrawalPageView>() == null)
            {
                report.skippedUnboundPrefabs.Add(path);
                continue;
            }
            boundPages++;
            if (Path.GetFileNameWithoutExtension(path) == "RulesPanel") rulesFound = true;
            foreach (int height in Heights)
            {
                var page = new PageReport { prefab = path, width = 1080, height = height };
                report.pages.Add(page);
                try { Render(prefab, page); }
                catch (Exception exception) { page.renderErrors.Add(exception.ToString()); }
                if (prefab.name == "TXRecordPanel")
                {
                    var bottom = new PageReport { prefab = path, width = 1080, height = height, recordsBottom = true };
                    report.pages.Add(bottom);
                    try { Render(prefab, bottom); }
                    catch (Exception exception) { bottom.renderErrors.Add(exception.ToString()); }
                }
            }
        }
        if (boundPages == 0) report.errors.Add("No bound WithdrawalPageView prefabs found. Run authoring first.");
        if (!rulesFound) report.errors.Add("Bound RulesPanel.prefab is missing.");
        foreach (PageReport page in report.pages)
        {
            int failures = page.missingComponents.Count + page.missingFonts.Count + page.missingTextures.Count
                + page.brokenObjectReferences.Count + page.invalidBindings.Count + page.renderErrors.Count + page.textOverflows.Count;
            if (failures > 0) report.errors.Add(Path.GetFileNameWithoutExtension(page.prefab) + " " + page.height + ": " + failures + " structural/render issues.");
        }
        string reportPath = Path.Combine(OutputFolder, "withdrawal-ui-report.json");
        File.WriteAllText(reportPath, JsonUtility.ToJson(report, true));
        Debug.Log("[WithdrawalUIRenderValidation] " + report.pages.Count + " page renders; report: " + reportPath);
        if (report.errors.Count != 0)
            throw new InvalidOperationException("Static withdrawal UI validation found " + report.errors.Count + " failing page/global checks. See " + reportPath);
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
            // At the two requested 1080-wide outputs, the original source's
            // 1080-wide CanvasScaler also resolves to scale 1. A fixed scale
            // avoids depending on the batch editor's unrelated Screen size.
            canvas.scaleFactor = 1;
            canvas.referencePixelsPerUnit = 100;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1
                | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;

            GameObject instance = Object.Instantiate(prefab, canvasObject.transform, false);
            instance.name = prefab.name;
            var page = instance.GetComponent<WithdrawalPageView>();
            report.configuredDesignSize = page.DesignSize;
            foreach (MonoBehaviour behaviour in instance.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour == null || behaviour is UIBehaviour || behaviour is WithdrawalPageView) continue;
                behaviour.enabled = false;
                report.disabledNonUiComponents.Add(Hierarchy(behaviour.transform, instance.transform));
            }
            AuditComponents(instance, report);
            AuditBindings(page, report);
            ApplyFixture(page, report, prefab.name);
            canvasObject.SetActive(true);
            instance.SetActive(true);
            Canvas.ForceUpdateCanvases();
            // This view method only installs empty callbacks and executes the
            // production Fit() presentation logic. It has no service access.
            page.Bind(null);
            Canvas.ForceUpdateCanvases();
            if (prefab.name == "TXRecordPanel")
            {
                ScrollRect scroll = instance.GetComponentInChildren<ScrollRect>(true);
                if (scroll != null)
                {
                    scroll.StopMovement();
                    scroll.verticalNormalizedPosition = report.recordsBottom ? 0 : 1;
                    Canvas.ForceUpdateCanvases();
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

    private static void ApplyFixture(WithdrawalPageView page, PageReport report, string prefabName)
    {
        // These strings intentionally reproduce the Host's display formats,
        // while their values are fixed synthetic data. No state engine or
        // service is created, and only this disposable instance is changed.
        report.fixtureNotes.Add("Synthetic validation content applied to an isolated instance; no actual application or payout.");
        page.SetText("test", "TEST MODE - NO REAL PAYOUT");
        page.SetText("notice", "");
        page.SetText("cash", "$123,456.78");
        page.SetText("diamonds", "999,999");
        page.SetText("frozen", "In application: $500.00");
        switch (prefabName)
        {
            case "TXPanel":
                report.fixture = "cash-main-wallet";
                page.SetText("tier0", "$0.40"); page.SetText("tier1", "$0.60"); page.SetText("tier2", "$500.00");
                for (int i = 0; i < 4; i++) { page.SetVisible("tier" + i, i < 3); page.SetVisible("selected" + i, i == 2); }
                page.SetText("amount", "$500.00"); page.SetText("balance", "$499.90");
                page.SetText("target", "Earn $0.10 more"); page.SetText("ratio", "99%");
                page.SetText("apply", "Apply $500.00"); page.SetButton(WithdrawalAction.Apply, false);
                page.SetText("rewards", "Claim rewards (128)");
                page.SetText("summary", "Earn $0.10 more to apply.\nRead all conditions in Rules.");
                page.SetProgress(.9998f);
                break;
            case "TXCoinPanel":
                report.fixture = "diamond-wallet-largest-published-tier";
                string[] tiers = { "$500.00", "$1,000.00", "$1,500.00", "$2,000.00" };
                for (int i = 0; i < tiers.Length; i++)
                {
                    page.SetText("tier" + i, tiers[i]); page.SetVisible("tier" + i, true);
                    page.SetVisible("selected" + i, i == 3);
                }
                page.SetText("amount", "$2,000.00"); page.SetText("balance", "123,456");
                page.SetText("target", "Earn 76,544 more diamonds"); page.SetText("ratio", "61%");
                page.SetText("apply", "Apply $2,000.00"); page.SetButton(WithdrawalAction.Apply, false);
                page.SetText("rewards", "Claim rewards (128)");
                page.SetText("summary", "All conditions are available in Rules before applying."); page.SetProgress(.61728f);
                break;
            case "TXChannelPanel":
                report.fixture = "channel-with-full-destination-label";
                page.SetText("amount", "Apply for $2,000.00");
                page.SetText("channel0", "[Selected] Test wallet (no account required)");
                page.SetText("channel1", "Test bank (simulation only)");
                break;
            case "TXUserInfoSurePanel":
                report.fixture = "full-confirmation-details";
                page.SetText("amount", "$2,000.00");
                page.SetText("details", "SIMULATED APPLICATION\n\nTest wallet (no account required)\n\n$2,000.00\n\nNo payment account is collected.\nThe full conditions were shown before applying.");
                break;
            case "TXProgress0Panel":
                report.fixture = "cash-main-500-all-six-conditions";
                SetRequestFixture(page, "Conditions in progress");
                page.SetText("conditions", "[OK] Clear fruit levels  2 / 2\n"
                    + "[OK] First level milestone  22 / 22\n"
                    + "[ ] Final level milestone  30 / 51\n"
                    + "[ ] Win fruit levels in one UTC day  9 / 10\n"
                    + "[ ] Days with the published daily win goal  4 / 5\n"
                    + "[ ] Fruit rank from completed levels  30 / 31\n");
                page.SetText("step", "Complete the disclosed conditions"); page.SetProgress(2f / 6);
                break;
            case "TXProgress1Panel":
                report.fixture = "simulated-review-countdown";
                SetRequestFixture(page, "Simulated review");
                page.SetText("conditions", "Application saved locally.\nNo account information is needed.\n\nSimulated review: 59s remaining");
                page.SetText("step", "SIMULATED REVIEW"); page.SetProgress(1);
                break;
            case "TXProgress2Panel":
                report.fixture = "long-simulated-completion-status";
                SetRequestFixture(page, "SIMULATED COMPLETE - NO PAYMENT");
                page.SetText("conditions", "Simulation completed.\n\nNo money was sent.\nThis result is saved in Records.");
                page.SetText("step", "SIMULATED COMPLETE - NO PAYMENT"); page.SetProgress(1);
                page.SetButton(WithdrawalAction.Cancel, false);
                break;
            case "TXProgress3Panel":
                report.fixture = "rejected-with-returned-units";
                SetRequestFixture(page, "Test rejected - units returned");
                page.SetText("conditions", "This test application was closed.\nThe reserved reward units were returned.\nYou can start another test.");
                page.SetText("step", "Test rejected - units returned"); page.SetButton(WithdrawalAction.Cancel, false);
                break;
            case "TXRecordPanel":
                report.fixture = report.recordsBottom ? "eight-records-scroll-bottom" : "eight-records-scroll-top";
                report.fixtureNotes.Add("All eight configured record rows are active. Top and bottom screenshots exercise ScrollRect clipping; offscreen rows are expected.");
                page.SetText("empty", "Your latest simulated applications");
                string[] amounts = { "$2,000.00", "$1,500.00", "$1,000.00", "$500.00", "$0.60", "$0.40", "$500.00", "$2,000.00" };
                string[] statuses = { "Test complete\nNo payment", "Test rejected - units returned", "Cancelled - units returned", "Simulated review",
                    "Conditions in progress", "Test complete\nNo payment", "Cancelled - units returned", "Test complete\nNo payment" };
                for (int i = 0; i < 8; i++)
                {
                    page.SetVisible("record" + i, true); page.SetText("recordAmount" + i, amounts[i]);
                    page.SetText("recordStatus" + i, statuses[i]);
                    page.SetText("recordTime" + i, "2026-09-" + (16 - i).ToString("D2") + " 23:59 UTC");
                }
                break;
            case "RewardGetPanel":
                report.fixture = "large-accumulated-pending-rewards";
                page.SetText("rewardCash", "+$123,456.70");
                page.SetText("rewardDiamonds", "+12,345,678 diamonds");
                page.SetText("rewardDetail", "Earned by your fruit matches and cleared levels.");
                page.SetButton(WithdrawalAction.Claim, true);
                break;
            case "RulesPanel":
                report.fixture = "cash-main-rules-all-published-condition-types";
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
        page.SetText("channel", "Test wallet (no account required)");
        page.SetText("requestId", "ID: 0123456789ab");
    }

    private static void AuditBindings(WithdrawalPageView view, PageReport report)
    {
        if (view.Content == null) report.invalidBindings.Add("Content is null.");
        if (view.DesignSize.x <= 0 || view.DesignSize.y <= 0) report.invalidBindings.Add("DesignSize is not positive.");
        if (view.Texts == null || view.Buttons == null || view.Objects == null)
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
            if (outside) report.outsideCanvas.Add(Hierarchy(graphic.transform, root.transform));
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
