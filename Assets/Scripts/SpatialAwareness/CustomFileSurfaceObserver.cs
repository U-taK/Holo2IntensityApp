using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CustomFileSurfaceObserver : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public SpatialMapData MeshSend()
    {
        List<Mesh> meshes = new List<Mesh>();
        // SpatialAwarenessSystemをIMixedRealityDataProviderAccessにキャストしてオブザーバーを取得します
        var access = CoreServices.SpatialAwarenessSystem as IMixedRealityDataProviderAccess;
        // 利用可能な最初のメッシュオブザーバーを取得します。通常、登録されているのは1つだけです。
        var observer = access.GetDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();
        // 既知のすべてのメッシュをループします
        SpatialMapData spatialMapData = new SpatialMapData(observer.Meshes.Count);
        foreach (SpatialAwarenessMeshObject meshObject in observer.Meshes.Values)
        {
            // ここでメッシュが取れます
            Mesh mesh = meshObject.Filter.mesh;

            spatialMapData.meshParts.Add(new MeshParts(mesh));
                       
            
        }
        string json = JsonUtility.ToJson(spatialMapData);
        Debug.Log("Done");
        Debug.Log(json);
        return spatialMapData;
    }
}

[Serializable]
public class SpatialMapData : TransferParent
{
    //メッシュの数
    public int meshCount;
    //メッシュの中身
    public List<MeshParts> meshParts;

    public SpatialMapData(int meshCount) : base()
    {
        keyword = "Map";
        this.meshCount = meshCount;
        meshParts = new List<MeshParts>();
    }

    public SpatialMapData()
    {

    }
}

[Serializable]
public class MeshParts
{
    //頂点数
    public int vertCount;
    //メッシュ構成順番の長さ
    public int trianglesCount;

    //頂点
    public Vector3[] vertices;


    //メッシュ構成順番
    public int[] triangles;

    public MeshParts(int vCount, int tCount, Vector3[] vertices, int[] triangles)
    {
        this.vertCount = vCount;
        this.trianglesCount = tCount;
        this.vertices = vertices;
        this.triangles = triangles;
    }

    public MeshParts(Mesh mesh)
    {
        this.vertCount = mesh.vertexCount;
        this.trianglesCount = mesh.triangles.Length;
        this.vertices = mesh.vertices;
        this.triangles = mesh.triangles;

    }
}

//////////////
///meshの必要パート
///meshそのものの数
///mesh.vertices; 頂点数
///mesh.triangles　三角形を作り出すための頂点
///



////// Create the mesh.
///Mesh mesh = new Mesh();
///mesh.vertices = vertices;
///mesh.triangles = triangleIndices;
/// Reconstruct the normals from the vertices and triangles.
////mesh.RecalculateNormals();
///WriteMeshHeader(writer, mesh.vertexCount, mesh.triangles.Length);
///WriteVertices(writer, mesh.vertices, transform, secondarySpace);
///WriteTriangleIndicies(writer, mesh.triangles);
