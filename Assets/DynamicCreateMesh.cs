using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class DynamicCreateMesh : MonoBehaviour
{
    private void Start()
    {
        // メッシュの作成
        var mesh = new Mesh();

        // 頂点座標配列をメッシュにセット
        mesh.SetVertices(new Vector3[]
        {
            new Vector3(0, 1f),
            new Vector3(1f, -1f),
            new Vector3(-1f, -1f),
        });

        // インデックス配列をメッシュにセット
        mesh.SetTriangles(new int[]
        {
            0, 1, 2
        }, 0);

        // MeshFilterを通してメッシュをMeshRendererにセット
        var filter = GetComponent<MeshFilter>();
        filter.sharedMesh = mesh;
    }
}
