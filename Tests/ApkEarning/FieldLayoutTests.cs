using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json.Linq;
using CorePlay;

static class FieldLayoutTests
{
    static int Main(string[] args)
    {
        var sizes = new Dictionary<int, float[]>();
        foreach (JObject row in (JArray)JObject.Parse(File.ReadAllText(args[1]))["eleImageConfigs"])
            sizes[(int)row["eleID"]] = new[] { (float)row["sizeX"], (float)row["sizeY"] };
        int levels = 0, profiles = 0, sprites = 0;
        float minimumScale = 1f;
        int[,] resolutions = { {720,1280}, {1080,2340}, {720,1600}, {1080,1920} };
        foreach (string path in Directory.GetFiles(args[0], "LevelConfigs.json", SearchOption.AllDirectories))
        foreach (JObject level in (JArray)JObject.Parse(File.ReadAllText(path))["Levels"])
        {
            var tiles = (JArray)level["normalTiles"];
            if (tiles == null || tiles.Count == 0) continue;
            levels++;
            var content = new FieldLayoutBounds(float.PositiveInfinity,float.PositiveInfinity,float.NegativeInfinity,float.NegativeInfinity);
            foreach (JObject tile in tiles)
            {
                float x = (float)tile["posX"], y = (float)tile["posY"];
                float[] size; if (!sizes.TryGetValue((int)tile["eleType"],out size)) size = new[] {140f,140f};
                float px = Math.Max(110f,size[0] * 0.5f), py = Math.Max(120f,size[1] * 0.5f);
                content.MinX = Math.Min(content.MinX,x-px); content.MinY = Math.Min(content.MinY,y-py);
                content.MaxX = Math.Max(content.MaxX,x+px); content.MaxY = Math.Max(content.MaxY,y+py);
            }
            for (int r = 0; r < resolutions.GetLength(0); r++)
            {
                float scaleFactor = Math.Min(resolutions[r,0]/1080f,resolutions[r,1]/2340f);
                float width = resolutions[r,0]/scaleFactor, height = resolutions[r,1]/scaleFactor;
                // The APK header scales with the actual canvas width. Reserve at
                // least 700 units, plus the tray and conservative safe-area gap.
                float headerInset = Math.Max(700f,600f*width/1080f);
                var viewport = new FieldLayoutBounds(-width/2+40,-height/2+740,width/2-40,height/2-headerInset);
                FieldLayoutFit fit = FieldLayoutGeometry.Fit(content,viewport,1f);
                if (fit.Scale <= 0 || fit.Scale > 1 || float.IsNaN(fit.Scale)) throw new Exception("Invalid scale");
                minimumScale = Math.Min(minimumScale,fit.Scale); profiles++;
                // Assert every independent authored sprite rectangle, including
                // 8-degree idle rotation, fits between the real header and tray.
                const double angle = Math.PI * 8 / 180;
                foreach (JObject tile in tiles)
                {
                    float[] size; if (!sizes.TryGetValue((int)tile["eleType"],out size)) size = new[] {140f,140f};
                    float halfX=(float)(Math.Cos(angle)*size[0]+Math.Sin(angle)*size[1])*0.5f;
                    float halfY=(float)(Math.Sin(angle)*size[0]+Math.Cos(angle)*size[1])*0.5f;
                    float cx=(float)tile["posX"]*fit.Scale+fit.X, cy=(float)tile["posY"]*fit.Scale+fit.Y;
                    if(cx-halfX*fit.Scale < viewport.MinX-0.01f || cx+halfX*fit.Scale > viewport.MaxX+0.01f ||
                       cy-halfY*fit.Scale < viewport.MinY-0.01f || cy+halfY*fit.Scale > viewport.MaxY+0.01f)
                        throw new Exception("Clipped sprite in level " + level["levelID"] + " at resolution " + resolutions[r,0]);
                    sprites++;
                }
            }
        }
        bool rejected = false;
        try { FieldLayoutGeometry.Fit(new FieldLayoutBounds(0,0,1,1),new FieldLayoutBounds(0,0,0,1),1); }
        catch(ArgumentException) { rejected = true; }
        if (!rejected || levels < 4000) throw new Exception("Invalid geometry or incomplete level coverage");
        Console.WriteLine("FIELD_LAYOUT_PASS levels="+levels+" viewport_cases="+profiles+" sprite_checks="+sprites+
            " minimum_scale="+minimumScale.ToString("0.000",CultureInfo.InvariantCulture));
        return 0;
    }
}
