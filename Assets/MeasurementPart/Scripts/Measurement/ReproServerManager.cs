using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.IO.LowLevel.Unsafe;
using HoloLensModule.Network;
using System;
using System.Threading.Tasks;
using AsioCSharpDll;
using System.IO;

public enum AlgorithmPattern
{
    CrossSpectrum,
    instantaneous,
    STFT,
    AmbisonicsT,
    AmbisonicsTF
}

public class ReproServerManager : MonoBehaviour
{
    TCPServer tServer;
    TransferData transferData = TransferData.transferData;

    //ポート番号規定値
    int port = 3333;

    [SerializeField]
    GameObject UIManager;

    [SerializeField]
    Dropdown algorithmList;

    [SerializeField]
    InputField inBlocksize;
    [SerializeField]
    InputField inOverlap;

    LogPanelManager logPanelManager;
    SettingManager settingManager;

    Queue<string> logQueue = new Queue<string>();
    Queue<IntensityLog> intensityLogs = new Queue<IntensityLog>();

    List<DataStorage> dataStorages = new List<DataStorage>();

    //再重畳の対象となるインテンシティの計算手法
    AlgorithmPattern nowAlogrithm = AlgorithmPattern.CrossSpectrum;
    MeasurementType nowMeasurementType = MeasurementType.Standard;

    //データ読み込みは1度だけにする
    bool initialLoad = true;

    int length_bit;

    // Start is called before the first frame update
    void Start()
    {
        logPanelManager = UIManager.GetComponent<LogPanelManager>();
        settingManager = UIManager.GetComponent<SettingManager>();
        length_bit = (int)(Mathf.Log(MeasurementParameter.SampleNum, 2f));

        algorithmList.onValueChanged.AddListener(ChangeAlgorithm);
    }

    // Update is called once per frame
    void Update()
    {
        if (logQueue.Count > 0)
        {
            var log = logQueue.Dequeue();
            logPanelManager.Writelog(log);
        }
        if (intensityLogs.Count > 0)
        {
            var log = intensityLogs.Dequeue();
            logPanelManager.WriteConsole(log.num, log.sendPos, log.intensity);
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
        UpdateSTFTParam();

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

        How2Measure measureType = new How2Measure(nowMeasurementType, MeasurementParameter.i_block);
        tServer.SendAllClient(transferData.SerializeJson<How2Measure>(measureType));
        SettingSender ud_setting = new SettingSender("NewSetting", MeasurementParameter.colormapID, MeasurementParameter.MaxIntensity, MeasurementParameter.MinIntensity, MeasurementParameter.objSize);
        //更新データをHololensに送信
        string json = transferData.SerializeJson<SettingSender>(ud_setting);
        tServer.SendAllClient(json);

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
    public async void LoadingDataAsync()
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
                        logQueue.Enqueue($"loading num is {data.measureNo}");
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
        ReproDataPackage data = new ReproDataPackage(dataStorages.Count, nowAlogrithm);
        
        foreach (var datastorage in dataStorages)
        {
            data.sendNums.Add(datastorage.measureNo);
            data.sendPoses.Add(datastorage.micLocalPos);
            data.sendRots.Add(datastorage.micLocalRot);

            switch (nowAlogrithm)
            {
                case AlgorithmPattern.CrossSpectrum://時間平均
                    data.intensities.Add(datastorage.intensityDir);
                    break;

                case AlgorithmPattern.instantaneous: //瞬時音響インテンシティ
                    var iintensity = AcousticSI.DirectMethod(datastorage.soundSignal, MeasurementParameter.AtmDensity, MeasurementParameter.MInterval);
                    data.intensities.Add(AcousticSI.SumIntensity(iintensity));
                    data.iintensities.AddRange(iintensity);
                    break;
                case AlgorithmPattern.STFT: //STFTを使った時間周波数領域での計算処理
                    var iintensity2 = MathFFTW.STFTmethod(datastorage.soundSignal, MeasurementParameter.n_overlap, MeasurementParameter.i_block, MeasurementParameter.Fs, MeasurementParameter.FreqMin, MeasurementParameter.FreqMax, MeasurementParameter.AtmDensity, MeasurementParameter.MInterval);
                    data.intensities.Add(AcousticSI.SumIntensity(iintensity2));
                    data.iintensities.AddRange(iintensity2);
                    break;
                case AlgorithmPattern.AmbisonicsT://アンビソニックマイクを使った時間領域のpsudoIntensityの推定
                    var iintensity3 = MathAmbisonics.TdomMethod(datastorage.soundSignal, MeasurementParameter.AtmDensity, 340);
                    data.intensities.Add(AcousticSI.SumIntensity(iintensity3));
                    data.iintensities.AddRange(iintensity3);
                    break;
                case AlgorithmPattern.AmbisonicsTF://アンビソニックマイクを使った時間周波数領域のpsudoIntensityの推定
                    var iintensity4 = MathAmbisonics.TFdomMethod(datastorage.soundSignal, MeasurementParameter.n_overlap, MeasurementParameter.i_block, MeasurementParameter.Fs, MeasurementParameter.FreqMin, MeasurementParameter.FreqMax, MeasurementParameter.AtmDensity, 340);
                    data.intensities.Add(AcousticSI.SumIntensity(iintensity4));
                    data.iintensities.AddRange(iintensity4);
                    break;
            }
            intensityLogs.Enqueue( new IntensityLog(datastorage.measureNo, datastorage.micLocalPos, data.intensities[data.intensities.Count - 1]));
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
        How2Measure measureType = new How2Measure(nowMeasurementType, MeasurementParameter.i_block);
        tServer.SendAllClient(transferData.SerializeJson<How2Measure>(measureType));
        //更新データをHololensに送信
        string json = transferData.SerializeJson<SettingSender>(ud_setting);
        tServer.SendAllClient(json);
        UpdateSTFTParam();

        //再計算を実行
        if (nowAlogrithm == AlgorithmPattern.CrossSpectrum)
        {
            ReCalcDataPackage recalcIntensity = await Task.Run(() => AsyncReCalc());
            string json2 = transferData.SerializeJson<ReCalcDataPackage>(recalcIntensity);
            tServer.SendAllClient(json2);
            logQueue.Enqueue("Update data");
        }
        else
        {
            ReCalcTransientDataPackage recalcTransIntensity = await Task.Run(() => AsyncReCalcTrans());
            string json2 = transferData.SerializeJson<ReCalcTransientDataPackage>(recalcTransIntensity);
            tServer.SendAllClient(json2);
            logQueue.Enqueue("Update data");
        }
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

    private ReCalcTransientDataPackage AsyncReCalcTrans()
    {
        ReCalcTransientDataPackage data = new ReCalcTransientDataPackage(dataStorages.Count);
        List<Vector3> iintensities = new List<Vector3>();
        foreach (DataStorage dataStorage in dataStorages)
        {
            iintensities.Clear();
            //時間変化する音響インテンシティを指定したアルゴリズムを元に計算
            switch (nowAlogrithm)
            {
                case AlgorithmPattern.instantaneous://直接法
                    var intensity = AcousticSI.DirectMethod(dataStorage.soundSignal, MeasurementParameter.AtmDensity, MeasurementParameter.MInterval);
                    data.intensities.Add(AcousticSI.SumIntensity(intensity));
                    data.iintensityList.AddRange(intensity);
                    break;
                case AlgorithmPattern.STFT://STFTを使った時間周波数領域での計算処理
                    var intensity2 = MathFFTW.STFTmethod(dataStorage.soundSignal, MeasurementParameter.n_overlap, MeasurementParameter.i_block, MeasurementParameter.Fs, MeasurementParameter.FreqMin, MeasurementParameter.FreqMax, MeasurementParameter.AtmDensity, MeasurementParameter.MInterval);
                    data.intensities.Add(AcousticSI.SumIntensity(intensity2));
                    data.iintensityList.AddRange(intensity2);
                    break;
                case AlgorithmPattern.AmbisonicsT://アンビソニックマイクを使った時間領域のpsudoIntensityの推定
                    var intensity3 = MathAmbisonics.TdomMethod(dataStorage.soundSignal, MeasurementParameter.AtmDensity, 340);
                    data.intensities.Add(AcousticSI.SumIntensity(intensity3));
                    data.iintensityList.AddRange(intensity3);
                    break;
                case AlgorithmPattern.AmbisonicsTF://アンビソニックマイクを使った時間周波数領域のpsudoIntensityの推定
                    var intensity4 = MathAmbisonics.TFdomMethod(dataStorage.soundSignal, MeasurementParameter.n_overlap, MeasurementParameter.i_block, MeasurementParameter.Fs, MeasurementParameter.FreqMin, MeasurementParameter.FreqMax, MeasurementParameter.AtmDensity, 340);
                    data.intensities.Add(AcousticSI.SumIntensity(intensity4));
                    data.iintensityList.AddRange(intensity4);
                    break;
            }
            intensityLogs.Enqueue(new IntensityLog(dataStorage.measureNo, dataStorage.micLocalPos, data.intensities[data.intensities.Count - 1]));
            data.sendNums.Add(dataStorage.measureNo);
        }

        return data;

    }

    void ChangeAlgorithm(int ID)
    {
        switch (ID)
        {
            case 0:
                nowAlogrithm = AlgorithmPattern.CrossSpectrum;
                nowMeasurementType = MeasurementType.Standard;
                break;
            case 1:
                nowAlogrithm = AlgorithmPattern.instantaneous;
                nowMeasurementType = MeasurementType.Transient;
                break;
            case 2:
                nowAlogrithm = AlgorithmPattern.STFT;
                nowMeasurementType = MeasurementType.Transient;
                break;
            case 3:
                nowAlogrithm = AlgorithmPattern.AmbisonicsT;
                nowMeasurementType = MeasurementType.Transient;
                break;
            case 4:
                nowAlogrithm = AlgorithmPattern.AmbisonicsTF;
                nowMeasurementType = MeasurementType.Transient;
                break;
        }
        UpdateSTFTParam();
    }

    void UpdateSTFTParam()
    {
        MeasurementParameter.i_block = int.Parse(inBlocksize.text);
        MeasurementParameter.n_overlap = int.Parse(inOverlap.text);
    }
}

class IntensityLog
{
    public Vector3 sendPos, intensity;
    public int num;
    public IntensityLog(int i_num, Vector3 i_sendPos, Vector3 i_intensity)
    {
        this.num = i_num;
        this.sendPos = i_sendPos;
        this.intensity = i_intensity;
    }
}
