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
        public static FieldLayoutFit Cover(FieldLayoutBounds artwork, FieldLayoutBounds screen)
        {
            Validate(artwork); Validate(screen);
            float scale = Math.Max((screen.MaxX - screen.MinX) / (artwork.MaxX - artwork.MinX),
                (screen.MaxY - screen.MinY) / (artwork.MaxY - artwork.MinY));
            return new FieldLayoutFit { Scale = scale,
                X = (screen.MinX + screen.MaxX) * 0.5f - (artwork.MinX + artwork.MaxX) * 0.5f * scale,
                Y = (screen.MinY + screen.MaxY) * 0.5f - (artwork.MinY + artwork.MaxY) * 0.5f * scale };
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
