#if !NETFX_CORE

using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace uOSC.DotNetMulti
{

public class Udp : uOSC.Udp
{
    enum State
    {
        Stop,
        Server,
        Client,
    }
    State state_ = State.Stop;

    Queue<byte[]> messageQueue_ = new Queue<byte[]>();
    object lockObject_ = new object();

    UdpClient udpClient_;
    IPEndPoint endPoint_;
    Thread thread_ = new Thread();

    public override int messageCount
    {
        get { return messageQueue_.Count; }
    }

    public override void StartServer(int port)
    {
        Stop();
        state_ = State.Server;

        endPoint_ = new IPEndPoint(IPAddress.Any, port);
        udpClient_ = new UdpClient(endPoint_);
        thread_.Start(() => 
        {
            while (udpClient_.Available > 0) 
            {
                var buffer = udpClient_.Receive(ref endPoint_);
                lock (lockObject_)
                {
                    messageQueue_.Enqueue(buffer);
                }
            }
        });
    }
     //マルチキャスト用
    public override void StartServer(string address, int port)
    {
        Stop();
        state_ = State.Server;

        var ip = IPAddress.Parse(address);
        endPoint_ = new IPEndPoint(IPAddress.Any, port);
        udpClient_ = new UdpClient(endPoint_);
        udpClient_.JoinMulticastGroup(ip);
        thread_.Start(() =>
        {
            while (udpClient_.Available > 0)
            {
                var buffer = udpClient_.Receive(ref endPoint_);
                lock (lockObject_)
                {
                    messageQueue_.Enqueue(buffer);
                }
            }
        });
    }


    public override void StartClient(string address, int port)
    {
        Stop();
        state_ = State.Client;

        var ip = IPAddress.Parse(address);
        endPoint_ = new IPEndPoint(ip, port);
        udpClient_ = new UdpClient();
        udpClient_.JoinMulticastGroup(ip);//マルチキャスト用
    }

    public override void Stop()
    {
        if (state_ == State.Stop) return;

        thread_.Stop();
        udpClient_.DropMulticastGroup(endPoint_.Address);//マルチキャスト用
        udpClient_.Close();
        state_ = State.Stop;
    }

    public override void Send(byte[] data, int size)
    {
        udpClient_.Send(data, size, endPoint_);
    }

    public override byte[] Receive()
    {
        byte[] buffer;
        lock (lockObject_)
        {
            buffer = messageQueue_.Dequeue();
        }
        return buffer;
    }

}

}

#endif