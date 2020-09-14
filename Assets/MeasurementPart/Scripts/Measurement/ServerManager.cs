///////////////////////////////////////////
///サーバの挙動を管理
/// [TODO] オブジェクト削除時の動作を導入
/// ///////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloLensModule.Network;
using System;
using AsioCSharpDll;

public class ServerManager : MonoBehaviour
{
    // Start is called before the first frame update

    //ポート
    [SerializeField] int Port = 3333;
    
    TCPServerManager tServer;
    TransferData transferData = TransferData.transferData;

    [SerializeField]
    GameObject UIManager;

    LogPanelManager logPanelManager;
    SettingManager settingManager;
    IntensityManager intensityManager;

    //送信するインテンシティデータ
    IntensityPackage sendIntensityData;


    void Start()
    {
        logPanelManager = UIManager.GetComponent<LogPanelManager>();
        settingManager = UIManager.GetComponent<SettingManager>();
        intensityManager = gameObject.GetComponent<IntensityManager>();

        //ASIOスタート
        string instLog = asiocsharpdll.PrepareAsio(MeasurementParameter.AsioDriverName, MeasurementParameter.Fs, MeasurementParameter.SampleNum);
        logPanelManager.Writelog(instLog);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /**tcp始めるボタンに紐づけ**/
    public void StartTCPServer()
    {

        tServer = new TCPServerManager(Port);
        //データ受信イベント
        tServer.ListenerMessageEvent += tServer_OnReceiveData;
        tServer.OnDisconnected += tServer_OnDisconnected;
        tServer.OnConnected += tServer_OnConnected;

        //計測データの設定反映
        settingManager.InitParam();

        Debug.Log("Init setting");
    }

    /// <summary>
	/// 接続断イベント
	/// </summary>
    void tServer_OnDisconnected(object sender, EventArgs e)
    {
        Debug.Log("Server接続解除");
    }

    /// <summary>
	/// 接続OKイベント
	/// </summary>
    void tServer_OnConnected(EventArgs e)
    {
        Debug.Log("Clientと接続完了");
        logPanelManager.Writelog("Clientと接続完了");
    }

    /// <summary>
	/// データ受信イベント
	/// </summary>
    void tServer_OnReceiveData(string ms)
    {
        var message = new HoloLensMessage();
        if (transferData.CanDesirializeJson<HoloLensMessage>(ms, out message))
        {
            switch (message.sendType)
            {
                case SendType.PositionSender://HoloLens側のマイクロホン位置を取得した際にオブジェクト生成を管理する
                    if (transferData.CanDesirializeJson<SendPosition>(ms, out var sendPosition))
                    {
                        //送信するインテンシティオブジェクトに変換
                        sendIntensityData = intensityManager.MicPosReceived(sendPosition);
                        if (sendIntensityData.sendType == SendType.Intensity)
                        {
                            tServer.SendMessage(transferData.SerializeJson<IntensityPackage>(sendIntensityData));
                            logPanelManager.WriteConsole(sendIntensityData.num, sendIntensityData.sendPos, sendIntensityData.intensity);
                        }
                            
                    }
                    break;
                case SendType.SettingSender://HoloLens側でsettingするパラメータを反映
                    if(transferData.CanDesirializeJson<SettingSender>(ms, out var holoSetting))
                        MeasurementParameter.HoloLensParameterUpdate(holoSetting);
                    break;
            }
        }


    }


    /// <summary>
    /// 
    /// </summary>
    public async void UpdateData()
    {
        settingManager.InitParam();
        SettingSender ud_setting = new SettingSender("NewSetting", MeasurementParameter.colormapID, MeasurementParameter.MaxIntensity, MeasurementParameter.MinIntensity, MeasurementParameter.objSize);
        //更新データをHololensに送信
        string json = transferData.SerializeJson<SettingSender>(ud_setting);
        tServer.SendMessage(json);
        //再計算を実行
        ReCalcDataPackage reCalcDataPackage = await intensityManager.RecalcIntensity();
        string json2 = transferData.SerializeJson<ReCalcDataPackage>(reCalcDataPackage);
        tServer.SendMessage(json2);
    }


    private void OnApplicationQuit()
    {
        if (tServer != null)
            StopTCPServer();
    }

    /**tcp止めるボタンにも紐付け可能**/
    /// <summary>
	/// TCPServer停止
	/// </summary>
    public void StopTCPServer()
    {
        try
        {
            //closeしてるけど送受信がどうなってるかは謎
            tServer.DisConnectClient();
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

}
