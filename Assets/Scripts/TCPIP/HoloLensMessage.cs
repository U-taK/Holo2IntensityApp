/////////////////////////////////
///HoloLens2がServerに送信するクラス
/////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 送信オブジェクトの親クラス
/// </summary>
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
