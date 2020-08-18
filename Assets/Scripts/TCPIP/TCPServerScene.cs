////////////////////////////////
///server側のオブジェクトに紐付け///
////////////////////////////////

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TCPServerScene : MonoBehaviour
{
    //こちらのホスト名
    [SerializeField] InputField ipText;
    //ポート
    [SerializeField] InputField portText;
    //送信データ縦横数
    [SerializeField] int matrixWidth = 33;
    [SerializeField] int matrixHeight = 33;
    //送信速度
    [SerializeField] float fps = 25f;
    //設定
    [SerializeField] Toggle measureStart;
    [SerializeField] Toggle saveStart;
    [SerializeField] InputField bpf_min;
    [SerializeField] InputField bpf_max;
    [SerializeField] InputField state;
    //ソケットの状態表示テキスト
    [SerializeField] Text socketStateText;

    //送信続行bool
    bool sendContinue = false;

    //Note:オリジナルクラスはTCPServer、System.Net.SocketsはTcpListener
    TCPServer tServer = new TCPServer();
    TStateObject serverState = TStateObject.tstateObject;
 //   StateObject.ColorData cd = new StateObject.ColorData();
    TStateObject.SettingsData settings = new TStateObject.SettingsData();

    void Start()
    {
        //接続OKイベント
        tServer.OnConnected += new TCPServer.ConnectedEventHandler(tServer_OnConnected);
        //接続断イベント
        tServer.OnDisconnected += new TCPServer.DisconnectedEventHandler(tServer_OnDisconnected);
        //データ受信イベント
        tServer.OnReceiveData += new TCPServer.ReceiveEventHandler(tServer_OnReceiveData);
    }

    void Update()
    {
        //受信したら受信データ表示
        //受信イベントでできたら良かったな
        serverState.SettingsObj2UIParams(settings, measureStart,
            saveStart, bpf_min, bpf_max, state);

        //ソケットの状態表示
        socketStateText.text = tServer.socketState;
    }

    /**tcp始めるボタンに紐付け**/
    /// <summary>
	/// TCPServer開始
	/// </summary>
    public void StartTCPServer()
    {
        string host = ipText.text;
        int port;
        if (int.TryParse(portText.text, out int result))
            port = result;
        else port = 3333;

        try
        {
            tServer.StartListening(host, port);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    /**配列送信始めるボタンに紐付け**/
    /// <summary>
	/// 配列データ送信
	/// </summary>
    public void SendArrayTCPServer()
    {
        if (sendContinue) sendContinue = false;
        else
        {
            sendContinue = true;
            StartCoroutine(SendMatrixJson());
        }
    }
    //json形式でどさっと送信試
    IEnumerator SendMatrixJson()
    {
        while (sendContinue)
        {
            /*
            //送信データ作成
            if(settings.saveData == true)
                cd.colorData = ColorPattern1();
            else
                cd.colorData = ColorPattern2();
            //json形式にシリアライズ
            string json = JsonUtility.ToJson(cd);
            
            //送信
            try
            {
                tServer.Send(json);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            */
            yield return new WaitForSeconds(1 / fps);
        }
    }
    /// <summary>
	/// 表示データパターン１、縦に流れるやつ
	/// </summary>
	/// <returns></returns>
    public float[] ColorPattern1()
    {
        float[] colorPattern = new float[matrixWidth * matrixHeight];
        for (int i = 0; i < colorPattern.Length; i++)
        {
            colorPattern[i] =
                ((float)i / (float)(colorPattern.Length)
                + Time.time) % 1;
    //        serverState.que.Enqueue(colorPattern[i]);
        }

        return colorPattern;
    }
    /// <summary>
	/// 表示パターン２、ランダム
	/// </summary>
	/// <returns></returns>
    public float[] ColorPattern2()
    {
        float[] colorPattern = new float[matrixWidth * matrixHeight];
        for (int i = 0; i < colorPattern.Length; i++)
        {
            colorPattern[i] =
                UnityEngine.Random.Range(0, 1.0f);
     //       serverState.que.Enqueue(colorPattern[i]);
        }

        return colorPattern;
    }

    /**設定情報送信始めるボタンに紐付け**/
    /// <summary>
	/// 設定情報送信
	/// </summary>
    public void SendSettingsTCPServer()
    {
        //UIの値から設定オブジェクトの設定
        serverState.UIParams2SettingsObj(ref settings, measureStart,
            saveStart, bpf_min, bpf_max, state);
        //json形式にシリアライズ
        string json = serverState.SettingsObj2SendJson(settings);
        
        //送信
        try
        {
            tServer.Send(json);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    /**tcp止めるボタンに紐付け**/
    /// <summary>
	/// TCPServer停止
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
	/// 接続断イベント
	/// </summary>
    void tServer_OnDisconnected(object sender, EventArgs e)
    {

    }

    /// <summary>
	/// 接続OKイベント
	/// </summary>
    void tServer_OnConnected(EventArgs e)
    {

    }

    /// <summary>
	/// データ受信イベント
	/// </summary>
    void tServer_OnReceiveData(object sender, string e)
    {
        //受信データから設定オブジェクト設定
        serverState.ReceivedJson2SettingsObj(e, ref settings);
    }

    private void OnApplicationQuit()
    {
        StopTCPServer();
    }
}
