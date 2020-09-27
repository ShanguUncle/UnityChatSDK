using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using System;
using UdpStreamProtocol;
using ChatProto;

/// <summary>
/// 网络传输列类型，用户可自定义TCP，UDP，P2P,Webrct，Unet,Photon...案例demo使用的是UDP
/// </summary>
public enum NetType{UdpStream,UdpP2P };
/// <summary>
/// 聊天类型音频，视频，音视频
/// </summary>
//public enum ChatType {Audio,Video,AV};
public class ChatDataHandler : MonoBehaviour {

    public NetType NetType;
    public ChatType ChatType { get; set; }

    public static ChatDataHandler Instance;
    public bool IsStartChat { get; set; }
    //udp分包长度<66500，对于个别平台对长度有限制，适当降低长度(10000)
    public int ChunkLength = 50000;
    long udpPacketIndex;
    void Start() {
        Instance = this;
    }

    /// <summary>
    /// 开始聊天
    /// </summary>
    public void StartChat() {

        switch (NetType) {
            case NetType.UdpP2P:
                break;
            case NetType.UdpStream:
                OnStartChat();
                break;
        }
    }
    /// <summary>
    /// 停止聊天
    /// </summary>
    public void StopChat()
    {
        switch (NetType)
        {
            case NetType.UdpP2P:
                //TODO P2P
                break;
            case NetType.UdpStream:
                StartCoroutine(OnStopChat());
                break;
        }
    }

    //开始聊天后，在FixedUpdate会把捕捉到的音频和视频通过网络传输
    //SDK会判断音视频的刷新率，自动识别声音大小，判断视频画面是否静止，优化数据大小
    //注意：FixedUpdate Time的值需要小于 1/(Framerate+5),可设置为0.02或更小
    void FixedUpdate() {
        if (!IsStartChat)
            return;

        switch (NetType) {
            case NetType.UdpStream:
                switch (ChatType)
                {
                    case ChatType.Audio:
                        SendAudio();
                        break;
                    case ChatType.Video:
                        SendVideo();
                        break;
                    case ChatType.AV:
                        SendAudio();
                        SendVideo();
                        break;
                    default:
                        break;
                }
                break;
            case NetType.UdpP2P:
                break;
        }

    }

    /// <summary>
    /// 发送音频数据
    /// </summary>
    void SendAudio() {
        //获取SDK捕捉的音频数据
        AudioPacket packet = UnityChatSDK.Instance.GetAudio();
        if (packet != null)
        {
            packet.Id = ChatManager.Instance.UserID;
            byte[] audio = GetPbAudioPacket(packet).ToByteArray();
            //UDP发送数据到服务器（可更改为自己的服务器发送接口）
            if (audio != null)
            {
                UdplDataModel model = new UdplDataModel();
                model.Request = RequestByte.REQUEST_AUDIO;

                CallInfo info = new CallInfo();
                info.UserID = ChatManager.Instance.UserID;
                info.CallID = ChatManager.Instance.CallID;
                info.PeerList.Add(ChatManager.Instance.ChatPeerID);

                model.ChatInfoData = info.ToByteArray();
                model.ChatData = audio;

                UdpSocketManager.Instance.Send(UdpMessageCodec.Encode(model));
            }    
        }
    }
    PbAudioPacket GetPbAudioPacket(AudioPacket audio)
    {
        PbAudioPacket pbPacket = new PbAudioPacket();
        pbPacket.Id = audio.Id;
        pbPacket.Position = audio.Position;
        pbPacket.Length = audio.Length;
        pbPacket.Data =ByteString.CopyFrom(audio.Data);
        pbPacket.Timestamp = audio.Timestamp;
        return pbPacket;
    }
    AudioPacket GetAudioPacket(PbAudioPacket packet)
    {
        AudioPacket aduio = new AudioPacket();
        aduio.Position = packet.Position;
        aduio.Length = packet.Length;
        aduio.Data = packet.Data.ToByteArray();
        aduio.Timestamp = packet.Timestamp;
        return aduio;
    }
    Queue<VideoPacket> videoPacketQueue = new Queue<VideoPacket>();
    /// <summary>
    /// 发送视频数据
    /// </summary>
    void SendVideo()
    {
        //获取SDK捕捉的视频数据
        VideoPacket packet = UnityChatSDK.Instance.GetVideo();

        if (UnityChatSDK.Instance.EnableSync)
        {
            if (packet != null)
                videoPacketQueue.Enqueue(packet);

            if (videoPacketQueue.Count >= UnityChatSDK.Instance.Framerate / UnityChatSDK.Instance.AudioSample)
            {
                packet = videoPacketQueue.Dequeue();
            }
            else
            {
                return;
            }
        }

        if (packet != null)
        {
            packet.Id = ChatManager.Instance.UserID;
            byte[] video = GetPbVideoPacket(packet).ToByteArray();

            udpPacketIndex++;
            List<UdpPacket> list = UdpPacketSpliter.Split(udpPacketIndex, video, ChunkLength);
            for (int i = 0; i < list.Count; i++)
            {
                UdplDataModel model = new UdplDataModel();
                model.Request = RequestByte.REQUEST_VIDEO;

                CallInfo info = new CallInfo();
                info.UserID = ChatManager.Instance.UserID;
                info.CallID = ChatManager.Instance.CallID;
                info.PeerList.Add(ChatManager.Instance.ChatPeerID);

                model.ChatInfoData = info.ToByteArray();
                model.ChatData = UdpPacketEncode(list[i]);

                UdpSocketManager.Instance.Send(UdpMessageCodec.Encode(model));
            }
        }
    }
    PbVideoPacket GetPbVideoPacket(VideoPacket video) 
    {
        PbVideoPacket pbPacket = new PbVideoPacket();

        pbPacket.Id = video.Id;
        pbPacket.Width = video.Width;
        pbPacket.Height = video.Height;
        pbPacket.Timestamp = video.Timestamp;

        if (video.Data!=null)
        pbPacket.Data =ByteString.CopyFrom(video.Data);

        if (video.FloatData != null)
            pbPacket.FloatData.AddRange(video.FloatData);
        return pbPacket;
    }
    VideoPacket GetVideoPacket(PbVideoPacket packet) 
    {
        VideoPacket video = new VideoPacket();
        video.Id = packet.Id;
        video.Width = packet.Width;
        video.Height = packet.Height;
        video.Timestamp = packet.Timestamp;

        if (packet.Data != null)
            video.Data = packet.Data.ToByteArray();

        if(packet.FloatData!=null)
        video.FloatData.AddRange(packet.FloatData);
        return video;
    }
    byte[] UdpPacketEncode(UdpPacket packet)
    {
        byte[] newByte = new byte[packet.Chunk.Length + 20];
        Buffer.BlockCopy(BitConverter.GetBytes(packet.Sequence), 0, newByte, 0, 8);
        Buffer.BlockCopy(BitConverter.GetBytes(packet.Total), 0, newByte, 8, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(packet.Index), 0, newByte, 12, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(packet.ChunkLength), 0, newByte, 16, 4);
        Buffer.BlockCopy(packet.Chunk, 0, newByte, 20, packet.Chunk.Length);
        return newByte;
    }
    public UdpPacket UdpPacketDecode(byte[] data)
    {
        byte[] sequenceByte = new byte[8];
        Buffer.BlockCopy(data, 0, sequenceByte, 0, 8);

        byte[] totalByte = new byte[4];
        Buffer.BlockCopy(data, 8, totalByte, 0, 4);

        byte[] indexByte = new byte[4];
        Buffer.BlockCopy(data, 12, indexByte, 0, 4);

        byte[] chunkLengthByte = new byte[4]; 
        Buffer.BlockCopy(data, 16, chunkLengthByte, 0, 4);

        byte[] chunkByte = new byte[data.Length-20];
        Buffer.BlockCopy(data, 20, chunkByte, 0, data.Length - 20);

        UdpPacket packet = new UdpPacket();

        packet.Sequence = BitConverter.ToInt64(sequenceByte, 0);
        packet.Total = BitConverter.ToInt32(totalByte, 0);
        packet.Index = BitConverter.ToInt32(indexByte, 0);
        packet.ChunkLength = BitConverter.ToInt32(chunkLengthByte, 0);
        packet.Chunk = chunkByte;

        return packet;
    }
 

    public void ReceiveAudio(byte[] data)
    {
        //SDK进行音频数据的解码及音频播放
        PbAudioPacket packet= PbAudioPacket.Parser.ParseFrom(data);
        UnityChatSDK.Instance.DecodeAudioData(packet.Id, GetAudioPacket(packet));
    }
    public void ReceiveVideo(byte[] data)
    {
        //SDK进行视频数据的解码及视频渲染
        PbVideoPacket packet= PbVideoPacket.Parser.ParseFrom(data);
        UnityChatSDK.Instance.DecodeVideoData(packet.Id, GetVideoPacket(packet));
    }

    public void OnStartChat()
    {
        try
        {
            UdpSocketManager.Instance.StartListening();
            UnityChatSDK.Instance.ChatType = ChatType;

            CaptureResult result= UnityChatSDK.Instance.StartCapture();
            print("StartChat:" + result);
            IsStartChat = true;
            udpPacketIndex = 0;
            print("OnStartChat");
        }
        catch (Exception e)
        {
            print("OnStartChat error:" + e.Message);
        }
    }

    IEnumerator OnStopChat()
    {
        yield return new WaitForEndOfFrame();
        try
        {
            UnityChatSDK.Instance.StopCpture();
            UdpSocketManager.Instance.StopListening();
            videoPacketQueue.Clear();
            IsStartChat = false;
            print("OnStopChat");
        }
        catch (Exception e)
        {
            print("OnStopChat error:" + e.Message);
        }
    }

    /// <summary>
    /// 网络状态
    /// </summary>
    /// <returns>网络是否可用</returns>
    public bool IsNetAvailable()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return false;
        }
        return true;
    }

}
