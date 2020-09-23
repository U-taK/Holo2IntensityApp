using HoloLensModule.Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloLensModule.Network;
using System;

public class TestServer4client : MonoBehaviour
{
    TCPServerManager tServer;
    TransferData transferData = TransferData.transferData;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            StartTCPServer();
    }

    public void StartTCPServer()
    {
        tServer = new TCPServerManager(3333);
        //データ受信イベント
        tServer.ListenerMessageEvent += tServer_OnReceiveData;
        tServer.OnDisconnected += tServer_OnDisconnected;
        tServer.OnConnected += tServer_OnConnected;
        Debug.Log("Init setting");
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

    /// <summary>
	/// データ受信イベント
	/// </summary>
    void tServer_OnReceiveData(string ms)
    {
        var message = new HoloLensMessage();
        if (transferData.CanDesirializeJson<HoloLensMessage>(ms, out message))
        {
            switch (message.sendType)
            {
                case SendType.PositionSender://HoloLens側のマイクロホン位置を取得した際にオブジェクト生成を管理する
                    transferData.DesirializeJson<SendPosition>(out var sendPosition);
                    Debug.Log("[Server] \"Send Position\" position: " + sendPosition.sendPos.x + " rotation:" + sendPosition.sendRot.x);



                    break;
                case SendType.SettingSender://HoloLens側でsettingするパラメータを反映
                    transferData.DesirializeJson<SettingSender>(out var holoSetting);
                    Debug.Log("[Server] \"Holo Setting\" ColorMapID" + holoSetting.colorMapID);
                    break;
            }
        }
    }
    private void OnApplicationQuit()
    {
        if (tServer != null)
            StopTCPServer();
    }
}