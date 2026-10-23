using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;

/// <summary>Renders the existing Image.fillAmount with nine-sliced, round end caps.
/// No extra transforms or polling, and source progress/data bindings stay unchanged.</summary>
[RequireComponent(typeof(Image))]
public sealed class Framework2AppleRoundedFill : BaseMeshEffect
{
    private Image source;
    protected override void OnEnable() { source = GetComponent<Image>(); base.OnEnable(); }
    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || source == null || source.type != Image.Type.Filled || source.sprite == null) return;
        if (source.fillMethod != Image.FillMethod.Horizontal) return;
        vh.Clear();
        float amount = Mathf.Clamp01(source.fillAmount);
        if (amount <= 0) return;
        Rect bounds = source.GetPixelAdjustedRect();
        float width = bounds.width * amount;
        float height = Mathf.Min(bounds.height, width);
        float left = source.fillOrigin == 1 ? bounds.xMax - width : bounds.xMin;
        float bottom = bounds.center.y - height * .5f;
        Sprite sprite = source.overrideSprite != null ? source.overrideSprite : source.sprite;
        Vector4 outer = DataUtility.GetOuterUV(sprite), inner = DataUtility.GetInnerUV(sprite);
        Vector4 border = sprite.border;
        float scale = height / sprite.rect.height;
        float capLeft = Mathf.Min(width * .5f, border.x * scale);
        float capRight = Mathf.Min(width * .5f, border.z * scale);
        float capBottom = Mathf.Min(height * .5f, border.y * scale);
        float capTop = Mathf.Min(height * .5f, border.w * scale);
        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
        {
            float x0 = Position(x, left, left + capLeft, left + width - capRight, left + width);
            float x1 = Position(x + 1, left, left + capLeft, left + width - capRight, left + width);
            float y0 = Position(y, bottom, bottom + capBottom, bottom + height - capTop, bottom + height);
            float y1 = Position(y + 1, bottom, bottom + capBottom, bottom + height - capTop, bottom + height);
            float u0 = Position(x, outer.x, inner.x, inner.z, outer.z), u1 = Position(x + 1, outer.x, inner.x, inner.z, outer.z);
            float v0 = Position(y, outer.y, inner.y, inner.w, outer.w), v1 = Position(y + 1, outer.y, inner.y, inner.w, outer.w);
            int index = vh.currentVertCount;
            Color32 color = source.color;
            vh.AddVert(new Vector3(x0, y0), color, new Vector2(u0, v0));
            vh.AddVert(new Vector3(x0, y1), color, new Vector2(u0, v1));
            vh.AddVert(new Vector3(x1, y1), color, new Vector2(u1, v1));
            vh.AddVert(new Vector3(x1, y0), color, new Vector2(u1, v0));
            vh.AddTriangle(index, index + 1, index + 2); vh.AddTriangle(index, index + 2, index + 3);
        }
    }
    private static float Position(int i, float a, float b, float c, float d) => i == 0 ? a : i == 1 ? b : i == 2 ? c : d;
}
