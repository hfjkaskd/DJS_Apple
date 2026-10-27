using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static partial class Framework2AppleSkin
{
    static Sprite referenceAtlas;
    const float ReferenceFontScale=1.35f;
    const float ReferenceMinimumFontRatio=.55f;

    static void ImportReferenceArt()
    {
        // Use the approved material itself, rather than generating a similar one.
        string path = Art + "Reference.png";
        if (!File.Exists(path)) File.Copy("Docs/CommercializationAB/Framework2Replica/ReskinPreview-AllUI-20261023/board-03.png", path);
        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false; importer.alphaIsTransparency = false;
        importer.wrapMode = TextureWrapMode.Clamp; importer.filterMode = FilterMode.Bilinear;
        importer.maxTextureSize = 2048; importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        referenceAtlas = AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // Input rectangles use the reference board's upper-left pixel coordinates.
    static Rect RefUV(float x, float y, float w, float h, bool mirrorX = false, bool mirrorY = false)
    {
        float left = x / 1672f, bottom = 1 - (y + h) / 941f;
        float width = w / 1672f, height = h / 941f;
        return new Rect(mirrorX ? left + width : left, mirrorY ? bottom + height : bottom,
            mirrorX ? -width : width, mirrorY ? -height : height);
    }

    static void RefSurface(Image image, string kind)
    {
        if (image == null || !image.fillCenter || referenceAtlas == null) return;
        Rect[] patches;
        float bx = 80, by = 80, radius = 50;
        bool pill = false; float cap = .5f;
        if (kind == "panel")
        {
            bx = 126; by = 126; radius = 92;
            patches = new[] {
                RefUV(28,655,42,44), RefUV(70,655,1,44), RefUV(28,655,42,44,true),
                RefUV(28,458,42,2), RefUV(42,451,3,3), RefUV(28,458,42,2,true),
                RefUV(28,655,42,44,false,true), RefUV(70,655,1,44,false,true), RefUV(28,655,42,44,true,true)
            };
        }
        else if (kind == "button" || kind == "title" || kind == "secondary")
        {
            float x = kind == "button" ? 56 : kind == "title" ? 319 : 889;
            float y = kind == "button" ? 595 : kind == "title" ? 249 : 529;
            float h = kind == "button" ? 60 : kind == "title" ? 59 : 55;
            float w = kind == "button" ? 34 : kind == "title" ? 30 : 28;
            float cleanX = kind == "button" ? 100 : kind == "title" ? 107 : 928;
            bool reverse = kind == "title";
            patches = new[] {
                RefUV(x,y+h*.5f,w,h*.5f,reverse), RefUV(cleanX,y+h*.5f,1,h*.5f), RefUV(x,y+h*.5f,w,h*.5f,!reverse),
                RefUV(x,y+h*.5f,w,1,reverse), RefUV(cleanX,y+h*.5f,1,1), RefUV(x,y+h*.5f,w,1,!reverse),
                RefUV(x,y,w,h*.5f,reverse), RefUV(cleanX,y,1,h*.5f), RefUV(x,y,w,h*.5f,!reverse)
            };
            pill = true; cap = w/h;
        }
        else if (kind == "close")
        {
            RefWhole(image, new Rect(354,265,50,50), 24); return;
        }
        else
        {
            bool input = kind == "input";
            float x = input ? 56 : 887, y = input ? 495 : 326;
            float w = input ? 314 : 316, h = input ? 53 : 183;
            float b = input ? 14 : 16;
            float cx = input ? 355 : 900, cy = input ? 514 : 360;
            patches = new[] {
                RefUV(x,y+h-b,b,b), RefUV(cx,y+h-b,1,b), RefUV(x+w-b,y+h-b,b,b),
                RefUV(x,cy,b,1), RefUV(cx,cy,1,1), RefUV(x+w-b,cy,b,1),
                RefUV(x,y,b,b), RefUV(cx,y,1,b), RefUV(x+w-b,y,b,b)
            };
            bx = by = input ? 40 : 46; radius = input ? 32 : 40;
        }
        var surface = image.GetComponent<Framework2ReferenceSurface>();
        if (surface == null) surface = image.gameObject.AddComponent<Framework2ReferenceSurface>();
        surface.Configure(referenceAtlas, patches, bx, by, radius, pill, cap);
    }

    static void RefWhole(Image image, Rect pixels, float radius)
    {
        if (image == null) return;
        float b = Mathf.Min(radius + 1, Mathf.Min(pixels.width, pixels.height) * .5f);
        Rect[] patches = new Rect[9];
        float[] xx = { pixels.x, pixels.x+b, pixels.xMax-b, pixels.xMax };
        float[] yy = { pixels.yMax, pixels.yMax-b, pixels.y+b, pixels.y };
        for (int row=0;row<3;row++) for(int col=0;col<3;col++)
            patches[row*3+col] = RefUV(xx[col], yy[row+1], xx[col+1]-xx[col], yy[row]-yy[row+1]);
        float scale = image.rectTransform.rect.width / pixels.width;
        var surface = image.GetComponent<Framework2ReferenceSurface>();
        if (surface == null) surface = image.gameObject.AddComponent<Framework2ReferenceSurface>();
        surface.Configure(referenceAtlas, patches, b*scale, b*scale, radius*scale);
        image.color = Color.white;
    }

    static void RefFinish(GameObject root)
    {
        var marker=root.GetComponent<Framework2AppleSkinMarker>();
        if(marker==null)marker=root.AddComponent<Framework2AppleSkinMarker>();
        marker.RecordAuthoringFonts();
        foreach (Text text in root.GetComponentsInChildren<Text>(true))
        {
            text.font = SkinFont; text.fontStyle = FontStyle.Normal;
            text.fontSize=Mathf.RoundToInt(text.fontSize*ReferenceFontScale);
            text.resizeTextMinSize=Mathf.Min(Mathf.RoundToInt(text.resizeTextMinSize*ReferenceFontScale),Mathf.RoundToInt(text.fontSize*ReferenceMinimumFontRatio));
            text.resizeTextMaxSize=Mathf.RoundToInt(text.resizeTextMaxSize*ReferenceFontScale);
            if(text.text=="CONFIRM" || text.text=="Confirm")
            {
                NavLiteral(text.transform,"Confirm");
                var caption=text.GetComponent<Framework2AppleCaption>();
                if(caption==null)caption=text.gameObject.AddComponent<Framework2AppleCaption>();
                caption.Configure(text,"Confirm");
            }
            if (text.color == Color.white && text.fontSize >= 40)
            {
                var outline = text.GetComponent<Outline>();
                if (outline != null) { outline.effectDistance = new Vector2(1.4f,-1.4f); outline.effectColor = new Color32(5,46,131,255); }
            }
        }
        foreach (Image image in root.GetComponentsInChildren<Image>(true))
        {
            if (image.GetComponent<Framework2ReferenceSurface>() != null) continue;
            if (image.GetComponent<Framework2ReferenceCutout>() != null) continue;
            if (image.sprite == null || !AssetDatabase.GetAssetPath(image.sprite).StartsWith(Art)) continue;
            string key = image.sprite.name;
            if (key == "panel" || key == "title" || key == "button" || key == "secondary" || key == "close" || key == "selected" || key == "inset")
                RefSurface(image, image.GetComponent<InputField>() != null ? "input" : key);
        }
        root.GetComponent<Framework2AppleLayoutProfiles>()?.ConfigureFontScale(ReferenceFontScale,ReferenceMinimumFontRatio);
    }
}
