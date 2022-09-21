using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class DynamicCreateMesh : MonoBehaviour
{
    const float range = 1.0f;
    private Mesh myMesh;
    private List<Vector3> myVertices;
    private List<int> myTriangles;
    private Vector3 myTriangleCenter;
    private List<(Vector3 X, Vector3 Y)> mySides;

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
        // myMesh.SetVertices(myVertices);

        var Z = GetZ(myVertices[0], myVertices[1], myVertices[2]);

        // 点2が左側の時は0,2,1の順、右側の時は0,1,2の順で結ぶ
        myTriangles = (Z > 0) ? new List<int> { 0, 2, 1 } : new List<int> { 0, 1, 2 };
        // myMesh.SetTriangles(myTriangles, 0);

        // 三角形の内心を保存
        myTriangleCenter = GetTriangleCenter(myVertices[0], myVertices[1], myVertices[2]);
        GameObject.Find("TriangleCenter").transform.position = myTriangleCenter;

        // 辺を保存
        mySides = new List<(Vector3, Vector3)>
        {
            (myVertices[0], myVertices[1]),
            (myVertices[1], myVertices[2]),
            (myVertices[2], myVertices[0]),
        };

        //=====================================================================
        // ↑ 核となる三角形を生成
        // ↓ そこから一つ三角形を増殖させて四角形を生成(最初の増殖は変の交差判定が機能しない場合があるため)
        //=====================================================================

        // 増殖元の辺を選択
        var choiceSideIndex = Random.Range(0, mySides.Count);
        var (vertex_0, vertex_1) = mySides[choiceSideIndex];

        // vertex_2の範囲を設定
        var x_max = System.Math.Max(vertex_0.x, vertex_1.x) + range;
        var x_min = System.Math.Min(vertex_0.x, vertex_1.x) - range;
        var y_max = System.Math.Max(vertex_0.y, vertex_1.y) + range;
        var y_min = System.Math.Min(vertex_0.y, vertex_1.y) - range;

        // vertex_2を選択
        var vertex_2 = new Vector3(Random.Range(x_max, x_min), Random.Range(y_max, y_min));

        // 核三角形の内心と、vertex_2のZ成分を算出
        var triangleCenter_Z = GetZ(vertex_0, vertex_1, myTriangleCenter);
        var vertex_2_Z = GetZ(vertex_0, vertex_1, vertex_2);

        // triangleCenter_Zとvertex_2_Zの正負が等しい場合、
        // 折返った形になっているので再試行する
        while ((triangleCenter_Z < 0 && vertex_2_Z < 0) || (triangleCenter_Z > 0 && vertex_2_Z > 0))
        {
            vertex_2 = new Vector3(Random.Range(x_max, x_min), Random.Range(y_max, y_min));
            vertex_2_Z = GetZ(vertex_0, vertex_1, vertex_2);
        }

        // 頂点を追加
        myVertices.Add(vertex_2);
        myMesh.SetVertices(myVertices);

        // vertex_2が左側の時は0,2,1の順、右側の時は0,1,2の順で結ぶ
        var newTriangles = (vertex_2_Z > 0)
            ? new List<int> { myVertices.IndexOf(vertex_0), 3, myVertices.IndexOf(vertex_1) }
            : new List<int> { myVertices.IndexOf(vertex_0), myVertices.IndexOf(vertex_1), 3 };
        // 三角形を追加
        myTriangles.AddRange(newTriangles);
        myMesh.SetTriangles(myTriangles, 0);

        // 折り目の辺を削除し、新たな二辺を追加する
        mySides.RemoveAt(choiceSideIndex);
        mySides.Add((vertex_0, vertex_2));
        mySides.Add((vertex_1, vertex_2));
    }

    // 増殖
    public void Proliferate()
    {
        var index = myVertices.Count;

        // 折り目となる辺のインデックス
        var choiceSideIndex = Random.Range(0, mySides.Count);
        var (vertex_0, vertex_1) = mySides[choiceSideIndex];

        // vertex_2の範囲を設定
        var x_max = System.Math.Max(vertex_0.x, vertex_1.x) + range;
        var x_min = System.Math.Min(vertex_0.x, vertex_1.x) - range;
        var y_max = System.Math.Max(vertex_0.y, vertex_1.y) + range;
        var y_min = System.Math.Min(vertex_0.y, vertex_1.y) - range;

        var vertex_2 = new Vector3(Random.Range(x_max, x_min), Random.Range(y_max, y_min));
        var vertex_2_Z = GetZ(vertex_0, vertex_1, vertex_2);

        GameObject.Find("NewVertice").transform.position = vertex_2;

        // 追加したい点と、既存の三角形全てとの内外判定を行う。
        Debug.Log(IsInPolygon(vertex_2));

        // todo ここで追加した辺と、既存の全辺との交差判定を行う。

        myVertices.Add(vertex_2);
        myMesh.SetVertices(myVertices);

        // vertex_2が左側の時は0,2,1の順、右側の時は0,1,2の順で結ぶ
        var newTriangles = (vertex_2_Z > 0)
            ? new List<int> { myVertices.IndexOf(vertex_0), index, myVertices.IndexOf(vertex_1) }
            : new List<int> { myVertices.IndexOf(vertex_0), myVertices.IndexOf(vertex_1), index };
        myTriangles.AddRange(newTriangles);
        myMesh.SetTriangles(myTriangles, 0);

        // 折り目の辺を削除し、新たな二辺を追加する
        mySides.RemoveAt(choiceSideIndex);
        mySides.Add((vertex_0, vertex_2));
        mySides.Add((vertex_1, vertex_2));
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

    // 点Pが多角形内に存在するか調べる
    private bool IsInPolygon(Vector3 P)
    {
        // 点Pが三角形内に存在するか調べる
        bool IsInTriangle(Vector3 p, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            bool b1, b2, b3;

            b1 = GetZ(p, v1, v2) < 0;
            b2 = GetZ(p, v2, v3) < 0;
            b3 = GetZ(p, v3, v1) < 0;

            return (b1 == b2) && (b2 == b3);
        }

        // 構成要素である全三角形との内外判定
        for (var i = 0; i < myTriangles.Count / 3; i++)
        {
            var index = i * 3;

            Vector3 v1, v2, v3;
            v1 = myVertices[myTriangles[index]];
            v2 = myVertices[myTriangles[index + 1]];
            v3 = myVertices[myTriangles[index + 2]];

            if (IsInTriangle(P, v1, v2, v3))
            {
                Debug.Log($"{i + 1}個目の三角形に内包されている");
                return true;
            }
            else
            {
                Debug.Log($"{i + 1}個目問題ない");
            }
        }

        return false;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            target.z = 0;
            GameObject.Find("ClickPoint").transform.position = target;

            if (IsItFoldedBack(target))
            {
                Debug.Log("衝突しています");
            }
            else
            {
                Debug.Log("衝突していません");
            }
        }
    }

    // 点Pと点a,bを結んだ辺が既存の辺と衝突しているか調べる
    private bool IsItFoldedBack(Vector3 P/*, Vector3 a, Vector3 b*/)
    {
        var testPoint_1 = new Vector3(-1, 0);
        var testPoint_2 = new Vector3(1, 0);
        var testPoint_3 = new Vector3(0, -1);

        var s = GetZ(testPoint_1, testPoint_2, testPoint_3);
        var t = GetZ(testPoint_1, testPoint_2, P);
        var u = GetZ(testPoint_3, P, testPoint_1);
        var v = GetZ(testPoint_3, P, testPoint_2);

        return (s * t < 0) && (u * v < 0);
    }
}
