using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 音视频通讯界面
/// </summary>
public class ChatUIManager : MonoBehaviour {

    public static ChatUIManager _instance;
    public GameObject InvitePanl;
    public GameObject CallPanl;

    public GameObject ChatPanl;

    public GameObject SelectFrendPanl;
    public Text SelectFriendTetx;
    public Text CallFriendTetx;
    public Text InviteFriendTetx;
    public string SelectFriendName { get; set; }
    public int SelectFriendID{ get; set; }

    public Texture2D DefultBlack;
    public RawImage StreamDisplay;

    public void ShowSelectFriend(string username,int userID) 
    {
        SelectFriendName = username;
        SelectFriendID = userID;
        SelectFriendTetx.text = username;
        SelectFrendPanl.SetActive(true);
    }

    private void Awake()
    {
        _instance = this;
    }
    void Start () {
		
	}
	
	void Update () {
        if (ChatManager._instance.InviteCome)
        {
            ChatManager._instance.InviteCome = false;
            //收到通话邀请
            peerInvite();
        }
        if (ChatManager._instance.UserComeIn)
        {
            ChatManager._instance.UserComeIn = false;
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
        InviteFriendTetx.text = ChatManager._instance.ChatPeerName;
        SoundManager._instance.PlayEffect("Call");
    }
    public void VoiceCall()
    {
        Call(1, SelectFriendID);
        print("语音通话");
    }
    public void VideoCall() 
    {
        print("视频通话");
        Call(2, SelectFriendID);
    }

    void Call(int type, int peer)
    {
        string callID = Guid.NewGuid().ToString();
        print("callID:" + callID);
        ChatManager._instance.Call(callID,type, ChatManager._instance.UserID,peer);
    }
    public void CallResult(bool online)
    {
        if (online)
        {
            SoundManager._instance.PlayEffect("Call");
            CallFriendTetx.text = SelectFriendName;
            CallPanl.SetActive(true);
        }
        else
        {
            ChatManager._instance.CallID = "";
            MessageManager._instance.ShowMessage("用户不在线！");
        }
    }
    /// <summary>
    /// 接听
    /// </summary>
    public void Accept()
    {
        InvitePanl.SetActive(false);
        ChatPanl.SetActive(true);
        ChatManager._instance.Accept();
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
        ChatManager._instance.Hang();
        SoundManager._instance.PlayEffect("Hang");

        StreamDisplay.texture = DefultBlack;
    }
}
