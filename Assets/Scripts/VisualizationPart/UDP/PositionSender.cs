using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uOSC
{

    public class PositionSender : MonoBehaviour
    {

        Vector3 sendPos;
        Quaternion sendRotate;

        [SerializeField]
        MicPositionMirror micPositionMirror;

        uOscClient client;

        // Use this for initialization
        public void SendStart(string ip)
        {
            client = GetComponent<uOscClient>();
            client.OnClientStart(ip);
            StartCoroutine("SendData");
        }

        private IEnumerator SendData()
        {
            while (true)
            {
                yield return new WaitForSeconds(1 / 6);
                if (UIManager._measure)
                {
                    micPositionMirror.GetSendInfo(out sendPos, out sendRotate);
                    client.Send("PositionSender", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"), sendPos.x, sendPos.y, sendPos.z, sendRotate.x, sendRotate.y, sendRotate.z, sendRotate.w);
                    //Debug.Log("send");
                }
            }
        }


        public void SendSetting()
        {
            client.Send("SettingSender", UIManager.ColorMapID, UIManager.LevelMin, UIManager.LevelMax, UIManager.ObjSIze);
            Debug.Log("Setting FInish");
        }

        public void delaySend(int i, DateTime date)
        {
            client.Send("DelaySend", i, date.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        }
    }
}