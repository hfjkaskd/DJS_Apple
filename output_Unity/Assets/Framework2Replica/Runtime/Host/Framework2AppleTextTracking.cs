using UnityEngine;
using UnityEngine.UI;

/// <summary>Authored letter spacing for live numeric HUD text. Values remain ordinary Text bindings.</summary>
[RequireComponent(typeof(Text))]
public sealed class Framework2AppleTextTracking : BaseMeshEffect
{
    [SerializeField] private float tracking;
    public void Configure(float value) { tracking = value; graphic.SetVerticesDirty(); }
    public override void ModifyMesh(VertexHelper mesh)
    {
        if (!IsActive() || tracking == 0 || mesh.currentVertCount < 4) return;
        int count = mesh.currentVertCount / 4;
        float centre = (count - 1) * .5f;
        UIVertex vertex = default;
        for (int i = 0; i < mesh.currentVertCount; i++)
        {
            mesh.PopulateUIVertex(ref vertex, i);
            vertex.position.x += (i / 4 - centre) * tracking;
            mesh.SetUIVertex(vertex, i);
        }
    }
}
