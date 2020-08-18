using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class TCPServer : MonoBehaviour
{
    /** メンバ **/
    //Socket
    private Socket server = null;
    //送受信文字列エンコード
    private Encoding enc = Encoding.UTF8;
    //受信スレッド
    private Task task = null;
    //本当にこいつでキャンセルされてるのか疑問
    CancellationTokenSource tokenSource = new CancellationTokenSource();
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

    //Thread signal
    private static ManualResetEvent socketEvent = new ManualResetEvent(false);

    /** プロパティ **/
    /// <summary>
    /// ソケットが閉じているか
    /// </summary>
    public bool isClosed
    {
        get { return (server == null); }
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
    public TCPServer()
    {
        //Socket生成
        server = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);
        socketState = "Generated";
    }
    public TCPServer(Socket sc)
    {
        server = sc;
        socketState = "Generated";
    }

    /// <summary>
	/// ソケット閉じ
	/// </summary>
    public void Close()
    {
        //タスクキャンセル
        CancelTask();

        //サーバ閉じ
        Debug.Log("サーバ閉じるよ");
        CloseSocket();
        //workSocket?閉じ
        StateObject.stateObject.CloseWorkSocket();

        //接続断イベント発生
        OnDisconnected(this, new EventArgs());
    }
    private void CancelTask()
    {
        tokenSource.Cancel();

        if (task.IsCompleted) Debug.Log("task is completed");
        if (task.IsCanceled) Debug.Log("task is canceled");
        if (task.IsFaulted) Debug.Log("task is faulted");
        Debug.Log("task status: " + task.Status);
    }
    private void CloseSocket()
    {
        if (server != null)
        {
            try
            {
                server.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                server.Close();
                server = null;
                Debug.Log("サーバ閉じたよ");
                socketState = "Disposed";
            }
        }
        else Debug.Log("サーバ閉じてたよ");
    }

    /// <summary>
	/// 接続開始
	/// </summary>
	/// <param name="host">ipアドレス</param>
	/// <param name="port">port番号</param>
    public void StartListening(string host, int port)
    {
        Debug.Log("接続するよ");
        //別スレッドにしないとUnityEditor固まっちゃう
        CancellationToken token = tokenSource.Token;
        task = Task.Run(() => Listen(host, port, token));
    }
    /// <summary>
	/// 接続待ち
	/// </summary>
	/// <param name="host">ipアドレス</param>
	/// <param name="port">port番号</param>
    private void Listen(string host, int port, CancellationToken token)
    {
        try
        {
            //IP作成
            var ipEndPoint = new IPEndPoint(Dns.GetHostAddresses(host)[0], port);
            server.Bind(ipEndPoint);
            //こいつの引数、保留中の接続のキューの最大長
            //要は接続できるクライアントの最大数？
            server.Listen(10);
            socketState = "Listening";

            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    Debug.Log("Canceled");
                    //キャンセルされたらtask終了
                    return;
                }

                socketEvent.Reset();

                Debug.Log("接続待ってるよ");
                server.BeginAccept(
                    new AsyncCallback(AcceptCallback), server);
                
                socketEvent.WaitOne();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
    private void AcceptCallback(IAsyncResult ar)
    {
        Debug.Log("接続したよ");
        socketState = "Connected";
        socketEvent.Set();
        var listener = (Socket)ar.AsyncState;
        var handler = listener.EndAccept(ar);
        
        //接続OKイベント発生
        OnConnected(new EventArgs());

        var serverState = StateObject.stateObject;
        serverState.workSocket = handler;
        Debug.Log("受信するよ");

        handler.BeginReceive(serverState.buffer, 0, StateObject.BufferSize, 0,
            new AsyncCallback(ReadCallback), serverState);
    }
    private void ReadCallback(IAsyncResult ar)
    {
        var content = String.Empty;

        var serverState = (StateObject)ar.AsyncState;
        var handler = serverState.workSocket;

        //受信
        int bytesRead = handler.EndReceive(ar);
        if (bytesRead > 0)
        {
            // There  might be more data, so store the data received so far.
            string resData = Encoding.UTF8.GetString(serverState.buffer, 0, bytesRead);
            
            //データ受信イベント
            OnReceiveData(this, resData);

            //TODO:受信中断、再開処理？
            
            if (resData.IndexOf("<EOF>") > -1)
            {
                Debug.Log("受信終了");
                socketState = "End Listening";
            }
            else
            {
                //受信続行
                handler.BeginReceive(serverState.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), serverState);
            }
        }
    }

    /// <summary>
	/// 送信
	/// </summary>
	/// <param name="data">送信データ</param>
    public void Send(String data)
    {
        var serverState = StateObject.stateObject;
        var handler = serverState.workSocket;

        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = enc.GetBytes(data);

        Debug.Log(data + "を送信するよ");
        // Begin sending the data to the remote device.  
        handler.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), handler);
    }
    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            var handler = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.  
            int bytesSent = handler.EndSend(ar);
            Debug.Log(bytesSent + "分送信したよ");
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }
}
