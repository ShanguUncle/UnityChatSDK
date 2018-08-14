using NetWorkPlugin;
using ProtobufNet;
using Protocol;
using Protocol.ProtobufNet;
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
            UserInfo user = ProtobufCodec.DeSerialize<UserInfo>(pdm.Message);
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
        OnlineUserInfo info = ProtobufCodec.DeSerialize<OnlineUserInfo>(pdm.Message);
        if (info != null)
        {
            ChatManager._instance.OnlineUserList = info.OnlineUserLiset;
        }
        else
        {
            ChatManager._instance.OnlineUserList = new Dictionary<string,int>();
        }
      

        ChatManager._instance.UserlistUpdate = true;

        int count = info == null ? 0 : info.OnlineUserLiset.Count;
        print("GetOnlineUser:"+ count);
    }
}
