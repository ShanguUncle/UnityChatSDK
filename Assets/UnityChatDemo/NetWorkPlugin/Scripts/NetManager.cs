using NetWorkPlugin;
using Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TCP通讯管理类
/// </summary>
public class NetManager : MonoBehaviour {

    public static NetManager _instance;
    private void Awake()
    {
        _instance = this;
        NetWorkManager.Instance.InitNetWork();
        NetWorkManager.Instance.ConnectResultEvent += OnServerConnect;
        NetWorkManager.Instance.DisconnectEvent += OnServerDisconnect;
    }

    //注册服务器连接回调
    private void OnServerConnect(bool res)
    {
        print("服务器连接:" + res);
        if (res)
        {
            MessageManager._instance.ShowMessage("服务器连接成功！");
            Config._instance.NetPanl.SetActive(false);

            //登录
            ChatManager._instance.Login(SystemInfo.deviceName, SystemInfo.deviceName);
        }
        else
        {
            MessageManager._instance.ShowMessage("服务器连接失败！", 3);
            Config._instance.NetPanl.SetActive(true);
        }
    }
    //注册服务器断开连接回调
    private void OnServerDisconnect()
    {
        onDisconnected = true;
        print("服务器断开连接！");
    }
    bool onDisconnected;
    private void FixedUpdate()
    {
        if (onDisconnected)
        {
            onDisconnected = false;
            MessageManager._instance.ShowMessage("服务器断开连接！");
            Config._instance.NetPanl.SetActive(true);
        }
    }
    void Start () {

        Connect();
    }

    public void Connect() 
    {
        if (CheckLegal(Config._instance.SipServerIP, Config._instance.SipServerPort))
            connectToServer(Config._instance.SipServerIP, Config._instance.SipServerPort);
    }
    bool CheckLegal(string ip,int port)
    {
        IPAddress ipaddress;
        if (!IPAddress.TryParse(ip, out ipaddress))
        {
            MessageManager._instance.ShowMessage("IP["+ip+"]不合法");
            return false;
        }
      
        if ((port > 65535) || (port < 0))
        {
            MessageManager._instance.ShowMessage("端口号["+ port+"]超出（0-65535)");
            return false;
        }
        return true;
    }
    void connectToServer(string ip,int port)
    {
        NetWorkManager.Instance.ConnectServer(ip, port);
    }

    public void DisConnect()
    {
        NetWorkManager.Instance.Disconnect();
    }

    private void OnDestroy()
    {
        NetWorkManager.Instance.Disconnect();
    }

}
