using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HoloLensModule.Network
{
    public class TCPServer
    {
        /** メンバ **/
        //Socket
        private Socket server = null;
        //送受信文字列エンコード
        private Encoding enc = Encoding.UTF8;
        //受信スレッド
        private Task task = null;
        //スレッドキャンセル
        private CancellationTokenSource tokenSource
            = new CancellationTokenSource();
        //NOTE:接続中のクライアントリストこっちにしたいけど、
        //System.ServiceModel.dll参照必要
        /*public SynchronizedCollection clientSockets { get; }
            = new SynchronizedCollection();*/
        public List<Socket> ClientSockets { get; } = new List<Socket>();

        StateObject serverState = StateObject.stateObject;

        /** イベント **/
        //データ受信イベント
        public delegate void ReceiveEventHandler(object sender, string ms);
        public event ReceiveEventHandler OnReceiveData;
        //データ送信イベント
        public delegate void SendEventHandler(object sender, string ms);
        public event SendEventHandler OnSendData;
        //接続断イベント
        public delegate void DisconnectedEventHandler(object sender, EventArgs e, string s);
        public event DisconnectedEventHandler OnDisconnected;
        //接続OKイベント
        public delegate void ConnectedEventHandler(EventArgs e, string log);
        public event ConnectedEventHandler OnConnected;
        //Thread signal
        private static ManualResetEvent allDone = new ManualResetEvent(false);

        /** プロパティ **/
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TCPServer(string host, int port)
        {
            //Socket生成
            server = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            //接続待ち開始
            StartListening(host, port);
        }
        public TCPServer(Socket sc, string host, int port)
        {
            server = sc;
            //接続待ち開始
            StartListening(host, port);
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
            //複数クライアント閉じ
            Debug.Log("クライアント閉じるよ");
            CloseClients();
            //workSocket?閉じ
            StateObject.stateObject.CloseWorkSocket();

            //接続断イベント発生
            string msg = "Close Server";
            OnDisconnected(this, new EventArgs(), msg);
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
                }
            }
            else Debug.Log("サーバ閉じてたよ");
        }
        private void CloseClients()
        {
            foreach (var clientSocket in this.ClientSockets)
            {
                Debug.Log(clientSocket.RemoteEndPoint + "を閉じるよ");
                clientSocket?.Close();
            }
            Debug.Log("全クライアント閉じたよ");
        }

        /// <summary>
        /// 接続開始
        /// </summary>
        /// <param name="host">ipアドレス</param>
        /// <param name="port">port番号</param>
        private void StartListening(string host, int port)
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

                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        Debug.Log("Canceled");
                        //キャンセルされたらtask終了
                        return;
                    }

                    allDone.Reset();

                    Debug.Log("接続待ってるよ");
                    server.BeginAccept(
                        new AsyncCallback(AcceptCallback), server);

                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
        private void AcceptCallback(IAsyncResult ar)
        {
            //Debug.Log("接続したよ");
            var serverState = StateObject.stateObject;
            allDone.Set();
            var listener = (Socket)ar.AsyncState;
            var handler = listener.EndAccept(ar);

            //接続中のクライアント、スレッドセーフに追加
            //TODO:本当はSynchronizedCollection使いたい
            lock (((ICollection)ClientSockets).SyncRoot)
                ClientSockets.Add(handler);
            Debug.Log(handler.RemoteEndPoint + "と接続したよ");
            string log = handler.RemoteEndPoint.ToString();
            //接続OKイベント発生
            OnConnected(new EventArgs(),log);

            serverState.workSocket = handler;
            Debug.Log("受信するよ");

            //接続要求待機再開
            /*listener.BeginAccept(
                new AsyncCallback(AcceptCallback), listener);*/
            //送信時に受信開始、接続要求待機終了
            handler.BeginReceive(serverState.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), serverState);
        }
        private void ReadCallback(IAsyncResult ar)
        {
            var serverState = (StateObject)ar.AsyncState;
            var handler = serverState.workSocket;

            try
            {
                //受信
                int bytesRead = handler.EndReceive(ar);
                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.
                    var content = Encoding.UTF8.GetString(serverState.buffer, 0, bytesRead);

                    //データ受信イベント
                    OnReceiveData(this, content);

                    //受信したのを接続中の全クライアントへ送信
                    //SendAllClient(content);

                    //TODO:受信中断、再開処理？
                    //受信続行
                    handler.BeginReceive(serverState.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), serverState);
                }
                else
                {

                    Debug.Log(handler.RemoteEndPoint + "が切断したよ");
                    //0bytes受信時は切断されたと判断
                    handler.Close();
                    //TODO:本当はSynchronizedCollection使いたい
                    lock (((ICollection)this.ClientSockets).SyncRoot)
                        this.ClientSockets.Remove(handler);

                    //接続断イベント
                    string msg = handler.RemoteEndPoint.ToString();
                    OnDisconnected(this, new EventArgs(), msg);
                }
            }
            catch (SocketException e)
            {
                if (e.NativeErrorCode.Equals(10054))
                {
                    //既存の接続がリモートホストによって強制的に切断された
                    //保持しているクライアント情報クリア
                    handler.Close();
                    //TODO:本当はSynchronizedCollection使いたい
                    lock (((ICollection)this.ClientSockets).SyncRoot)
                        this.ClientSockets.Remove(handler);
                    //接続断イベント
                    OnDisconnected(this, new EventArgs(), e.Message);
                }
                else
                {
                    string msg = string.Format("Disconnected!: error code {0} : {1}",
                        e.NativeErrorCode, e.Message);
                    Debug.Log(msg);
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }

        /// <summary>
        /// 送信
        /// </summary>
        /// <param name="data">送信データ</param>
        public void Send(String data)
        {
            try
            {
                var serverState = StateObject.stateObject;
                var handler = serverState.workSocket;

                //文字列に変換 
                var byteData = enc.GetBytes(data);

                Debug.Log(data + "を送信するよ");
                //データ送信イベント
                //OnSendData(this, data);

                // Begin sending the data to the remote device.  
                handler.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), handler);
            }
            catch (SocketException e)
            {
                if (e.NativeErrorCode.Equals(10054))
                {
                    //既存の接続がリモートホストによって強制的に切断された
                    //接続断イベント
                    OnDisconnected(this, new EventArgs(), e.Message);
                }
                else
                {
                    string msg = string.Format("Disconnected!: error code {0} : {1}",
                       e.NativeErrorCode, e.Message);
                    Debug.Log(msg);
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }
        public void Send(Socket clientSocket, String data)
        {
            try
            {
                //文字列に変換 
                var byteData = enc.GetBytes(data);

                Debug.Log(data + "を送信するよ");
                //データ送信イベント
                //OnSendData(this, data);

                // Begin sending the data to the remote device.  
                clientSocket.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), clientSocket);
            }
            catch (SocketException e)
            {
                if (e.NativeErrorCode.Equals(10054))
                {
                    //既存の接続がリモートホストによって強制的に切断された
                    //接続断イベント
                    OnDisconnected(this, new EventArgs(), e.Message);
                }
                else
                {
                    string msg = string.Format("Disconnected!: error code {0} : {1}",
                       e.NativeErrorCode, e.Message);
                    Debug.Log(msg);
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                var handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                var bytesSent = handler.EndSend(ar);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        /// <summary>
        /// 全クライアントへ送信
        /// </summary>
        /// <param name="data">送信データ</param>
        public void SendAllClient(string data)
        {
            foreach (var clientSocket in ClientSockets)
                Send(clientSocket, data);
        }
    }
}