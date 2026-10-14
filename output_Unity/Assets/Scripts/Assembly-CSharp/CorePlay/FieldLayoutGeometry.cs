using System;

namespace CorePlay
{
    public struct FieldLayoutBounds
    {
        public float MinX, MinY, MaxX, MaxY;
        public FieldLayoutBounds(float minX, float minY, float maxX, float maxY)
        { MinX = minX; MinY = minY; MaxX = maxX; MaxY = maxY; }
    }

    public struct FieldLayoutFit
    {
        public float Scale, X, Y;
    }

    // Uniform presentation transform only: source tile coordinates remain unchanged.
    public static class FieldLayoutGeometry
    {
        public static FieldLayoutFit Fit(FieldLayoutBounds content, FieldLayoutBounds viewport, float maximumScale)
        {
            Validate(content); Validate(viewport);
            if (!Finite(maximumScale) || maximumScale <= 0f) throw new ArgumentOutOfRangeException(nameof(maximumScale));
            float scale = Math.Min(maximumScale, Math.Min((viewport.MaxX - viewport.MinX) / (content.MaxX - content.MinX),
                (viewport.MaxY - viewport.MinY) / (content.MaxY - content.MinY)));
            return new FieldLayoutFit { Scale = scale,
                X = (viewport.MinX + viewport.MaxX) * 0.5f - (content.MinX + content.MaxX) * 0.5f * scale,
                Y = (viewport.MinY + viewport.MaxY) * 0.5f - (content.MinY + content.MaxY) * 0.5f * scale };
        }
        private static void Validate(FieldLayoutBounds bounds)
        {
            if (!Finite(bounds.MinX) || !Finite(bounds.MaxX) || !Finite(bounds.MinY) || !Finite(bounds.MaxY) ||
                bounds.MaxX <= bounds.MinX || bounds.MaxY <= bounds.MinY)
                throw new ArgumentException("The configured field viewport must have positive bounds.");
        }
        private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }
}
