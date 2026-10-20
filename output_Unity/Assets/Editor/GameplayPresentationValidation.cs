using System;
using System.IO;
using CorePlay;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>Checks the saved gameplay prefab with the same layout method used on a device.</summary>
public static class GameplayPresentationValidation
{
    private const string CorePath = "Assets/res/local/coreplay/CorePlayUI.prefab";

    [MenuItem("Tools/Validation/Gameplay tree and fruit alignment")]
    public static void Run()
    {
        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(CorePath);
        Require(source != null, "Gameplay prefab is missing.");
        int[,] profiles = { { 1080, 2400 }, { 1080, 2340 }, { 720, 1280 }, { 1080, 1920 } };
        int cases = 0;
        for (int i = 0; i < profiles.GetLength(0); i++)
        {
            // Use both a full-screen and an inset parent. Neither may change
            // the artwork dimensions or the fruit-to-artwork relationship.
            ValidateProfile(source, profiles[i, 0], profiles[i, 1], Vector2.zero);
            ValidateProfile(source, profiles[i, 0], profiles[i, 1], new Vector2(100, 195));
            cases += 2;
        }
        string result = "GAMEPLAY_PRESENTATION_PASS prefab_profiles=" + cases +
            " shared_transform=true full_screen_coverage=true safe_area_stretch=false repeated_fit_stable=true";
        Debug.Log(result);
        string output = Environment.GetEnvironmentVariable("DJS_APPLE_VALIDATION_OUTPUT");
        if (!string.IsNullOrEmpty(output))
        {
            Directory.CreateDirectory(output);
            File.WriteAllText(Path.Combine(output, "gameplay-presentation-results.txt"), result + Environment.NewLine);
        }
    }

    private static void ValidateProfile(GameObject source, int width, int height, Vector2 safeInsets)
    {
        var canvasObject = new GameObject("Gameplay layout validation", typeof(RectTransform), typeof(Canvas));
        canvasObject.hideFlags = HideFlags.HideAndDontSave;
        canvasObject.SetActive(false);
        try
        {
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var screen = (RectTransform)canvas.transform;
            float scale = Mathf.Min(width / 1080f, height / 2340f);
            screen.sizeDelta = new Vector2(width / scale, height / scale);
            GameObject instance = Object.Instantiate(source, screen, false);
            // Keep runtime behaviours asleep; this fixture never initializes
            // gameplay services, reads player saves, or changes player progress.
            instance.SetActive(false);
            canvasObject.SetActive(true);
            var page = (RectTransform)instance.transform;
            page.anchorMin = Vector2.zero; page.anchorMax = Vector2.one;
            page.offsetMin = new Vector2(0, safeInsets.y);
            page.offsetMax = new Vector2(0, -safeInsets.x);
            page.ForceUpdateRectTransforms();
            CorePlayUI ui = instance.GetComponent<CorePlayUI>();
            RectTransform root = ui.m_FieldPresentationRoot;
            Require(root != null && ui.m_BgImg.rectTransform == root, "Tree image does not use the shared presentation root.");
            Require(ui.m_BgTrans.parent == root, "Fruit map is outside the presentation root.");
            Require(root.GetComponent<Orange.ProtectedAreaAdaptReverse>() == null, "The tree still stretches with safe-area insets.");
            Require(Approximately(ui.m_BgTrans.localPosition, Vector3.zero) && Approximately(ui.m_BgTrans.localScale, Vector3.one),
                "The map contains a separate fruit offset or scale.");
            Rect design = root.rect;
            ui.FitGameField();
            Require(root.rect == design, "Screen fitting changes the authored image rectangle.");
            Require(Mathf.Approximately(root.localScale.x, root.localScale.y), "Scene fit is not uniform.");
            var corners = new Vector3[4];
            root.GetWorldCorners(corners);
            Vector3 lower = screen.InverseTransformPoint(corners[0]);
            Vector3 upper = screen.InverseTransformPoint(corners[2]);
            Require(lower.x <= screen.rect.xMin + .01f && lower.y <= screen.rect.yMin + .01f &&
                upper.x >= screen.rect.xMax - .01f && upper.y >= screen.rect.yMax - .01f, "Background exposes a screen edge.");
            Transform layer = ui.m_BgTrans.Find("Layer");
            Require(layer != null, "Authored fruit layer is missing.");
            foreach (Vector3 point in new[] { Vector3.zero, new Vector3(-510, -300), new Vector3(510, 800) })
            {
                Vector3 onTree = ui.m_BgImg.rectTransform.TransformPoint(point);
                Vector3 fruit = layer.TransformPoint(point);
                Require(Approximately(onTree, fruit), "Fruit coordinates drift from the matching tree coordinates.");
                Require(Approximately(fruit, ui.GetItemWorldPosition((int)point.x, (int)point.y)), "Gameplay coordinate conversion disagrees with rendering.");
            }
            Vector3 position = root.localPosition, localScale = root.localScale;
            ui.FitGameField();
            Require(Approximately(position, root.localPosition) && Approximately(localScale, root.localScale), "Repeated fitting accumulates offsets.");
        }
        finally { Object.DestroyImmediate(canvasObject); }
    }

    private static bool Approximately(Vector3 a, Vector3 b) => (a - b).sqrMagnitude < .0001f;
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
