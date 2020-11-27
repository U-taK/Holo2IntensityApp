using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloLensModule.Network;
using System;
using AsioCSharpDll;

public class ServerManager : MonoBehaviour
{
    TCPServer tServer;
    TransferData transferData = TransferData.transferData;

    //ポート番号規定値
    int port = 3333;

    [SerializeField]
    GameObject UIManager;

    LogPanelManager logPanelManager;
    SettingManager settingManager;
    IntensityManager intensityManager;

    Holo2FileSurfaceObserver surfaceObserver;
    //送信するインテンシティデータ
    IntensityPackage sendIntensityData;

    //受信データのストック先
    //Queue<SpatialMapSender> spatialMaps = new Queue<SpatialMapSender>();
    Queue<SpatialMesh> spatialMeshs = new Queue<SpatialMesh>();

    Queue<SendPosition> positionPackages = new Queue<SendPosition>();

    Queue<DeleteData> deleteDatas = new Queue<DeleteData>();

    Queue<string> logQueue = new Queue<string>();

    bool onServer = false;//サーバーの受付を開始したかどうか

    // Start is called before the first frame update
    void Start()
    {
        logPanelManager = UIManager.GetComponent<LogPanelManager>();
        settingManager = UIManager.GetComponent<SettingManager>();
        intensityManager = gameObject.GetComponent<IntensityManager>();
        surfaceObserver = gameObject.GetComponent<Holo2FileSurfaceObserver>();

        //Asioスタート
        string instLog = asiocsharpdll.PrepareAsio(MeasurementParameter.AsioDriverName, MeasurementParameter.Fs, MeasurementParameter.SampleNum);
        logPanelManager.Writelog(instLog);
    }

    // Update is called once per frame
    void Update()
    {
        //旧型spatialmap送信システム
        /*if (spatialMaps.Count > 0)
        {
            var mapData = spatialMaps.Dequeue();
            surfaceObserver.LoadMesh(mapData);
        }*/
        if (positionPackages.Count > 0)
        {
            var rPosition = positionPackages.Dequeue();
            StartCoroutine("SendIntensity", rPosition);
        }
        if (spatialMeshs.Count > 0)
        {
            var spatialMesh = spatialMeshs.Dequeue();
            surfaceObserver.LoadEachMesh(spatialMesh);
        }
        if (deleteDatas.Count > 0)
        {
            var deleteData = deleteDatas.Dequeue();
            intensityManager.DeleteIntensity(deleteData);
            //シェアリング相手にもデータ消去申請
            string json = transferData.SerializeJson<DeleteData>(deleteData);
            tServer.SendAllClient(json);
            logPanelManager.Writelog("Intensitiy ID " + deleteData.intensityID + " is deleted.");
        }
        if(logQueue.Count > 0)
        {
            var log = logQueue.Dequeue();
            logPanelManager.Writelog(log);
        }
    }

    IEnumerator SendIntensity(SendPosition position)
    {
        //送信するインテンシティオブジェクトに変換
        sendIntensityData = intensityManager.MicPosReceived(position);
        yield return null;
        if (sendIntensityData.sendType == SendType.Intensity)
        {
            tServer.SendAllClient(transferData.SerializeJson<IntensityPackage>(sendIntensityData));
            logPanelManager.WriteConsole(sendIntensityData.num, sendIntensityData.sendPos, sendIntensityData.intensity);
        }
        yield return null;
    }

    /**tcp始めるボタンに紐づけ**/
    public void StartTCPServer()
    {
        if (!onServer)
        {
            //ローカルサーバーを使うことを推定        
            var host = MeasurementParameter.TCPAdress != null ? MeasurementParameter.TCPAdress : "192.168.0.42";
            tServer = new TCPServer(host, port);
            //データ受信イベント
            tServer.OnReceiveData += new TCPServer.ReceiveEventHandler(tServer_OnReceiveData);
            tServer.OnDisconnected += new TCPServer.DisconnectedEventHandler(tServer_OnDisconnected);
            tServer.OnConnected += new TCPServer.ConnectedEventHandler(tServer_OnConnected);
            onServer = true;
        }
        //計測データの設定反映
        settingManager.InitParam();

        Debug.Log("Init Setting");
    }

    /// <summary>
    /// 接続断イベント
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void tServer_OnDisconnected(object sender, EventArgs e,string s)
    {
        Debug.Log("Server接続解除");
        logQueue.Enqueue(s+" Client is disconnected");
    }

    /// <summary>
    /// 接続OKイベント
    /// </summary>
    /// <param name="e"></param>
    void tServer_OnConnected(EventArgs e, string log)
    {
        Debug.Log("Clientと接続完了");
        logQueue.Enqueue(log + " is connected.");
        How2Measure measureType = new How2Measure(MeasurementType.Standard,MeasurementParameter.i_block);
        tServer.SendAllClient(transferData.SerializeJson<How2Measure>(measureType));

        //共有された方のアプリケーションでもsettingパラメータを共有
        var holoSetting = new SettingSender("ForSharing", MeasurementParameter.colormapID, MeasurementParameter.MaxIntensity, MeasurementParameter.MinIntensity, MeasurementParameter.objSize);
        tServer.SendAllClient(transferData.SerializeJson<SettingSender>(holoSetting));

    }

    /// <summary>
    /// アプリ終了時に呼び出し、Serverを止める
    /// </summary>
    public void StopTCPServer()
    {
        try
        {
            //closeしてるけど送受信がどうなってるかは謎
            tServer.Close();
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    /// <summary>
    /// データ受信イベント
    /// </summary>
    /// <param name="ms"></param>
    void tServer_OnReceiveData(object sender, string ms)
    {
        var jsons = transferData.DevideData2Jsons(ms);
        foreach (var json in jsons)
        {
            var message = new HoloLensMessage();
            if (transferData.CanDesirializeJson<HoloLensMessage>(json, out message))
            {
                switch (message.sendType)
                {
                    case SendType.PositionSender: //HoloLens側のマイクロホン位置を所得
                        transferData.DesirializeJson<SendPosition>(out var sendPosition);
                        Debug.Log("[Server] Send Position num;" + sendPosition.name + " position: " + sendPosition.sendPos.x + "rotation" + sendPosition.sendRot.x);
                        positionPackages.Enqueue(sendPosition);
                        break;
                    case SendType.SettingSender:
                        transferData.DesirializeJson<SettingSender>(out var holoSetting);
                        tServer.SendAllClient(transferData.SerializeJson<SettingSender>(holoSetting));
                        MeasurementParameter.HoloLensParameterUpdate(holoSetting);
                        Debug.Log("[Server] Holo setting ColorMapID: " + holoSetting.colorMapID);
                        logQueue.Enqueue($"Intensity range: {holoSetting.lvMin} ~ {holoSetting.lvMax}");
                        break;                    
                    case SendType.SpatialMesh:
                        transferData.DesirializeJson<SpatialMesh>(out var spatialMesh);
                        spatialMeshs.Enqueue(spatialMesh);
                        break;
                    case SendType.DeleteData:
                        transferData.DesirializeJson<DeleteData>(out var deleteData);
                        deleteDatas.Enqueue(deleteData);
                        break;
                }
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
        tServer.SendAllClient(json);
        //再計算を実行
        ReCalcDataPackage reCalcDataPackage = await intensityManager.RecalcIntensity();
        string json2 = transferData.SerializeJson<ReCalcDataPackage>(reCalcDataPackage);
        tServer.SendAllClient(json2);
    }

    private void OnApplicationQuit()
    {
        if (tServer != null)
            StopTCPServer();
        asiocsharpdll.StopAsioMain();
    }
}
