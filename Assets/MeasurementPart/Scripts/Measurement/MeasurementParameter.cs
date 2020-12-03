using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasurementParameter : MonoBehaviour
{
    //TCP用のアドレス
    public static string TCPAdress;

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

    static string targetSource = "SIN1kHzsMin";

    public static string TargetSource
    {
        set { targetSource = value; }
        get { return targetSource; }
    }
    //周波数バンド
    static int freqMin = 700;
    static int freqMax = 1000;
    public static int FreqMin
    {
        set { freqMin = value; }
        get { return freqMin; }
    }
    public static int FreqMax
    {
        set { freqMax = value; }
        get { return freqMax; }
    }


    //気体密度
    public static float Temp = 25;
    public static float Atm = 1013;
    static float atmDensity = 1.02f;
    public static float AtmDensity
    {
        set { atmDensity = value; }
        get { return atmDensity; }
    }

    //プロット間隔
    static float objInterval = 0.1f;
    public static float ObjInterval
    {
        set { objInterval = value; }
        get { return objInterval; }
    }
    //インテンシティのレベルレンジ
    static float minIntensity = 65;
    public static float MinIntensity
    {
        set { minIntensity = value; }
        get { return minIntensity; }
    }
    static float maxIntensity = 105;
    public static float MaxIntensity
    {
        set { maxIntensity = value; }
        get { return maxIntensity; }
    }

    //保存先のパス
    static string saveDir = "";
    public static string SaveDir
    {
        set { saveDir = value; }
        get { return saveDir; }
    }

    public static int colormapID = 2;//0がgrayscale,1がparula,2がjet
    public static float lvMin = 60;
    public static float lvMax = 105;
    public static float objSize = 0.05f;

    //HoloLensから送信されてきたデータを設定に反映
    public static void HoloLensParameterUpdate(SettingSender setting)
    {
        colormapID = setting.colorMapID;
        lvMax = setting.lvMax;
        lvMin = setting.lvMin;
        setting.objSize = objSize;
    }

    //データの保存数
    public static int plotNumber;

    //過渡音を対象にしたインテンシティを計算する際のブロックサイズ
    public static int i_block = 1;
}
