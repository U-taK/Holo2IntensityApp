////////////////////////////////////
///HoloLens2側のアプリケーション
///通信テスト
////////////////////////////////////


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TCPClientExample : MonoBehaviour
{
    //接続先ホスト名
    [SerializeField] string Ip;
    //接続先ポート
    [SerializeField] string Port;
    [SerializeField] MeshRenderer box;
    [SerializeField] TextMesh number;
    [SerializeField] TextMesh message;

    //Note:オリジナルクラスはTCPClient、System.Net.SocketsはTcpClient
    TCPClient tClient = new TCPClient();
    StateObject clientState = StateObject.stateObject;
    TransferData transferData = TransferData.transferData;

    // Start is called before the first frame update
    void Start()
    {
        //接続OKイベント
        tClient.OnConnected += new TCPClient.ConnectedEventHandler(tClient_OnConnected);
        //接続断イベント
        tClient.OnDisconnected += new TCPClient.DisconnectedEventHandler(tClient_OnDisconnected);
        //データ受信イベント
        tClient.OnReceiveData += new TCPClient.ReceiveEventHandler(tClient_OnReceiveData);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /**tcp始めるボタンに紐付け**/
    /// <summary>
    /// TCPClient開始、接続
    /// </summary>
    public void StartTCPClient()
    {
        string host = Ip;
        int port;
        if (int.TryParse(Port, out int result))
            port = result;
        else port = 3333;

        try
        {
            tClient.Connect(host, port);
            //受信開始
            tClient.StartReceive();
            Debug.Log("準備完了");
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
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    //送信パターン1(ボタンに貼り付け)
    public void SendPattern1()
    {
        //送信データの1パターン目
        TransferParent sendData1 = new TransferParent("Test1", "Test1 aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", 10);
        string json = transferData.SerializeJson<TransferParent>(sendData1);
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

    //送信パターン2(ボタンに貼り付け)
    public void SendPattern2()
    {
        //送信データの1パターン目
        TransferParent sendData2 = new TransferParent("Test2", "Test2 sender", 5);
        string json = transferData.SerializeJson<TransferParent>(sendData2);
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

    /// <summary>
    /// 接続断イベント
    /// </summary>
    void tClient_OnDisconnected(object sender, EventArgs e)
    {
        Debug.Log("Client接続解除");
    }

    /// <summary>
	/// 接続OKイベント
	/// </summary>
    void tClient_OnConnected(EventArgs e)
    {
        Debug.Log("Client接続完了");
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
        //if (e.Contains("startMeasure"))
        TransferParent data = new TransferParent();
        if (transferData.CanDesirializeJson<TransferParent>(e, out data))
        {
            //表示データを更新
            if (data.keyword == "Test3") 
            {
                box.material.color = Color.green;
                number.text = data.testNum.ToString();
                message.text = data.Comment;
            }
            else if (data.keyword == "Test4")
            {
                box.material.color = Color.red;
                number.text = data.testNum.ToString();
                message.text = data.Comment;
            }
        }
        //受信データ->表示データ変換
        //       else
        //           clientState.ReceivedJson2ReceivedObj(e, ref cd);
    }

    private void OnApplicationQuit()
    {
        StopTCPClient();
    }

}
