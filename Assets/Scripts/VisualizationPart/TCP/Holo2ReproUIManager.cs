////////////////////////////////////////////////
///Sharing用のシーンに対応したUIManager
///////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

public class Holo2ReproUIManager : MonoBehaviour
{
    [SerializeField]
    TextMesh[] IPs = new TextMesh[4];

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 初期パラメータ、IPアドレスの設定
    /// </summary>
    public void InitUIParameter()
    {
        Holo2MeasurementParameter.IP = null;
        for (int i = 0; i < 4; i++)
        {
            Holo2MeasurementParameter.IP += IPs[i].text;
            if (i != 3)
                Holo2MeasurementParameter.IP += ".";
        }
    }
}
