//////////////////////
/// 4点マイクをセットにしてデータを保持しておくようにする
/// //////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MicSetClass
{
    // 名前
    public string name = "default";
    // マイクの間隔[m]
    public float micInterval = 0.05f;
    // キャリブレーションデータ(RMS)
    public double[] calibData = new double[4] { 1, 1, 1, 1 };

    public MicSetClass(float in_mInterval, double[] in_calibData, string inName = null)
    {
        if (inName != null)
            name = inName;
        micInterval = in_mInterval;
        calibData = in_calibData;
    }
}
