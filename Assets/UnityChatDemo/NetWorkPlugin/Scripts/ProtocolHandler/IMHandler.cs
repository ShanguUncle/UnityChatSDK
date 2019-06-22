using ChatProto.Proto;
using NetWorkPlugin;
using Protocol;
using System;
using UnityEngine;

/// <summary>
/// 通讯数据处理类
/// </summary>
public class IMHandler : MonoBehaviour, IHandler
{
    public void MessageReceive(ProtocolDataModel pdm)
    {
        switch (pdm.Request)
        {
            case IMProtocol.IM_NONE:
                break;
            case IMProtocol.IM_CALL_SRE:
                peerCall(pdm);
                break;
            case IMProtocol.IM_STATE:
                callResult(pdm);
                break;
            case IMProtocol.IM_ACCEPT_SRES:
                peerAccept(pdm);
                break;
            case IMProtocol.IM_HANG_SRES:
                peerHang(pdm);
                break;
            case IMProtocol.IM_SENMESSAGE_SRES:
                peerMessage(pdm);
                break;
            default:
                print("非法IMProtocol:" + pdm.Request);
                break;
        }
    }

    private void peerCall(ProtocolDataModel pdm)
    {
        IMInfo info = IMInfo.Parser.ParseFrom(pdm.Message);
        ChatManager._instance.InviteCome = true;
        ChatManager._instance.ChatPeerName = info.UserName;
        ChatManager._instance.ChatPeerID= info.UserID;
        ChatManager._instance.CallID = info.CallID;

        int type = info.CallType;
        ChatDataHandler.Instance.ChatType = (ChatType)type;

        print("收到通话邀请：" + info.UserName+",CallID:" + info.CallID);
    }

    private void peerMessage(ProtocolDataModel pdm)
    {
        print("收到消息："+pdm.Message.Length);
    }

    private void peerHang(ProtocolDataModel pdm)
    {
        print("对方挂断");
        ChatUIManager._instance.Hang();
    }

    private void peerAccept(ProtocolDataModel pdm)
    {
        print("对方接听");
        ChatManager._instance.UserComeIn = true;
        //开始udp传输
    }

    private void callResult(ProtocolDataModel pdm)
    {
        ResultCode code = (ResultCode)(BitConverter.ToInt32(pdm.Message, 0));
        print("呼叫结果："+ code.ToString());
        MessageManager._instance.ShowMessage("呼叫结果：" + code.ToString());
        ChatUIManager._instance.CallResult(code== ResultCode.RESULT_ONLINE);
    }
}
