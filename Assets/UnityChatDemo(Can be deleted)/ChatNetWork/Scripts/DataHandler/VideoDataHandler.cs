using ChatNetWork;
using ChatProto;
using ChatProtocol;
using System;
using UnityEngine;

/// <summary>
/// chat data handler
/// </summary>
public class VideoDataHandler : MonoBehaviour, IHandler
{
    public void MessageReceive(DataModel model)
    {
        switch (model.Request)
        {
            case ChatDataProtocol.CHAT_NONE:
                break;
            case ChatDataProtocol.CHAT_AUDIO:
                OnReceiveAudio(model);
                break;
            case ChatDataProtocol.CHAT_VIDEO:
                OnReceiveVideo(model);
                break;
            default:
                break;
        }
    }
    void OnReceiveAudio(DataModel model)
    {
        ChatDataHandler.Instance.ReceiveAudio(model.Message);
    }
    void OnReceiveVideo(DataModel model)
    {
        ChatDataHandler.Instance.ReceiveVideo(model.Message);
    }
}
