using ChatProto;
using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UdpStreamProtocol;
using UnityEngine;

/// <summary>
/// udp通讯管理类
/// </summary>
public class UdpSocketManager : MonoBehaviour
{

    public static UdpSocketManager Instance;
 
    public Queue<byte[]> ReceivedAudioDataQueue = new Queue<byte[]>(); 
    public Queue<byte[]> ReceivedVideoDataQueue = new Queue<byte[]>();

    ConcurrentDictionary<long, List<UdpPacket>> packetCache=new ConcurrentDictionary<long, List<UdpPacket>>();
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
        Instance = this;
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

    DateTime udpHeratTime;

    public int UdpOutTime = 20;
    private void Update() //FixedUpdate()  
    {
        if (isRunning && (DateTime.Now - udpHeratTime).TotalSeconds > UdpOutTime)
        {
            print("udp heart out time!!!");
            ChatUIManager.Instance.Hang();
        }
        lock (ReceivedAudioDataQueue)
        {
            if (ReceivedAudioDataQueue.Count > 0)
            {
                ChatDataHandler.Instance.ReceiveAudio(ReceivedAudioDataQueue.Dequeue());
            }
            if (ReceivedAudioDataQueue.Count > 15)
            {
                ReceivedAudioDataQueue.Clear();
            }
        }
        lock (ReceivedVideoDataQueue)
        {
            if (ReceivedVideoDataQueue.Count > 0)
            {
                VideoHandler(ReceivedVideoDataQueue.Dequeue());
            }
            if (ReceivedVideoDataQueue.Count > 15)
            {
                ReceivedVideoDataQueue.Clear();
            }
        }
    }

    private void VideoHandler(byte[] message)
    {
        UdpPacket packet = ChatDataHandler.Instance.UdpPacketDecode(message);

        if (packet.Total ==1)
        {
            ChatDataHandler.Instance.ReceiveVideo(packet.Chunk);
        }
        else if (packet.Total>1)//需要组包
        {
            //超时未收到完整包，清理
            lock (packetCache)
            {
                if (packetCache.Count > 15 && packet.Index == 0) packetCache.Clear();
            }

            byte[] data= AddPacket(packet);
            if(data!=null) ChatDataHandler.Instance.ReceiveVideo(data);
        }   
    }

    byte[] AddPacket(UdpPacket udpPacket)
    {
        if (packetCache.ContainsKey(udpPacket.Sequence))
        {
            List<UdpPacket> udpPackets = null;
            if (packetCache.TryGetValue(udpPacket.Sequence, out udpPackets))
            {
                udpPackets.Add(udpPacket);

                if (udpPackets.Count == udpPacket.Total)
                {
                    packetCache.TryRemove(udpPacket.Sequence, out udpPackets);

                    udpPackets = udpPackets.OrderBy(u => u.Index).ToList();
                    int allLength = udpPackets.Sum(u => u.Chunk.Length);

                    //int maxPacketLength = udpPackets.Select(u => u.Chunk.Length).Max();

                    byte[] wholePacket = new byte[allLength];
                    foreach (var item in udpPackets)
                    {
                        Buffer.BlockCopy(item.Chunk, 0, wholePacket, item.Index * udpPacket.ChunkLength, item.Chunk.Length);
                    }
                    return wholePacket;
                }
            }
            return null;
        }
        else
        {
            List<UdpPacket> udpPackets = new List<UdpPacket>();
            udpPackets.Add(udpPacket);
            packetCache.AddOrUpdate(udpPacket.Sequence,udpPackets, (k, v) => { return udpPackets; });
            return null;
        }
    }

    public void OnReceiveData(byte[] data)
    {
        try
        {
            UdplDataModel model = UdpMessageCodec.Decode(data);
            switch (model.Request)
            {
                case RequestByte.REQUEST_HEART:
                    udpHeratTime = DateTime.Now;
                    break;
                case RequestByte.REQUEST_AUDIO:
                    ReceivedAudioDataQueue.Enqueue(model.ChatData);
                    break;
                case RequestByte.REQUEST_VIDEO:
                    ReceivedVideoDataQueue.Enqueue(model.ChatData);
                    break;
            }
        }
        catch (Exception e)
        {
            print("ReceivedDataQueue decode error:" + e.Message + "," + e.StackTrace);
        }
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
        upClient.Connect(Config.Instance.ChatStreamServerIP, Config.Instance.ChatStreamServerPort);
        upClient.OnReceiveData += OnReceiveData;
#else
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        udpSendSocket.IniSocket(socket,Config.Instance.ChatStreamServerIP, Config.Instance.ChatStreamServerPort);
        udpReceiveSocket.InitSocket(socket, Config.Instance.ChatStreamServerIP, Config.Instance.ChatStreamServerPort);
        udpReceiveSocket.OnReceiveData += OnReceiveData;
#endif
        print("Start listening");
        StartCoroutine(sendHeart());

        udpHeratTime = DateTime.Now;
    }
    //发送udp心跳包
    IEnumerator sendHeart()
    {
        print("start heart...");
        while (isRunning)
        {
            yield return new WaitForSeconds(2);
            CallInfo callInfo = new CallInfo();
            callInfo.UserID = ChatManager.Instance.UserID;

            UdplDataModel model = new UdplDataModel();
            model.ChatInfoData = callInfo.ToByteArray();
            model.Request = RequestByte.REQUEST_HEART;

            byte[] data = UdpMessageCodec.Encode(model); 
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
