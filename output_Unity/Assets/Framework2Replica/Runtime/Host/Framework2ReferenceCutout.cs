using UnityEngine;
using UnityEngine.UI;

/// <summary>An authored sprite contour; no texture readback, masking camera, or per-frame work.</summary>
[RequireComponent(typeof(Image))]
[DisallowMultipleComponent]
public sealed class Framework2ReferenceCutout : BaseMeshEffect
{
    [SerializeField] private string atlasResourcePath;
    private Sprite atlas;
    [SerializeField] private Vector2[] positions;
    [SerializeField] private Vector2[] uvs;
    [SerializeField] private int[] triangles;
    [SerializeField] private float[] opacity;

    public void Configure(Sprite source, Vector2[] points, Vector2[] textureCoordinates, int[] indices,float[] alpha=null)
    {
        atlasResourcePath="Framework2AppleSkin/"+source.name;
        atlas=source; positions=points; uvs=textureCoordinates; triangles=indices; opacity=alpha; ApplyAtlas();
        if(graphic!=null)graphic.SetVerticesDirty();
    }
    protected override void OnEnable(){base.OnEnable();ApplyAtlas();}
    void ApplyAtlas()
    {
        if(atlas==null)atlas=Framework2AppleSkinAssets.LoadReferenceAtlas(atlasResourcePath);
        if(atlas==null)return;
        Image image=graphic as Image;
        if(image==null)return;
        image.overrideSprite=atlas; image.type=Image.Type.Simple; image.preserveAspect=false;
    }
    public override void ModifyMesh(VertexHelper mesh)
    {
        if(!IsActive() || positions==null || triangles==null)return;
        mesh.Clear(); Rect rect=graphic.rectTransform.rect;
        for(int i=0;i<positions.Length;i++)
        {
            var v=UIVertex.simpleVert; v.color=graphic.color;
            if(opacity!=null && opacity.Length==positions.Length)v.color.a=(byte)(v.color.a*opacity[i]);
            v.position=new Vector3(rect.xMin+positions[i].x*rect.width,rect.yMin+positions[i].y*rect.height,0);
            v.uv0=uvs[i]; mesh.AddVert(v);
        }
        for(int i=0;i<triangles.Length;i+=3)mesh.AddTriangle(triangles[i],triangles[i+1],triangles[i+2]);
    }
}
