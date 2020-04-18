using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ChatProto;
using Google.Protobuf;
using NetWorkPlugin;
using Protocol;
using UnityEngine;

public class ChatSendMessageManager : MonoBehaviour {

    public static ChatSendMessageManager Instance;
    public enum MessageType
    {
        Text,
        Pic,
        Voice
    }
    private void Awake()
    {
        Instance = this;
    }
    public delegate void OnReciveMessage(byte[] data);
    public OnReciveMessage OnReciveText;
    public OnReciveMessage OnRecivePic;
    public OnReciveMessage OnReciveVoice;

    public string MessagePeerName;
    internal void OnReceivePeerMessage(ProtocolDataModel pdm)
    {
        IMInfo info = IMInfo.Parser.ParseFrom(pdm.Message);

        MessagePeerName = info.UserName;

        MessageType tyep = (MessageType)info.MessageData[0];
        byte[] data = new byte[info.MessageData.Length-1];
        Buffer.BlockCopy(info.MessageData.ToByteArray(), 1, data,0, data.Length);
        switch (tyep)
        {
            case MessageType.Text:
                OnReciveText?.Invoke(data);
                break;
            case MessageType.Pic:
                OnRecivePic?.Invoke(data);
                break;
            case MessageType.Voice:
                OnReciveVoice?.Invoke(data);
                break;
            default:
                break;
        }
    }
    public void SendPeerMessage(byte[] message, MessageType t) 
    {
        byte[] type = new byte[1] { (byte)t };
        byte[] data = new byte[message.Length + 1];
        Buffer.BlockCopy(type, 0, data, 0, 1);
        Buffer.BlockCopy(message, 0, data, 1, message.Length);

        IMInfo info = new IMInfo();
        info.UserID = ChatManager.Instance.UserID;
        info.PeerID = ChatUIManager.Instance.SelectFriendID;
        info.UserName=ChatManager.Instance.UserName;

        info.MessageData = ByteString.CopyFrom(data);

        ProtocolDataModel pd = new ProtocolDataModel();
        pd.Type = ProtocolType.TYPE_IM;
        pd.Request = IMProtocol.IM_SENMESSAGE_CRES;
        pd.Message = info.ToByteArray();

        NetWorkManager.Instance.Send(pd);
    }
}
