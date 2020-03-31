using ChatProto;
using Google.Protobuf;
using NetWorkPlugin;
using Protocol;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UdpStreamProtocol;
using UnityEngine;

/// <summary>
/// 音视频通讯数据接口
/// </summary>
public class ChatManager : MonoBehaviour {

    public static ChatManager Instance;

    //用户ID
    public int UserID { get; set; } 
    //用户名
    public string UserName { get; set; }
    //用户头像url地址
    public string UserPortrait { get; set; } 
    //呼叫ID
    public long CallID { get; set; }
    //呼叫请求
    public bool InviteCome { get; set; }
    //聊天对象名字
    public string ChatPeerName { get; set; } 
    //聊天对象ID
    public int ChatPeerID { get; set; }
    //用户列表刷新
    public bool UserlistUpdate { get; set; }
    //聊天对象接听
    public bool UserComeIn { get; set; } 
    //在线用户列表(用户名，用户ID)
    public Dictionary<int,string>  OnlineUserList { get; set; }


    private void Awake()
    {
        Instance = this;
    }
    void Start () {
        CallID = 0;
        OnlineUserList =new Dictionary<int, string>();
    }
    /// <summary>
    /// /登录
    /// </summary>
    /// <param name="account">账号</param>
    /// <param name="password">密码</param>
    public void Login(string userName,int userId=0)
    {
        ProtocolDataModel pd = new ProtocolDataModel();
        pd.Type = ProtocolType.TYPE_MYSQL;
        pd.Request = MySqlDataProtocol.MYSQL_LOGIN_CRES;

        LoginInfo info = new LoginInfo();
        info.UserName = userName;
        info.UserID = userId;
        pd.Message = info.ToByteArray();

        NetWorkManager.Instance.Send(pd);

    }
    /// <summary>
    /// 获取在线列表
    /// </summary>
    public void GetOnlineUserList()
    {
        ProtocolDataModel pd = new ProtocolDataModel();
        pd.Type = ProtocolType.TYPE_MYSQL;
        pd.Request = MySqlDataProtocol.MYSQL_ONLINEUSER_CREQ;

        NetWorkManager.Instance.Send(pd);
    }
    /// <summary>
    /// 呼叫
    /// </summary>
    /// <param name="callID">呼叫ID</param>
    /// <param name="type">呼叫类型 1：音频 2：视频</param>
    /// <param name="from">呼叫者 ID</param>
    /// <param name="to">被呼叫者ID </param>
    public void Call(long callID, ChatType type,int from,int to)
    {
        if (CallID == 0)
        {
            CallID = callID;
            ProtocolDataModel pd = new ProtocolDataModel();
            pd.Type = ProtocolType.TYPE_IM;
            pd.Request = IMProtocol.IM_CALL_CRE;

            IMInfo info = new IMInfo();
            info.UserName = UserName;
            info.UserID = UserID;
            info.CallID = callID;
            info.CallType = (int)type;

            ChatDataHandler.Instance.ChatType = type;

            info.PeerID = to;
            ChatPeerID = to;
            pd.Message = info.ToByteArray();
            NetWorkManager.Instance.Send(pd);       
        }
    
    }
    /// <summary>
    /// 挂断
    /// </summary>
    public void Hang() 
    {
        if (CallID != 0)
        {
            ProtocolDataModel pd = new ProtocolDataModel();
            pd.Type = ProtocolType.TYPE_IM;
            pd.Request = IMProtocol.IM_HANG_CRES;

            IMInfo info = new IMInfo();
            info.PeerID = ChatPeerID;
            pd.Message = info.ToByteArray();

            NetWorkManager.Instance.Send(pd);

            //send udp hang
            CallInfo callInfo = new CallInfo();
            callInfo.UserID = UserID;
            callInfo.CallID = CallID;

            UdplDataModel model = new UdplDataModel();
            model.Request = RequestByte.REQUEST_HANG;
            model.ChatInfoData = callInfo.ToByteArray();
            byte[] data = UdpMessageCodec.Encode(model);

            UdpSocketManager.Instance.Send(UdpMessageCodec.Encode(model));

            //结束udp传输
            ChatDataHandler.Instance.StopChat();

            CallID = 0;
        }
    }
    /// <summary>
    /// 接听
    /// </summary>
    public void Accept()
    {
        ProtocolDataModel pd = new ProtocolDataModel();
        pd.Type = ProtocolType.TYPE_IM;
        pd.Request = IMProtocol.IM_ACCEPT_CRES;

        IMInfo info = new IMInfo();
        info.UserName = UserName;
        info.UserID = UserID;
        info.PeerID = ChatPeerID;
        //info.CallType = type;
        pd.Message = info.ToByteArray();
        NetWorkManager.Instance.Send(pd);

        //开始udp传输
        ChatDataHandler.Instance.StartChat();
    }

}
