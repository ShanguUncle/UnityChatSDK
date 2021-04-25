using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// Processing audio and video codec logic
/// </summary>
public class UnityChatDataHandler : MonoBehaviour {

    //this uid used for testing, set your uid in specific application
    public int TestUid = 1001;
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
       OnStartChat(ChatType.Video);
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
            UnityChatSDK.Instance.StopCapture();
            videoPacketQueue.Clear();
            ReceivedAudioDataQueue.Clear();
            ReceivedVideoDataQueue.Clear();
            IsStartChat = false;
            print("OnStopChat");
        }
        catch (Exception e)
        {
            print("OnStopChat error:" + e.Message);
        }
    }


    // after starting chat, in FixedUpdate the capture of audio and video will be transmitted via your own network
    // note: The value of FixedUpdate Time needs to be less than 1 / (Framerate + 5), which can be set to 0.025 or less
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
            packet.Id = TestUid;//use your userID
            byte[] audio = GetAudioPacketData(packet);

            if (audio != null)
            {
                //send data through your own network,such as TCP，UDP，P2P,Webrct，Unet,Photon...,the demo uses UDP for testing.
                SendDataByYourNetwork(audio);

                //On receiving audio data,just for testing
                ReceivedAudioDataQueue.Enqueue(audio);
            }
        }
    }
    byte[] GetAudioPacketData(AudioPacket packet)
    {
        //you can codec packet by google.protobuf/protobufNet...(the demo used google.protobuf)
        return ObjectToBytes(packet);
    }

    /// <summary>
    /// send video data
    /// </summary>
    void SendVideo()
    {
        //capture video data by SDK
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

        packet.Id = TestUid;//use your userID
        byte[] video = GetVideoPacketData(packet);
        SendDataByYourNetwork(video);

        //On receiving video data,just for testing
        ReceivedVideoDataQueue.Enqueue(video);
    }
    byte[] GetVideoPacketData(VideoPacket packet) 
    {
        //you can codec packet by google.protobuf/protobufNet...(the demo used google.protobuf)
        return ObjectToBytes(packet);
    }
    void SendDataByYourNetwork(byte[] data)
    {
        //you need to do
    }
    //==================onReceive data========================
    /// <summary>
    /// called when audio data is received
    /// </summary>
    /// <param name="data"></param>
    public void OnReceiveAudio(byte[] data) 
    {
        //decode audio data and playback
        AudioPacket packet = DecodeAudioPacket(data);
        UnityChatSDK.Instance.DecodeAudioData(packet);
    }
    AudioPacket DecodeAudioPacket(byte[] data)
    {
        //decode bytes to packet
        return BytesToObject<AudioPacket>(data);
    }

    /// <summary>
    /// called when video data is received
    /// </summary>
    /// <param name="data"></param>
    public void OnReceiveVideo(byte[] data) 
    {
        //decode video data and render video
        VideoPacket packet = DecodeVideoPacket(data);
        UnityChatSDK.Instance.DecodeVideoData(packet);
    }
    VideoPacket DecodeVideoPacket(byte[] data)
    {
        //decode bytes to packet
        return BytesToObject<VideoPacket>(data);
    }

    /// <summary>
    /// data serialization
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    /// <returns></returns>
    public static byte[] ObjectToBytes<T>(T t)
    {
        if (t == null) return null;
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream stream = new MemoryStream();
        formatter.Serialize(stream, t);
        return stream.ToArray();
    }
    /// <summary>
    /// data deserialization
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    public static T BytesToObject<T>(byte[] data)
    {
        if (data == null) return default(T);
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream stream = new MemoryStream(data);
        return (T)formatter.Deserialize(stream);
    }

}
