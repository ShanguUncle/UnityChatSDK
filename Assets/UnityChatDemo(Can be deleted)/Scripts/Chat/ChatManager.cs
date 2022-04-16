using ChatNetWork;
using ChatProto;
using ChatProtocol;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Audio and video chat logic manager
/// </summary>
public class ChatManager : MonoBehaviour {

    public static ChatManager Instance;

    public int UserID { get; set; } 
    public string UserName { get; set; }
    //CallID unique every time
    public long CallID { get; set; }

    //Online UserInfo list
    public List<UserInfo> OnlineUserList { get; set; } = new List<UserInfo>();
    //Chat Users 
    public List<UserInfo> ChatPeers { get; set; } = new List<UserInfo>();

    bool isChatting;

    public delegate void OnChatAccept();
    public OnChatAccept OnChatAcceptgDg;

    public delegate void OnChatHang();
    public OnChatHang OnChatHangDg;

    private void Awake()
    {
        Instance = this;
    }
    void Start () {
        CallID = 0;
    }

    /// <summary>
    /// Update username and uid
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="userId"></param>
    public void Login(string userName,int userId=0)
    {
        LoginInfo info = new LoginInfo();
        info.UserName = userName;
        info.UserID = userId;
        byte[] data = info.ToByteArray();
        DataModel model = new DataModel(ChatProtocolType.TYPE_MYSQL, MySqlDataProtocol.MYSQL_LOGIN, data);
        ChatNetworkManager.Instance.Send(model);
    }
    /// <summary>
    /// Get online userlist
    /// </summary>
    public void GetOnlineUserList()
    {
        DataModel pd = new DataModel();
        pd.Type = ChatProtocolType.TYPE_MYSQL;
        pd.Request = MySqlDataProtocol.MYSQL_ONLINEUSER;

        ChatNetworkManager.Instance.Send(pd);
    }
    internal void OnlineUserChanged()
    {
        for (int i = 0; i < ChatPeers.Count; i++)
        {
            UserInfo info= OnlineUserList.Find((UserInfo user)=> { return user.UserID == ChatPeers[i].UserID; });
            if (info == null) 
            {
                Debug.Log("OnPeerOffline: uid = " + ChatPeers[i].UserID);
                IMInfo im = new IMInfo();
                im.UserID = ChatPeers[i].UserID;
                //User offline, hang up
                OnHang(im);
            }
        }
    }
    public UserInfo GetUserInfoById(int id)
    {
        return OnlineUserList.Find((UserInfo user) => { return user.UserID == id; });
    }
    public void Call(long callID, ChatType type, int userId, List<UserInfo> peerId)
    {
        if (CallID == 0)
        {
            isChatting = true;
            CallID = callID;
            DataModel model = new DataModel();
            model.Type = ChatProtocolType.TYPE_IM;
            model.Request = IMProtocol.IM_CALL;

            IMInfo info = new IMInfo();
            info.CallID = callID;
            info.Type = (int)type;
            info.UserID = userId;
            info.UserList.AddRange(peerId);

            model.Message = info.ToByteArray();
            ChatNetworkManager.Instance.Send(model);
        }
    }

    public void OnCall(IMInfo info)
    {
        isChatting = false;
        ChatPeers.Clear();
        ChatPeers.AddRange(info.UserList);
        CallID = info.CallID;
        ChatUIManager.Instance.OnPeerCall((ChatType)info.Type);
    }
    /// <summary>
    /// Accept call invitation
    /// </summary>
    public void Accept(int userId, List<UserInfo> peerId)
    {
        DataModel model = new DataModel();
        model.Type = ChatProtocolType.TYPE_IM;
        model.Request = IMProtocol.IM_ACCEPT;

        IMInfo info = new IMInfo();
        info.CallID = CallID;
        info.UserID = userId;
        info.UserList.AddRange(peerId);

        model.Message = info.ToByteArray();

        ChatNetworkManager.Instance.Send(model);

        //Start udp transmission
        ChatDataHandler.Instance.StartChat();

        OnChatAcceptgDg?.Invoke();
    }

    //on user accept the call
    public void OnAccpet(IMInfo info)
    {
        if (isChatting)
        {
            ChatUIManager.Instance.OnPeerAccept();
            ChatUIManager.Instance.OnPeerJoin(info.UserID);
            OnChatAcceptgDg?.Invoke();
        }
    }

    /// <summary>
    /// Hang up call
    /// </summary>
    public void Hang()
    {
        if (CallID != 0)
        {
            DataModel model = new DataModel();
            model.Type = ChatProtocolType.TYPE_IM;
            model.Request = IMProtocol.IM_HANG;

            IMInfo info = new IMInfo();
            info.UserID = UserID;
            info.CallID = CallID;
            info.UserList.AddRange(ChatPeers);

            model.Message = info.ToByteArray();

            ChatNetworkManager.Instance.Send(model);

            //send udp hang
            UdplDataModel udp = new UdplDataModel();
            udp.Request = UdpRequest.REQUEST_HANG;
            udp.ChatInfoData = model.Message;
            byte[] udpData = UdpMessageCodec.Encode(udp);

            UdpSocketManager.Instance.Send(udpData);

            //stop udp
            ChatDataHandler.Instance.StopChat();

            CallID = 0;
            ChatPeers.Clear();
            isChatting = false;

            OnChatHangDg?.Invoke();
        }
    }

    internal void OnHang(IMInfo info)
    {
        //on user hangs up, remove the video panel
        UserInfo user = ChatPeers.Find((UserInfo u) => { return u.UserID == info.UserID; });
        if (user != null)
        {
            print("user leave:" + user.UserID + "," + user.UserName);
            ChatPeers.Remove(user);
            ChatUIManager.Instance.OnUserLeave(user.UserID);
        }
        //if the current number of calls ==1,just left yourself
        if (ChatPeers.Count == 1)
        {
            ChatUIManager.Instance.Hang();
            OnChatHangDg?.Invoke();
        }
    }

    public void SendMessageToPeers(int userId, int type, byte[] data, List<int> peerId)
    {
        DataModel model = new DataModel();
        model.Type = ChatProtocolType.TYPE_MESSAGE;
        model.Request = SendMessageProtocol.MESSAGE_SEND_SOME;

        MessageInfo info = new MessageInfo();
        info.UserID = userId;
        info.Type = type;
        info.PeerList.AddRange(peerId);
        info.MessageData = ByteString.CopyFrom(data);
        model.Message = info.ToByteArray();
        ChatNetworkManager.Instance.Send(model);
    }

}
