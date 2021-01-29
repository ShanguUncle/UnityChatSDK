using ChatNetWork;
using ChatProto;
using ChatProtocol;
using System;
using UnityEngine;

/// <summary>
/// chat send message handler
/// </summary>
public class MessageHandler : MonoBehaviour, IHandler
{
    public void MessageReceive(DataModel model)
    {
        switch (model.Request)
        {
            case SendMessageProtocol.MESSAGE_SEND_ALL:
            case SendMessageProtocol.MESSAGE_SEND_OTHER:
            case SendMessageProtocol.MESSAGE_SEND_SOME:
                MessageInfo message = MessageInfo.Parser.ParseFrom(model.Message);
                SendChatMessageManager.Instance?.OnMessage(message);
                break;
            default:
                break;
        }
    }
}
