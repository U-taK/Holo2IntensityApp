using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerMessage
{
    public SendType sendType = SendType.None;

    public ServerMessage()
    {
    }
}

public class IntensityPackage:ServerMessage
{
    public Vector3 sendPos;
    public Quaternion sendRot;
    public Vector3 intensity;
    public int num;
    public IntensityPackage(Vector3 sendPos, Quaternion sendRot, Vector3 intensity, int j):base()
    {
        sendType = SendType.Intensity;
        this.sendPos = sendPos;
        this.sendRot = sendRot;
        this.intensity = intensity;
        this.num = j;
    }
    public IntensityPackage(SendPosition sendPosition, Vector3 intensity, int j) : base()
    {
        sendType = SendType.Intensity;
        this.sendPos = sendPosition.sendPos;
        this.sendRot = sendPosition.sendRot;
        this.intensity = intensity;
        this.num = j;
    }
    public IntensityPackage() : base()
    {
        sendType = SendType.None;
    }
}

public class ReCalcDataPackage: ServerMessage
{
    public int storageNum;
    public List<Vector3> intensities;
    public List<int> sendNums;
    public ReCalcDataPackage(int storageNum): base()
    {
        sendType = SendType.ReCalcData;
        this.storageNum = storageNum;
    }
}