using UnityEngine;
using UnityEngine.UI;

/// <summary>Draws live Text values using the approved bitmap numerals. No runtime hierarchy creation or polling.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class Framework2AppleHudNumerals : MaskableGraphic
{
    [SerializeField] private Text source;
    [SerializeField] private Framework2AppleHudGlyphSet glyphSet;
    [SerializeField] private float referenceScale=1;
    [SerializeField] private Vector2 opticalOffset;
    public override Texture mainTexture => glyphSet!=null ? glyphSet.Atlas : base.mainTexture;
    public bool HasCompleteGlyphCoverage()
    {
        if(source==null || glyphSet==null)return false;
        string value=source.text;
        for(int i=0;i<value.Length;i++)
            if(!glyphSet.TryGet(value[i],out var glyph) || glyph.advance<=0)return false;
        return true;
    }
    public void Configure(Text text, Framework2AppleHudGlyphSet characters, float scale, Vector2 offset)
    {source=text;glyphSet=characters;referenceScale=scale;opticalOffset=offset;raycastTarget=false;SetAllDirty();}
    protected override void OnEnable()
    {
        base.OnEnable();
        if(source!=null)source.RegisterDirtyVerticesCallback(SourceChanged);
    }
    protected override void OnDisable()
    {
        if(source!=null)source.UnregisterDirtyVerticesCallback(SourceChanged);
        base.OnDisable();
    }
    private void SourceChanged(){SetVerticesDirty();}
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        if(source==null || glyphSet==null || string.IsNullOrEmpty(source.text))return;
        string value=source.text;
        float advance=0;
        for(int i=0;i<value.Length;i++)
            if(glyphSet.TryGet(value[i],out var glyph))advance+=glyph.advance;
        Rect rect=rectTransform.rect;
        float scale=Mathf.Min(referenceScale,rect.width/Mathf.Max(advance,1));
        scale=Mathf.Min(scale,rect.height/Mathf.Max(glyphSet.Ascent-glyphSet.Descent,1));
        float x=rect.center.x-advance*scale*.5f+opticalOffset.x;
        float baseline=rect.center.y-(glyphSet.Ascent+glyphSet.Descent)*scale*.5f+opticalOffset.y;
        for(int i=0;i<value.Length;i++)
        {
            if(!glyphSet.TryGet(value[i],out var glyph))continue;
            if(glyph.size.x>0 && glyph.size.y>0)
            {
                float left=x+glyph.bearing.x*scale,bottom=baseline+glyph.bearing.y*scale;
                float right=left+glyph.size.x*scale,top=bottom+glyph.size.y*scale;
                int start=mesh.currentVertCount;Color32 tint=color;
                mesh.AddVert(new Vector3(left,bottom),tint,new Vector2(glyph.uv.xMin,glyph.uv.yMin));
                mesh.AddVert(new Vector3(left,top),tint,new Vector2(glyph.uv.xMin,glyph.uv.yMax));
                mesh.AddVert(new Vector3(right,top),tint,new Vector2(glyph.uv.xMax,glyph.uv.yMax));
                mesh.AddVert(new Vector3(right,bottom),tint,new Vector2(glyph.uv.xMax,glyph.uv.yMin));
                mesh.AddTriangle(start,start+1,start+2);mesh.AddTriangle(start,start+2,start+3);
            }
            x+=glyph.advance*scale;
        }
    }
}
