using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Demo界面
/// </summary>
public class MainUIManager : MonoBehaviour {

    public static MainUIManager _instance;

    public Transform AddressContent;
    public GameObject FriendItemPrefab;

    private void Awake()
    {
        _instance = this;
    }

    void Start () {
		
	}
	
	void Update () {
		

	}
    private void FixedUpdate()
    {
        if (ChatManager.Instance.UserlistUpdate)
        {
            ChatManager.Instance.UserlistUpdate = false;
            //更新通讯录
            UpdateUserList();
        }

    }
    /// <summary>
    /// 登陆(deom登录随便设置用户名和密码即可，此版本服务器不做数据库验证)
    /// </summary>
    public void Login()
    {
        ChatManager.Instance.Login(SystemInfo.deviceName);
    }
    /// <summary>
    /// 请求在线用户
    /// </summary>
    public void  GetOnlineUserList()
    {
        ChatManager.Instance.GetOnlineUserList();
    }
    //更新在线用户列表
    void UpdateUserList()
    {
        foreach (var child in AddressContent.GetComponentsInChildren<FriendItem>())  
        {
            DestroyImmediate(child.gameObject); 
        }
        foreach (var list in ChatManager.Instance.OnlineUserList) 
        { 
            GameObject go = Instantiate(FriendItemPrefab, AddressContent);
            FriendItem item = go.GetComponent<FriendItem>();
            item.FriendName = list.Value;
            go.transform.Find("Text").GetComponent<Text>().text= list.Value;
            item.FriendID = list.Key;
        }
    }
    /// <summary>
    /// 登录结果
    /// </summary>
    /// <param name="result"></param>
    public void LoginResult(bool result)
    {
        if (result)
        {
            MessageManager._instance.ShowMessage("login successful!");
        
            //更新用户信息
            print("UserID:" + ChatManager.Instance.UserID+
          ",UserName:" +  ChatManager.Instance.UserName +
           ",UserPortrait:" + ChatManager.Instance.UserPortrait);

            //刷新列表
            GetOnlineUserList();
        }
        else
        {
            MessageManager._instance.ShowMessage("Login failed！");
        }
    }

}
