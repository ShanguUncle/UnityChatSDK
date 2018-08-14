using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UdpStreamProtocol;
using UnityEngine;

/// <summary>
/// udp通讯管理类
/// </summary>
public class UdpSocketManager : MonoBehaviour
{

    public static UdpSocketManager _instance;
 
    public Queue<byte[]> ReceivedDataQueue = new Queue<byte[]>();

    private DateTime packetStartTime;

#if !UNITY_EDITOR && UNITY_WSA
    private UdpClient upClient;
#else
    private Socket socket;
    private UdpSendSocket udpSendSocket;
    private UdpReceiveSocket udpReceiveSocket;

#endif

    private void Awake()
    {
        _instance = this;
    }
    void Start()
    {

#if !UNITY_EDITOR && UNITY_WSA
        upClient = new UdpClient();
#else
        udpSendSocket =new UdpSendSocket();
        udpReceiveSocket = new UdpReceiveSocket();
#endif

    }

    DateTime UdpHeratTime;

    private int UdpOutTime = 10;
    private void FixedUpdate() 
    {
        if (isRunning && (DateTime.Now - UdpHeratTime).TotalSeconds > UdpOutTime)
        {
            print("udp心跳超时！！！");
            ChatUIManager._instance.Hang();
        }

        if (ReceivedDataQueue.Count > 0)
        {
            try
            {
                UdplDataModel model = UdpMessageCodec.decode(ReceivedDataQueue.Dequeue());
                switch (model.Request)
                {
                    case RequestByte.REQUEST_HEART:
                        UdpHeratTime= DateTime.Now;
                        break;
                    case RequestByte.REQUEST_STREAM:
                        ChatDataHandler.Instance.ReceiveStreamRemote(model.Message);
                        break;
                    case RequestByte.REQUEST_AUDIO:
                        ChatDataHandler.Instance.ReceiveAudio(model.Message);
                        break;
                    case RequestByte.REQUEST_VIDEO:
                        ChatDataHandler.Instance.ReceiveVideo(model.Message);
                        break;
                }
            }
            catch (Exception e)
            {
                print("ReceivedDataQueue decode error:"+e.Message+","+e.StackTrace);
            }
        
        }    
		if (ReceivedDataQueue.Count > 5)
        {
            ReceivedDataQueue.Clear();
        }

    }
    public void OnReceiveData(byte[] data)
    {
        ReceivedDataQueue.Enqueue(data);
    }


    bool isRunning=false;
    /// <summary>
    /// 开始udp监听
    /// </summary>
    public void StartListening()
    {
        if (isRunning) return;
        isRunning = true;

#if !UNITY_EDITOR && UNITY_WSA
        upClient.Connect(Config._instance.ChatStreamServerIP, Config._instance.ChatStreamServerPort);
        upClient.OnReceiveData += OnReceiveData;
#else
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        udpSendSocket.IniSocket(socket,Config._instance.ChatStreamServerIP, Config._instance.ChatStreamServerPort);
        udpReceiveSocket.InitSocket(socket, Config._instance.ChatStreamServerIP, Config._instance.ChatStreamServerPort);
        udpReceiveSocket.OnReceiveData += OnReceiveData;
#endif
        print("Start listening");
        StartCoroutine(sendHeart());

        UdpHeratTime = DateTime.Now;
    }
    //发送udp心跳包
    IEnumerator sendHeart()
    {
        print("start heart...");
        while (isRunning)
        {
            yield return new WaitForSeconds(3);

            UdplDataModel model = new UdplDataModel(RequestByte.REQUEST_HEART, BitConverter.GetBytes(ChatManager._instance.UserID));
            byte[] data = UdpMessageCodec.encode(model); 
            Send(data);
        }
        print("stop heart...");
    }
    /// <summary>
    /// 停止udp监听
    /// </summary>
    public void StopListening()
    {
        if (!isRunning) return;
        isRunning = false;

#if !UNITY_EDITOR && UNITY_WSA
        upClient.StopListening();
        upClient.OnReceiveData -= OnReceiveData;
#else
        try
        {
            socket.Dispose();
            socket = null;

            udpSendSocket.UnInit();
            udpReceiveSocket.OnReceiveData -= OnReceiveData;
        }
        catch (Exception e)
        {
            print("!UnInitUdpNet error: " + e.Message + ", " + e.StackTrace);
        }
#endif
        print("UnInitUdpNet OK!");
    }
    /// <summary>
    /// 发送udp数据
    /// </summary>
    /// <param name="buff"></param>
    public void Send(byte[] buff)
    {
       if (buff.Length > 65500) { print("buff > 65500 !!!"); return; }
#if !UNITY_EDITOR && UNITY_WSA
        upClient.SendUdpMessage(buff);
#else
        udpSendSocket.SendToAsyncByUDP(buff);
#endif
     
    }

    private void OnDestroy()
    {
        StopListening();
    }
}
