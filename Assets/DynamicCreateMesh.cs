using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class DynamicCreateMesh : MonoBehaviour
{
    private Mesh myMesh;
    private Vector3[] myVertices;
    private int[] myTriangles;

    private void Start()
    {
        // メッシュの作成
        myMesh = new Mesh();

        Generate();

        // MeshFilterを通してメッシュをMeshRendererにセット
        var filter = GetComponent<MeshFilter>();
        filter.sharedMesh = myMesh;

        // MeshRendererにMaterialをセット
        var meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Unlit/ColorShader"));
    }

    public void Generate()
    {
        const float min = -2.0f;
        const float max = 2.0f;

        // 頂点座標配列をメッシュにセット
        myVertices = new Vector3[]
        {
            new Vector3(0, -1),
            new Vector3(0, 1),
            new Vector3(Random.Range(min, max), Random.Range(min, max)),
        };
        myMesh.SetVertices(myVertices);

        // 点0から点1へのベクトル
        var a = myVertices[1] - myVertices[0];

        // 点0から点2へのベクトル
        var b = myVertices[2] - myVertices[0];

        // aとbの外積のZ成分
        var Z = a.x * b.y - b.x * a.y;

        // 点2が左側の時は0,2,1の順、右側の時は0,1,2の順で結ぶ
        myTriangles = (Z > 0) ? new int[] { 0, 2, 1 } : new int[] { 0, 1, 2 };
        myMesh.SetTriangles(myTriangles, 0);
    }
}
