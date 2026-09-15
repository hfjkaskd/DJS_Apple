using UnityEngine;

/// <summary>Fits the authored design as one unit while keeping its center on the full canvas.</summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class HarvestReferenceLayout : MonoBehaviour
{
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private RectTransform dimmerRoot;
    [SerializeField] private Vector2 designResolution = new Vector2(1154.524f, 2340f);
    [SerializeField] private Vector2 occupiedSize = new Vector2(1083.333f, 1956.19f);

    private RectTransform hostRect;
    private Canvas rootCanvas;
    private RectTransform canvasRect;
    private bool hierarchyDirty = true;
    private bool refreshing;

    private void OnEnable() { InvalidateHierarchy(); }
    private void OnTransformParentChanged() { InvalidateHierarchy(); }
    private void OnCanvasHierarchyChanged() { InvalidateHierarchy(); }
    private void OnRectTransformDimensionsChange() { RefreshLayout(); }

    private void InvalidateHierarchy()
    {
        hierarchyDirty = true;
        RefreshLayout();
    }

    private void CacheHierarchy()
    {
        hostRect = transform as RectTransform;
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        rootCanvas = parentCanvas == null ? null : parentCanvas.rootCanvas;
        canvasRect = rootCanvas == null ? null : rootCanvas.transform as RectTransform;
        hierarchyDirty = false;
    }

    public void RefreshLayout()
    {
        if (refreshing || !isActiveAndEnabled) return;
        refreshing = true;
        try
        {
            if (hierarchyDirty || hostRect == null || rootCanvas == null || canvasRect == null)
                CacheHierarchy();
            if (hostRect == null || canvasRect == null || contentRoot == null ||
                contentRoot.parent != hostRect || !PositiveFinite(designResolution.x) ||
                !PositiveFinite(designResolution.y) || !PositiveFinite(occupiedSize.x) ||
                !PositiveFinite(occupiedSize.y)) return;

            Rect canvas = canvasRect.rect;
            Rect available = hostRect.rect;
            if (!PositiveFinite(canvas.width) || !PositiveFinite(canvas.height)) return;

            Vector3 center = hostRect.InverseTransformPoint(canvasRect.TransformPoint(canvas.center));
            if (dimmerRoot != null && dimmerRoot.parent == hostRect)
            {
                Vector3 canvasWidth = hostRect.InverseTransformVector(canvasRect.TransformVector(new Vector3(canvas.width, 0f, 0f)));
                Vector3 canvasHeight = hostRect.InverseTransformVector(canvasRect.TransformVector(new Vector3(0f, canvas.height, 0f)));
                Vector2 dimmerSize = new Vector2(Mathf.Abs(canvasWidth.x) + Mathf.Abs(canvasHeight.x),
                    Mathf.Abs(canvasWidth.y) + Mathf.Abs(canvasHeight.y));
                if (dimmerRoot.localScale != Vector3.one) dimmerRoot.localScale = Vector3.one;
                if (dimmerRoot.sizeDelta != dimmerSize) dimmerRoot.sizeDelta = dimmerSize;
                if (dimmerRoot.localPosition != center) dimmerRoot.localPosition = center;
            }
            // Safe-area insets can be asymmetric. Fit around the full-canvas center,
            // using the nearer edge on each axis, rather than the inset root's center.
            float availableWidth = Mathf.Max(0f, 2f * Mathf.Min(center.x - available.xMin, available.xMax - center.x));
            float availableHeight = Mathf.Max(0f, 2f * Mathf.Min(center.y - available.yMin, available.yMax - center.y));
            float uniform = Mathf.Min(Mathf.Min(canvas.width / designResolution.x, canvas.height / designResolution.y),
                Mathf.Min(availableWidth / occupiedSize.x, availableHeight / occupiedSize.y));

            Vector3 scale = new Vector3(uniform, uniform, 1f);
            Vector2 contentCenter = contentRoot.rect.center;
            Vector3 position = new Vector3(center.x - contentCenter.x * uniform,
                center.y - contentCenter.y * uniform, center.z);
            if (contentRoot.localScale != scale) contentRoot.localScale = scale;
            if (contentRoot.localPosition != position) contentRoot.localPosition = position;
        }
        finally
        {
            refreshing = false;
        }
    }

    private static bool PositiveFinite(float value)
    {
        return value > 0f && !float.IsInfinity(value);
    }
}
