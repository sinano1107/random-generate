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

        // 頂点座標配列をメッシュにセット
        myVertices = new Vector3[] {
            new Vector3(0, 1f),
            new Vector3(1f, -1f),
            new Vector3(-1f, -1f),
        };
        myMesh.SetVertices(myVertices);

        // インデックス配列をメッシュにセット
        myTriangles = new int[] { 0, 1, 2 };
        myMesh.SetTriangles(myTriangles, 0);

        // MeshFilterを通してメッシュをMeshRendererにセット
        var filter = GetComponent<MeshFilter>();
        filter.sharedMesh = myMesh;

        // MeshRendererにMaterialをセット
        var meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Unlit/ColorShader"));
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space)) //スペースキーの入力
        {
            for (int i = 0; i < myVertices.Length; i++)
            {
                //頂点をずらす
                myVertices[i] += new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f));
            }
            myMesh.SetVertices(myVertices); //新しい頂点を割り当てる
        }
    }
}
