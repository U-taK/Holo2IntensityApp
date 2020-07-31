using UnityEngine;
using UnityEngine.Events;

namespace uOSC
{

public class uOscServer : MonoBehaviour
{
    [SerializeField]
    string address = "127.0.0.1";//マルチキャスト用

    [SerializeField]
    int port = 3333;

#if NETFX_CORE
    Udp udp_ = new Uwp.Udp();
//    Udp udp_ = new UwpMulti.Udp();//マルチキャスト用
    Thread thread_ = new UwpMulti.Thread();
#else
    //    Udp udp_ = new DotNetMulti.Udp();
        Udp udp_ = new DotNet.Udp();

    Thread thread_ = new DotNetMulti.Thread();
#endif
    Parser parser_ = new Parser();

    public class DataReceiveEvent : UnityEvent<Message> {};
    public DataReceiveEvent onDataReceived { get; private set; }

    void Awake()
    {
        onDataReceived = new DataReceiveEvent();
    }

    void OnEnable()
    {
            udp_.StartServer(port);
            //udp_.StartServer(address, port);//マルチキャスト用
            thread_.Start(UpdateMessage);
    }

    void OnDisable()
    {
        thread_.Stop();
        udp_.Stop();
    }

    void Update()
    {
        while (parser_.messageCount > 0)
        {
            var message = parser_.Dequeue();
            onDataReceived.Invoke(message);
        }
    }

    void UpdateMessage()
    {
        while (udp_.messageCount > 0) 
        {
            var buf = udp_.Receive();
            int pos = 0;
            parser_.Parse(buf, ref pos, buf.Length);
        }
    }
}

}