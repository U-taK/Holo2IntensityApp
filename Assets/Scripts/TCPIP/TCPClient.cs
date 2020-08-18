using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class TCPClient
{
    /** メンバ **/
    //Socket
    private Socket client = null;
    //送受信文字列エンコード
    private Encoding enc = Encoding.UTF8;
    //ソケットの状態、手動
    public string socketState { get; set; } = "Null";

    /** イベント **/
    //データ受信イベント
    public delegate void ReceiveEventHandler(object sender, string e);
    public event ReceiveEventHandler OnReceiveData;
    //接続断イベント
    public delegate void DisconnectedEventHandler(object sender, EventArgs e);
    public event DisconnectedEventHandler OnDisconnected;
    //接続OKイベント
    public delegate void ConnectedEventHandler(EventArgs e);
    public event ConnectedEventHandler OnConnected;

    /** プロパティ **/
    /// <summary>
    /// ソケットが閉じているか
    /// </summary>
    public bool IsClosed
    {
        get { return (client == null); }
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public virtual void Dispose()
    {
        //Socketを閉じる
        Close();
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public TCPClient()
    {
        //Socket生成
        client = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);
        socketState = "Generated";
    }
    public TCPClient(Socket sc)
    {
        client = sc;
        socketState = "Generated";
    }

    /// <summary>
    /// Socket閉じる
    /// </summary>
    public void Close()
    {
        //クライアント閉じ
        Debug.Log("クライアント閉じるよ");
        CloseSocket();
        //workSocket?閉じ
        StateObject.stateObject.CloseWorkSocket();

        //接続断イベント発生
        OnDisconnected(this, new EventArgs());
    }
    private void CloseSocket()
    {
        if (client != null)
        {
            try
            {
                //Socketを無効
                client.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                //Socketを閉じる
                client.Close();
                client = null;
                Debug.Log("クライアント閉じたよ");
                socketState = "Disposed";
            }
        }
        else Debug.Log("クライアント閉じてたよ");
    }

    /// <summary>
    /// Hostに接続
    /// </summary>
    /// <param name="host">接続先ホスト</param>
    /// <param name="port">ポート</param>
    public void Connect(string host, int port)
    {
        //IP作成
        var ipEndPoint = new IPEndPoint(
            Dns.GetHostAddresses(host)[0], port);
        Debug.Log("接続するよ");
        //接続
        client.BeginConnect(ipEndPoint,
            new AsyncCallback(ConnectCallback), client);
    }
    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.
            var client = (Socket)ar.AsyncState;
            
            // Complete the connection.
            client.EndConnect(ar);
            
            Debug.Log(client.RemoteEndPoint.ToString()
                + "に接続してもらえたよ");
            socketState = "Connected";

            //接続OKイベント発生
            OnConnected(new EventArgs());
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    /// <summary>
    /// データ受信開始
    /// </summary>
    public void StartReceive()
    {
        try
        {
            var clienState = StateObject.stateObject;
            clienState.workSocket = client;

            Debug.Log("受信するよ");
            client.BeginReceive(clienState.buffer, 0, StateObject.BufferSize,
                0, new AsyncCallback(ReceiveCallback), clienState);
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }
    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            var clientState = (StateObject)ar.AsyncState;
            var client = clientState.workSocket;

            //受信
            int bytesRead = client.EndReceive(ar);
            if(bytesRead > 0)
            {
                string resData = Encoding.UTF8.GetString(clientState.buffer, 0, bytesRead);
                
                //データ受信イベント
                OnReceiveData(this, resData);

                //TODO:接続切れたら受信終了処理したいな

                if (client.Poll(1000, SelectMode.SelectRead) && (client.Available == 0))
                {
                    return;
                }
                if (resData.IndexOf("<EOF>") > -1)
                {
                    Debug.Log("受信終了");
                    socketState = "End Receiving";
                    client.Close();
                }
                else
                {
                    //受信続行
                    client.BeginReceive(clientState.buffer, 0, StateObject.BufferSize,
                        0, new AsyncCallback(ReceiveCallback), clientState);
                }
            }
            /*else
            {
                //TODO:どうなったら受信終了？
                //if(data.IndexOf("<EOF>") > -1)
                Debug.Log("受信終了");
                //永遠に受信し続ける
                client.BeginReceive(clientState.buffer, 0, StateObject.BufferSize,
                        0, new AsyncCallback(ReceiveCallback), clientState);
            }*/
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }

    /// <summary>
	/// 送信
	/// </summary>
	/// <param name="str">送信データ</param>
    public void StartSend(string str)
    {
        if (!IsClosed)
        {
            //文字列をBYTE配列に変換
            byte[] sendBytes = enc.GetBytes(str + "\r\n");
            Debug.Log(sendBytes + "を送信するよ");
            
            client.BeginSend(sendBytes, 0, sendBytes.Length, 0,
                new AsyncCallback(SendCallback), client);
        }
    }
    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            var handler = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.  
            int bytesSent = handler.EndSend(ar);
            //Debug.Log("Sent" + bytesSent + "bytes to client.");
            Debug.Log(bytesSent + "分送信したよ");

            //TODO:どっかで送信終了しときたい
            //handler.Shutdown(SocketShutdown.Both);
            //handler.Close();
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }
}
