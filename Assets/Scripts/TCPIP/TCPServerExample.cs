using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloLensModule.Network;

public class TCPServerExample : MonoBehaviour
{
    //ポート
    [SerializeField] int Port;

    [SerializeField] MeshRenderer box;
    TCPServerManager tServer;
    StateObject serverState = StateObject.stateObject;
    TransferData transferData = TransferData.transferData;

    Color color;
    bool GotData = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            StartTCPServer();
        if (Input.GetKeyDown(KeyCode.Alpha2))
            StopTCPServer();
        if (Input.GetKeyDown(KeyCode.Alpha3))
            SendPattern3();
        if (Input.GetKeyDown(KeyCode.Alpha4))
            SendPattern4();
        if (GotData)
        {
            box.material.color = color;
            GotData = false;
        }
    }

    /**tcp始めるボタンに紐付け**/
    /// <summary>
    /// TCPServer開始
    /// </summary>
    public void StartTCPServer()
    {
        tServer = new TCPServerManager(Port);
        //データ受信イベント
        tServer.ListenerMessageEvent += tServer_OnReceiveData;
        tServer.OnDisconnected += tServer_OnDisconnected;
        tServer.OnConnected += tServer_OnConnected;
        Debug.Log("Server OK");
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
            tServer.DisConnectClient();
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    //送信パターン3(ボタンに貼り付け)
    public void SendPattern3()
    {
        //送信データの3パターン目
        TransferParent sendData3 = new TransferParent("Test3", "Test3 bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb", 3);
        string json = transferData.SerializeJson<TransferParent>(sendData3);

        //送信
        try
        {
           tServer.SendMessage(json);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
    //送信パターン4(ボタンに貼り付け)
    public void SendPattern4()
    {
        //送信データの4パターン目
        TransferParent sendData4 = new TransferParent("Test4", "Test4", 4);
        string json = transferData.SerializeJson<TransferParent>(sendData4);

        //送信
        try
        {
          tServer.SendMessage(json);
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
        Debug.Log("Server接続解除");
    }

    /// <summary>
	/// 接続OKイベント
	/// </summary>
    void tServer_OnConnected(EventArgs e)
    {
        Debug.Log("Clientと接続完了");
    }

    /// <summary>
	/// データ受信イベント
	/// </summary>
    void tServer_OnReceiveData(string ms)
    {
        //受信データから設定オブジェクト設定
        TransferParent data = new TransferParent();
        if (transferData.CanDesirializeJson<TransferParent>(ms, out data))
        {
            GotData = true;
            //表示データを更新
            if (data.keyword == "Test1")
            {
                color = Color.black;
                Debug.Log(data.keyword + "番号"+data.testNum.ToString()
                + "コメント:" +  data.Comment);
            }
            else if (data.keyword == "Test2")
            {
               color = Color.red;
                Debug.Log(data.keyword + "番号" + data.testNum.ToString()
                + "コメント:" + data.Comment);
            }
        }
    }

    private void OnApplicationQuit()
    {
        if(tServer != null)
            StopTCPServer();
    }

}
