using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;

namespace HoloLensModule.Network
{
    public class StateObject
    {
        /** メンバ **/
        //Client Socket
        public List<Socket> workSockets { get; set; } = new List<Socket>();

        //受信素データバッファサイズ
        //多めに16384ぐらい取ってみる
        public const int BufferSize = 65536;
        //受信素データ
        public byte[] buffer { get; set; } = new byte[BufferSize];

        /** プロパティ **/
        /// <summary>
        /// workSocket閉じ
        /// </summary>
        public void CloseWorkSocket()
        {
            if (workSockets.Count > 0)
            {
                try
                {
                    foreach(var workSocket in workSockets)
                        workSocket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
                finally
                {
                    foreach (var workSocket in workSockets)
                    {
                        workSocket.Close();
                        //workSocket = null;
                    }
                    Debug.Log("workSocket閉じたよ");
                }
            }
            else Debug.Log("workSocket閉じてたよ");
        }
        
    }

    public class ServerStateObject: StateObject
    {
        /* メンバ */
        //接続状態のクライアント数
        public int connectedClientNum { get; set; } = 0;
    }

    public class ClientStateObject: StateObject
    {
        /* メンバ */
        //接続しているかどうか
        public bool isConnected { get; set; } = false;
        //自分のポート番号
        public int myPort { get; set; }
        //接続状態のクライアント数
        public int connectedClientNum { get; set; } = 0;
    }
}
