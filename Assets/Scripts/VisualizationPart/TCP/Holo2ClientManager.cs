using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloLensModule.Network;
using System;
using System.Threading.Tasks;
using uOSC;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;

[RequireComponent(typeof(Holo2FileSurfaceObserver))]
[RequireComponent(typeof(InstanceManager))]
[RequireComponent(typeof(Holo2UIManager))]
public class Holo2ClientManager : MonoBehaviour
{
    TCPClient tClient;

    TransferData transferData = TransferData.transferData;

    Holo2FileSurfaceObserver surfaceObserver;
    InstanceManager instanceMaanger;
    Holo2UIManager uIManager;
    Holo2ReproUIManager uiManagerRepro;
    //過渡音計測のトリガー
    Interactable interactable_trigger;

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
    Queue<ReCalcTransientDataPackage> recalcTransDataPackage = new Queue<ReCalcTransientDataPackage>();
    Queue<ReproDataPackage> reproDatas = new Queue<ReproDataPackage>();
    Queue<TransIntensityPackage> iIntensitiesPackages = new Queue<TransIntensityPackage>();
    Queue<DeleteData> deleteDatas = new Queue<DeleteData>();

    [SerializeField]
    private GameObject indicatorObject;

    [SerializeField]
    private TextMesh sharing_status;

    private IProgressIndicator indicator;

    List<int> measureID = new List<int>();

    //接続状態が変化したら変更
    bool c_status_changed = false;
    int c_counter = 0;

    //座標送信数
    int counter = 0;

    [SerializeField]
    GameObject testMap;

    /// <summary>
    /// 準備ができたら
    /// </summary>
    public void StartTCPClient4Reproduction()
    {
        uiManagerRepro = gameObject.GetComponent<Holo2ReproUIManager>();
        uiManagerRepro.InitUIParameter();
        string host = Holo2MeasurementParameter.IP;        
        try
        {
            tClient = new TCPClient(host, port);
            //データ受信イベント
#if WINDOWS_UWP
            tClient.ListenerMessageEvent +=   new TCPClient.ListenerMessageEventHandler(tClient_OnReceiveData);
#else
            tClient.OnReceiveData += new TCPClient.ReceiveEventHandler(tClient_OnReceiveData);
#endif
            tClient.OnConnected += tClient_OnConnected;
            tClient.OnDisconnected += tClient_OnDisconnected;
#if WINDOWS_UWP
#else
            //受信開始
            tClient.StartReceive();
#endif
            Debug.Log("Client OK");
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    /// <summary>
    /// 準備ができたら
    /// </summary>
    public void StartTCPClient()
    {
        uIManager = gameObject.GetComponent<Holo2UIManager>();
        uIManager.InitUIParameter();
        string host = Holo2MeasurementParameter.IP;
        surfaceObserver = gameObject.GetComponent<Holo2FileSurfaceObserver>();

        try
        {
            tClient = new TCPClient(host, port);
            //データ受信イベント
#if WINDOWS_UWP
            tClient.ListenerMessageEvent +=   new TCPClient.ListenerMessageEventHandler(tClient_OnReceiveData);
#else
            tClient.OnReceiveData += new TCPClient.ReceiveEventHandler(tClient_OnReceiveData);
#endif
            tClient.OnConnected += tClient_OnConnected;
            tClient.OnDisconnected += tClient_OnDisconnected;
#if WINDOWS_UWP
#else
            //受信開始
            tClient.StartReceive();
#endif
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

        if (intensityPackages.Count > 0)
        {
            var package = intensityPackages.Dequeue();
            StartCoroutine("InstantObject", package);
        }
        if (recalcDataPackages.Count > 0)
        {
            var package = recalcDataPackages.Dequeue();
            StartCoroutine("ReCalcIntensityMap", package);
        }

        if (reproDatas.Count > 0)
        {
            var reprodata = reproDatas.Dequeue();
            StartCoroutine("ReProIntensityMap", reprodata);

        }

        //接続状態をUIの上部ランプで判断できるようにしている
        if (c_status_changed)
        {
            //sharingの状態を伝える
            if(sharing_status != null)
                sharing_status.text = "Connected Server";

            if (c_counter % 2 == 0)
                c_status.GetComponent<MeshRenderer>().material.color = Color.green;
            else
                c_status.GetComponent<MeshRenderer>().material.color = Color.red;
           
            c_status_changed = false;
            c_counter++;
        }
        if(iIntensitiesPackages.Count > 0)
        {
            var package = iIntensitiesPackages.Dequeue();
            StartCoroutine("InstantIntensityObject", package);
        }
        if(deleteDatas.Count > 0)
        {
            var deleteData = deleteDatas.Dequeue();
            instanceMaanger.DeleteVectorObj(deleteData.intensityID);
        }
        if(recalcTransDataPackage.Count > 0)
        {
            var package = recalcTransDataPackage.Dequeue();
            StartCoroutine("ReCalcTransIntensityMap", package);
        }
    }

    /// <summary>
    /// 時間平均音響インテンシティオブジェクトの作成
    /// </summary>
    /// <param name="package">Serverから送信されたインテンシティ、座標を含むオブジェクト</param>
    /// <returns></returns>
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

    /// <summary>
    /// 瞬時音響インテンシティオブジェクトの作成(表示は一時的に時間平均のものとする)
    /// </summary>
    /// <param name="package">Serverから送信されたインテンシティ、座標、瞬時音響インテンシティを含むオブジェクト</param>
    /// <returns></returns>
    IEnumerator InstantIntensityObject(TransIntensityPackage package)
    {
        var intensityLv = AcousticMathNew.CalcuIntensityLevel(package.sumIntensity);
        //コーンの色を生成
        Color vecObjColor = ColorBar.DefineColor(Holo2MeasurementParameter.ColorMapID, intensityLv,
            Holo2MeasurementParameter.LevelMin, Holo2MeasurementParameter.LevelMax);
        //オブジェクトを生成
        instanceMaanger.CreateInstantIIntensityObj(package, vecObjColor, Holo2MeasurementParameter.ObjSize);
        measureID.Add(package.num);
        Debug.Log(DateTime.Now.ToString("MM/dd/HH:mm:ss.fff") + "Display measurement point No." + package.num);
        yield return null;
    }

    IEnumerator ReCalcIntensityMap(ReCalcDataPackage recalcData)
    {
        for(int n = 0;n < recalcData.storageNum; n++)
        {

                float intensityLv = AIMath.CalcuIntensityLevel(recalcData.intensities[n]);
                Color ObjColor = ColorBar.DefineColor(Holo2MeasurementParameter.ColorMapID, intensityLv, Holo2MeasurementParameter.LevelMin, Holo2MeasurementParameter.LevelMax);
            instanceMaanger.ChangeIntensityObj(recalcData.sendNums[n], recalcData.intensities[n], ObjColor);

            yield return null;
        }
        Debug.Log("Recalculation finished");
           
        yield return null;
    }

    IEnumerator ReCalcTransIntensityMap(ReCalcTransientDataPackage recalcData)
    {
        var frame = recalcData.iintensityList.Count / recalcData.storageNum;
        for (int n = 0; n < recalcData.storageNum; n++)
        {
            var iintensity = new Vector3[frame];
            for (int j = 0; j < frame; j++)
            {
                iintensity[j] = recalcData.iintensityList[n * frame + j];
            }
            float intensityLv = AIMath.CalcuIntensityLevel(recalcData.intensities[n]);
            Color ObjColor = ColorBar.DefineColor(Holo2MeasurementParameter.ColorMapID, intensityLv, Holo2MeasurementParameter.LevelMin, Holo2MeasurementParameter.LevelMax);
            instanceMaanger.ChangeInstantIntensityObj(recalcData.sendNums[n], recalcData.intensities[n], ObjColor, iintensity);

            yield return null;
        }
    }

    IEnumerator ReProIntensityMap(ReproDataPackage reproData)
    {
        //音響インテンシティ
        if (reproData.algorithm == AlgorithmPattern.CrossSpectrum)
        {
            for (int n = 0; n < reproData.storageNum; n++)
            {
                var intensityLv = AcousticMathNew.CalcuIntensityLevel(reproData.intensities[n]);
                //コーンの色を生成
                Color vecObjColor = ColorBar.DefineColor(Holo2MeasurementParameter.ColorMapID, intensityLv,
                    Holo2MeasurementParameter.LevelMin, Holo2MeasurementParameter.LevelMax);
                //オブジェクトを生成
                instanceMaanger.CreateInstantObj(reproData.sendNums[n], reproData.sendPoses[n], reproData.sendRots[n], reproData.intensities[n], vecObjColor, Holo2MeasurementParameter.ObjSize);

                yield return null;
            }
        }
        else //過渡音を対象にした瞬時音響インテンシティ
        {
            var frame = reproData.iintensities.Count / reproData.storageNum;
            for (int n = 0; n < reproData.storageNum; n++)
            {
                var intensityLv = AcousticMathNew.CalcuIntensityLevel(reproData.intensities[n]);
                //コーンの色を生成
                Color vecObjColor = ColorBar.DefineColor(Holo2MeasurementParameter.ColorMapID, intensityLv,
                    Holo2MeasurementParameter.LevelMin, Holo2MeasurementParameter.LevelMax);
                //オブジェクトを生成

                var iintensity = new Vector3[frame];
                for (int j = 0; j < frame; j++)
                {
                    iintensity[j] = reproData.iintensities[n * frame + j];
                }
                TransIntensityPackage transIntensityPackage = new TransIntensityPackage(reproData.sendPoses[n], reproData.sendRots[n], reproData.intensities[n], iintensity, reproData.sendNums[n]);
                instanceMaanger.CreateInstantIIntensityObj(transIntensityPackage, vecObjColor, Holo2MeasurementParameter.ObjSize);
                yield return null;
            }
        }
    }

    /// <summary>
    /// Holo2MeasurementParameterをServerに送信
    /// その後、空間マップを送信
    /// </summary>
    public async void SendSetting()
    {
        indicator = indicatorObject.GetComponent<IProgressIndicator>();
        //UIパラメータを更新
        uIManager.UpdateUIParameter();
        //送信オブジェクトをパッケージ化
        var sendSettingData = new SettingSender("", Holo2MeasurementParameter.ColorMapID,
            Holo2MeasurementParameter.LevelMax, Holo2MeasurementParameter.LevelMin, Holo2MeasurementParameter.ObjSize);
        string json = transferData.SerializeJson<SettingSender>(sendSettingData);
        try
        {
            tClient.StartSend(json);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            if (!Holo2MeasurementParameter.on_connected)
            {
                StartTCPClient();
            }
        }
        await indicator.OpenAsync();
        indicator.Message = "Mesh to Json";

        //空間マップの送信準備
        /* var mapMeshFileters = testMap.GetComponentsInChildren<MeshFilter>();
        foreach(var mapMeshFilter in mapMeshFileters)
        {
            indicator.Message = "Serialize Json";
            // ここでメッシュが取れます
            Mesh mesh = mapMeshFilter.sharedMesh;
            SpatialMesh data = new SpatialMesh("newMap", mesh);

            indicator.Message = "Send data";
            string jsonS = await Task.Run(() => transferData.SerializeJson<SpatialMesh>(data));
            tClient.StartSend(jsonS);
        }*/
        IMixedRealitySpatialAwarenessMeshObserver observer = await Task.Run(() => surfaceObserver.MapSendObserver());

        foreach (SpatialAwarenessMeshObject meshObject in observer.Meshes.Values)
        {
            indicator.Message = "Serialize Json";
            // ここでメッシュが取れます
            Mesh mesh = meshObject.Filter.mesh;
            SpatialMesh data = new SpatialMesh("newMap", mesh);

            indicator.Message = "Send data";
            string jsonS = await Task.Run(() => transferData.SerializeJson<SpatialMesh>(data));
            tClient.StartSend(jsonS);
        }

        //空間メッシュのデータ更新を止める
        // Cast the Spatial Awareness system to IMixedRealityDataProviderAccess to get an Observer
        var access = CoreServices.SpatialAwarenessSystem as IMixedRealityDataProviderAccess;

        // Get the first Mesh Observer available, generally we have only one registered
        var observers = access.GetDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();

        // Suspends observation of spatial mesh data
        observers.Suspend();


        await indicator.CloseAsync();
    }

    /// <summary>
    /// Startボタンに紐づけ
    /// </summary>
    public void StartMeasure()
    {
        counter = 0;
        if (Holo2MeasurementParameter.measurementType == MeasurementType.Standard)
            StartCoroutine("CoSendData");
        else if (Holo2MeasurementParameter.measurementType == MeasurementType.Transient)
        {
            //エアタップを計測トリガーに(ただしほかのエアタップが不能に)
            interactable_trigger = this.GetComponent<Interactable>();
            interactable_trigger.IsEnabled = true;
        }

    }

    /// <summary>
    /// 固定時間で座標を送信(時間平均の場合)
    /// </summary>
    /// <returns></returns>
    private IEnumerator CoSendData()
    {
        yield return new WaitForSeconds(1 / 6);
        while (Holo2MeasurementParameter._measure)
        {
            // [TODO] 時間固定(10fps)で送り続けています。どうするかは未定
            yield return new WaitForSeconds(1);
            if (UIManager._measure)
            {
                SendData(counter);
                counter++;                
            }
        }
    }
    /// <summary>
    /// 座標送信プロセス(時間平均＋過渡音どちらも適応)
    /// </summary>
    private void SendData(int counter)
    {
        micPositionMirror.GetSendInfo(out sendPos, out sendRotate);
        //送信用データの作成
        var sendPosition = new SendPosition(counter.ToString(), sendPos, sendRotate);
        string json = transferData.SerializeJson<SendPosition>(sendPosition);
        tClient.StartSend(json);
    }

    public void SendDeleteData(int dNum)
    {
        var deleteData = new DeleteData("delete", dNum);
        string json = transferData.SerializeJson<DeleteData>(deleteData);
        tClient.StartSend(json);
    }

    /// <summary>
    /// データ受信イベント
    /// </summary>
    /// <param name="ms">jsonファイル</param>
    void tClient_OnReceiveData(string ms)
    {
        var jsons = transferData.DevideData2Jsons(ms);
        foreach (string json in jsons)
        {
            var message = new ServerMessage();
            if (transferData.CanDesirializeJson<ServerMessage>(json, out message))
            {
                switch (message.sendType)
                {
                    case SendType.MeasurementType:
                        transferData.DesirializeJson<How2Measure>(out var how2Measure);
                        Holo2MeasurementParameter.measurementType = how2Measure.measureType;
                        Holo2MeasurementParameter.i_block = how2Measure.blockSize;
                        break;
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
                        Debug.Log("catch recalcdata");
                        recalcDataPackages.Enqueue(recalcDataPackage);
                        break;
                    case SendType.ReCalcTransData:
                        transferData.DesirializeJson<ReCalcTransientDataPackage>(out var reCalcTransientDataPackage);
                        recalcTransDataPackage.Enqueue(reCalcTransientDataPackage);
                        break;                       
                    case SendType.ReproData:
                        transferData.DesirializeJson<ReproDataPackage>(out var reproDataPackage);
                        //シェアリング側のみ実行
                        if (!Holo2MeasurementParameter._measure)
                        {
                            reproDatas.Enqueue(reproDataPackage);
                        }
                        break;
                    case SendType.IIntensities: //瞬時音響インテンシティ
                        transferData.DesirializeJson<TransIntensityPackage>(out var transIntensityPackage);
                        iIntensitiesPackages.Enqueue(transIntensityPackage);
                        break;
                    case SendType.DeleteData:
                        transferData.DesirializeJson<DeleteData>(out var deleteData);
                        deleteDatas.Enqueue(deleteData);
                        break;
                }
            }
            else
            {
                Debug.Log("cannot desirialize");
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

    /// <summary>
    /// 過渡音計測時に使用、Interactableに付随
    /// エアタップ時にデータの送信ができるようにする
    /// </summary>
    public void MeasurementTrigger()
    {
        Debug.Log("Air tapped ");
        if (Holo2MeasurementParameter._measure && Holo2MeasurementParameter.measurementType == MeasurementType.Transient)
        {
            SendData(counter);
            counter++;
            Debug.Log("Data send" + (counter - 1).ToString());
        }
    }

    /// <summary>
    /// データ欠損などでクライアントからサーバにデータを送っても反応がなくなった際に押す
    /// </summary>
    public void CleanTransfer()
    {
        transferData.CleanStorage();
    }
}
