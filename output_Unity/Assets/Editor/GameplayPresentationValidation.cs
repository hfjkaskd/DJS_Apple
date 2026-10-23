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
            " shared_transform=true full_screen_coverage=true safe_area_stretch=false repeated_fit_stable=true b_reserved_viewport_effective=true b_backdrop_covers_screen=true";
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
            ValidateReservedViewport(ui, screen, layer);
        }
        finally { Object.DestroyImmediate(canvasObject); }
    }

    private static void ValidateReservedViewport(CorePlayUI ui, RectTransform screen, Transform layer)
    {
        var binding = ui.GetComponent<FruitEarningVariants.EarningVariantGameplayView>();
        Require(binding != null && binding.GameplayViewport != null, "B's authored viewport binding is missing.");
        RectTransform viewport = binding.GameplayViewport;
        RectTransform root = ui.m_FieldPresentationRoot;
        Require(ui.m_FieldBackdrop != null && !ui.m_FieldBackdrop.gameObject.activeSelf, "The authored B backdrop must be present and disabled for A.");
        Require(!ui.m_FieldBackdrop.raycastTarget && ui.m_FieldBackdrop.GetComponent<UnityEngine.UI.Button>() == null,
            "The background must not intercept gameplay clicks.");
        Require(ui.m_FieldBackdrop.transform.parent == root.parent && ui.m_FieldBackdrop.transform.GetSiblingIndex() < root.GetSiblingIndex(),
            "The B backdrop must render behind the shared tree/fruit root.");
        ui.m_BgImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/res_server/server_maps/map1002/Map1002.asset");
        Require(ui.m_BgImg.sprite != null, "The original Apple map Sprite fixture is missing.");
        ui.m_FieldBackdrop.gameObject.SetActive(true);
        Vector3 fullPosition = root.localPosition, fullScale = root.localScale;
        Vector3 trayPosition = ui.m_CollectArea.position, trayScale = ui.m_CollectArea.lossyScale;
        viewport.offsetMax = new Vector2(viewport.offsetMax.x, -400);
        viewport.ForceUpdateRectTransforms();
        ui.FitGameField(viewport);
        Require(ui.m_FieldBackdrop.sprite == ui.m_BgImg.sprite, "The B backdrop does not reuse the exact already-loaded map Sprite.");
        var backdropCorners = new Vector3[4];
        ui.m_FieldBackdrop.rectTransform.GetWorldCorners(backdropCorners);
        Vector3 backgroundLower = screen.InverseTransformPoint(backdropCorners[0]);
        Vector3 backgroundUpper = screen.InverseTransformPoint(backdropCorners[2]);
        Require(backgroundLower.x <= screen.rect.xMin + .01f && backgroundLower.y <= screen.rect.yMin + .01f &&
            backgroundUpper.x >= screen.rect.xMax - .01f && backgroundUpper.y >= screen.rect.yMax - .01f,
            "The B backdrop exposes a full-canvas edge behind the reserved field.");
        Require(!Approximately(fullPosition, root.localPosition) || !Approximately(fullScale, root.localScale),
            "B's reserved top inset does not affect the shared tree and fruit root.");
        Vector3 firstPosition = root.localPosition, firstScale = root.localScale;
        viewport.offsetMax = new Vector2(viewport.offsetMax.x, -600);
        viewport.ForceUpdateRectTransforms();
        ui.FitGameField(viewport);
        Require(!Approximately(firstPosition, root.localPosition) || !Approximately(firstScale, root.localScale),
            "A changed B HUD inset does not update the shared field presentation.");
        foreach (Vector3 point in new[] { Vector3.zero, new Vector3(-510, -300), new Vector3(510, 800) })
            Require(Approximately(ui.m_BgImg.rectTransform.TransformPoint(point), layer.TransformPoint(point)),
                "B's reserved layout separates fruit coordinates from the tree artwork.");
        Require(Approximately(trayPosition, ui.m_CollectArea.position) && Approximately(trayScale, ui.m_CollectArea.lossyScale),
            "B's field layout unexpectedly moves or scales the independent collection tray.");
        Vector3 reservedPosition = root.localPosition, reservedScale = root.localScale;
        ui.FitGameField(); // Board restoration and next-level initialization use this call.
        Require(Approximately(reservedPosition, root.localPosition) && Approximately(reservedScale, root.localScale),
            "An ordinary board layout call resets B's reserved viewport to the full screen.");
        // An explicit target also allows a deterministic return to full coverage.
        ui.FitGameField(screen);
        Require(Approximately(fullPosition, root.localPosition) && Approximately(fullScale, root.localScale),
            "Restoring full-canvas coverage changes the original A field layout.");
    }

    private static bool Approximately(Vector3 a, Vector3 b) => (a - b).sqrMagnitude < .0001f;
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
