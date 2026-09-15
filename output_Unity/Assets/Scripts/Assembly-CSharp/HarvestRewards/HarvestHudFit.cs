using UnityEngine;

/// <summary>Preserves the authored HUD margins on narrow canvases without enlarging it.</summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class HarvestHudFit : MonoBehaviour
{
    [SerializeField] private float referenceCanvasWidth = 1154.524f;

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
            if (hostRect == null || canvasRect == null || !(referenceCanvasWidth > 0f) ||
                float.IsInfinity(referenceCanvasWidth)) return;

            float canvasWidth = canvasRect.rect.width;
            if (!(canvasWidth > 0f) || float.IsInfinity(canvasWidth)) return;
            float uniform = Mathf.Min(1f, canvasWidth / referenceCanvasWidth);
            Vector3 scale = new Vector3(uniform, uniform, 1f);
            if (hostRect.localScale != scale) hostRect.localScale = scale;
        }
        finally
        {
            refreshing = false;
        }
    }
}
