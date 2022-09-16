using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class DynamicCreateMesh : MonoBehaviour
{
    private Mesh myMesh;
    private List<Vector3> myVertices;
    private List<int> myTriangles;
    private Vector3 myTriangleCenter;

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

    // 生成
    public void Generate()
    {
        const float min = -2.0f;
        const float max = 2.0f;

        // リセット
        myMesh.SetTriangles(new int[] { }, 0);

        // 頂点座標配列をメッシュにセット
        myVertices = new List<Vector3>()
        {
            new Vector3(Random.Range(min, max), Random.Range(min, max)),
            new Vector3(Random.Range(min, max), Random.Range(min, max)),
            new Vector3(Random.Range(min, max), Random.Range(min, max)),
        };
        myMesh.SetVertices(myVertices);

        var Z = GetZ(myVertices[0], myVertices[1], myVertices[2]);

        // 点2が左側の時は0,2,1の順、右側の時は0,1,2の順で結ぶ
        myTriangles = (Z > 0) ? new List<int> { 0, 2, 1 } : new List<int> { 0, 1, 2 };
        myMesh.SetTriangles(myTriangles, 0);

        // 三角形の内心を保存
        myTriangleCenter = GetTriangleCenter(myVertices[0], myVertices[1], myVertices[2]);
        GameObject.Find("TriangleCenter").transform.position = myTriangleCenter;
    }

    // 増殖
    public void Proliferate()
    {
        const float range = 3.0f;

        var vertex_0_index = Random.Range(0, 3);
        var vertex_1_index = Random.Range(0, 3);
        // 二点が被らなくなるまでシャッフル
        while (vertex_0_index == vertex_1_index)
        {
            vertex_1_index = Random.Range(0, 3);
        }

        var vertex_0 = myVertices[vertex_0_index];
        var vertex_1 = myVertices[vertex_1_index];

        // vertex_2の範囲を設定
        var x_max = System.Math.Max(vertex_0.x, vertex_1.x) + range;
        var x_min = System.Math.Min(vertex_0.x, vertex_1.x) - range;
        var y_max = System.Math.Max(vertex_0.y, vertex_1.y) + range;
        var y_min = System.Math.Min(vertex_0.y, vertex_1.y) - range;

        var vertex_2 = new Vector3(Random.Range(x_max, x_min), Random.Range(y_max, y_min));
        var triangleCenter_Z = GetZ(vertex_0, vertex_1, myTriangleCenter);
        var vertex_2_Z = GetZ(vertex_0, vertex_1, vertex_2);

        // triangleCenter_Zとvertex_2_Zの正負が等しい場合、
        // 折返った形になっているので再試行する
        while ((triangleCenter_Z < 0 && vertex_2_Z < 0) || (triangleCenter_Z > 0 && vertex_2_Z > 0))
        {
            vertex_2 = new Vector3(Random.Range(x_max, x_min), Random.Range(y_max, y_min));
            vertex_2_Z = GetZ(vertex_0, vertex_1, vertex_2);
        }

        myVertices.Add(vertex_2);
        myMesh.SetVertices(myVertices);

        // vertex_2が左側の時は0,2,1の順、右側の時は0,1,2の順で結ぶ
        var newTriangles = (vertex_2_Z > 0)
            ? new List<int> { vertex_0_index, 3, vertex_1_index }
            : new List<int> { vertex_0_index, vertex_1_index, 3 };
        myTriangles.AddRange(newTriangles);
        myMesh.SetTriangles(myTriangles, 0);
    }

    // 三角形の内心の座標を求める
    private Vector3 GetTriangleCenter(Vector3 A, Vector3 B, Vector3 C)
    {
        var a = Vector3.Distance(B, C);
        var b = Vector3.Distance(C, A);
        var c = Vector3.Distance(A, B);

        var x = (a * A.x + b * B.x + c * C.x) / (a + b + c);
        var y = (a * A.y + b * B.y + c * C.y) / (a + b + c);

        return new Vector3(x, y);
    }

    // 外積のZを計算 返り値が正なら左、負なら右
    // Start: 直線の始点
    // End: 直線の終点
    // Target: 調べたい点
    private float GetZ(Vector3 Start, Vector3 End, Vector3 Target)
    {
        // 終点から始点への直線のベクトル
        var a = End - Start;

        // 始点から目標へのベクトル
        var b = Target - Start;

        // aとbの外積のZ成分
        var Z = a.x * b.y - b.x * a.y;
        return Z;
    }
}
