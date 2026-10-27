using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Nine authored UV patches from the approved artwork. Text and controls remain
/// real Unity UI; only their reusable surface is sampled from the reference.
/// </summary>
[RequireComponent(typeof(Image))]
[DisallowMultipleComponent]
public sealed class Framework2ReferenceSurface : BaseMeshEffect
{
    [SerializeField] private string atlasResourcePath;
    private Sprite atlas;
    [SerializeField] private Rect[] uvPatches = new Rect[9];
    [SerializeField] private float borderWidth = 90;
    [SerializeField] private float borderHeight = 90;
    [SerializeField] private float cornerRadius = 70;
    [SerializeField] private bool proportionalCaps;
    [SerializeField] private float capAspect = .5f;

    public void Configure(Sprite source, Rect[] patches, float borderX, float borderY, float radius,
        bool pill = false, float capRatio = .5f)
    {
        atlasResourcePath="Framework2AppleSkin/"+source.name;
        atlas = source; uvPatches = patches; borderWidth = borderX; borderHeight = borderY;
        cornerRadius = radius; proportionalCaps = pill; capAspect = capRatio;
        ApplyAtlas();
        if (graphic != null) graphic.SetVerticesDirty();
    }

    protected override void OnEnable() { base.OnEnable(); ApplyAtlas(); }

    private void ApplyAtlas()
    {
        if(atlas==null)atlas=Framework2AppleSkinAssets.LoadReferenceAtlas(atlasResourcePath);
        if (atlas == null) return;
        Image image = graphic as Image;
        if (image == null) return;
        image.overrideSprite = atlas; image.type = Image.Type.Simple;
        image.preserveAspect = false;
    }

    public override void ModifyMesh(VertexHelper vertices)
    {
        if (!IsActive() || uvPatches == null || uvPatches.Length != 9) return;
        Rect rect = graphic.rectTransform.rect;
        if (rect.width <= 0 || rect.height <= 0) return;
        vertices.Clear();
        float bx = Mathf.Min(proportionalCaps ? rect.height * capAspect : borderWidth, rect.width * .5f);
        float by = Mathf.Min(proportionalCaps ? rect.height * .5f : borderHeight, rect.height * .5f);
        float radius = Mathf.Min(proportionalCaps ? rect.height * .48f : cornerRadius, Mathf.Min(bx, by));
        float x0 = rect.xMin, x1 = x0 + bx, x2 = rect.xMax - bx, x3 = rect.xMax;
        float y0 = rect.yMin, y1 = y0 + by, y2 = rect.yMax - by, y3 = rect.yMax;
        Patch(vertices, 0, x0, y0, x1, y1, radius, true, false, false);
        Patch(vertices, 1, x1, y0, x2, y1, 0, false, false, false);
        Patch(vertices, 2, x2, y0, x3, y1, radius, true, true, false);
        Patch(vertices, 3, x0, y1, x1, y2, 0, false, false, false);
        Patch(vertices, 4, x1, y1, x2, y2, 0, false, false, false);
        Patch(vertices, 5, x2, y1, x3, y2, 0, false, false, false);
        Patch(vertices, 6, x0, y2, x1, y3, radius, true, false, true);
        Patch(vertices, 7, x1, y2, x2, y3, 0, false, false, false);
        Patch(vertices, 8, x2, y2, x3, y3, radius, true, true, true);
    }

    private void Patch(VertexHelper vh, int patch, float left, float bottom, float right,
        float top, float radius, bool rounded, bool flipX, bool flipY)
    {
        if (right <= left || top <= bottom) return;
        int start = vh.currentVertCount;
        if (!rounded)
        {
            Vertex(vh, patch, left, bottom, right, top, 0, 0);
            Vertex(vh, patch, left, bottom, right, top, 0, 1);
            Vertex(vh, patch, left, bottom, right, top, 1, 1);
            Vertex(vh, patch, left, bottom, right, top, 1, 0);
            vh.AddTriangle(start, start + 1, start + 2); vh.AddTriangle(start, start + 2, start + 3);
            return;
        }
        float rx = radius / (right - left), ry = radius / (top - bottom);
        CornerVertex(vh, patch, left, bottom, right, top, 1, 1, flipX, flipY);
        CornerVertex(vh, patch, left, bottom, right, top, 0, 1, flipX, flipY);
        CornerVertex(vh, patch, left, bottom, right, top, 0, ry, flipX, flipY);
        const int segments = 12;
        for (int i = 1; i <= segments; i++)
        {
            float angle = (180 + 90f * i / segments) * Mathf.Deg2Rad;
            CornerVertex(vh, patch, left, bottom, right, top,
                rx + Mathf.Cos(angle) * rx, ry + Mathf.Sin(angle) * ry, flipX, flipY);
        }
        CornerVertex(vh, patch, left, bottom, right, top, 1, 0, flipX, flipY);
        for (int i = start + 1; i < vh.currentVertCount - 1; i++) vh.AddTriangle(start, i, i + 1);
    }

    private void CornerVertex(VertexHelper vh, int patch, float l, float b, float r, float t,
        float x, float y, bool flipX, bool flipY)
    { Vertex(vh, patch, l, b, r, t, flipX ? 1 - x : x, flipY ? 1 - y : y); }

    private void Vertex(VertexHelper vh, int patch, float l, float b, float r, float t, float x, float y)
    {
        Rect uv = uvPatches[patch];
        var vertex = UIVertex.simpleVert; vertex.color = graphic.color;
        vertex.position = new Vector3(Mathf.Lerp(l, r, x), Mathf.Lerp(b, t, y));
        vertex.uv0 = new Vector2(uv.x + uv.width * x, uv.y + uv.height * y);
        vh.AddVert(vertex);
    }
}
