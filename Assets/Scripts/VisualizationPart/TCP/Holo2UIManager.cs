////////////////////////////////////////////////
///UIで設定された内容をServer、HoloLens2内で共有
///(注意)UDP対応のシーン内にUIManagerがあるがここでは使わない
///////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

public class Holo2UIManager : MonoBehaviour
{
    //ラジオボタン
    [SerializeField]
    InteractableToggleCollection colorMapID;
    //InteractiveSet colormapID;

    //音響インテンシティレベルのレンジの小さい方
    [SerializeField]    
    TextMesh lvMin;
    //音響インテンシティレベルのレンジの大きい方
    [SerializeField]
    TextMesh lvMax;
    //Instantiateするオブジェクトのサイズ
    [SerializeField]
    TextMesh size;

    [SerializeField]
    TextMesh[] IPs = new TextMesh[4];


    //UI上のパラメータを反映
    public void UpdateUIParameter()
    {
        Holo2MeasurementParameter.IP = null;
        for (int i = 0; i < 4; i++)
        {
            Holo2MeasurementParameter.IP += IPs[i].text;
            if (i != 3)
                Holo2MeasurementParameter.IP += ".";
        }

        // ColorMapID = colormapID.SelectedIndices[0];
        Holo2MeasurementParameter.ColorMapID = colorMapID.CurrentIndex;
        Holo2MeasurementParameter.LevelMin = float.Parse(lvMin.text);
        Holo2MeasurementParameter.LevelMax = float.Parse(lvMax.text);
        Holo2MeasurementParameter.ObjSize = float.Parse(size.text);
    }
}
