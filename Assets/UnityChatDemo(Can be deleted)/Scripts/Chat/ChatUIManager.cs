using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 音视频通讯界面
/// </summary>
public class ChatUIManager : MonoBehaviour {

    public static ChatUIManager Instance;
    public GameObject InvitePanl;
    public GameObject CallPanl;

    public GameObject ChatPanl;

    public GameObject SelectFrendPanl;
    public Text SelectFriendTetx;
    public Text CallFriendTetx;
    public Text InviteFriendTetx;

    public Texture2D DefultBlack;
    public RawImage StreamDisplay;

    public void ShowSelectFriend(string username,int userID) 
    {
        ChatManager.Instance.ChatPeerName = username;
        ChatManager.Instance.ChatPeerID= userID;
        SelectFriendTetx.text = username;
        SelectFrendPanl.SetActive(true);
    }

    private void Awake()
    {
        Instance = this;
    }
    void Start () {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}
	
	void Update () {
        if (ChatManager.Instance.InviteCome)
        {
            ChatManager.Instance.InviteCome = false;
            //收到通话邀请
            peerInvite();
        }
        if (ChatManager.Instance.UserComeIn)
        {
            ChatManager.Instance.UserComeIn = false;
            //接通，开始传输、计时
            peerAccept();
        }
    }
    void peerAccept()
    {
        InvitePanl.SetActive(false);
        CallPanl.SetActive(false);
        ChatPanl.SetActive(true);

        TimeKeeping._instance.StartTime();
        //开始udp传输
        ChatDataHandler.Instance.StartChat();
    }
    void peerInvite()
    {
        CallPanl.SetActive(false);
        InvitePanl.SetActive(true);
        InviteFriendTetx.text = ChatManager.Instance.ChatPeerName;
        SoundManager._instance.PlayEffect("Call");
    }
    public void VoiceCall()
    {
        Call(ChatType.Audio, ChatManager.Instance.ChatPeerID);
        print("VoiceCall");
    }
    public void VideoCall() 
    {
        print("VideoCall");
        Call(ChatType.AV, ChatManager.Instance.ChatPeerID);
    }

    void Call(ChatType type, int peer)
    {
        long callID = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);
        print("callID:" + callID);
        ChatManager.Instance.Call(callID,type, ChatManager.Instance.UserID,peer);

        UnityChatSDK.Instance.AddChatPeer(peer,FindObjectOfType<UnityChatSet>().ChatPeerRawImage[0]);
    }
    public void CallResult(bool online)
    {
        print("CallResult online:"+ online);
        if (online)
        {
            SoundManager._instance.PlayEffect("Call");
            CallFriendTetx.text = ChatManager.Instance.ChatPeerName;
            CallPanl.SetActive(true);
        }
        else
        {
            ChatManager.Instance.CallID = 0;
            MessageManager._instance.ShowMessage("user is not on line!");
        }
    }
    /// <summary>
    /// 接听
    /// </summary>
    public void Accept()
    {
        UnityChatSDK.Instance.AddChatPeer(ChatManager.Instance.ChatPeerID, FindObjectOfType<UnityChatSet>().ChatPeerRawImage[0]);
        InvitePanl.SetActive(false);
        ChatPanl.SetActive(true);
        ChatManager.Instance.Accept();
        TimeKeeping._instance.StartTime();
    }
    /// <summary>
    /// 挂断
    /// </summary>
    public void Hang() 
    {   
        InvitePanl.SetActive(false);
        CallPanl.SetActive(false);
        ChatPanl.SetActive(false);

        TimeKeeping._instance.StopTime();
        ChatManager.Instance.Hang();
        SoundManager._instance.PlayEffect("Hang");

        if(StreamDisplay!=null)
        StreamDisplay.texture = DefultBlack;

        UnityChatSDK.Instance.ClearChatPeer();
    }
}
