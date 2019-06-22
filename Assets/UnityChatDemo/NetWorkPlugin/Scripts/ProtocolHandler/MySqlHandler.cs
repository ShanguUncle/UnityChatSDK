using ChatProto.Proto;
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
            case MySqlDataProtocol.MYSQL_REG_SRES:
                Register(pdm);
                break;
            case MySqlDataProtocol.MYSQL_ONLINEUSE_SRES:
                GetOnlineUser(pdm);
                break;
            default:
                print("非法MySqlHandler:" + pdm.Request);
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
            ChatManager._instance.UserID = user.UserID;
            ChatManager._instance.UserName = user.UserName;
            ChatManager._instance.UserPortrait = user.UserPortrait;
        }
      
    }
    private void Register(ProtocolDataModel pdm)
    {
        ResultCode result = (ResultCode)(BitConverter.ToInt32(pdm.Message, 0));
        print("Register result:" + result.ToString());

    }
    private void GetOnlineUser(ProtocolDataModel pdm) 
    {
        OnlineUserInfo info = OnlineUserInfo.Parser.ParseFrom(pdm.Message);
        if (info != null)
        {
            ChatManager._instance.OnlineUserList.Clear();
            for (int i = 0; i < info.OnlineUserLiset.Count; i++)
            {
                ChatManager._instance.OnlineUserList.Add(int.Parse(info.OnlineUserLiset[i].Split(',')[0]), info.OnlineUserLiset[i].Split(',')[1]);
            }
        }
        else
        {
            ChatManager._instance.OnlineUserList = new Dictionary<int, string>();
        }
      

        ChatManager._instance.UserlistUpdate = true;

        int count = info == null ? 0 : info.OnlineUserLiset.Count;
        print("GetOnlineUser:"+ count);
    }
}
