using ChatProto;
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
                PeerCall(pdm);
                break;
            case IMProtocol.IM_STATE:
                CallResult(pdm);
                break;
            case IMProtocol.IM_ACCEPT_SRES:
                PeerAccept(pdm);
                break;
            case IMProtocol.IM_HANG_SRES:
                PeerHang(pdm);
                break;
            case IMProtocol.IM_SENMESSAGE_SRES:
                PeerMessage(pdm);
                break;
            default:
                break;
        }
    }

    private void PeerCall(ProtocolDataModel pdm)
    {
        IMInfo info = IMInfo.Parser.ParseFrom(pdm.Message);
        ChatManager.Instance.InviteCome = true;
        ChatManager.Instance.ChatPeerName = info.UserName;
        ChatManager.Instance.ChatPeerID= info.UserID;
        ChatManager.Instance.CallID = info.CallID;

        int type = info.CallType;
        ChatDataHandler.Instance.ChatType = (ChatType)type;

        print("Receive call：" + info.UserName+",CallID:" + info.CallID);
    }

    private void PeerMessage(ProtocolDataModel pdm)
    {
        print("Receive message：" + pdm.Message.Length);
    }

    private void PeerHang(ProtocolDataModel pdm)
    {
        print("Peer Hang");
        ChatUIManager.Instance.Hang();
    }

    private void PeerAccept(ProtocolDataModel pdm)
    {
        print("Peer Accept");
        ChatManager.Instance.UserComeIn = true;
    }

    private void CallResult(ProtocolDataModel pdm)
    {
        ResultCode code = (ResultCode)(BitConverter.ToInt32(pdm.Message, 0));
        print("Call Result：" + code.ToString());
        MessageManager._instance.ShowMessage("Call Result：" + code.ToString());
        ChatUIManager.Instance.CallResult(code== ResultCode.RESULT_ONLINE);
    }
}
