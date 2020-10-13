using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class SettingManager : MonoBehaviour
{

    //周波数レンジ[Hz]
    //中心周波数
    [SerializeField]
    InputField in_cFreq;
    [SerializeField]
    ToggleGroup OBTG;

    //空気密度
    [SerializeField]
    InputField inTemp;
    [SerializeField]
    InputField inAtm;

    //オブジェクト同士の間
    [SerializeField]
    InputField inOInterval;
    //インテンシティのレベルレンジ
    [SerializeField]
    InputField in_minIntensity;
    [SerializeField]
    InputField in_maxIntensity;

    //保存先のパス
    [SerializeField]
    InputField inSaveDir;

    //再重畳用パラメータ
    [SerializeField]
    InputField plotNum;
    [SerializeField]
    InputField oSize;
    [SerializeField]
    InputField micInterval;

    LogPanelManager logPanelManager;

    private void OnEnable()
    {
        logPanelManager = GetComponent<LogPanelManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
            InitParam();
    }
    /// <summary>
    /// MeasurementParameterに入力データ(計測条件)をセット
    /// 計測シーンのMeasurementStartにて呼び出される
    /// </summary>
    public void InitParam()
    {
        //周波数バンド
        CalcFreq();
        logPanelManager.Writelog("MinFreq:" + MeasurementParameter.FreqMin);
        logPanelManager.Writelog("MaxFreq:" + MeasurementParameter.FreqMax);
        
        //気体密度
        MeasurementParameter.Temp = int.Parse(inTemp.text);
        MeasurementParameter.Atm = float.Parse(inAtm.text);
        CalculateAtmDensity(MeasurementParameter.Atm, MeasurementParameter.Temp);
        logPanelManager.Writelog("Temp:" + MeasurementParameter.Temp + ",Atom:" + MeasurementParameter.Atm);
        logPanelManager.Writelog("atmDensity:" + MeasurementParameter.AtmDensity);

        //オブジェクト間間隔
        MeasurementParameter.ObjInterval = float.Parse(inOInterval.text);
        //インテンシティのレベルレンジ
        MeasurementParameter.MinIntensity = float.Parse(in_minIntensity.text);
        MeasurementParameter.MaxIntensity = float.Parse(in_maxIntensity.text);

        //保存先のパス
        MeasurementParameter.SaveDir = inSaveDir.text;
        logPanelManager.Writelog("interval:" + MeasurementParameter.ObjInterval);
        logPanelManager.Writelog("Intensity range is" + MeasurementParameter.MinIntensity + "~" + MeasurementParameter.MaxIntensity);
        logPanelManager.Writelog("save directory is" + MeasurementParameter.SaveDir);
    }

    /// <summary>
    /// 再重畳シーンにて呼び出し
    /// 計測条件を再設定
    /// </summary>
    public void InitParam4Repro()
    {
        //プロット数
        MeasurementParameter.plotNumber = int.Parse(plotNum.text);
        //オブジェクトサイズ
        MeasurementParameter.objSize = float.Parse(oSize.text);
        //マイクロホン間隔
        MeasurementParameter.MInterval = float.Parse(micInterval.text);

        //周波数バンド
        CalcFreq();
        logPanelManager.Writelog("MinFreq:" + MeasurementParameter.FreqMin);
        logPanelManager.Writelog("MaxFreq:" + MeasurementParameter.FreqMax);

        //気体密度
        MeasurementParameter.Temp = int.Parse(inTemp.text);
        MeasurementParameter.Atm = float.Parse(inAtm.text);
        CalculateAtmDensity(MeasurementParameter.Atm, MeasurementParameter.Temp);
        logPanelManager.Writelog("Temp:" + MeasurementParameter.Temp + ",Atom:" + MeasurementParameter.Atm);
        logPanelManager.Writelog("atmDensity:" + MeasurementParameter.AtmDensity);
       
        //インテンシティのレベルレンジ
        MeasurementParameter.MinIntensity = float.Parse(in_minIntensity.text);
        MeasurementParameter.MaxIntensity = float.Parse(in_maxIntensity.text);

        //保存先のパス
        MeasurementParameter.SaveDir = inSaveDir.text;
        logPanelManager.Writelog("interval:" + MeasurementParameter.ObjInterval);
        logPanelManager.Writelog("Intensity range is" + MeasurementParameter.MinIntensity + "~" + MeasurementParameter.MaxIntensity);
        logPanelManager.Writelog("save directory is" + MeasurementParameter.SaveDir);
    }

    private void CalcFreq()
    {
        var cFreq = int.Parse(in_cFreq.text);
        var OBvalue = OBTG.ActiveToggles().First().GetComponentsInChildren<Text>().First(t => t.name == "Label").text;
        switch (OBvalue)
        {
            case "1":
                OctaveBandFilter(cFreq, 1);
                break;
            case "1/2":
                OctaveBandFilter(cFreq, 2);
                break;
            case "1/3":
                OctaveBandFilter(cFreq, 3);
                break;
        }
    }

    /// <summary>
    /// 周波数領域を自動的に計算。計算した周波数バンド[freqMin,freqMax]はMeasurementParameterに渡される
    /// </summary>
    /// <param name="cfreq"></param>
    /// <param name="reciprocal"></param>
    private void OctaveBandFilter(int cfreq, int reciprocal)
    {
        float size = 1f/(reciprocal * 2f);
        double width = (double)Mathf.Pow(2, size);
        MeasurementParameter.FreqMin = (int)(cfreq / width);
        MeasurementParameter.FreqMax = (int)(cfreq * width);
        Debug.Log("Freqmin:" + MeasurementParameter.FreqMin);
        Debug.Log("Freqmax:" + MeasurementParameter.FreqMax);
    }

    private void CalculateAtmDensity(float atm, float temp)
    {
        //大気密度の計算法:ρ=P/{R(t+273.15)} ただしRは乾燥空気の気体定数2.87としている
        MeasurementParameter.AtmDensity = atm / (2.87f * (temp + 273.15f));
    }


}
