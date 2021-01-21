using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using System;
using ChatProtocol;
using ChatProto;

/// <summary>
/// Network type, you can customize TCP, UDP, P2P, Webrct, Unet, Photon... The case demo uses UDP
/// </summary>
public enum NetType{UdpStream,UdpP2P };

public class ChatDataHandler : MonoBehaviour {

    public NetType NetType;

    public static ChatDataHandler Instance;
    public bool IsStartChat { get; set; }
    //Udp data chunk length <66500, and there are length restrictions on some platforms,you should adjust set 10000 or so
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

    //After starting the chat, the captured data of audio and video will be sent over the network in FixedUpdate
    //SDK will determine the refresh rate of audio and video, automatically recognize the sound size, determine whether the video is still, and optimize the data size
    //Note: The value of FixedUpdate Time needs to be less than 1 / (Framerate + 5), which can be set to 0.025 or less
    void FixedUpdate() {
        if (!IsStartChat)
            return;

        switch (NetType) {
            case NetType.UdpStream:
                switch (UnityChatSDK.Instance.ChatType)
                {
                    case ChatType.Audio:
                        SendAudio();
                        break;
                    case ChatType.Video:
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
    /// Send audio data
    /// </summary>
    void SendAudio() {
        //Get audio data by SDK
        AudioPacket packet = UnityChatSDK.Instance.GetAudio();
        if (packet != null)
        {
            packet.Id = ChatManager.Instance.UserID;
            byte[] audio = GetPbAudioPacket(packet).ToByteArray();

            //UDP Send data to server
            if (audio != null)
            {
                UdplDataModel model = new UdplDataModel();
                model.Request = UdpRequest.REQUEST_AUDIO;

                IMInfo info = new IMInfo();
                info.UserID = ChatManager.Instance.UserID;
                info.CallID = ChatManager.Instance.CallID;
                info.UserList.Add(ChatManager.Instance.ChatPeers);

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
        aduio.Id = packet.Id;
        aduio.Position = packet.Position;
        aduio.Length = packet.Length;
        aduio.Data = packet.Data.ToByteArray();
        aduio.Timestamp = packet.Timestamp;
        return aduio;
    }
    Queue<VideoPacket> videoPacketQueue = new Queue<VideoPacket>();

    /// <summary>
    /// Send video data
    /// </summary>
    void SendVideo()
    {
        //Get video data by SDK
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
                model.Request = UdpRequest.REQUEST_VIDEO;

                IMInfo info = new IMInfo();
                info.UserID = ChatManager.Instance.UserID;
                info.CallID = ChatManager.Instance.CallID;
                info.UserList.Add(ChatManager.Instance.ChatPeers);

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
        //decoded audio data by google.protobuf
        PbAudioPacket packet = PbAudioPacket.Parser.ParseFrom(data);
        //decode audio data by UnityChatSDK
        UnityChatSDK.Instance.DecodeAudioData(GetAudioPacket(packet));
    }
    public void ReceiveVideo(byte[] data)
    {
        //decoded video data by google.protobuf
        PbVideoPacket packet = PbVideoPacket.Parser.ParseFrom(data);
        //decode video data by UnityChatSDK
        UnityChatSDK.Instance.DecodeVideoData(GetVideoPacket(packet));
    }

    public void OnStartChat()
    {
        try
        {
            UdpSocketManager.Instance.StartListening();

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
    /// network status
    /// </summary>
    /// <returns>network available</returns>
    public bool IsNetAvailable()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return false;
        }
        return true;
    }

}
