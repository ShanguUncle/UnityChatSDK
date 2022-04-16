
using ChatNetWork;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;


/// <summary>
/// TCP chat network manager
/// https://github.com/ShanguUncle/UnityChatSDK
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
    /// connect to the server
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
    /// disconnect to the server
    /// </summary>
    public void DisconnectServer()
    {
        client.Disconnect();
    }


    /// <summary>
    /// Send data to the server
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
            OnDisconnect();
            print("offline!");
        }
    }

    /// <summary>
    /// Receive server message callback
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
    /// Connection result callback
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

    public int GetDelayMS { get{ if (client!=null && client.Connected) return NetDataHanlerCenter.Instance.DelayMS; else return -1; } }
}
