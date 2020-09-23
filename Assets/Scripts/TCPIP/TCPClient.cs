//////////////////////////////////
///HoloLens2-PC間のクライアント///
//////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
#if WINDOWS_UWP
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using System.IO;
using System.Diagnostics;
#else
using System.Net;
using System.Net.Sockets;
using System.Threading;
#endif

namespace HoloLensModule.Network
{
    public class TCPClient
    {
        /** メンバ **/
#if WINDOWS_UWP
    private StreamSocket socket = null;
    private StreamWriter writer = null;
    private Task writetask = null;
#else
        //Socket
        private Socket client = null;
#endif
        //TODO:淘汰？
        private bool isActiveThread = true;
        //送受信文字列エンコード
        private Encoding enc = Encoding.UTF8;
        //シーン上にログ出し

        StateObject clientState =  StateObject.stateObject;

        /** イベント **/
        //接続OKイベント
        public delegate void ConnectedEventHandler(EventArgs e);
        public event ConnectedEventHandler OnConnected;
        //接続断イベント
        public delegate void DisconnectedEventHandler(object sender, EventArgs e);
        public event DisconnectedEventHandler OnDisconnected;
#if WINDOWS_UWP
    //UWP
    //データ受信イベント
    public delegate void ListenerMessageEventHandler(string ms);
    public event ListenerMessageEventHandler ListenerMessageEvent;
    public delegate void ListenerByteEventHandler(byte[] data);
    public event ListenerByteEventHandler ListenerByteEvent;
#else
        //データ受信イベント
        public delegate void ReceiveEventHandler(string ms);
        public event ReceiveEventHandler OnReceiveData;
        //データ送信イベント
        public delegate void SendEventHandler(object sender, string e);
        public event SendEventHandler OnSendData;
#endif

        /** プロパティ **/
#if WINDOWS_UWP
#else
        /// <summary>
        /// ソケットが閉じているか
        /// </summary>
        public bool IsClosed
        {
            get { return (client == null); }
        }
#endif

#if WINDOWS_UWP
#else
        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {
            //Socketを閉じる
            Close();
        }
#endif

        /// <summary>
        /// コンストラクタ、接続も一緒
        /// </summary>
        public TCPClient(string host, int port)
        {
#if WINDOWS_UWP

        socket = new StreamSocket();
#else

            //Socket生成
            client = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
#endif
           
            //接続
            Connect(host, port);
        }
#if WINDOWS_UWP
#else
        public TCPClient(Socket sc, string host, int port)
        {
            client = sc;
            //接続
            Connect(host, port);
        }
#endif

        /// <summary>
        /// Socket閉じる
        /// </summary>
        public void Close()
        {
#if WINDOWS_UWP
        if (writetask != null || writetask.IsCompleted != true)
            writetask = null;   //dispose的なのあるかな
        isActiveThread = false;
        socket.Dispose();
        if (writer != null)
        {
            writer.Dispose();
        }
        writer = null;
#else
            //クライアント閉じ
            Debug.Log("クライアント閉じるよ");
            CloseSocket();
            //workSocket?閉じ
            clientState.CloseWorkSocket();
#endif
            //接続断イベント発生
            OnDisconnected(this, new EventArgs());
        }

#if WINDOWS_UWP
#else
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
                }
            }
            else Debug.Log("クライアント閉じてたよ");
        }
#endif

        /// <summary>
        /// Hostに接続、インスタンス生成時に
        /// </summary>
        /// <param name="host">接続先ホスト</param>
        /// <param name="port">ポート</param>
        private void Connect(string host, int port)
        {
#if WINDOWS_UWP
        Task.Run(async() =>
        {
            log += "task start\n";
            try
            {
                await socket.ConnectAsync(new HostName(host), port.ToString());

            }
            catch (Exception e)
            {
                var trace = new StackTrace(e, true);
                
                return;
            }
            try
            {
                writer = new StreamWriter(socket.OutputStream.AsStreamForWrite());

            }
            catch (Exception e)
            {

                var trace = new StackTrace(e, true);
                return;
            }
            StreamReader reader;
            try
            {

            }
            catch (Exception e)
            {
                var trace = new StackTrace(e, true);

                return;
            }
            byte[] bytes = new byte[65536];
            clientState.socketState = StateObject.SocketState.Connected;
            //接続OKイベント発生
            OnConnected(new EventArgs());
            while (isActiveThread)
            {
                try
                {
                    int num = await reader.BaseStream.ReadAsync(bytes, 0, bytes.Length);
                    if (num > 0)
                    {
                        byte[] data = new byte[num];
                        Array.Copy(bytes, 0, data, 0, num);
                        if (ListenerMessageEvent != null) 
                            ListenerMessageEvent(Encoding.UTF8.GetString(data));
                        /*if (ListenerByteEvent != null) 
                            ListenerByteEvent(data);*/
                    }
                }
                catch (Exception e)
                {
                }
            }
            socket.Dispose();
            if (writer != null)
            {
                writer.Dispose();
            }
            writer = null;
        });
#else
            //IP作成
            var ipEndPoint = new IPEndPoint(
            Dns.GetHostAddresses(host)[0], port);
            Debug.Log("接続するよ");
            //接続
            client.BeginConnect(ipEndPoint,
                new AsyncCallback(ConnectCallback), client);
#endif
        }
#if WINDOWS_UWP
#else
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
               
                //接続OKイベント発生
                OnConnected(new EventArgs());
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
#endif

#if WINDOWS_UWP
#else
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
            catch (Exception e)
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
                if (bytesRead > 0)
                {
                    string resData = Encoding.UTF8.GetString(clientState.buffer, 0, bytesRead);

                    //データ受信イベント
                    OnReceiveData(resData);

                    //TODO:接続切れたら受信終了処理したいな

                    if (client.Poll(1000, SelectMode.SelectRead) && (client.Available == 0))
                    {
                        return;
                    }
                    if (resData.IndexOf("<EOF>") > -1)
                    {
                        Debug.Log("受信終了");
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
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
#endif

        /// <summary>
        /// 送信
        /// </summary>
        /// <param name="str">送信データ</param>
        public void StartSend(string str)
        {
#if WINDOWS_UWP
        byte[] data = enc.GetBytes(str);
        if (writetask == null || writetask.IsCompleted == true)
        {
            if (writer != null)
            {
                writetask = Task.Run(async () =>
                {
                    await writer.BaseStream.WriteAsync(data, 0, data.Length);
                    await writer.FlushAsync();
                });
            }
        }
#else
            if (!IsClosed)
            {
                //文字列をBYTE配列に変換
                byte[] sendBytes = enc.GetBytes(str + "\r\n");
                Debug.Log(sendBytes + "を送信するよ");

                client.BeginSend(sendBytes, 0, sendBytes.Length, 0,
                    new AsyncCallback(SendCallback), client);
            }
#endif
        }
#if WINDOWS_UWP
#else
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
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
#endif
    }
}