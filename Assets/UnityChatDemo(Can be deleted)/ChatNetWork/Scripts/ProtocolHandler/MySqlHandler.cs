using ChatProto;
using NetWorkPlugin;
using Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 数据库操作处理类
/// </summary>
public class MySqlHandler : MonoBehaviour, IHandler
{
    public void MessageReceive(ProtocolDataModel pdm)
    {
        switch (pdm.Request)
        {
            case MySqlDataProtocol.MYSQL_NONE:
                break;
            case MySqlDataProtocol.MYSQL_LOGIN_SRES:
                Login(pdm);
                break;
            case MySqlDataProtocol.MYSQL_ONLINEUSE_SRES:
                GetOnlineUser(pdm);
                break;
            default:
                break;
        }
    }
    private void Login(ProtocolDataModel pdm) 
    {
        if (pdm.Message.Length == 4)
        {
            ResultCode result = (ResultCode)(BitConverter.ToInt32(pdm.Message, 0));
            print("Login result:" + result.ToString());

            bool res = result == ResultCode.RESULT_SUCCESS ? true : false;
            MainUIManager._instance.LoginResult(res);
        }
        else
        {
            UserInfo user = UserInfo.Parser.ParseFrom(pdm.Message);
            ChatManager.Instance.UserID = user.UserID;
            ChatManager.Instance.UserName = user.UserName;
        }
      
    }

    private void GetOnlineUser(ProtocolDataModel pdm) 
    {
        OnlineUserInfo info = OnlineUserInfo.Parser.ParseFrom(pdm.Message);
        if (info != null)
        {
            ChatManager.Instance.OnlineUserList.Clear();
            for (int i = 0; i < info.UserList.Count; i++)
            {
                ChatManager.Instance.OnlineUserList.Add(info.UserList[i].UserID, info.UserList[i].UserName);
            }
        }
        else
        {
            ChatManager.Instance.OnlineUserList = new Dictionary<int, string>();
        }
      

        ChatManager.Instance.UserlistUpdate = true;

        int count = info == null ? 0 : info.UserList.Count;
        print("GetOnlineUser:"+ count);
    }
}
