using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

public static partial class Framework2AppleSkin
{
    static Framework2AppleHudGlyphSet amountGlyphs, countGlyphs, levelGlyphs;
    struct ApprovedGlyph
    {
        public char character;
        public RectInt crop;
        public float baseline, advance, scale;
        public bool white;
        public ApprovedGlyph(char c,int x,int y,int w,int h,float baseline,float advance,float scale=1,bool white=false)
        {character=c;crop=new RectInt(x,y,w,h);this.baseline=baseline;this.advance=advance;this.scale=scale;this.white=white;}
    }

    static void ImportApprovedHudGlyphs()
    {
        var source=new Texture2D(2,2,TextureFormat.RGBA32,false);
        source.LoadImage(File.ReadAllBytes("Docs/CommercializationAB/Framework2Replica/ReskinPreview-AllUI-20261023/board-01.png"));
        amountGlyphs=MakeHudGlyphSet(source,"HudAmountGlyphs",28,23,-3,new[]{
            new ApprovedGlyph('$',230,91,16,28,115,17),new ApprovedGlyph('1',247,94,10,22,115,12),
            new ApprovedGlyph('2',259,94,17,22,115,17),new ApprovedGlyph('3',276,94,16,23,115,17),
            new ApprovedGlyph('.',293,109,7,7,115,7),new ApprovedGlyph('4',300,94,18,22,115,18),
            new ApprovedGlyph('5',318,94,16,23,115,16),
            // Zero and separator share the approved small HUD typeface.
            new ApprovedGlyph('0',330,162,12,17,178,16,1.4f),new ApprovedGlyph(',',325,174,5,7,178,7,1.4f)
        });
        countGlyphs=MakeHudGlyphSet(source,"HudCountGlyphs",20,17,-2,new[]{
            new ApprovedGlyph('$',208,160,11,21,178,12),new ApprovedGlyph('1',220,163,8,15,178,9),
            new ApprovedGlyph('2',229,162,12,17,178,12),new ApprovedGlyph('3',241,163,11,16,178,11),
            new ApprovedGlyph('.',252,174,6,5,178,5),new ApprovedGlyph('4',257,162,13,16,178,13),
            new ApprovedGlyph('5',270,163,11,16,178,13),new ApprovedGlyph('/',287,162,9,17,178,10),
            new ApprovedGlyph('0',330,162,12,17,178,12),new ApprovedGlyph(',',325,174,5,7,178,6),
            new ApprovedGlyph('%',487,162,18,17,178,19)
        });
        levelGlyphs=MakeHudGlyphSet(source,"HudLevelGlyphs",38,29,-3,new[]{
            new ApprovedGlyph('5',83,88,26,35,120,26,1,true)
        },true);
        Object.DestroyImmediate(source);
    }

    static Framework2AppleHudGlyphSet MakeHudGlyphSet(Texture2D board,string name,int fontSize,float ascent,float descent,ApprovedGlyph[] approved,bool white=false)
    {
        const int width=512;
        int height=white?64:256;
        var atlas=new Texture2D(width,height,TextureFormat.RGBA32,false);
        atlas.SetPixels32(new Color32[width*height]);
        Font fallback=AssetDatabase.LoadAssetAtPath<Font>("Assets/Framework2Replica/Resources/Framework2Replica/External/Assets/Font/Roboto-Bold.ttf");
        string chars="";for(int c=32;c<=126;c++)chars+=(char)c;
        chars+="£€¥₱₫₹";
        fallback.RequestCharactersInTexture(chars,fontSize,FontStyle.Normal);
        Texture fontTexture=fallback.material.mainTexture;
        var temporary=RenderTexture.GetTemporary(fontTexture.width,fontTexture.height,0,RenderTextureFormat.ARGB32);
        Graphics.Blit(fontTexture,temporary);
        RenderTexture previous=RenderTexture.active;RenderTexture.active=temporary;
        var readable=new Texture2D(fontTexture.width,fontTexture.height,TextureFormat.RGBA32,false);
        readable.ReadPixels(new Rect(0,0,fontTexture.width,fontTexture.height),0,0);readable.Apply(false);
        RenderTexture.active=previous;RenderTexture.ReleaseTemporary(temporary);
        var entries=new List<Framework2AppleHudGlyphSet.Glyph>();
        int cursorX=2,cursorY=2,rowHeight=0;
        foreach(char character in chars)
        {
            fallback.GetCharacterInfo(character,out CharacterInfo info,fontSize,FontStyle.Normal);
            ApprovedGlyph? replacement=null;
            foreach(var item in approved)if(item.character==character){replacement=item;break;}
            int w=replacement.HasValue?replacement.Value.crop.width:Mathf.Max(info.glyphWidth,0);
            int h=replacement.HasValue?replacement.Value.crop.height:Mathf.Max(info.glyphHeight,0);
            if(white && (character<'0' || character>'9'))w=h=0;
            int padding=white && !replacement.HasValue && w>0?2:0;
            int originalW=w,originalH=h;
            w+=padding*2;h+=padding*2;
            if(cursorX+w+2>width){cursorX=2;cursorY+=rowHeight+4;rowHeight=0;}
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)
            {
                Color color;
                if(replacement.HasValue)
                {
                    var glyph=replacement.Value;
                    color=HudPixel(board,glyph.crop.x+x,glyph.crop.y+y);
                    if(glyph.white)
                    {
                        // The white digit includes its authored blue edge. Keep
                        // only the neighbourhood of the glyph, not the badge face.
                        float whiteInk=0;
                        for(int oy=-2;oy<=2;oy++)for(int ox=-2;ox<=2;ox++)
                        {Color c=HudPixel(board,glyph.crop.x+x+ox,glyph.crop.y+y+oy);whiteInk=Mathf.Max(whiteInk,Mathf.Min(c.r,Mathf.Min(c.g,c.b)));}
                        color.a=Mathf.Clamp01((whiteInk-.55f)*4);
                    }
                    else
                    {
                        Color paper=HudPixel(board,character=='%'?515:glyph.crop.y<150?217:435,glyph.crop.y+y);
                        float alpha=Mathf.Clamp01(Mathf.Max(1-color.r/Mathf.Max(paper.r,.01f),1-color.g/Mathf.Max(paper.g,.01f),1-color.b/Mathf.Max(paper.b,.01f)));
                        if(alpha<.08f)color=Color.clear;
                        else
                        {
                            color=new Color(Mathf.Clamp01((color.r-paper.r*(1-alpha))/alpha),
                                Mathf.Clamp01((color.g-paper.g*(1-alpha))/alpha),Mathf.Clamp01((color.b-paper.b*(1-alpha))/alpha),alpha);
                        }
                    }
                }
                else
                {
                    float alpha=HudFallbackAlpha(readable,info,x-padding,y-padding,originalW,originalH);
                    if(white)
                    {
                        float edge=alpha;
                        for(int oy=-2;oy<=2;oy++)for(int ox=-2;ox<=2;ox++)
                            if(ox*ox+oy*oy<=4)edge=Mathf.Max(edge,HudFallbackAlpha(readable,info,x-padding+ox,y-padding+oy,originalW,originalH));
                        color=Color.Lerp(new Color(5/255f,46/255f,131/255f),Color.white,edge>0?alpha/edge:0);color.a=edge;
                    }
                    else color=new Color(88/255f,23/255f,8/255f,alpha);
                }
                atlas.SetPixel(cursorX+x,cursorY+h-1-y,color);
            }
            float glyphScale=replacement.HasValue?replacement.Value.scale:1;
            Vector2 bearing=replacement.HasValue?new Vector2(white?0:-1*glyphScale,(replacement.Value.baseline-replacement.Value.crop.yMax)*glyphScale):new Vector2(info.minX-padding,info.minY-padding);
            float advance=replacement.HasValue?replacement.Value.advance:info.advance;
            if(character==' ')advance=fontSize==20?4:7;
            entries.Add(new Framework2AppleHudGlyphSet.Glyph { character=character,
                uv=new Rect(cursorX/(float)width,cursorY/(float)height,w/(float)width,h/(float)height),
                size=new Vector2(w*glyphScale,h*glyphScale),bearing=bearing,advance=advance });
            if(w>0)cursorX+=w+4;rowHeight=Mathf.Max(rowHeight,h);
        }
        if(cursorY+rowHeight>=height)throw new System.InvalidOperationException("HUD font atlas overflow.");
        Object.DestroyImmediate(readable);atlas.Apply(false);
        string texturePath=Art+name+".png";
        File.WriteAllBytes(texturePath,atlas.EncodeToPNG());Object.DestroyImmediate(atlas);
        AssetDatabase.ImportAsset(texturePath,ImportAssetOptions.ForceUpdate);
        var importer=(TextureImporter)AssetImporter.GetAtPath(texturePath);
        importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;
        importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.maxTextureSize=512;importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;
        importer.SaveAndReimport();
        string assetPath=Art+name+".asset";
        var set=AssetDatabase.LoadAssetAtPath<Framework2AppleHudGlyphSet>(assetPath);
        if(set==null){set=ScriptableObject.CreateInstance<Framework2AppleHudGlyphSet>();AssetDatabase.CreateAsset(set,assetPath);}
        set.Configure(AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath),entries.ToArray(),ascent,descent);
        EditorUtility.SetDirty(set);return set;
    }

    static float HudFallbackAlpha(Texture2D texture,CharacterInfo info,int x,int y,int w,int h)
    {
        if(x<0 || y<0 || x>=w || y>=h)return 0;
        Vector2 low=Vector2.Lerp(info.uvBottomLeft,info.uvBottomRight,(x+.5f)/w);
        Vector2 high=Vector2.Lerp(info.uvTopLeft,info.uvTopRight,(x+.5f)/w);
        Vector2 uv=Vector2.Lerp(low,high,1-(y+.5f)/h);
        return texture.GetPixelBilinear(uv.x,uv.y).a;
    }

    static void HudNumerals(GameObject root,string path,Framework2AppleHudGlyphSet glyphs,float scale,Vector2 offset)
    {
        Text source=root.transform.Find(path).GetComponent<Text>();
        var node=NavNode(source.transform,"ApprovedNumerals");NavStretch(node,0,0,0,0);
        var graphic=node.GetComponent<Framework2AppleHudNumerals>();
        if(graphic==null)graphic=node.gameObject.AddComponent<Framework2AppleHudNumerals>();
        graphic.Configure(source,glyphs,scale,offset);graphic.color=Color.white;
        Color transparent=source.color;transparent.a=0;source.color=transparent;
        source.canvasRenderer.cullTransparentMesh=true;
        var outline=source.GetComponent<Outline>();if(outline!=null)outline.enabled=false;
    }
}
