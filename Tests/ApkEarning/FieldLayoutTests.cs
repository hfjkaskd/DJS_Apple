using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using CorePlay;

static class FieldLayoutTests
{
    struct V { public double X, Y, Z; public V(double x, double y, double z = 0) { X = x; Y = y; Z = z; } }
    struct Key { public double Time; public V Value, In, Out; }
    sealed class Motion { public double Angle, MinScale = 1, MaxScale = 1; }
    static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
    static double Number(string text) { return double.Parse(text, Inv); }
    static string Scalar(string body, string field)
    {
        Match m = Regex.Match(body, @"(?m)^\s*" + Regex.Escape(field) + @":\s*([^\r\n]*)");
        Require(m.Success, "Missing YAML field " + field); return m.Groups[1].Value.Trim();
    }
    static V Vector(string body, string field)
    {
        Match m = Regex.Match(Scalar(body, field), @"\{x: ([^,]+), y: ([^,}]+)(?:, z: ([^,}]+))?");
        Require(m.Success, "Invalid vector " + field);
        return new V(Number(m.Groups[1].Value), Number(m.Groups[2].Value), m.Groups[3].Success ? Number(m.Groups[3].Value) : 0);
    }
    static string Reference(string body, string field)
    {
        Match m = Regex.Match(Scalar(body, field), @"fileID: (-?\d+)");
        Require(m.Success, "Missing reference " + field); return m.Groups[1].Value;
    }
    static Dictionary<string, string> Objects(string path)
    {
        var result = new Dictionary<string, string>();
        foreach (Match m in Regex.Matches(File.ReadAllText(path), @"(?ms)^--- !u!\d+ &(-?\d+)[^\r\n]*\r?\n(.*?)(?=^--- !u!|\z)"))
            result.Add(m.Groups[1].Value, m.Groups[2].Value);
        return result;
    }
    static string RectNamed(Dictionary<string, string> objects, string name)
    {
        foreach (string body in objects.Values)
            if (body.StartsWith("RectTransform:") && Scalar(objects[Reference(body, "m_GameObject")], "m_Name") == name) return body;
        throw new Exception("Missing RectTransform " + name);
    }
    static string BodyContaining(Dictionary<string, string> objects, string field)
    {
        foreach (string body in objects.Values) if (body.Contains("  " + field + ":")) return body;
        throw new Exception("Missing component field " + field);
    }
    static FieldLayoutBounds BoundsOfRect(string body)
    {
        V size = Vector(body, "m_SizeDelta"), pivot = Vector(body, "m_Pivot");
        return new FieldLayoutBounds((float)(-size.X * pivot.X), (float)(-size.Y * pivot.Y),
            (float)(size.X * (1 - pivot.X)), (float)(size.Y * (1 - pivot.Y)));
    }
    static double Axis(V v, int axis) { return axis == 0 ? v.X : axis == 1 ? v.Y : v.Z; }
    static void Include(double value, ref double min, ref double max) { min = Math.Min(min, value); max = Math.Max(max, value); }
    static void IncludeCubicRoot(double t, double a, double b, double c, double d, ref double min, ref double max)
    {
        if (t > 0 && t < 1) Include(((a * t + b) * t + c) * t + d, ref min, ref max);
    }
    // Read actual Unity curves, including extrema between keys. Weighted keys
    // must be supported explicitly if the artist enables them in these clips.
    static double[] CurveRange(List<Key> keys, int axis)
    {
        double min = double.PositiveInfinity, max = double.NegativeInfinity;
        foreach (Key k in keys) Include(Axis(k.Value, axis), ref min, ref max);
        for (int i = 0; i + 1 < keys.Count; i++)
        {
            Key l = keys[i], r = keys[i + 1]; double dt = r.Time - l.Time;
            double p = Axis(l.Value, axis), q = Axis(r.Value, axis);
            double m = Axis(l.Out, axis) * dt, n = Axis(r.In, axis) * dt;
            double a = 2 * p - 2 * q + m + n, b = -3 * p + 3 * q - 2 * m - n, c = m;
            if (Math.Abs(a) < 1e-12)
            {
                if (Math.Abs(b) > 1e-12) IncludeCubicRoot(-c / (2 * b), a, b, c, p, ref min, ref max);
            }
            else
            {
                double discriminant = 4 * b * b - 12 * a * c;
                if (discriminant >= 0)
                {
                    double root = Math.Sqrt(discriminant);
                    IncludeCubicRoot((-2 * b + root) / (6 * a), a, b, c, p, ref min, ref max);
                    IncludeCubicRoot((-2 * b - root) / (6 * a), a, b, c, p, ref min, ref max);
                }
            }
        }
        return new[] { min, max };
    }
    static Motion ReadMotion(string path)
    {
        var motion = new Motion(); string yaml = File.ReadAllText(path); bool found = false;
        foreach (string field in new[] { "m_EulerCurves", "m_ScaleCurves" })
        {
            Match section = Regex.Match(yaml, @"(?ms)^  " + field + @":(.*?)(?=^  \w+:)");
            Require(section.Success, "Missing animation curves in " + path);
            foreach (string curve in Regex.Split(section.Groups[1].Value, @"(?m)^  - curve:"))
            {
                if (!curve.Contains("path: Fruit/Rotation")) continue;
                var keys = new List<Key>();
                foreach (Match match in Regex.Matches(curve, @"(?ms)^      - serializedVersion: \d+\r?\n(.*?)(?=^      - serializedVersion:|^      m_PreInfinity:)"))
                {
                    string key = match.Groups[1].Value;
                    Require(Scalar(key, "weightedMode") == "0", "Weighted animation keys require envelope support: " + path);
                    keys.Add(new Key { Time = Number(Scalar(key, "time")), Value = Vector(key, "value"), In = Vector(key, "inSlope"), Out = Vector(key, "outSlope") });
                }
                Require(keys.Count > 0, "No animation keys in " + path);
                if (field == "m_EulerCurves")
                {
                    double[] range = CurveRange(keys, 2); motion.Angle = Math.Max(Math.Abs(range[0]), Math.Abs(range[1])) * Math.PI / 180;
                    found = true;
                }
                else
                {
                    double[] x = CurveRange(keys, 0), y = CurveRange(keys, 1);
                    motion.MinScale = Math.Min(x[0], y[0]); motion.MaxScale = Math.Max(x[1], y[1]);
                }
            }
        }
        Require(found && motion.Angle > 0 && motion.MaxScale > 0, "Invalid idle motion: " + path); return motion;
    }
    static FieldLayoutBounds SpriteEnvelope(V size, V imageOffset, V pivotOffset, Motion motion)
    {
        double minX = double.PositiveInfinity, minY = double.PositiveInfinity, maxX = double.NegativeInfinity, maxY = double.NegativeInfinity;
        foreach (double x in new[] { imageOffset.X - size.X / 2, imageOffset.X + size.X / 2 })
        foreach (double y in new[] { imageOffset.Y - size.Y / 2, imageOffset.Y + size.Y / 2 })
        {
            var angles = new List<double> { -motion.Angle, 0, motion.Angle };
            double xStationary = Math.Atan2(-y, x), yStationary = Math.Atan2(x, y);
            for (int k = -2; k <= 2; k++)
            {
                double a = xStationary + k * Math.PI, b = yStationary + k * Math.PI;
                if (a >= -motion.Angle && a <= motion.Angle) angles.Add(a);
                if (b >= -motion.Angle && b <= motion.Angle) angles.Add(b);
            }
            foreach (double angle in angles)
            foreach (double scale in new[] { motion.MinScale, motion.MaxScale })
            {
                double px = (x * Math.Cos(angle) - y * Math.Sin(angle)) * scale + pivotOffset.X;
                double py = (x * Math.Sin(angle) + y * Math.Cos(angle)) * scale + pivotOffset.Y;
                Include(px, ref minX, ref maxX); Include(py, ref minY, ref maxY);
            }
        }
        return new FieldLayoutBounds((float)minX, (float)minY, (float)maxX, (float)maxY);
    }
    static void Reject(FieldLayoutBounds art, FieldLayoutBounds screen)
    {
        bool rejected = false; try { FieldLayoutGeometry.Cover(art, screen); } catch (ArgumentException) { rejected = true; }
        Require(rejected, "Invalid bounds must be rejected");
    }
    static int Main(string[] args)
    {
        Require(args.Length == 7, "Expected level directory, sizes, CorePlay prefab, Game scene, CollectItem prefab, idle directory, HUD prefab");
        var core = Objects(args[2]); string owner = BodyContaining(core, "m_FieldPresentationRoot");
        string presentation = core[Reference(owner, "m_FieldPresentationRoot")], map = core[Reference(owner, "m_BgTrans")];
        Require(Scalar(core[Reference(presentation, "m_GameObject")], "m_Name") == "Game", "Tree must be part of the common presentation root");
        Require(Reference(map, "m_Father") == Reference(owner, "m_FieldPresentationRoot"), "Map must remain under the tree root");
        V mapPosition = Vector(map, "m_AnchoredPosition"), mapScale = Vector(map, "m_LocalScale");
        Require(mapPosition.X == 0 && mapPosition.Y == 0 && mapScale.X == 1 && mapScale.Y == 1, "Map must not independently move or resize");
        FieldLayoutBounds artwork = BoundsOfRect(presentation);
        var scene = Objects(args[3]); string scaler = BodyContaining(scene, "m_ReferenceResolution");
        Require(Scalar(scaler, "m_ScreenMatchMode") == "1", "Update test for a non-Expand CanvasScaler");
        V reference = Vector(scaler, "m_ReferenceResolution");
        var item = Objects(args[4]);
        V imageOffset = Vector(RectNamed(item, "ItemImage"), "m_AnchoredPosition");
        V fruitOffset = Vector(RectNamed(item, "Fruit"), "m_AnchoredPosition"), rotationOffset = Vector(RectNamed(item, "Rotation"), "m_AnchoredPosition");
        V pivotOffset = new V(fruitOffset.X + rotationOffset.X, fruitOffset.Y + rotationOffset.Y);
        var motions = new Motion[3];
        for (int i = 0; i < motions.Length; i++) motions[i] = ReadMotion(Path.Combine(args[5], "anim_Fruits_Idle" + i + ".anim"));
        string hud = BodyContaining(Objects(args[6]), "ReferenceViewportTop");
        double header = Number(Scalar(hud, "ReferenceViewportTop")), headerWidth = Number(Scalar(hud, "ReferenceWidth"));
        var sizes = new Dictionary<int, V>();
        foreach (JObject row in (JArray)JObject.Parse(File.ReadAllText(args[1]))["eleImageConfigs"])
            sizes[(int)row["eleID"]] = new V((double)row["sizeX"], (double)row["sizeY"]);
        var envelopes = new Dictionary<int, FieldLayoutBounds>();
        int[,] resolutions = { {1080,2400}, {1080,2340}, {720,1280}, {1080,1920} };
        var fits = new FieldLayoutFit[4]; var screens = new FieldLayoutBounds[4];
        var factors = new double[4]; var minHorizontal = new double[] { 1e9, 1e9, 1e9, 1e9 };
        var minHud = (double[])minHorizontal.Clone(); var minTray = (double[])minHorizontal.Clone();
        for (int r = 0; r < 4; r++)
        {
            factors[r] = Math.Min(resolutions[r, 0] / reference.X, resolutions[r, 1] / reference.Y);
            float width = (float)(resolutions[r, 0] / factors[r]), height = (float)(resolutions[r, 1] / factors[r]);
            screens[r] = new FieldLayoutBounds(-width / 2, -height / 2, width / 2, height / 2);
            fits[r] = FieldLayoutGeometry.Cover(artwork, screens[r]); FieldLayoutFit f = fits[r];
            Require(f.Scale > 0 && Math.Abs(f.X) < .001 && Math.Abs(f.Y) < .001, "Centered design must remain centered");
            Require(artwork.MinX * f.Scale <= -width / 2 + .001 && artwork.MaxX * f.Scale >= width / 2 - .001 &&
                artwork.MinY * f.Scale <= -height / 2 + .001 && artwork.MaxY * f.Scale >= height / 2 - .001, "Background fails to cover screen");
        }
        int levels = 0, sprites = 0;
        foreach (string path in Directory.GetFiles(args[0], "LevelConfigs.json", SearchOption.AllDirectories))
        foreach (JObject level in (JArray)JObject.Parse(File.ReadAllText(path))["Levels"])
        {
            JArray tiles = (JArray)level["normalTiles"]; if (tiles == null || tiles.Count == 0) continue; levels++;
            foreach (JObject tile in tiles)
            {
                int type = (int)tile["eleType"]; FieldLayoutBounds envelope;
                if (!envelopes.TryGetValue(type, out envelope))
                {
                    V size; if (!sizes.TryGetValue(type, out size)) size = new V(140, 140); // Existing runtime default.
                    envelope = SpriteEnvelope(size, imageOffset, pivotOffset, motions[type % 3]); envelopes.Add(type, envelope);
                }
                for (int r = 0; r < 4; r++)
                {
                    FieldLayoutFit f = fits[r]; FieldLayoutBounds screen = screens[r];
                    double x = (double)tile["posX"], y = (double)tile["posY"];
                    double left = (x + envelope.MinX) * f.Scale + f.X, right = (x + envelope.MaxX) * f.Scale + f.X;
                    double bottom = (y + envelope.MinY) * f.Scale + f.Y, top = (y + envelope.MaxY) * f.Scale + f.Y;
                    double horizontal = Math.Min(left - screen.MinX, screen.MaxX - right);
                    double hudGap = screen.MaxY - header * (screen.MaxX - screen.MinX) / headerWidth - top;
                    // 700 canvas units = saved tray top 505 + conservative bottom inset 195.
                    double trayGap = bottom - (screen.MinY + 700);
                    Require(horizontal >= -.01 && hudGap >= -.01 && trayGap >= -.01,
                        "Clipped/occluded sprite: " + level["levelID"] + " type=" + type + " screen=" + resolutions[r, 0] + "x" + resolutions[r, 1] +
                        " margins=" + horizontal.ToString("0.00", Inv) + "," + hudGap.ToString("0.00", Inv) + "," + trayGap.ToString("0.00", Inv));
                    minHorizontal[r] = Math.Min(minHorizontal[r], horizontal * factors[r]);
                    minHud[r] = Math.Min(minHud[r], hudGap * factors[r]); minTray[r] = Math.Min(minTray[r], trayGap * factors[r]); sprites++;
                }
            }
            // A later or restored board must reuse the same transform regardless of tile distribution.
            for (int r = 0; r < 4; r++)
            {
                FieldLayoutFit repeat = FieldLayoutGeometry.Cover(artwork, screens[r]);
                Require(repeat.Scale == fits[r].Scale && repeat.X == fits[r].X && repeat.Y == fits[r].Y, "Board-dependent presentation transform");
            }
        }
        FieldLayoutBounds unit = new FieldLayoutBounds(0, 0, 1, 1);
        Reject(new FieldLayoutBounds(0, 0, 0, 1), unit); Reject(unit, new FieldLayoutBounds(0, 0, 1, -1));
        Reject(new FieldLayoutBounds(float.NaN, 0, 1, 1), unit); Reject(unit, new FieldLayoutBounds(0, 0, float.PositiveInfinity, 1));
        Require(levels >= 4000, "Incomplete level coverage");
        Console.WriteLine("FIELD_LAYOUT_PASS levels=" + levels + " sprite_checks=" + sprites + " artwork=" + (artwork.MaxX - artwork.MinX) + "x" + (artwork.MaxY - artwork.MinY));
        for (int i = 0; i < motions.Length; i++) Console.WriteLine("IDLE_" + i + " degrees=" + (motions[i].Angle * 180 / Math.PI).ToString("0.000", Inv) + " max_scale=" + motions[i].MaxScale.ToString("0.0000", Inv));
        for (int r = 0; r < 4; r++) Console.WriteLine("SCREEN " + resolutions[r, 0] + "x" + resolutions[r, 1] + " minimum_pixel_gaps side=" + minHorizontal[r].ToString("0.00", Inv) + " hud=" + minHud[r].ToString("0.00", Inv) + " tray=" + minTray[r].ToString("0.00", Inv));
        return 0;
    }
}
