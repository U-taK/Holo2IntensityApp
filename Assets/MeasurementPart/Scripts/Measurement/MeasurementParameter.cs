using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasurementParameter : MonoBehaviour
{
    //サンプリング周波数
    static int fs = 44100;
    public static int Fs
    {
        set { fs = value; }
        get { return fs; }
    }
    //サンプル数
    static int sampleNum = 4096;
    public static int SampleNum
    {
        set { sampleNum = value; }
        get { return sampleNum; }
    }

    //キャリブレーションの値
    static double[] calibValue = new double[4] { 1, 1, 1, 1 };
    public static double[] CalibValue
    {
        set { calibValue = value; }
        get { return calibValue; }
    }
    public static int CalibSize
    {
        get { return calibValue.Length; }
    }

    //マイクロホン間間隔
    static float mInterval = 0.05f;
    public static float MInterval
    {
        set { mInterval = value; }
        get { return mInterval; }
    }

    //マイクセットの名前
    public static string mSetName = "default";

    //Asioのドライバー名
    static string asioDriver = "MOTU Pro Audio";

    public static string AsioDriverName
    {
        set { asioDriver = value; }
        get { return asioDriver; }
    }
}
