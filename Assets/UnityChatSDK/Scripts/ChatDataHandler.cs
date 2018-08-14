using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UdpStreamProtocol;

/// <summary>
/// 网络传输列类型，用户可自定义TCP，UDP，P2P,Webrct，Unet,Photon...案例demo使用的是UDP
/// </summary>
public enum NetType{UdpStream,UdpP2P };
/// <summary>
/// 聊天类型音频，视频，音视频
/// </summary>
public enum ChatType {Audio,Video,AV};
public class ChatDataHandler : MonoBehaviour {

	public NetType  netType;
    public ChatType chatType { get; set; }

	public static  ChatDataHandler Instance;
    bool isStartChat;

    void Start() {
		Instance = this;
	}

	/// <summary>
    /// 开始聊天
    /// </summary>
	public void StartChat(  ) {

		switch( netType ) {
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
        switch (netType)
        {
            case NetType.UdpP2P:
                break;
            case NetType.UdpStream:             
                OnStopChat();            
                break;
        }
    }

    //开始聊天后，在FixedUpdate会把捕捉到的音频和视频通过网络传输
    //SDK会判断音视频的刷新率，自动识别声音大小，判断视频画面是否静止，优化数据大小
    //注意：FixedUpdate Time的值需要小于 1/(Framerate+5),一般设置为0.04
    void FixedUpdate() { 
		if( !isStartChat )
			return;

        switch ( netType ) {
			case NetType.UdpStream:
                switch (chatType)
                {
                    case ChatType.AV:
                        SendStream();
                        //SendVideo();
                        //SendAudio();
                        break;
                    case ChatType.Video:
                        SendVideo();
                        break;
                    case ChatType.Audio:
                        SendAudio();
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
        byte[] audio = UnityChatSDK.GetAudio();

        //UDP发送数据到服务器（了更改为自己的服务器发送接口）
        if (audio != null)
            UdpSocketManager._instance.Send(EncodeChatDataID(audio, RequestByte.REQUEST_AUDIO));
    }
    /// <summary>
    /// 发送音视频数据
    /// </summary>
	void SendStream() {
        //获取SDK捕捉的音视频数据
        byte[] stream = UnityChatSDK.GetAv();
        if (stream != null)
        {
            UdpSocketManager._instance.Send(EncodeChatDataID(stream,RequestByte.REQUEST_STREAM));
        }      
	}
    /// <summary>
    /// 发送视频数据
    /// </summary>
	void SendVideo() {
        //获取SDK捕捉的视频数据
        byte[] video = UnityChatSDK.GetVideo();
		if( video != null )
            UdpSocketManager._instance.Send(EncodeChatDataID(video, RequestByte.REQUEST_VIDEO));
    }   
    /// <summary>
    /// 编码ChatData,添加音视频的ChatPeerID和UserID
    /// </summary>
    /// <param name="data">音视频数据</param>
    /// <param name="res">音视类型</param>
    /// <returns></returns>
    byte[] EncodeChatDataID(byte[] data, byte res) 
    {
        byte[] newByte = new byte[data.Length + 8];
        Buffer.BlockCopy(BitConverter.GetBytes(ChatManager._instance.ChatPeerID), 0, newByte, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(ChatManager._instance.UserID), 0, newByte, 4, 4);
        Buffer.BlockCopy(data, 0, newByte, 8, data.Length);

        UdplDataModel model = new UdplDataModel(res, newByte);
        return UdpMessageCodec.encode(model);
    }
    /// <summary>
    /// 解码编码ChatData
    /// </summary>
    /// <param name="data"></param>
    /// <returns>音视频数据</returns>
    byte[] DeCodeChatDatID(byte[] data)  
    { 
        byte[] newByte = new byte[data.Length - 8];
        Buffer.BlockCopy(data,8, newByte, 0, data.Length-8);
        return newByte;
    }
   
    //接收到音视频数据后的回调
    public void ReceiveStreamRemote(byte[] data)
    {
        //SDK进行音视频数据的解码及视频渲染，音频播放
        UnityChatSDK.DecodeAvData(DeCodeChatDatID(data));
    } 

    public void ReceiveAudio(byte[] data)
    {
        //SDK进行音频数据的解码及音频播放
        UnityChatSDK.DecodeAudioData((DeCodeChatDatID(data)));
    }

    public void ReceiveVideo(byte[] data)
    {
        //SDK进行视频数据的解码及视频渲染
        UnityChatSDK.DecodeVideoData((DeCodeChatDatID(data)));
    }

    public void OnStartChat()
    {
        try
        {
            UdpSocketManager._instance.StartListening();
            if (chatType == ChatType.AV || chatType == ChatType.Video)
            {
                UnityChatSDK.Instance.ChatType = 2;
            }
            else
            {
                UnityChatSDK.Instance.ChatType = 1;
            }
            UnityChatSDK.Instance.StartCpture();
            isStartChat = true;
            print("OnStartChat");
        }
        catch (Exception e)
        {
            print("OnStartChat error:" + e.Message);
        }

    }

    public void OnStopChat()
    {
        try
        {
            UdpSocketManager._instance.StopListening();
 
            UnityChatSDK.Instance.StopCpture();
            isStartChat = false;
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
