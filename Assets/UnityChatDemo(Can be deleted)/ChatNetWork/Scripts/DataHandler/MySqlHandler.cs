using ChatNetWork;
using ChatProto;
using ChatProtocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// chat mysql handler
/// </summary>
public class MySqlHandler : MonoBehaviour, IHandler
{
    public void MessageReceive(DataModel model)
    {
        switch (model.Request)
        {
            case MySqlDataProtocol.MYSQL_NONE:
                break;
            case MySqlDataProtocol.MYSQL_LOGIN:
                OnLogin(model);
                break;
            case MySqlDataProtocol.MYSQL_ONLINEUSER:
                OnGetOnlineUser(model);
                break;
            default:
                break;
        }
    }
    private void OnLogin(DataModel model) 
    {
        if (model.Message.Length == 4)
        {
            ResultCode result = (ResultCode)(BitConverter.ToInt32(model.Message, 0));
            print("Login result:" + result.ToString());
            MessageManager.Instance.ShowMessage("login result:" + result);
        }
        else
        {
            UserInfo info = UserInfo.Parser.ParseFrom(model.Message);
            MainUIManager.Instance.UserInfo = info;
            ChatManager.Instance.UserID = info.UserID;
            ChatManager.Instance.UserName = info.UserName;
            print("UserID:" + ChatManager.Instance.UserID + ",UserName:" + ChatManager.Instance.UserName);
        }    
    }

    private void OnGetOnlineUser(DataModel model) 
    {
        OnlineUserInfo list = OnlineUserInfo.Parser.ParseFrom(model.Message);
        print("online userList count:" + list.UserList.Count);
        ChatManager.Instance.OnlineUserList.Clear();
        ChatManager.Instance.OnlineUserList.AddRange(list.UserList);
        ChatManager.Instance.OnlineUserChanged();
        MainUIManager.Instance.UpdateUserList();
    }
}
