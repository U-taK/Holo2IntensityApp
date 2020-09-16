/////////////////////////////////
///HoloLens2がServerに送信するクラス
/////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 送信オブジェクトの親クラス
/// </summary>
[Serializable]
public class HoloLensMessage
{
    public string name;
    public SendType sendType = SendType.None;


    public HoloLensMessage(string name)
    {
        this.name = name;
    }

    public HoloLensMessage()
    {

    }
}
/// <summary>
/// 座標送信時に使用
/// </summary>
[Serializable]
public class SendPosition : HoloLensMessage
{
    public Vector3 sendPos;
    public Quaternion sendRot;

    public SendPosition(string name, Vector3 sendPos, Quaternion sendRot) : base(name)
    {
        sendType = SendType.PositionSender;
        this.sendPos = sendPos;
        this.sendRot = sendRot;
    }

    public SendPosition() : base()
    {
        sendType = SendType.PositionSender;
    }
}

/// <summary>
/// 設定更新時に使用(特に最初の設定)
/// </summary>
[Serializable]
public class SettingSender: HoloLensMessage
{
    public int colorMapID;
    public float lvMax;
    public float lvMin;
    public float objSize;
    public SettingSender(string name,int cMapID, float lvMax, float lvMin, float oSize) : base(name)
    {
        sendType = SendType.SettingSender;
        this.colorMapID = cMapID;
        this.lvMax = lvMax;
        this.lvMin = lvMin;
        this.objSize = oSize;
    }

    public SettingSender() : base()
    {
        sendType = SendType.SettingSender;
    }
}

/// <summary>
/// 空間マップを測定開始前に送信
/// </summary>
[Serializable]
public class SpatialMapSender : HoloLensMessage
{
    //メッシュの数
    public int meshCount;
    //メッシュの中身
    public List<MeshParts> meshParts;

    public SpatialMapSender(string name, int meshCount) : base(name)
    {
        sendType = SendType.SpatialMap;
        this.meshCount = meshCount;
        meshParts = new List<MeshParts>();
    }

    public SpatialMapSender() : base()
    {
        sendType = SendType.SpatialMap;
    }
}

/// <summary>
/// 空間マップを構成するために1つ1つのMeshの構成内容
/// </summary>
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