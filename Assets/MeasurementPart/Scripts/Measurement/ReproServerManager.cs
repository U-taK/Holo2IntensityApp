using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.IO.LowLevel.Unsafe;
using HoloLensModule.Network;
using System;
using System.Threading.Tasks;
using AsioCSharpDll;
using System.IO;

public class ReproServerManager : MonoBehaviour
{
    TCPServer tServer;
    TransferData transferData = TransferData.transferData;

    //ポート番号規定値
    int port = 3333;

    [SerializeField]
    GameObject UIManager;

    LogPanelManager logPanelManager;
    SettingManager settingManager;

    Queue<string> logQueue = new Queue<string>();

    List<DataStorage> dataStorages = new List<DataStorage>();

    //データ読み込みは1度だけにする
    bool initialLoad = true;

    int length_bit;

    // Start is called before the first frame update
    void Start()
    {
        logPanelManager = UIManager.GetComponent<LogPanelManager>();
        settingManager = UIManager.GetComponent<SettingManager>();
        length_bit = (int)(Mathf.Log(MeasurementParameter.SampleNum, 2f));
    }

    // Update is called once per frame
    void Update()
    {
        if (logQueue.Count > 0)
        {
            var log = logQueue.Dequeue();
            logPanelManager.Writelog(log);
        }
    }

    /**tcp始めるボタンに紐づけ**/
    public void StartTCPServer()
    {
        //ローカルサーバーを使うことを推定        
        var host = MeasurementParameter.TCPAdress != null ? MeasurementParameter.TCPAdress : "192.168.0.42";
        tServer = new TCPServer(host, port);
        //データ受信イベント
        tServer.OnReceiveData += new TCPServer.ReceiveEventHandler(tServer_OnReceiveData);
        tServer.OnDisconnected += new TCPServer.DisconnectedEventHandler(tServer_OnDisconnected);
        tServer.OnConnected += new TCPServer.ConnectedEventHandler(tServer_OnConnected);

        //計測データの設定反映
        settingManager.InitParam4Repro();

        Debug.Log("Init Setting");
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
    /// 使う予定なし
    /// </summary>
    /// <param name="ms"></param>
    void tServer_OnReceiveData(object sender, string ms)
    {
    }

    private void OnApplicationQuit()
    {
        if (tServer != null)
            StopTCPServer();
    }

    /// <summary>
    /// バイナリファイルをインテンシティデータに変換
    /// </summary>
    public async void LoadingData()
    {
        await Task.Run(() => AsyncLoadBin());
    }

    async Task AsyncLoadBin()
    {
        if (initialLoad)
        {
            //録音＆マイク位置バイナリファイル保存
            for (int dataIndex = 0; dataIndex < MeasurementParameter.plotNumber; dataIndex++)
            {
                string pathName = MeasurementParameter.SaveDir + @"\measurepoint_" + dataIndex.ToString() + ".bytes";
                DataStorage data = new DataStorage();

                if (File.Exists(pathName))
                {
                    await Task.Run(() =>
                    {
                        using (BinaryReader br = new BinaryReader(File.Open(pathName, FileMode.Open)))
                        {

                            data.soundSignal = new double[4][];
                            for (int micID = 0; micID < 4; micID++)
                            {
                                data.soundSignal[micID] = new double[MeasurementParameter.SampleNum];
                                for (int sample = 0; sample < MeasurementParameter.SampleNum; sample++)
                                {
                                    data.soundSignal[micID][sample] = br.ReadDouble();
                                }
                            }
                            float vx = (float)br.ReadDouble();
                            float vy = (float)br.ReadDouble();
                            float vz = (float)br.ReadDouble();
                            data.micLocalPos = new Vector3(vx, vy, vz);
                            float rx = (float)br.ReadDouble();
                            float ry = (float)br.ReadDouble();
                            float rz = (float)br.ReadDouble();
                            float rw = (float)br.ReadDouble();
                            data.micLocalRot = new Quaternion(rx, ry, rz, rw);
                            br.Close();
                        }

                        data.measureNo = dataIndex;
                        data.intensityDir = AcousticMathNew.CrossSpectrumMethod(data.soundSignal, MeasurementParameter.Fs, length_bit,
                MeasurementParameter.FreqMin, MeasurementParameter.FreqMax, MeasurementParameter.AtmDensity, MeasurementParameter.MInterval);
                        var level = AcousticMathNew.CalcuIntensityLevel(data.intensityDir);
                        dataStorages.Add(data);
                    });
                }
            }
            logQueue.Enqueue("Data Loading is finished!");
            initialLoad = false;

            SettingSender ud_setting = new SettingSender("NewSetting", MeasurementParameter.colormapID, MeasurementParameter.MaxIntensity, MeasurementParameter.MinIntensity, MeasurementParameter.objSize);
            //更新データをHololensに送信
            string json = transferData.SerializeJson<SettingSender>(ud_setting);
            tServer.SendAllClient(json);
        }
        else
        {
            logQueue.Enqueue("already read data");
        }
    }

    public async void SendReproData()
    {
        //送信用データの構築
        var reproData = await Task.Run(() => AsyncSendData());

        //ReproDataPackage->json
        string json = transferData.SerializeJson<ReproDataPackage>(reproData);
        tServer.SendAllClient(json);
        logQueue.Enqueue("Send reproduction data");
    }

    public ReproDataPackage AsyncSendData()
    {
        //インテンシティ計算の実行
        ReproDataPackage data = new ReproDataPackage(dataStorages.Count);
        foreach(var datastorage in dataStorages)
        {
            data.sendNums.Add(datastorage.measureNo);
            data.sendPoses.Add(datastorage.micLocalPos);
            data.sendRots.Add(datastorage.micLocalRot);
            data.intensities.Add(datastorage.intensityDir);
        }

        return data;
    }

    /// <summary>
    /// 条件を変更して再計算
    /// </summary>
    public async void UpdateData()
    {
        settingManager.InitParam4Repro();
        SettingSender ud_setting = new SettingSender("NewSetting", MeasurementParameter.colormapID, MeasurementParameter.MaxIntensity, MeasurementParameter.MinIntensity, MeasurementParameter.objSize);
        //更新データをHololensに送信
        string json = transferData.SerializeJson<SettingSender>(ud_setting);
        tServer.SendAllClient(json);

        //再計算を実行
        ReCalcDataPackage recalcIntensity = await Task.Run(() => AsyncReCalc());
        string json2 = transferData.SerializeJson<ReCalcDataPackage>(recalcIntensity);
        tServer.SendAllClient(json2);
        logQueue.Enqueue("Update data");
    }

    private ReCalcDataPackage AsyncReCalc()
    {
        //送信データの作成
        ReCalcDataPackage data = new ReCalcDataPackage(dataStorages.Count);
        foreach (DataStorage dataStorage in dataStorages)
        {
            Vector3 intensity = AcousticMathNew.CrossSpectrumMethod(dataStorage.soundSignal, MeasurementParameter.Fs, length_bit,
                MeasurementParameter.FreqMin, MeasurementParameter.FreqMax, MeasurementParameter.AtmDensity, MeasurementParameter.MInterval);
            data.intensities.Add(intensity);
            data.sendNums.Add(dataStorage.measureNo);
        }
        return data;
    }
}
