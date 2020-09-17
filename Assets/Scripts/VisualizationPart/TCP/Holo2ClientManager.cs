using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloLensModule.Network;
using System;
using System.Threading.Tasks;
using uOSC;

[RequireComponent(typeof(Holo2FileSurfaceObserver))]
[RequireComponent(typeof(InstanceManager))]
[RequireComponent(typeof(Holo2UIManager))]
public class Holo2ClientManager : MonoBehaviour
{
    TCPClientManager tClient;

    TransferData transferData = TransferData.transferData;

    Holo2FileSurfaceObserver surfaceObserver;
    InstanceManager instanceMaanger;
    Holo2UIManager uIManager;

    bool gotData;

    Vector3 sendPos;
    Quaternion sendRotate;

    //接続先ポート
    [SerializeField]
    int port = 3333;

    [SerializeField]
    GameObject c_status;

    [SerializeField]
    MicPositionMirror micPositionMirror;

    Queue<IntensityPackage> intensityPackages = new Queue<IntensityPackage>();
    Queue<ReCalcDataPackage> recalcDataPackages = new Queue<ReCalcDataPackage>();

    List<int> measureID = new List<int>();

    //接続状態が変化したら変更
    bool c_status_changed = false;
    int c_counter = 0;

    /// <summary>
    /// 準備ができたら
    /// </summary>
    public void StartTCPClient()
    {
        uIManager = gameObject.GetComponent<Holo2UIManager>();
        uIManager.UpdateUIParameter();
        string host = Holo2MeasurementParameter.IP;
        surfaceObserver = gameObject.GetComponent<Holo2FileSurfaceObserver>();

        try
        {
            tClient = new TCPClientManager(host, port);
            //データ受信イベント
            tClient.ListenerMessageEvent += tClient_OnReceiveData;
            tClient.OnConnected += tClient_OnConnected;
            tClient.OnDisconnected += tClient_OnDisconnected;
            Debug.Log("Client OK");
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        instanceMaanger = GetComponent<InstanceManager>();   
    }

    // Update is called once per frame
    void Update()
    {
        if (Holo2MeasurementParameter._measure)
        {
            if(intensityPackages.Count > 0)
            {
                var package = intensityPackages.Dequeue();
                StartCoroutine("InstantObject", package);
            }
            if(recalcDataPackages.Count > 0)
            {
                var package = recalcDataPackages.Dequeue();
                StartCoroutine("ReproIntensityMap", package);
            }
        }

        //接続状態をUIの上部ランプで判断できるようにしている
        if (c_status_changed)
        {

            if (c_counter % 2 == 0)
                c_status.GetComponent<MeshRenderer>().material.color = Color.green;
            else
                c_status.GetComponent<MeshRenderer>().material.color = Color.red;
           
            c_status_changed = false;
            c_counter++;
        }
    }

    IEnumerator InstantObject(IntensityPackage package)
    {
        var intensityLv = AcousticMathNew.CalcuIntensityLevel(package.intensity);
        //コーンの色を生成
        Color vecObjColor = ColorBar.DefineColor(Holo2MeasurementParameter.ColorMapID, intensityLv,
            Holo2MeasurementParameter.LevelMin, Holo2MeasurementParameter.LevelMax);
        //オブジェクトを生成
        instanceMaanger.CreateInstantObj(package, vecObjColor, Holo2MeasurementParameter.ObjSize);
        measureID.Add(package.num);
        Debug.Log(DateTime.Now.ToString("MM/dd/HH:mm:ss.fff") + "Display measurement point No." + package.num);
        yield return null;
    }

    IEnumerator ReproIntensityMap(ReCalcDataPackage recalcData)
    {
        for(int n = 0;n < recalcData.storageNum; n++)
        {

                float intensityLv = AIMath.CalcuIntensityLevel(recalcData.intensities[n]);
                Color ObjColor = ColorBar.DefineColor(Holo2MeasurementParameter.ColorMapID, intensityLv, Holo2MeasurementParameter.LevelMin, Holo2MeasurementParameter.LevelMax);
            instanceMaanger.ChangeIntensityObj(recalcData.sendNums[n], recalcData.intensities[n], ObjColor);

            yield return null;
        }
    }

    /// <summary>
    /// Holo2MeasurementParameterをServerに送信
    /// その後、空間マップを送信
    /// </summary>
    public async void SendSetting()
    {
        //送信オブジェクトをパッケージ化
        var sendSettingData = new SettingSender("", Holo2MeasurementParameter.ColorMapID,
            Holo2MeasurementParameter.LevelMax, Holo2MeasurementParameter.LevelMin, Holo2MeasurementParameter.ObjSize);
        string json = transferData.SerializeJson<SettingSender>(sendSettingData);
        try
        {
            tClient.SendMessage(json);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            if (!Holo2MeasurementParameter.on_connected)
            {
                StartTCPClient();
            }
        }

        //空間マップの送信準備
        var data = await Task.Run(() => surfaceObserver.MapSend());

        string jsonS = await Task.Run(() => transferData.SerializeJson<SpatialMapSender>(data));

        try
        {
            tClient.SendMessage(jsonS);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    /// <summary>
    /// Startボタンに紐づけ
    /// </summary>
    public void StartMeasure()
    {
        StartCoroutine("SendData");
    }

    /// <summary>
    /// 固定時間で座標を送信
    /// </summary>
    /// <returns></returns>
    private IEnumerator SendData()
    {
        yield return new WaitForSeconds(1 / 6);
        int counter = 0;
        while (Holo2MeasurementParameter._measure)
        {
            // [TODO] 時間固定(10fps)で送り続けています。どうするかは未定
            yield return new WaitForSeconds(1);
            if (UIManager._measure)
            {
                micPositionMirror.GetSendInfo(out sendPos, out sendRotate);
                //送信用データの作成
                var sendPosition = new SendPosition(counter++.ToString(), sendPos, sendRotate);
                string json = transferData.SerializeJson<SendPosition>(sendPosition);
                tClient.SendMessage(json);
                //Debug.Log("send");
            }
        }
    }



    /// <summary>
    /// データ受信イベント
    /// </summary>
    /// <param name="ms">jsonファイル</param>
    void tClient_OnReceiveData(string ms)
    {
        var message = new ServerMessage();
        if(transferData.CanDesirializeJson<ServerMessage>(ms, out message))
        {
            switch (message.sendType)
            {
                case SendType.Intensity:
                    transferData.DesirializeJson<IntensityPackage>(out var intensityData);
                    var intensityLv = AIMath.CalcuIntensityLevel(intensityData.intensity);
                    //インテンシティレベルが指定した範囲内なら
                    if (intensityLv >= Holo2MeasurementParameter.LevelMin || intensityLv <= Holo2MeasurementParameter.LevelMax)
                    {
                        //キューに生成予定のインテンシティ情報を埋め込み
                        intensityPackages.Enqueue(intensityData); 
                    }

                    break;
                case SendType.SettingSender:
                    transferData.DesirializeJson<SettingSender>(out var settingSender);
                    Holo2MeasurementParameter.ColorMapID = settingSender.colorMapID;
                    Holo2MeasurementParameter.LevelMin = settingSender.lvMin;
                    Holo2MeasurementParameter.LevelMax = settingSender.lvMax;
                    Holo2MeasurementParameter.ObjSize = settingSender.objSize;
                    //Sharingを実装したら追加
                    //manager.ReadyShare();
                    break;
                case SendType.ReCalcData:
                    transferData.DesirializeJson<ReCalcDataPackage>(out var recalcDataPackage);
                    recalcDataPackages.Enqueue(recalcDataPackage);
                    break;
            }
        }
    }

    void tClient_OnDisconnected(object sender, EventArgs e)
    {
        Debug.Log("Client接続解除");
        c_status_changed = true;
        Holo2MeasurementParameter.on_connected = false;
    }

    void tClient_OnConnected(EventArgs e)
    {
        Debug.Log("Serverと接続完了");        
        Holo2MeasurementParameter.on_connected = true;
        c_status_changed = true;
    }
}
