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

/// <summary>Saved message-card rendering and repeated state-switch isolation checks.</summary>
public static partial class ApkEarningValidation
{
    private static readonly string[] MessageFixtureStates = { "", "activating", "activated", "summary", "diamond-preview" };
    private const string MessageBoardResource = "OrchardEarning/Messages/OrchardMessageBoard";
    // Top-left normalized coordinates of the ivory board, excluding the upper illustration
    // and its thick frame. Tolerance is already included in this artwork-specific safe area.
    private static readonly Rect MessageBoardSafeArea = new Rect(.075f, .515f, .85f, .355f);

    [Serializable]
    private sealed class MessageValidationReport
    {
        public string generatedUtc;
        public string scope = "Saved LocalEarningSequence prefab rendered by the existing fixture camera at three portrait sizes. " +
            "Assessment, activating, activated, summary and the summary layout's diamond-preview variant use explicit synthetic text. " +
            "A separate isolated copy switches through all five states and back to assessment without recreation, checking root exclusivity, " +
            "hidden receipt/bonus groups, no added Buttons, visible artwork, text overflow and text containment. " +
            "It also visits receipt then bonuses before restoring assessment, checking that the shared MessageDisclosure follows its Message group. " +
            "These are Unity prefab previews, not phone or reference APK screenshots. No Host, service, player save or payment operation runs.";
        public Report fixtures;
        public int savedResourceSlots, savedStrongArtworkReferences;
        public List<MessageStateCheck> transitions = new List<MessageStateCheck>();
        public List<MessageGroupCheck> groupTransitions = new List<MessageGroupCheck>();
        public List<string> errors = new List<string>();
    }

    [Serializable]
    private sealed class MessageGroupCheck
    {
        public string state;
        public int width, height, activeMessageLayouts;
        public bool messageVisible, receiptVisible, bonusesVisible, disclosureVisible;
        public List<string> errors = new List<string>();
    }

    [Serializable]
    private sealed class MessageStateCheck
    {
        public string state, artwork, body;
        public int width, height, expectedLayout, activeLayouts, activeButtons, visibleTextChecks, uniqueLoadedArtworkSprites;
        public Rect artworkScreenBounds, boardSafeScreenBounds;
        public List<string> errors = new List<string>();
    }

    [MenuItem("Tools/Apk Earning/Validate Message Card Layouts")]
    public static void ValidateMessageLayouts()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before validating saved message cards.");
        string directory = Environment.GetEnvironmentVariable("DJS_APPLE_MESSAGE_OUTPUT");
        if (string.IsNullOrWhiteSpace(directory))
            directory = "E:/CodexCacheBackups/DJS_Apple-message-redesign-20261023/after";
        Directory.CreateDirectory(directory);
        string path = PanelFolder + "LocalEarningSequence.prefab";
        var report = new MessageValidationReport {
            generatedUtc = DateTime.UtcNow.ToString("O"),
            fixtures = new Report { generatedUtc = DateTime.UtcNow.ToString("O"),
                scope = "Fifteen actual camera renders of the saved message card; explicitly synthetic preview data." }
        };
        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        AuditPrefab(source, path, report.fixtures);
        AuditSavedMessageResourceBindings(source, report);
        foreach (Vector2Int size in new[] { new Vector2Int(1080, 1920), new Vector2Int(1080, 2400), new Vector2Int(720, 1600) })
        {
            foreach (string state in MessageFixtureStates)
                RenderFixture("LocalEarningSequence", path, size, false, report.fixtures,
                    state, true, false, directory);
            AuditMessageStateTransitions(path, size, report);
        }
        foreach (RenderReport render in report.fixtures.renders)
        {
            if (render.activeButtons != 0)
                report.errors.Add(render.state + " " + render.width + "x" + render.height +
                    ": informational message unexpectedly exposes " + render.activeButtons + " Buttons.");
            foreach (string warning in render.warnings)
                if (warning.IndexOf("overflows", StringComparison.Ordinal) >= 0)
                    report.errors.Add(render.state + ": " + warning);
        }
        report.errors.AddRange(report.fixtures.errors);
        foreach (MessageStateCheck check in report.transitions)
            foreach (string error in check.errors)
                report.errors.Add(check.state + " " + check.width + "x" + check.height + ": " + error);
        foreach (MessageGroupCheck check in report.groupTransitions)
            foreach (string error in check.errors)
                report.errors.Add(check.state + " " + check.width + "x" + check.height + ": " + error);
        File.WriteAllText(Path.Combine(directory, "message-layout-validation.json"), JsonUtility.ToJson(report, true));
        if (report.errors.Count > 0)
            throw new InvalidOperationException("Message card validation found " + report.errors.Count + " errors. See " + directory);
        Debug.Log("APK_EARNING_MESSAGE_LAYOUT_PASS renders=" + report.fixtures.renders.Count +
            " stateSwitches=" + report.transitions.Count + " groupSwitches=" + report.groupTransitions.Count + " output=" + directory);
    }

    private static void AuditSavedMessageResourceBindings(GameObject source, MessageValidationReport report)
    {
        ApkEarningPageView page = source == null ? null : source.GetComponent<ApkEarningPageView>();
        if (page == null) { report.errors.Add("Cannot inspect saved message resource bindings."); return; }
        report.savedResourceSlots = page.ResourceImages == null ? 0 : page.ResourceImages.Length;
        if (report.savedResourceSlots != 4)
            report.errors.Add("The saved message prefab must configure exactly four resource artwork slots.");
        var targets = new HashSet<Image>();
        if (page.ResourceImages != null)
            foreach (ApkEarningPageView.ResourceImageSlot slot in page.ResourceImages)
            {
                if (slot == null || slot.Target == null)
                { report.errors.Add("Saved resource artwork slot has no target."); continue; }
                if (!targets.Add(slot.Target)) report.errors.Add("Saved resource artwork slots duplicate an Image target.");
                if (slot.Path != MessageBoardResource)
                    report.errors.Add("All four message slots must use the approved shared board resource.");
            }
        for (int i = 0; i < 4; i++)
        {
            Image art = page.GetImage("messageLayouts[" + i + "].Artwork");
            if (art == null || !targets.Contains(art))
            { report.errors.Add("Message artwork " + i + " is absent from the resource bindings."); continue; }
            if (art.sprite != null || art.overrideSprite != null)
                report.savedStrongArtworkReferences++;
        }
        if (report.savedStrongArtworkReferences != 0)
            report.errors.Add("Saved message Artwork Images must not strongly reference the large sprite.");
    }

    private static void AuditMessageStateTransitions(string path, Vector2Int size, MessageValidationReport report)
    {
        Scene scene = EditorSceneManager.NewPreviewScene();
        GameObject canvasObject = null, cameraObject = null;
        RenderTexture texture = null;
        try
        {
            cameraObject = new GameObject("MessageAuditCamera");
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false; camera.scene = scene; camera.orthographic = true;
            camera.orthographicSize = size.y * .5f; camera.aspect = (float)size.x / size.y;
            camera.transform.position = new Vector3(0, 0, -10);
            camera.nearClipPlane = .01f; camera.farClipPlane = 100;
            texture = new RenderTexture(size.x, size.y, 24, RenderTextureFormat.ARGB32);
            texture.Create(); camera.targetTexture = texture;
            canvasObject = new GameObject("MessageAuditCanvas", typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(canvasObject, scene); canvasObject.SetActive(false);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera;
            canvas.planeDistance = 1; canvas.referencePixelsPerUnit = 100;
            canvas.scaleFactor = Mathf.Min(size.x / 1080f, size.y / 2340f);
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (source == null) throw new InvalidOperationException("Missing message prefab: " + path);
            GameObject instance = Object.Instantiate(source, canvas.transform, false);
            ApkEarningPageView page = instance.GetComponent<ApkEarningPageView>();
            if (page == null) throw new InvalidOperationException("Message prefab has no PageView.");
            // The inactive parent prevents unrelated runtime initialization, matching RenderFixture.
            foreach (MonoBehaviour component in instance.GetComponentsInChildren<MonoBehaviour>(true))
                if (component != null && !(component is UIBehaviour) && !(component is ApkEarningPageView))
                    Object.DestroyImmediate(component);
            foreach (Animation animation in instance.GetComponentsInChildren<Animation>(true)) animation.enabled = false;
            foreach (Animator animator in instance.GetComponentsInChildren<Animator>(true)) animator.enabled = false;
            canvasObject.SetActive(true); instance.SetActive(true);
            foreach (string state in MessageFixtureStates)
            {
                ApplyPreviewState(page, "LocalEarningSequence", state);
                Canvas.ForceUpdateCanvases(); page.RefreshLayout();
                foreach (TMP_Text text in instance.GetComponentsInChildren<TMP_Text>(false)) text.ForceMeshUpdate(true, true);
                Canvas.ForceUpdateCanvases();
                AuditVisibleMessage(page, state, camera, size, report);
            }
            foreach (string state in new[] { "receipt", "bonuses", "" })
            {
                ApplyPreviewState(page, "LocalEarningSequence", state);
                Canvas.ForceUpdateCanvases(); page.RefreshLayout();
                foreach (TMP_Text text in instance.GetComponentsInChildren<TMP_Text>(false)) text.ForceMeshUpdate(true, true);
                Canvas.ForceUpdateCanvases();
                AuditMessageGroupTransition(page, state, size, report);
                if (string.IsNullOrEmpty(state)) AuditVisibleMessage(page, state, camera, size, report);
            }
        }
        catch (Exception exception)
        {
            report.errors.Add("Message state-switch fixture " + size + ": " + exception);
        }
        finally
        {
            if (cameraObject != null) cameraObject.GetComponent<Camera>().targetTexture = null;
            if (canvasObject != null) Object.DestroyImmediate(canvasObject);
            if (cameraObject != null) Object.DestroyImmediate(cameraObject);
            if (texture != null) { texture.Release(); Object.DestroyImmediate(texture); }
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }

    private static void AuditMessageGroupTransition(ApkEarningPageView page, string state,
        Vector2Int size, MessageValidationReport report)
    {
        bool isMessage = string.IsNullOrEmpty(state);
        var check = new MessageGroupCheck {
            state = isMessage ? "assessment-restored" : state, width = size.x, height = size.y
        };
        report.groupTransitions.Add(check);
        GameObject message = page.GetObject("messageGroup");
        GameObject receipt = page.GetObject("receiptGroup");
        GameObject bonuses = page.GetObject("bonusesGroup");
        TMP_Text disclosure = page.GetText("MessageDisclosure");
        check.messageVisible = message != null && message.activeInHierarchy;
        check.receiptVisible = receipt != null && receipt.activeInHierarchy;
        check.bonusesVisible = bonuses != null && bonuses.activeInHierarchy;
        check.disclosureVisible = disclosure != null && disclosure.isActiveAndEnabled;
        if (message == null || receipt == null || bonuses == null)
            check.errors.Add("All three sequence groups must remain bound.");
        if (disclosure == null || message == null || !disclosure.transform.IsChildOf(message.transform))
            check.errors.Add("MessageDisclosure must be an authored child of the Message group.");
        if (check.messageVisible != isMessage || check.receiptVisible != (state == "receipt") ||
            check.bonusesVisible != (state == "bonuses"))
            check.errors.Add("Sequence group visibility did not match the requested state.");
        if (check.disclosureVisible != isMessage)
            check.errors.Add("MessageDisclosure must hide for receipt/bonuses and return for assessment.");
        for (int i = 0; i < 4; i++)
        {
            GameObject root = page.GetObject("messageLayouts[" + i + "].Root");
            if (root != null && root.activeInHierarchy) check.activeMessageLayouts++;
        }
        if (check.activeMessageLayouts != (isMessage ? 1 : 0))
            check.errors.Add("Message layouts leaked into receipt/bonuses or failed to return.");
    }

    private static void AuditVisibleMessage(ApkEarningPageView page, string state, Camera camera,
        Vector2Int size, MessageValidationReport report)
    {
        int expected = state == "activating" ? 1 : state == "activated" ? 2 :
            state == "summary" || state == "diamond-preview" ? 3 : 0;
        var check = new MessageStateCheck {
            state = string.IsNullOrEmpty(state) ? "assessment" : state,
            width = size.x, height = size.y, expectedLayout = expected
        };
        report.transitions.Add(check);
        for (int i = 0; i < 4; i++)
        {
            GameObject root = page.GetObject("messageLayouts[" + i + "].Root");
            if (root == null) { check.errors.Add("Missing message root " + i); continue; }
            if (root.activeInHierarchy) check.activeLayouts++;
            if (root.activeInHierarchy != (i == expected)) check.errors.Add("Incorrect visibility for message root " + i);
        }
        if (check.activeLayouts != 1) check.errors.Add("Exactly one message layout must be visible.");
        GameObject message = page.GetObject("messageGroup");
        if (message == null || !message.activeInHierarchy) check.errors.Add("Message group is not visible.");
        foreach (string key in new[] { "receiptGroup", "bonusesGroup" })
        {
            GameObject group = page.GetObject(key);
            if (group == null || group.activeInHierarchy) check.errors.Add(key + " must exist and remain hidden.");
        }
        check.activeButtons = page.GetComponentsInChildren<Button>(false).Length;
        if (check.activeButtons != 0) check.errors.Add("Informational message must not add action Buttons.");
        Image art = page.GetImage("messageLayouts[" + expected + "].Artwork");
        if (art == null || !art.isActiveAndEnabled || art.sprite == null)
        {
            check.errors.Add("Active message artwork is missing, disabled or unloaded.");
            return;
        }
        check.artwork = art.sprite.name;
        check.artworkScreenBounds = ScreenBounds(art.rectTransform, camera);
        Rect frame = check.artworkScreenBounds;
        check.boardSafeScreenBounds = new Rect(frame.xMin + frame.width * MessageBoardSafeArea.xMin,
            frame.yMax - frame.height * MessageBoardSafeArea.yMax,
            frame.width * MessageBoardSafeArea.width, frame.height * MessageBoardSafeArea.height);
        if (!MessageContains(new Rect(0, 0, size.x, size.y), check.artworkScreenBounds, 1))
            check.errors.Add("Message artwork crosses viewport bounds.");
        var loadedArtwork = new HashSet<Sprite>();
        if (page.ResourceImages != null)
            foreach (ApkEarningPageView.ResourceImageSlot slot in page.ResourceImages)
            {
                if (slot == null || slot.Target == null || slot.Target.sprite == null)
                    check.errors.Add("Production RefreshLayout did not load all four configured artwork slots.");
                else loadedArtwork.Add(slot.Target.sprite);
            }
        check.uniqueLoadedArtworkSprites = loadedArtwork.Count;
        if (check.uniqueLoadedArtworkSprites != 1)
            check.errors.Add("All four message artworks must share one loaded Sprite instance.");
        foreach (string suffix in new[] { "Title", "Body", "Disclosure" })
        {
            TMP_Text text = page.GetText(suffix == "Disclosure" ? "MessageDisclosure" :
                "messageLayouts[" + expected + "]." + suffix);
            if (text == null) { check.errors.Add("Missing " + suffix + " binding."); continue; }
            if (suffix == "Body") check.body = text.text;
            if (!text.isActiveAndEnabled)
            {
                if (suffix != "Title") check.errors.Add("Message " + suffix + " must be visible.");
                continue; // Assessment intentionally has one wide body and no duplicate title.
            }
            check.visibleTextChecks++;
            if (string.IsNullOrWhiteSpace(text.text)) check.errors.Add(suffix + " is empty.");
            if (text.isTextOverflowing) check.errors.Add(suffix + " overflows its authored text rectangle.");
            Rect bounds = ScreenBounds(text.rectTransform, camera);
            if (!MessageContains(check.boardSafeScreenBounds, bounds, 1))
                check.errors.Add(suffix + " rectangle extends outside the ivory board safe area.");
        }
        TMP_Text titleText = page.GetText("messageLayouts[" + expected + "].Title");
        TMP_Text bodyText = page.GetText("messageLayouts[" + expected + "].Body");
        if (titleText != null && titleText.isActiveAndEnabled && bodyText != null && bodyText.isActiveAndEnabled)
        {
            Rect overlap = IntersectBounds(ScreenBounds(titleText.rectTransform, camera),
                ScreenBounds(bodyText.rectTransform, camera));
            if (overlap.width > 1 && overlap.height > 1)
                check.errors.Add("Visible title and body text rectangles overlap.");
        }
    }

    private static bool MessageContains(Rect outer, Rect inner, float tolerance)
    {
        return inner.xMin >= outer.xMin - tolerance && inner.xMax <= outer.xMax + tolerance &&
            inner.yMin >= outer.yMin - tolerance && inner.yMax <= outer.yMax + tolerance;
    }
}
