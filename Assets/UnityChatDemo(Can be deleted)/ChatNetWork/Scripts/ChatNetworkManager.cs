
using ChatNetWork;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;


/// <summary>
/// TCP通讯管理类
/// </summary>
public class ChatNetworkManager : MonoBehaviour {

    public static ChatNetworkManager Instance;
    SocketClient client;

    bool isOnConnectResult;
    bool isOnDisconnect;
    public Action<bool> OnConnectResultAction;
    public Action OnDisconnectAction;
    public Queue<byte[]> ReceiveDataQueue = new Queue<byte[]>();
    private void Awake()
    {
        Instance = this;
        Init();
    }
    void Init()
    {
        client = new SocketClient();
        client.OnConnect += OnConnect;
        client.OnDisconnect += OnDisconnect;
        client.OnReceiveData += OnReceiveData;
    }
    /// <summary>
    /// 连接服务器
    /// </summary>
    public void ConnectServer()
    {
        IPAddress ipAddress;
        if (!IPAddress.TryParse(Config.Instance.ServerIP, out ipAddress) || Config.Instance.TcpPort < 0 || Config.Instance.TcpPort > 65535)
        {
            Debug.LogError("ip or port is wrong!");
            return;
        }
        client.ConnectServer(Config.Instance.ServerIP, Config.Instance.TcpPort);
    }
    /// <summary>
    /// 断开连接
    /// </summary>
    public void DisconnectServer()
    {
        client.Close();
    }


    /// <summary>
    /// 向服务器发送数据
    /// </summary>
    /// <param name="model"></param>
    internal void Send(DataModel model)
    {
        if (client.Connected)
        {
            client.Send(model);
        }
        else
        {
            print("offline!");
        }
    }

    /// <summary>
    /// 收到消息回调
    /// </summary>
    /// <param name="data"></param>
    private void OnReceiveData(byte[] data)
    {
        lock (ReceiveDataQueue)
        {
            ReceiveDataQueue.Enqueue(data);
        }
    }

    /// <summary>
    /// 断开连接回调
    /// </summary>
    private void OnDisconnect()
    {
        print("OnDisconnect");
        isOnDisconnect = true;
    }

    private void OnDestroy()
    {
        DisconnectServer();
    }

    bool connectResult;


    /// <summary>
    /// 连接结果回调
    /// </summary>
    /// <param name="result"></param>
    private void OnConnect(bool result)
    {
        print("OnConnect:" + result);
        connectResult = result;
        isOnConnectResult = true;
    }


    void Update()
    {
        if (isOnDisconnect)
        {
            isOnDisconnect = false;
            OnDisconnectAction?.Invoke();
        }
        if (isOnConnectResult)
        {
            isOnConnectResult = false;
            OnConnectResultAction?.Invoke(connectResult);
        }
    }

    //注册服务器连接回调
    private void OnServerConnect(bool res)
    {
        print("Server Connect:" + res);
        if (res)
        {
            MessageManager.Instance.ShowMessage("Server connect success!");
            Config.Instance.NetPanl.SetActive(false);

            //登录
            ChatManager.Instance.Login(SystemInfo.deviceName);
        }
        else
        {
            MessageManager.Instance.ShowMessage("Server connect fail!", 3);
            Config.Instance.NetPanl.SetActive(true);
        }
    }

}
