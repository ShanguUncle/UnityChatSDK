using ChatNetWork;
using ChatProto;
using ChatProtocol;
using System;
using UnityEngine;

/// <summary>
/// chat data handler
/// </summary>
public class IMHandler : MonoBehaviour, IHandler
{
    public void MessageReceive(DataModel model)
    {
        switch (model.Request)
        {
            case IMProtocol.IM_NONE:
                break;
            case IMProtocol.IM_CALL:
                OnCall(model);
                break;
            case IMProtocol.IM_ACCEPT:
                OnAccpet(model);
                break;
            case IMProtocol.IM_HANG:
                OnHang(model);
                break;
            default:
                break;
        }
    }
    void OnCall(DataModel model)
    {
        IMInfo info = IMInfo.Parser.ParseFrom(model.Message);
        print("receive call:" + info.UserID);
        ChatManager.Instance.OnCall(info);
    }
    void OnAccpet(DataModel model)
    {
        IMInfo info = IMInfo.Parser.ParseFrom(model.Message);
        print("receive accept:" + info.UserID);
        ChatManager.Instance.OnAccpet(info);
    }
    void OnHang(DataModel model)
    {
        IMInfo info = IMInfo.Parser.ParseFrom(model.Message);
        print("receive hang:" + info.UserID);
        ChatManager.Instance.OnHang(info);
    }
}
