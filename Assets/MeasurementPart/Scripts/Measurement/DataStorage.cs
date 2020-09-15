using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataStorage
{

    //計測番号
    public int measureNo;
    //計測点座標
    public Vector3 micLocalPos;
    public Quaternion micLocalRot;
    //計測信号
    public double[][] soundSignal;
    //インテンシティ
    public Vector3 intensityDir;

    public DataStorage()
    {
    }

    public DataStorage(int j, Vector3 vector, Quaternion quaternion, double[][] measureSig, Vector3 intensity)
    {
        measureNo = j;
        micLocalPos = vector;
        micLocalRot = quaternion;
        soundSignal = measureSig;
        intensityDir = intensity;
    }

    public bool CheckPlotDistance(Vector3 nowPos, float settingDist)
    {
        float distance = (nowPos - micLocalPos).sqrMagnitude;
        return distance > Mathf.Pow(settingDist, 2);
    }
}
