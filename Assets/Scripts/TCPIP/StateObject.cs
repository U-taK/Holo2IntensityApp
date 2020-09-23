using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;

namespace HoloLensModule.Network
{
    public class StateObject
    {
        private static StateObject _stateObject = new StateObject();
        public static StateObject stateObject {get { return _stateObject; } }
        private StateObject() { }


        /** メンバ **/
        //Client Socket
        public Socket workSocket { get; set; } = null;

        //受信素データバッファサイズ
        //多めに16384ぐらい取ってみる
        public const int BufferSize = 16384;
        //受信素データ
        public byte[] buffer { get; set; } = new byte[BufferSize];

        /** プロパティ **/
        /// <summary>
        /// workSocket閉じ
        /// </summary>
        public void CloseWorkSocket()
        {
            if (workSocket != null)
            {
                try
                {
                    workSocket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
                finally
                {
                    workSocket.Close();
                    workSocket = null;
                    Debug.Log("workSocket閉じたよ");
                }
            }
            else Debug.Log("workSocket閉じてたよ");
        }
        
    }
}
