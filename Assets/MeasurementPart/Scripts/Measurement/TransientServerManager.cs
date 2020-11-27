using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloLensModule.Network;
using System;
using AsioCSharpDll;
using System.IO;

public class TransientServerManager : MonoBehaviour
{
    TCPServer tServer;
    TransferData transferData = TransferData.transferData;

    //ポート番号規定値
    int port = 3333;

    [SerializeField]
    GameObject UIManager;

    LogPanelManager logPanelManager;
    SettingManager settingManager;
    TransientIntensityManager tIntensityManager;

    Queue<string> logQueue = new Queue<string>();

    Queue<SendPosition> positionPackages = new Queue<SendPosition>();

    Queue<DeleteData> deleteDatas = new Queue<DeleteData>();

    bool onServer = false; //サーバーの受付を開始したかどうか

    // Start is called before the first frame update
    void Start()
    {
        logPanelManager = UIManager.GetComponent<LogPanelManager>();
        settingManager = UIManager.GetComponent<SettingManager>();
        tIntensityManager = gameObject.GetComponent<TransientIntensityManager>();

        //出力音源読み込み
        double[] oSignal = ReadSignal();
        //Asioスタート
        string instLog = asiocsharpdll.TransientPrepareAsio(MeasurementParameter.AsioDriverName, MeasurementParameter.Fs, MeasurementParameter.SampleNum, oSignal.Length, oSignal); ;
        logPanelManager.Writelog(instLog);
    }

    // Update is called once per frame
    void Update()
    {
        if (positionPackages.Count > 0)
        {
            var rPosition = positionPackages.Dequeue();
            tIntensityManager.MicPosReceived(rPosition);
        }
        if (deleteDatas.Count > 0)
        {
            var deleteData = deleteDatas.Dequeue();
            tIntensityManager.DeleteIntensity(deleteData);
            //シェアリング相手にもデータ消去申請
            string json = transferData.SerializeJson<DeleteData>(deleteData);
            tServer.SendAllClient(json);
            logPanelManager.Writelog("Intensitiy ID " + deleteData.intensityID + " is deleted.");
        }
        if (logQueue.Count > 0)
        {
            var log = logQueue.Dequeue();
            logPanelManager.Writelog(log);
        }
    }

    public void SendIntensity(TransIntensityPackage package)
    {
        tServer.SendAllClient(transferData.SerializeJson<TransIntensityPackage>(package));
        logPanelManager.WriteConsole(package.num, package.sendPos, package.sumIntensity);
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
                        MeasurementParameter.HoloLensParameterUpdate(holoSetting);
                        Debug.Log("[Server] Holo setting ColorMapID: " + holoSetting.colorMapID);
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
    /// 出力信号読込
    /// </summary>
    /// <returns></returns>
    public double[] ReadSignal()
    {
        //byteファイル風読み込み        
        TextAsset asset = Resources.Load(MeasurementParameter.TargetSource, typeof(TextAsset)) as TextAsset;
        //読込
        double[] tspSignal = new double[asset.bytes.Length / 8];
        tspSignal = Bytes2array(asset, asset.bytes.Length / 8);

        logPanelManager.Writelog("音源長さ:" + asset.bytes.Length / 2);
        return tspSignal;
    }

    /// <summary>
    /// bytesファイルをdouble型配列に
    /// </summary>
    /// <param name="asset">bytesファイル</param>
    /// <param name="leng">読み込み長さ</param>
    public double[] Bytes2array(TextAsset asset, int leng)
    {
        double[] sound = new double[leng];
        using (Stream fs = new MemoryStream(asset.bytes))
        {
            using (BinaryReader br = new BinaryReader(fs))
            {
                int i = 0;
                while (i < leng)
                {
                    //符号付2byte読み込み
                    //sound[i] = (double)br.ReadInt16();
                    sound[i] = (double)br.ReadDouble();
                    i++;
                }
            }
        }
        return sound;
    }

    /// <summary>
    /// 接続断イベント
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void tServer_OnDisconnected(object sender, EventArgs e, string s)
    {
        Debug.Log("Server接続解除");
        logQueue.Enqueue(s + " Client is disconnected");
    }

    /// <summary>
    /// 接続OKイベント
    /// </summary>
    /// <param name="e"></param>
    void tServer_OnConnected(EventArgs e, string log)
    {
        Debug.Log("Clientと接続完了");
        logQueue.Enqueue(log + " is connected.");
        How2Measure measureType = new How2Measure(MeasurementType.Transient, MeasurementParameter.i_block);
        tServer.SendAllClient(transferData.SerializeJson<How2Measure>(measureType));
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

    private void OnApplicationQuit()
    {
        if (tServer != null)
            StopTCPServer();
        asiocsharpdll.StopAsioMain();
    }


}
