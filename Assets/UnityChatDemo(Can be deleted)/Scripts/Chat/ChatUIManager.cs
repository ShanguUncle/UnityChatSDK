using ChatProto;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Audio and video chat ui manager
/// </summary>
public class ChatUIManager : MonoBehaviour {

    public static ChatUIManager Instance;

    public GameObject InvitePanel;
    public GameObject CallPanel;
    public GameObject ChatPanel;
    public GameObject SendChatMessagePanel;
    public Transform ChatPeersContent;
    public GameObject PeerImagePrefab;

    public Text CallFriendText;
    public Text InviteFriendTetx;
    public Text ChatFriendText;

    public Texture2D DefultBlack;

    public VideoTexure SelectedPeerVideo;

    private void Awake()
    {
        Instance = this;
    }
    void Start () {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        UnityChatSDK.Instance.OnPeerJoin += OnPeerJoin;
    }

    List<GameObject> peerImageList = new List<GameObject>();
    public void OnPeerJoin(int uid)
    {
        if (ChatManager.Instance.CallID == 0) return;

        Debug.Log("OnPeerJoin: uid = " + uid );
        GameObject go = peerImageList.Find((GameObject obj)=> { return obj.name == uid.ToString(); });
        if (go!=null) return;

        GameObject peer = Instantiate(PeerImagePrefab, ChatPeersContent);
        Toggle tog = peer.GetComponent<Toggle>();
        tog.group = ChatPeersContent.GetComponent<ToggleGroup>();

        peer.name = uid.ToString();
        VideoTexure video = peer.transform.Find("RawImage").gameObject.GetComponent<VideoTexure>();
        video.ID = uid;
        peerImageList.Add(peer);

        if (peerImageList.Count == 1)
        {
            SelectedPeerVideo.ID = uid;
            tog.isOn = true;
        }
    }

    public void OnUserLeave(int uid)
    {
        Debug.Log("OnUserLeave: uid = " + uid );
        GameObject go = peerImageList.Find((GameObject obj) => { return obj.name == uid.ToString(); });
        if (go!= null)
        {
            peerImageList.Remove(go);
            Destroy(go);
        }
    }

    void Update () {

    }
    public void OnPeerAccept()
    {
        InvitePanel.SetActive(false);
        CallPanel.SetActive(false);
        ChatPanel.SetActive(true);

        //start joining the chat
        if (ChatManager.Instance.ChatPeers.Count == 2)
        {
            UserInfo info = ChatManager.Instance.ChatPeers.Find((UserInfo u) => { return u.UserID == ChatManager.Instance.ChatPeers[1].UserID; });
            ChatFriendText.text = "In call:" + info.UserName;
        }
        else if (ChatManager.Instance.ChatPeers.Count > 2)
        {
            string group = "";
            for (int i = 0; i < ChatManager.Instance.ChatPeers.Count; i++)
            {
                group += ChatManager.Instance.ChatPeers[i].UserName + ",";
            }
            ChatFriendText.text = "In group call(" + ChatManager.Instance.ChatPeers.Count + "):" + group;
        }
        //start udp transmission
        ChatDataHandler.Instance.StartChat();
    }
    public void OnPeerCall(ChatType type)
    {
        CallPanel.SetActive(false);
        InvitePanel.SetActive(true);
        InviteFriendTetx.text = type == ChatType.Audio ? "Voice\n" : "Video\n";
        UnityChatSDK.Instance.ChatType = type;

        if (ChatManager.Instance.ChatPeers.Count == 2)
        {
            UserInfo info = ChatManager.Instance.ChatPeers.Find((UserInfo u) => { return u.UserID == ChatManager.Instance.ChatPeers[0].UserID; });
            InviteFriendTetx.text += "Call invitation:" + info.UserName;
        }
        else if (ChatManager.Instance.ChatPeers.Count > 2)
        {
            string group = "";
            for (int i = 0; i < ChatManager.Instance.ChatPeers.Count; i++)
            {
                group += ChatManager.Instance.ChatPeers[i].UserName + ",";
            }
            InviteFriendTetx.text += "Group call invitation(" + ChatManager.Instance.ChatPeers.Count + "):" + group;
        }
        SoundManager._instance.PlayEffect("Call");
    }
    public void VoiceCall()
    {
        Call(ChatType.Audio);
    }
    public void VideoCall() 
    {
        Call(ChatType.Video);
    }
    public void SendChatMessage() 
    {
        if (MainUIManager.Instance.SelectedFriendList.Count == 0 && ChatManager.Instance.ChatPeers.Count == 0)
        {
            MessageManager.Instance.ShowMessage("please select a user!");
            return;
        }
        SendChatMessagePanel.SetActive(true);
    }
    void Call(ChatType type)
    {
        if (MainUIManager.Instance.SelectedFriendList.Count == 0)
        {
            MessageManager.Instance.ShowMessage("please select a user!");
            return;
        }
        print("Call:" + type);
        UnityChatSDK.Instance.ChatType = type;
        ChatManager.Instance.ChatPeers.Clear();
        ChatManager.Instance.ChatPeers.Add(MainUIManager.Instance.UserInfo);
        for (int i = 0; i < MainUIManager.Instance.SelectedFriendList.Count; i++)
        {
            ChatManager.Instance.ChatPeers.Add(MainUIManager.Instance.SelectedFriendList[i]);
        }
        long callID = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);
        print("callID:" + callID);
        ChatManager.Instance.Call(callID, type, ChatManager.Instance.UserID, ChatManager.Instance.ChatPeers);

        CallPanel.SetActive(true);
        if (ChatManager.Instance.ChatPeers.Count == 2)
        {
            UserInfo info = ChatManager.Instance.ChatPeers.Find((UserInfo u) => { return u.UserID == ChatManager.Instance.ChatPeers[1].UserID; });
            CallFriendText.text = type == ChatType.Audio ? "Voice\n" : "Video\n";
            CallFriendText.text += "Calling:" + info.UserName;
        }
        else if (ChatManager.Instance.ChatPeers.Count > 2)
        {
            string group = "";
            for (int i = 0; i < ChatManager.Instance.ChatPeers.Count; i++)
            {
                group += ChatManager.Instance.ChatPeers[i].UserName + ",";
            }
            CallFriendText.text = "Calling group(" + ChatManager.Instance.ChatPeers.Count + "):" + group;
        }

        if (ChatManager.Instance.ChatPeers.Count <= 2)
        {
            ChatPeersContent.transform.localScale = Vector3.zero;
        }
        else { ChatPeersContent.transform.localScale = Vector3.one; } 
    }

    /// <summary>
    /// Accept call
    /// </summary>
    public void Accept()
    {
        ChatManager.Instance.Accept(ChatManager.Instance.UserID, ChatManager.Instance.ChatPeers);
        InvitePanel.SetActive(false);
        ChatPanel.SetActive(true);

        if (ChatManager.Instance.ChatPeers.Count == 2)
        {
            UserInfo info = ChatManager.Instance.ChatPeers.Find((UserInfo u) => { return u.UserID == ChatManager.Instance.ChatPeers[0].UserID; });
            ChatFriendText.text = "In call:" + info.UserName;
        }
        else if (ChatManager.Instance.ChatPeers.Count > 2)
        {
            string group = "";
            for (int i = 0; i < ChatManager.Instance.ChatPeers.Count; i++)
            {
                group += ChatManager.Instance.ChatPeers[i].UserName + ",";
            }
            ChatFriendText.text = "In group call(" + ChatManager.Instance.ChatPeers.Count + "):" + group;
        }

        if (ChatManager.Instance.ChatPeers.Count <= 2)
        {
            ChatPeersContent.transform.localScale = Vector3.zero;
        }
        else { ChatPeersContent.transform.localScale = Vector3.one; }
    }
    /// <summary>
    /// Hang up call
    /// </summary>
    public void Hang() 
    {
        foreach (Transform child in ChatPeersContent)
        {
            Destroy(child.gameObject);
        }
        peerImageList.Clear();

        InvitePanel.SetActive(false);
        CallPanel.SetActive(false);
        ChatPanel.SetActive(false);

        ChatManager.Instance.Hang();
        SoundManager._instance.PlayEffect("Hang");
    }
}
