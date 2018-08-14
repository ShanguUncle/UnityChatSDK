using NetWorkPlugin;
using Protocol;
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
        NetWorkManager nw = new NetWorkManager();
        nw.InitNetWork(1024);
    }
    void Start () {

        AutoConnect();

    }

    private void FixedUpdate()
    {
        if (NetMessageUtil._instance.Isdisconnected)
        {
            MessageManager._instance.ShowMessage("服务器断开连接！");
            NetMessageUtil._instance.Isdisconnected = false;
            Config._instance.NetPanl.SetActive(true);
        }
    }

    void AutoConnect()
    {
        if(CheckLegal(Config._instance.SipServerIP, Config._instance.SipServerPort))
        connectToServer(Config._instance.SipServerIP, Config._instance.SipServerPort);
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
        Invoke("chectRes",3);
    }
    void chectRes()
    {
        if (NetWorkManager.Instance.IsConnect())
        {
            MessageManager._instance.ShowMessage("服务器连接成功！");
            //登录
            ChatManager._instance.Login(SystemInfo.deviceName, SystemInfo.deviceName);
        }
        else
        {
            MessageManager._instance.ShowMessage("服务器连接失败！",5);
            Config._instance.NetPanl.SetActive(true);
        }
    }
    public void DisConnect()
    {
        NetWorkManager.Instance.DisConnectServer();
    }

    private void OnDestroy()
    {
        NetWorkManager.Instance.OnDestroy();
    }

}
