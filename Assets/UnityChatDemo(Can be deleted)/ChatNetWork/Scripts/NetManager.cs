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
        print("Server Connect:" + res);
        if (res)
        {
            MessageManager._instance.ShowMessage("Server connect success!");
            Config.Instance.NetPanl.SetActive(false);

            //登录
            ChatManager.Instance.Login(SystemInfo.deviceName);
        }
        else
        {
            MessageManager._instance.ShowMessage("Server connect fail!", 3);
            Config.Instance.NetPanl.SetActive(true);
        }
    }
    //注册服务器断开连接回调
    private void OnServerDisconnect()
    {
        onDisconnected = true;
        print("Server Disconnect!");
    }
    bool onDisconnected;
    private void FixedUpdate()
    {
        if (onDisconnected)
        {
            onDisconnected = false;
            MessageManager._instance.ShowMessage("Server Disconnect!");
            Config.Instance.NetPanl.SetActive(true);
        }
    }
    void Start () {

        Connect();
    }

    public void Connect() 
    {
        if (CheckLegal(Config.Instance.SipServerIP, Config.Instance.SipServerPort))
            connectToServer(Config.Instance.SipServerIP, Config.Instance.SipServerPort);
    }
    bool CheckLegal(string ip,int port)
    {
        IPAddress ipaddress;
        if (!IPAddress.TryParse(ip, out ipaddress))
        {
            MessageManager._instance.ShowMessage("IP["+ip+"]is not leagle");
            return false;
        }
      
        if ((port > 65535) || (port < 0))
        {
            MessageManager._instance.ShowMessage("port["+ port+"]is over（0-65535)");
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
