using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace uOSC
{
    public class HoloTransfer : MonoBehaviour
    {
        public TextMesh text;
        // Start is called before the first frame update
        void Start()
        {
            var server = GetComponent<uOscServer>();
            server.onDataReceived.AddListener(OnDataReceived);

        }

        // Update is called once per frame
        void Update()
        {
            var client = GetComponent<uOscClient>();
            client.Send("/uOSC/test", "send HMD message to PC");
        }

        void OnDataReceived(Message message)
        {
            // address
            var msg = message.address + ": ";

            // timestamp
            msg += "(" + message.timestamp.ToLocalTime() + ") ";

            // values
            foreach (var value in message.values)
            {
                msg += value.GetString() + " ";
            }

            text.text = msg;
        }
    }
}