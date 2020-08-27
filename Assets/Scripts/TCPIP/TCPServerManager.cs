//////////////////////////////
///HoloLens2-PC間のサーバー
///PC側でサーバーを置くことのみを想定しているため,UDP実装はなし
//////////////////////////////

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace HoloLensModule.Network
{
    public class TCPServerManager
    {
        //データ受信イベント(json)
        public delegate void ListenerMessageEventHandler(string ms);
        public event ListenerMessageEventHandler ListenerMessageEvent;
        //データ受信イベント(バイナリ)
        public delegate void ListenerByteEventHandler(byte[] data);
        public event ListenerByteEventHandler ListenerByteEvent;

        //接続断イベント
        public delegate void DisconnectedEventHandler(object sender, EventArgs e);
        public event DisconnectedEventHandler OnDisconnected;

        //接続OKイベント
        public delegate void ConnectedEventHandler(EventArgs e);
        public event ConnectedEventHandler OnConnected;

        private TcpListener tcpListener = null;
        private List<NetworkStream> streamList = new List<NetworkStream>();
        private Thread sendthread = null;
        private NetworkStream stream = null;

        private bool isActiveThread = true;


        public TCPServerManager() { }

        //コンストラクタで接続開始
        public TCPServerManager(int port)
        {
            ConnectServer(port);
        }

        /// <summary>
        /// ポート番号のみを指定、サーバー側なのでIPは指定する必要なし
        /// </summary>
        /// <param name="port"></param>
        public void ConnectServer(int port)
        {
            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(AcceptTcpClient, tcpListener);
        }

        /// <summary>
        /// jsonファイルを送信(バイナリに変換してから)
        /// </summary>
        /// <param name="ms">jsonファイル</param>
        public bool SendMessage(string ms)
        {
            return SendMessage(Encoding.UTF8.GetBytes(ms));
        }

        /// <summary>
        /// バイナリファイルを送信
        /// </summary>
        /// <param name="data">送信データ</param>
        public bool SendMessage(byte[] data)
        {
            if (sendthread == null || sendthread.ThreadState != ThreadState.Running)
            {
                if (stream != null)
                {
                    sendthread = new Thread(() => { stream.Write(data, 0, data.Length); });
                    sendthread.Start();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// ボタンに紐づけ
        /// 終了処理
        /// </summary>
        public void DisConnectClient()
        {
            if (isActiveThread)
            {
                tcpListener.Stop();
                if (sendthread != null)
                {
                    sendthread.Abort();
                    sendthread = null;
                }
                stream = null;
                isActiveThread = false;
                UnityEngine.Debug.Log("サーバ閉じるよ");
                //接続断イベント発生
                OnDisconnected(this, new EventArgs());
            }
            else
                UnityEngine.Debug.Log("サーバ閉じてたよ");
        }

        /// <summary>
        /// クライアントとの接続に成功すると実行
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptTcpClient(IAsyncResult ar)
        {
            var listener = (TcpListener)ar.AsyncState;

            var tcpClient = listener.EndAcceptTcpClient(ar);
            listener.BeginAcceptTcpClient(AcceptTcpClient, listener);
            tcpClient.ReceiveTimeout = 100;
            stream = tcpClient.GetStream();
            streamList.Add(stream);
            var bytes = new byte[tcpClient.ReceiveBufferSize];
            if (OnConnected != null)
                OnConnected(new EventArgs());
            //受信待ち
            while (isActiveThread)
            {
                try
                {
                    //streamにデータがあれば読み込み
                    var num = stream.Read(bytes, 0, bytes.Length);
                    if (num > 0)
                    {
                        var data = new byte[num];
                        Array.Copy(bytes, 0, data, 0, num);
                        //イベントで取得データを処理
                        if (ListenerMessageEvent != null)
                            ListenerMessageEvent(Encoding.UTF8.GetString(data));
                        if (ListenerByteEvent != null)
                            ListenerByteEvent(data);
                    }
                }
                catch (Exception e)
                {
                    Console.Write(e);
                }
                //ここの処理が不明
                if (tcpClient.Client.Poll(1000, SelectMode.SelectRead) && tcpClient.Client.Available == 0) break;
            }
            stream.Close();
            stream = null;
            tcpClient.Close();
        }
    }
}