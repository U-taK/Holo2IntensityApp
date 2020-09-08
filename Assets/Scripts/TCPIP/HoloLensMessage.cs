using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SendType
{
    None,
    PositionSender,
    SettingSender
}
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
