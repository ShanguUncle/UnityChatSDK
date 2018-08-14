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
        if (ChatManager._instance.UserlistUpdate)
        {
            ChatManager._instance.UserlistUpdate = false;
            //更新通讯录
            UpdateUserList();
        }

    }
    /// <summary>
    /// 登陆(deom登录随便设置用户名和密码即可，此版本服务器不做数据库验证)
    /// </summary>
    public void Login()
    {
        ChatManager._instance.Login("用户名", "密码");
    }
    /// <summary>
    /// 请求在线用户
    /// </summary>
    public void  GetOnlineUserList()
    {
        ChatManager._instance.GetOnlineUserList();
    }
    //更新在线用户列表
    void UpdateUserList()
    {
        foreach (var child in AddressContent.GetComponentsInChildren<FriendItem>())  
        {
            DestroyImmediate(child.gameObject); 
        }
        foreach (var list in ChatManager._instance.OnlineUserList) 
        { 
            GameObject go = Instantiate(FriendItemPrefab, AddressContent);
            FriendItem item = go.GetComponent<FriendItem>();
            item.FriendName = list.Key;
            go.transform.Find("Text").GetComponent<Text>().text= list.Key;
            item.FriendID = list.Value;
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
            MessageManager._instance.ShowMessage("登录成功！");
        
            //更新用户信息
            print("UserID:" + ChatManager._instance.UserID+
          ",UserName:" +  ChatManager._instance.UserName +
           ",UserPortrait:" + ChatManager._instance.UserPortrait);

            //刷新列表
            GetOnlineUserList();
        }
        else
        {
            MessageManager._instance.ShowMessage("登录失败！");
        }
    }

}
