using ChatNetWork;
using ChatProto;
using ChatProtocol;
using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

    private ChatUdpClient udpClient; 

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        udpClient = new ChatUdpClient();
    }

    DateTime udpHeratTime;

    public int UdpOutTime = 10;
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
            byte[] data= AddPacket(packet);
            if(data!=null) ChatDataHandler.Instance.ReceiveVideo(data);
        }

        if (packetCache.Count > 100) packetCache.Clear();
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
                case UdpRequest.REQUEST_HEART:
                    udpHeratTime = DateTime.Now;
                    break;
                case UdpRequest.REQUEST_AUDIO:
                    ReceivedAudioDataQueue.Enqueue(model.ChatData);
                    break;
                case UdpRequest.REQUEST_VIDEO:
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

        udpClient.Start(Config.Instance.ServerIP, Config.Instance.UdpPort);
        udpClient.OnReceiveData += OnReceiveData;

        print("Start listening");
        StartCoroutine(SendHeart());

        udpHeratTime = DateTime.Now;
    }
    //发送udp心跳包
    IEnumerator SendHeart()
    {
        print("start heart...");
        while (isRunning)
        {
            yield return new WaitForSeconds(0.5f);
            IMInfo info = new IMInfo();
            info.UserID = ChatManager.Instance.UserID;
            info.CallID = ChatManager.Instance.CallID;

            UdplDataModel model = new UdplDataModel();
            model.ChatInfoData = info.ToByteArray();
            model.Request = UdpRequest.REQUEST_HEART;

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

        udpClient.Stop();

        packetCache.Clear();
    }
    /// <summary>
    /// 发送udp数据
    /// </summary>
    /// <param name="buff"></param>
    public void Send(byte[] buff)
    {
        udpClient.Send(buff);
    }

    private void OnDestroy()
    {
        StopListening();
    }
}
