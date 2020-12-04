using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ServerMessage
{
    public SendType sendType = SendType.None;

    public ServerMessage()
    {
    }
}

[Serializable]
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

[Serializable]
public class TransIntensityPackage: ServerMessage
{
    public Vector3 sendPos;
    public Quaternion sendRot;
    public Vector3 sumIntensity;
    public Vector3[] IIntensities;
    public int num;
    public TransIntensityPackage(Vector3 sendPos, Quaternion sendRot, Vector3 intensity, Vector3[] iintensities, int j) : base()
    {
        sendType = SendType.IIntensities;
        this.sendPos = sendPos;
        this.sendRot = sendRot;
        this.sumIntensity = intensity;
        this.IIntensities = iintensities;
        this.num = j;
    }
    public TransIntensityPackage(SendPosition sendPosition, Vector3 intensity, Vector3[] iintensities, int j) : base()
    {
        sendType = SendType.IIntensities;
        this.sendPos = sendPosition.sendPos;
        this.sendRot = sendPosition.sendRot;
        this.sumIntensity = intensity;
        this.IIntensities = iintensities;
        this.num = j;
    }
    public TransIntensityPackage() : base()
    {
        sendType = SendType.None;
    }
}

[Serializable]
public class ReCalcTransientDataPackage: ServerMessage
{
    public int storageNum;
    public List<Vector3> intensities;
    public List<Vector3> iintensityList;
    public List<int> sendNums;
    public ReCalcTransientDataPackage(int storageNum): base()
    {
        sendType = SendType.ReCalcTransData;
        this.storageNum = storageNum;
        sendNums = new List<int>();
        intensities = new List<Vector3>();
        iintensityList = new List<Vector3>();
    }

    public ReCalcTransientDataPackage() : base()
    {
        sendType = SendType.None;
    }
}


[Serializable]
public class ReCalcDataPackage: ServerMessage
{
    public int storageNum;
    public List<Vector3> intensities;
    public List<int> sendNums;
    public ReCalcDataPackage(int storageNum): base()
    {
        sendType = SendType.ReCalcData;
        this.storageNum = storageNum;
        sendNums = new List<int>();
        intensities = new List<Vector3>();
    }

    public ReCalcDataPackage(): base()
    {
        sendType = SendType.None;
    }
}

[Serializable]
public class ReproDataPackage: ReCalcDataPackage
{
    public AlgorithmPattern algorithm; 
    public List<Vector3> sendPoses;
    public List<Quaternion> sendRots;
    public List<Vector3> iintensities;
    public ReproDataPackage(int storageNum,AlgorithmPattern algorithm) : base(storageNum)
    {
        sendType = SendType.ReproData;
        this.algorithm = algorithm;
        sendNums = new List<int>();
        sendPoses = new List<Vector3>();
        sendRots = new List<Quaternion>();
        intensities = new List<Vector3>();
        iintensities = new List<Vector3>();
    }

    public ReproDataPackage() : base()
    {

    }
}

[Serializable]
public class How2Measure: ServerMessage
{
    public MeasurementType measureType;
    public int blockSize;

    public How2Measure(MeasurementType type, int b_size): base()
    {
        sendType = SendType.MeasurementType;
        measureType = type;
        blockSize = b_size;
    }

    public How2Measure(): base()
    {
        sendType = SendType.None;
    }
}