using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloLensModule.Network;
using System;

public class TestClient4Server : MonoBehaviour
{
    TCPClientManager tClient;

    TransferData transferData = TransferData.transferData;

    Vector3 sendPos = Vector3.zero;
    Quaternion sendRot = Quaternion.identity;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            StartTCPClient();
        if (Input.GetKeyDown(KeyCode.Alpha2))
            SendSetting();
        if (Input.GetKeyDown(KeyCode.Alpha3))
            SendData();
    }

    public void StartTCPClient()
    {
        string host = "127.0.0.1";

        try
        {
            tClient = new TCPClientManager(host, 3333);
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

    public void SendSetting()
    {
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

    }

    private void SendData()
    {
        int counter = 0;
        var sendPosition = new SendPosition(counter++.ToString(), sendPos, sendRot);
         string json = transferData.SerializeJson<SendPosition>(sendPosition);
                tClient.SendMessage(json);
                //Debug.Log("send");
            
        
    }

    /// <summary>
    /// データ受信イベント
    /// </summary>
    /// <param name="ms">jsonファイル</param>
    void tClient_OnReceiveData(string ms)
    {
        var message = new ServerMessage();
        if (transferData.CanDesirializeJson<ServerMessage>(ms, out message))
        {
            
        }
        
    }

    void tClient_OnDisconnected(object sender, EventArgs e)
    {
        Debug.Log("Client接続解除");

    }

    void tClient_OnConnected(EventArgs e)
    {
        Debug.Log("Serverと接続完了");

    }


}
