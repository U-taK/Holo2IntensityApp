//using HoloToolkit.Examples.InteractiveElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uOSC;
using Microsoft.MixedReality.Toolkit.UI;

public class UIManager : MonoBehaviour
{

    [SerializeField]
    InteractableToggleCollection colorMapID;
    //InteractiveSet colormapID;

    [SerializeField]
    TextMesh lvMin;
    [SerializeField]
    TextMesh lvMax;
    [SerializeField]
    TextMesh size;
    [SerializeField]
    PositionSender positionSender;

    //colormapの種類を選択 0:grayscale,1:parula,2:jet
    public static int ColorMapID = 1;
    //表示したいレベルの範囲を指定
    public static float LevelMin = 60;
    public static float LevelMax = 105;
    //コーンのサイズ
    public static float ObjSIze = 0.05f;
    //計測可能か？
    public static bool _measure = false;
    public static bool _instance = false;

    [SerializeField] TextMesh[] IPs = new TextMesh[4];
    public static string IP;
    //
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitSetting()
    {
        for (int i = 0; i < 4; i++)
        {
            IP += IPs[i].text;
            if (i != 3)
                IP += ".";
        }
        positionSender.SendStart(IP);
        // ColorMapID = colormapID.SelectedIndices[0];
        ColorMapID = colorMapID.CurrentIndex;
        LevelMin = float.Parse(lvMin.text);
        LevelMax = float.Parse(lvMax.text);
        ObjSIze = float.Parse(size.text);
        positionSender.SendSetting();
    }
}
