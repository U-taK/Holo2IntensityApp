////////////////////////////////////////////////
///HoloLens2のシーン内で共有するパラメータ
///(注意)UDP対応のシーン内にUIManagerがあるがここでは使わない
///////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Holo2MeasurementParameter : MonoBehaviour
{
    //colormapの種類を選択 0:grayscale,1:parula,2:jet
    static int colorMapID = 1;
    public static int ColorMapID
    {
        set { colorMapID = value; }
        get { return colorMapID; }
    }

    //表示したいレベルの範囲を指定
    static float levelMin = 60;
    static float levelMax = 105;

    public static float LevelMin
    {
        set { levelMin = value; }
        get { return levelMin; }
    }

    public static float LevelMax
    {
        set { levelMax = value; }
        get { return levelMax; }
    }

    //コーンのサイズ
    static float objSize = 0.05f;
    public static float ObjSize
    {
        set { objSize = value; }
        get { return objSize; }
    }

    //計測可能か？
    public static bool _measure = false;
    public static bool _instance = false;
    public static bool on_connected = false;

    //ServerのIPアドレス
    public static string IP;

    //時間平均か過渡音計測を決めるステータス
    public static MeasurementType measurementType = MeasurementType.Standard;

    public static int fs = 44100;
}
