using ChatProto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Demo UI manager
/// </summary>
public class MainUIManager : MonoBehaviour
{

    public static MainUIManager Instance;

    public Transform FriendContent;
    public GameObject FriendItemPrefab;
    public InputField UsernamInputField;


    public UserInfo UserInfo { get; internal set; }
    public List<UserInfo> SelectedFriendList = new List<UserInfo>();

    public Toggle AllFriendToggle;
    List<Toggle> FriendToggleList = new List<Toggle>();

    public Text DelayText;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        UpdateUserList();
        ChatNetworkManager.Instance.OnDisconnectAction += OnDisconnectAction;
        ChatNetworkManager.Instance.OnConnectResultAction += OnConnectResultAction;
        UsernamInputField.text = SystemInfo.deviceName;
    }

    private void OnConnectResultAction(bool result)
    {
        MessageManager.Instance.ShowMessage("Connect server:" + result);
        Config.Instance.NetPanl.SetActive(!result);
        //login info
        ChatManager.Instance.Login(SystemInfo.deviceName);
    }

    private void OnDisconnectAction()
    {
        MessageManager.Instance.ShowMessage("Server disconnect!");
        ChatManager.Instance.OnlineUserList.Clear();
        UpdateUserList();
        Config.Instance.NetPanl.SetActive(true);
    }
    void Update()
    {
        DelayText.text = ChatNetworkManager.Instance.GetDelayMS == -1 ? "": ChatNetworkManager.Instance.GetDelayMS+"ms";

    }
    /// <summary>
    /// deom login just upload the user name and uid(If you don’t set uid, the server will set automatically)
    /// </summary>
    public void Login()
    {
        if (string.IsNullOrEmpty(UsernamInputField.text))
        {
            MessageManager.Instance.ShowMessage("please input username!");
            return;
        }
        ChatManager.Instance.Login(UsernamInputField.text);
    }
    /// <summary>
    /// Get the list of online users
    /// </summary>
    public void GetOnlineUserList()
    {
        ChatManager.Instance.GetOnlineUserList();
    }
    //Update online user list
    public void UpdateUserList()
    {
        foreach (var child in FriendContent.GetComponentsInChildren<FriendItem>())
        {
            DestroyImmediate(child.gameObject);
        }
        FriendToggleList.Clear();
        SelectedFriendList.Clear();
        for (int i = 0; i < ChatManager.Instance.OnlineUserList.Count; i++)
        {
            if (!useSelfTest && ChatManager.Instance.OnlineUserList[i].UserID == UserInfo.UserID) continue;
            GameObject go = Instantiate(FriendItemPrefab, FriendContent);
            FriendToggleList.Add(go.GetComponent<Toggle>());
            FriendItem item = go.GetComponent<FriendItem>();
            item.UserInfo = ChatManager.Instance.OnlineUserList[i];
            item.FriendID = ChatManager.Instance.OnlineUserList[i].UserID;
            item.FriendName = ChatManager.Instance.OnlineUserList[i].UserName;
            go.transform.Find("Text").GetComponent<Text>().text = item.FriendName;
        }
        AllFriendToggle.isOn = false;
    }

    public void OnAllFriendToggleChanged()
    {
        for (int i = 0; i < FriendToggleList.Count; i++)
        {
            FriendToggleList[i].isOn = AllFriendToggle.isOn;
        }
    }

    bool useSelfTest; 
    public void OnSelfTestToggleChanged(Toggle tog)
    {
        useSelfTest = tog.isOn;
        UpdateUserList();
    }

    public void SendMessage(int type,byte[]data)
    {
        if (SelectedFriendList.Count == 0 && ChatManager.Instance.ChatPeers.Count == 0)
        {
            MessageManager.Instance.ShowMessage("please select a user!");
            return;
        }

        List<int> ids = new List<int>();

        if (!ChatUIManager.Instance.ChatPanel.activeInHierarchy)
        {
            for (int i = 0; i < SelectedFriendList.Count; i++)
            {
                ids.Add(SelectedFriendList[i].UserID);
            }
        }
        else if (ChatManager.Instance.ChatPeers.Count > 0) 
        {
            for (int i = 0; i < ChatManager.Instance.ChatPeers.Count; i++)
            {
                //if(ChatManager.Instance.ChatPeers[i].UserID!= ChatManager.Instance.UserID)
                ids.Add(ChatManager.Instance.ChatPeers[i].UserID);
            }
        }

        ChatManager.Instance.SendMessageToPeers(ChatManager.Instance.UserID, type, data, ids);
    }

}