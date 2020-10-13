using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using System;

public class Holo2FileSurfaceObserver : MonoBehaviour
{
    //デシリアライズ側のシーンのみ必要
    //デシリアライズする空間マップの原点
    [SerializeField]
    GameObject origin;
    //空間マップのマテリアル
    [SerializeField]
    Material space_Material;

    int mCounter = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //SpatialMesh→空間マップ
    public void LoadEachMesh(SpatialMesh data)
    {
        StartCoroutine("ReproMesh", data);
    }

    IEnumerator ReproMesh(SpatialMesh data)
    {
        var mesh = new Mesh();
        mesh.vertices = data.vertices;
        mesh.triangles = data.triangles;
        // Reconstruct the normals from the vertices and triangles.
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        yield return null;

        //生成したmeshをもとにgameオブジェクトに変換
        Mesh2Object(mesh, mCounter++);        
        
        yield return null;
    }




    //空間マップをjsonへシリアライズ可能なクラスSpatialMapSenderに変換
    public SpatialMapSender MapSend()
    {
        // SpatialAwarenessSystemをIMixedRealityDataProviderAccessにキャストしてオブザーバーを取得します
        var access = CoreServices.SpatialAwarenessSystem as IMixedRealityDataProviderAccess;
        // 利用可能な最初のメッシュオブザーバーを取得します。通常、登録されているのは1つだけです。
        var observer = access.GetDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();
        // 既知のすべてのメッシュをループします
        SpatialMapSender spatialMapSender = new SpatialMapSender("new map",observer.Meshes.Count);

        foreach (SpatialAwarenessMeshObject meshObject in observer.Meshes.Values)
        {
            // ここでメッシュが取れます
            Mesh mesh = meshObject.Filter.mesh;

            spatialMapSender.meshParts.Add(new MeshParts(mesh));


        }

        return spatialMapSender;
    }

    //空間マップをjsonへシリアライズ可能なクラスSpatialMapSenderに変換
    public IMixedRealitySpatialAwarenessMeshObserver MapSendObserver()
    {
        // SpatialAwarenessSystemをIMixedRealityDataProviderAccessにキャストしてオブザーバーを取得します
        var access = CoreServices.SpatialAwarenessSystem as IMixedRealityDataProviderAccess;
        // 利用可能な最初のメッシュオブザーバーを取得します。通常、登録されているのは1つだけです。
        var observer = access.GetDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();
        return observer;
    }

    //SpatialMapSender→空間マップ
    public void LoadMesh(SpatialMapSender data)
    {
        StartCoroutine("ReproMesh", data);
    }

    IEnumerator ReproMesh(SpatialMapSender data)
    {
        int mCount = data.meshCount;
        for (int m = 0; m < mCount; m++)
        {
            var parts = data.meshParts[m];

            //Meshを作成
            var mesh = Parts2Mesh(parts);
            Debug.Log("Mesh Done " + m);
            yield return null;

            //生成したmeshをもとにgameオブジェクトに変換
            Mesh2Object(mesh, m);
            yield return null;
        }
        yield return null;
    }

    /// <summary>
    /// MeshPartsからMeshを生成
    /// </summary>
    /// <param name="meshParts">SpatialMapSenderに含まれるMesh</param>
    /// <returns></returns>
    Mesh Parts2Mesh(MeshParts meshParts)
    {
        var mesh = new Mesh();
        mesh.vertices = meshParts.vertices;
        mesh.triangles = meshParts.triangles;
        // Reconstruct the normals from the vertices and triangles.
        mesh.RecalculateNormals();
        return mesh;
    }

    void Mesh2Object(Mesh mesh, int m)
    {
        var surface = new GameObject("Surface-" + m, componentsRequiredForSurfaceMesh);
        surface.transform.SetParent(origin.transform);
        var filter = surface.GetComponent<MeshFilter>();

        filter.sharedMesh = mesh;
        filter.mesh = mesh;

        var renderer = surface.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = space_Material;

        var collider = surface.GetComponent<MeshCollider>();
        collider.sharedMesh = null;
        collider.sharedMesh = filter.sharedMesh;
        Debug.Log("Object Instantiate " + m);
    }


    protected readonly Type[] componentsRequiredForSurfaceMesh =
        {
            typeof(MeshFilter),
            typeof(MeshRenderer),
            typeof(MeshCollider)
        };
}
