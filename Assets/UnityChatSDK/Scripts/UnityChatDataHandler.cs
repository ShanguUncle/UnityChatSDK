using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Xml.Serialization;

public class UnityChatDataHandler : MonoBehaviour {

    public bool IsStartChat { get; set; }

    Queue<VideoPacket> videoPacketQueue = new Queue<VideoPacket>();

    //temp save the data received from the your server and handler in update
    public Queue<byte[]> ReceivedAudioDataQueue = new Queue<byte[]>();
    public Queue<byte[]> ReceivedVideoDataQueue = new Queue<byte[]>();

    void Start()
    {

    }
    /// <summary>
    /// start video chat
    /// </summary>
    public void StartVideoChat()
    {
       OnStartChat(ChatType.AV);
       //note:link rawimage by id
       UnityChatSDK.Instance.AddChatPeer(1001, FindObjectOfType<UnityChatSet>().ChatPeerRawImage[0]);
    }
    /// <summary>
    /// start audio chat
    /// </summary>
    public void StartAudioChat()
    {
        OnStartChat(ChatType.Audio);
    }

    void OnStartChat(ChatType type)
    {
        try
        {
            UnityChatSDK.Instance.ChatType = type;
    
            CaptureResult result = UnityChatSDK.Instance.StartCapture();
            print("StartChat:" + result);
            IsStartChat = true;
            print("OnStartChat");
        }
        catch (Exception e)
        {
            print("OnStartChat error:" + e.Message);
        }
    }

    /// <summary>
    ///Stop chat
    /// </summary>
    public void StopChat()
    {
        StartCoroutine(OnStopChat());
    }

    IEnumerator OnStopChat()
    {
        yield return new WaitForEndOfFrame();
        try
        {
            UnityChatSDK.Instance.StopCpture();
            videoPacketQueue.Clear();
            UnityChatSDK.Instance.ClearChatPeer();
            IsStartChat = false;
            print("OnStopChat");
        }
        catch (Exception e)
        {
            print("OnStopChat error:" + e.Message);
        }
    }


    // after starting chat, in FixedUpdate the capture of audio and video will be transmitted via your own network
    // note: The value of FixedUpdate Time needs to be less than 1 / (Framerate + 5), which can be set to 0.02 or less
    void FixedUpdate()
    {
        if (!IsStartChat)
            return;

        switch (UnityChatSDK.Instance.ChatType)
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
    }
    private void Update() 
    {
        lock (ReceivedAudioDataQueue)
        {
            if (ReceivedAudioDataQueue.Count > 0)
            {
               OnReceiveAudio(ReceivedAudioDataQueue.Dequeue());
            }
        }
        lock (ReceivedVideoDataQueue)
        {
            if (ReceivedVideoDataQueue.Count > 0)
            {
                OnReceiveVideo(ReceivedVideoDataQueue.Dequeue());
            }
        }
    }
//==================send data========================
    /// <summary>
    /// send audio data
    /// </summary>
    void SendAudio()
    {
        //capture audio data by SDK
        AudioPacket packet = UnityChatSDK.Instance.GetAudio();
        if (packet != null)
        {
            packet.Id = 1001;//use your userID
            byte[] audio = GetPbAudioPacket(packet);

            if (audio != null)
            {
                //send data through your own network,such as TCP，UDP，P2P,Webrct，Unet,Photon...,the demo uses UDP.
                SendDataByYourNetwork(audio);

                //just for testing
                ReceivedAudioDataQueue.Enqueue(audio);
            }
        }
    }
    byte[] GetPbAudioPacket(AudioPacket packet)
    {
        //you need to do
        //coding packet to bytes by google.protobuf/protobufNet...

        //use XmlSerializer for testing,not a good choice
        using (MemoryStream memorry = new MemoryStream())
        {
            try
            {
                new XmlSerializer(typeof(AudioPacket)).Serialize(memorry, packet);
                return memorry.ToArray();
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// send video data
    /// </summary>
    void SendVideo()
    {
        //获取SDK捕捉的视频数据
        VideoPacket packet = UnityChatSDK.Instance.GetVideo();
        if (packet == null || packet.Data == null || packet.Data.Length == 0) return;

        if (UnityChatSDK.Instance.EnableSync)
        {
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
        else 
        {
            packet.Id = 1001;//use your userID
            byte[] video = GetPbVideoPacket(packet);
            SendDataByYourNetwork(video);

            //just for testing
            ReceivedVideoDataQueue.Enqueue(video);
        }
    }
    byte[] GetPbVideoPacket(VideoPacket packet) 
    {
        //you need to do
        //coding packet to bytes by google.protobuf/protobufNet...

        //use XmlSerializer for testing,not a good choice
        using (MemoryStream memorry = new MemoryStream())
        {
            try
            {
                new XmlSerializer(typeof(VideoPacket)).Serialize(memorry, packet);
                return memorry.ToArray();
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
    void SendDataByYourNetwork(byte[] data)
    {
        //you need to do
    }
    //==================receive data========================
    /// <summary>
    /// called when audio data is received
    /// </summary>
    /// <param name="data"></param>
    public void OnReceiveAudio(byte[] data) 
    {
        //decode audio data and playback
        AudioPacket packet = DecodeAudioPacket(data);
        UnityChatSDK.Instance.DecodeAudioData(packet.Id, packet);
    }
    AudioPacket DecodeAudioPacket(byte[] data)
    {
        //you need to do
        //decode bytes to packet

        //use XmlSerializer for testing,not a good choice
        using (MemoryStream memorry = new MemoryStream(data))
        {
            try
            {
                return (AudioPacket)(new XmlSerializer(typeof(AudioPacket)).Deserialize(memorry));
            }
            catch (Exception e)
            {
                return default(AudioPacket);
            }
        }
    }

    /// <summary>
    /// called when video data is received
    /// </summary>
    /// <param name="data"></param>
    public void OnReceiveVideo(byte[] data) 
    {
        //decode video data and render video
        VideoPacket packet = DecodeVideoPacket(data);
        UnityChatSDK.Instance.DecodeVideoData(packet.Id, packet);
    }
    VideoPacket DecodeVideoPacket(byte[] data)
    {
        //you need to do
        //decode bytes to packet

        //use XmlSerializer for testing,not a good choice
        using (MemoryStream memorry = new MemoryStream(data))
        {
            try
            {
                return (VideoPacket)(new XmlSerializer(typeof(VideoPacket)).Deserialize(memorry));
            }
            catch (Exception e)
            {
                return default(VideoPacket);
            }
        }
    }


}
