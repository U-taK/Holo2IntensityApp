////////////////////////////////////
///HoloLens2側のアプリケーション
///通信テスト
////////////////////////////////////


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloLensModule.Network;

public class TCPClientExample : MonoBehaviour
{
    //接続先ホスト名
    [SerializeField] string Ip;
    //接続先ポート
    [SerializeField] string Port;
    [SerializeField] MeshRenderer box;
    [SerializeField] TextMesh number;
    [SerializeField] TextMesh message;
    //データの保持
    Color color;
    String num_string,mes_string;
    
    TCPClientManager tClient;
    TransferData transferData = TransferData.transferData;
    bool gotData = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (gotData)
        {
            box.material.color = color;
            number.text = num_string;
            message.text = mes_string;
            gotData = false;
        }
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
            tClient = new TCPClientManager(Ip, int.Parse(Port));
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

    /**tcp止めるボタンに紐付け**/
    /// <summary>
    /// TCPClient停止
    /// </summary>
    public void StopTCPClient()
    {
        try
        {
            //closeしてるけど送受信がどうなってるかは謎
            tClient.DisConnectClient();
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
            tClient.SendMessage(json);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    //送信パターン2(ボタンに貼り付け)
    public void SendPattern2()
    {
        //送信データの2パターン目
        TransferParent sendData2 = new TransferParent("Test2", "Test2 sender", 5);
        string json = transferData.SerializeJson<TransferParent>(sendData2);
        //送信
        try
        {
            tClient.SendMessage(json);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    /// <summary>
	/// データ受信イベント
	/// </summary>
    void tClient_OnReceiveData(string ms)
    {
        
        TransferParent data = new TransferParent();
        if (transferData.CanDesirializeJson<TransferParent>(ms, out data))
        {
            Debug.Log(data.keyword);
            gotData = true;
            //表示データを更新
            if (data.keyword == "Test3") 
            {
                color = Color.green;
                num_string = data.testNum.ToString();
                mes_string = data.Comment;
            }
            else if (data.keyword == "Test4")
            {
                color = Color.red;
                num_string = data.testNum.ToString();
                mes_string = data.Comment;
            }
            else if (data.keyword == "Test1")
            {
                color = Color.black;
                num_string = data.testNum.ToString();
                mes_string = data.Comment;
                Debug.Log(data.keyword + "番号" + data.testNum.ToString()
                + "コメント:" + data.Comment);
            }
            else if (data.keyword == "Test2")
            {
                color = Color.red;
                num_string = data.testNum.ToString();
                mes_string = data.Comment;
                Debug.Log(data.keyword + "番号" + data.testNum.ToString()
                + "コメント:" + data.Comment);
            }
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
        Debug.Log("Serverと接続完了");
    }

    private void OnApplicationQuit()
    {
        if(tClient != null)
            StopTCPClient();
    }

}
