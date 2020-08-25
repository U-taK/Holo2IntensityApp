//////////////////////////////
///HoloLens2-PC間のクライアント
///PC,Holoどちらでも動作確認済み
//////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
#if WINDOWS_UWP
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using System.IO;
using System.Diagnostics;
#else
using System.Net.Sockets;
using System.Threading;
#endif

namespace HoloLensModule.Network
{
    public class TCPClientManager
    {
        //データ受信イベント
        public delegate void ListenerMessageEventHandler(string ms);
        public event ListenerMessageEventHandler ListenerMessageEvent;

        public delegate void ListenerByteEventHandler(byte[] data);
        public event ListenerByteEventHandler ListenerByteEvent;

        //接続断イベント
        public delegate void DisconnectedEventHandler(object sender, EventArgs e);
        public event DisconnectedEventHandler OnDisconnected;
        //接続OKイベント
        public delegate void ConnectedEventHandler(EventArgs e);
        public event ConnectedEventHandler OnConnected;

#if WINDOWS_UWP
        private StreamWriter writer = null;
        private Task writetask = null;
#else
        private Thread sendthread = null;
        private NetworkStream stream = null;
#endif
        private bool isActiveThread = true;

        public TCPClientManager() { }

        /// <summary>
        ///サーバーとの接続を図る 
        /// </summary>
        /// <param name="ipaddress">サーバーアドレス</param>
        /// <param name="port"></param>
        public TCPClientManager(string ipaddress, int port)
        {
            ConnectClient(ipaddress, port);
        }

        public void ConnectClient(string ipaddress, int port)
        {
#if WINDOWS_UWP
            Task.Run(async() =>
            {
                StreamSocket socket = new StreamSocket();
                await socket.ConnectAsync(new HostName(ipaddress), port.ToString());
                writer = new StreamWriter(socket.OutputStream.AsStreamForWrite());
                StreamReader reader = new StreamReader(socket.InputStream.AsStreamForRead());
                byte[] bytes = new byte[65536];
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
                            if (ListenerMessageEvent != null) ListenerMessageEvent(Encoding.UTF8.GetString(data));
                            if (ListenerByteEvent != null) ListenerByteEvent(data);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Write(e);
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
            var tcpclient = new TcpClient();
            tcpclient.BeginConnect(ipaddress, port, ConnectCallback, tcpclient);
#endif
        }

        public bool SendMessage(string ms)
        {
            return SendMessage(Encoding.UTF8.GetBytes(ms));
        }

        public bool SendMessage(byte[] data)
        {
#if WINDOWS_UWP
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
            if (sendthread == null || sendthread.ThreadState != ThreadState.Running)
            {
                if (stream != null)
                {
                    sendthread = new Thread(() => { stream.Write(data, 0, data.Length); });
                    sendthread.Start();
                    return true;
                }
            }
#endif
            return false;
        }


        /// <summary>
        /// 終了処理
        /// ボタンに紐づけ
        /// </summary>
        public void DisConnectClient()
        {
#if WINDOWS_UWP
            if (writer!=null)
            {
                writer.Dispose();
            }
            writer = null;
#else
            isActiveThread = false;
            if (sendthread != null)
            {
                sendthread.Abort();
                sendthread = null;
            }
            stream = null;
#endif
            //接続断イベント発生
            OnDisconnected(this, new EventArgs());
        }
#if WINDOWS_UWP
#else
        /// <summary>
        /// 接続確認後動作
        /// データの受信
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectCallback(IAsyncResult ar)
        {
            var tcp = (TcpClient)ar.AsyncState;
            tcp.EndConnect(ar);
            tcp.ReceiveTimeout = 100;
            stream = tcp.GetStream();
            var bytes = new byte[tcp.ReceiveBufferSize];
            //接続OKイベント発生
            OnConnected(new EventArgs());
            while (isActiveThread)
            {
                try
                {
                    var num = stream.Read(bytes, 0, bytes.Length);
                    //データ受信待ち
                    if (num > 0)
                    {
                        var data = new byte[num];
                        Array.Copy(bytes, 0, data, 0, num);
                        //受信データはイベントで処理
                        if (ListenerMessageEvent != null) ListenerMessageEvent(Encoding.UTF8.GetString(data));
                        if (ListenerByteEvent != null) ListenerByteEvent(data);
                    }
                }
                catch (Exception e)
                {
                    Console.Write(e);
                }
            }
            stream.Close();
            stream = null;
            tcp.Close();
        }
#endif
    }
}