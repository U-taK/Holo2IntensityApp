////////////////////////////////
///client側のオブジェクトに紐付け///
////////////////////////////////

using UnityEngine;
using UnityEngine.UI;
using System;

public class TCPClientScene : MonoBehaviour
{
    //接続先ホスト名
    [SerializeField] InputField ipText;
    //接続先ポート
    [SerializeField] InputField portText;
    //設定
    [SerializeField] Toggle measureStart;
    [SerializeField] Toggle saveStart;
    [SerializeField] InputField bpf_min;
    [SerializeField] InputField bpf_max;
    [SerializeField] InputField state;
    //ソケットの状態表示テキスト
    [SerializeField] Text socketStateText;

    //Note:オリジナルクラスはTCPClient、System.Net.SocketsはTcpClient
    TCPClient tClient = new TCPClient();
    TStateObject clientState = TStateObject.tstateObject;
    TStateObject.SettingsData settings = new TStateObject.SettingsData();
 //   StateObject.ColorData cd = new StateObject.ColorData();

    void Start()
    {
        //接続OKイベント
        tClient.OnConnected += new TCPClient.ConnectedEventHandler(tClient_OnConnected);
        //接続断イベント
        tClient.OnDisconnected += new TCPClient.DisconnectedEventHandler(tClient_OnDisconnected);
        //データ受信イベント
        tClient.OnReceiveData += new TCPClient.ReceiveEventHandler(tClient_OnReceiveData);
    }

    void Update()
    {
        //設定データ受信したら設定データ表示
        //受信イベントでできたら良かったな
        clientState.SettingsObj2UIParams(settings, measureStart,
            saveStart, bpf_min, bpf_max, state);

        //ソケットの状態表示
        socketStateText.text = tClient.socketState;
    }

    /**tcp始めるボタンに紐付け**/
    /// <summary>
	/// TCPClient開始、接続
	/// </summary>
    public void StartTCPClient()
    {
        string host = ipText.text;
        int port;
        if (int.TryParse(portText.text, out int result))
            port = result;
        else port = 3333;

        try
        {
            tClient.Connect(host, port);
            //受信開始
            tClient.StartReceive();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    /**送信始めるボタンに紐付け**/
    /// <summary>
	/// データ送信
	/// </summary>
    public void SendSettingsTCPClient()
    {
        //送信データオブジェクトの設定
        clientState.UIParams2SettingsObj(ref settings, measureStart,
            saveStart, bpf_min, bpf_max, state);
        //jsonシリアライズ
        string json = clientState.SettingsObj2SendJson(settings);
        
        //送信
        try
        {
            tClient.StartSend(json);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    /**受信始めるボタンに紐付け(現在未使用)**/
    /**今はtcp始めると勝手に受信開始するようになってる**/
    /// <summary>
	/// データ受信
	/// </summary>
    public void ReceiveTCPClient()
    {
        try
        {
            tClient.StartReceive();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    /**tcp止めるボタンに紐付け**/
    /// <summary>
	/// TCPClient停止
	/// </summary>
    public void StopTCPClient()
    {
        try
        {
            //closeしてるけど送受信がどうなってるかは謎
            tClient.Close();
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }

    /// <summary>
	/// 接続断イベント
	/// </summary>
    void tClient_OnDisconnected(object sender, EventArgs e)
    {
        
    }

    /// <summary>
	/// 接続OKイベント
	/// </summary>
    void tClient_OnConnected(EventArgs e)
    {

    }

    /// <summary>
	/// データ受信イベント
	/// </summary>
    void tClient_OnReceiveData(object sender, string e)
    {
        //受信データに"startMeasure"が含まれてたら設定情報に変換
        //TODO:受信データに数字と記号だけだったら表示データに変換
        //とかの方が賢いかも、正規表現めんどいけど
        //受信データ->設定情報変換
        if (e.Contains("startMeasure"))
            clientState.ReceivedJson2SettingsObj(e, ref settings);
        //受信データ->表示データ変換
 //       else
 //           clientState.ReceivedJson2ReceivedObj(e, ref cd);
    }

    private void OnApplicationQuit()
    {
        StopTCPClient();
    }
}
