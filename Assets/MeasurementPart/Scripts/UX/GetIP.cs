/////////////////////////////////////////////////////
///ServerであるPCのIPアドレスをTextに貼り付けて表示
////////////////////////////////////////////////////


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Net.NetworkInformation;

public class GetIP : MonoBehaviour
{
    InputField inputField;
    // Start is called before the first frame update
    void Start()
    {
        inputField = GetComponent<InputField>();
        
        if (inputField != null)
        {
            var adress = GetMyIPAddress();
            inputField.text = adress;
            MeasurementParameter.TCPAdress = adress;
            Debug.Log(MeasurementParameter.TCPAdress);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //サーバ側のIPアドレスを取得
    private string GetMyIPAddress()
    {
        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
            {
                //なぜ2なのか不明
                if (ni.Name == "イーサネット 2")
                {

                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            //do what you want with the IP here... add it to a list, just get the first and break out. Whatever.
                            return ip.Address.ToString();
                        }
                    }
                }
            }
        }
        return "0:0:0:0";
    }

    //IPアドレスがとれなかった場合、直入力を推奨
    public void OnIPChange()
    {
        MeasurementParameter.TCPAdress = inputField.text;
    }
}
