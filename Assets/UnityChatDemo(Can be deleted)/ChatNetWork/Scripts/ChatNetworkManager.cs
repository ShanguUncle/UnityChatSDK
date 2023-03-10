using System;
using System.Collections.Generic;
#if UNITY_EDITOR || !UNITY_WEBGL
using ChatNetWork;
#endif
using System.Net;
using UnityEngine;
using ChatProtocol;

/// <summary>
/// TCP chat network manager
/// https://github.com/ShanguUncle/UnityChatSDK
/// </summary>
public class ChatNetworkManager : MonoBehaviour {

    public static ChatNetworkManager Instance;

#if UNITY_EDITOR || !UNITY_WEBGL
    SocketClient client;
#endif

    bool isOnConnectResult;
    bool isOnDisconnect;
    public Action<bool> OnConnectResultAction;
    public Action OnDisconnectAction;
    public Queue<byte[]> ReceiveDataQueue = new Queue<byte[]>();
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        Init();
    }
    void Init()
    {
#if UNITY_EDITOR || !UNITY_WEBGL
        if (NetConfig.Instance.NetType != NetType.WebSocket)
        {
            client = new SocketClient();
            client.OnConnect += OnConnect;
            client.OnDisconnect += OnDisconnect;
            client.OnReceiveData += OnReceiveData;
        }
#endif
        WebSocketNetwork.Instance.OnConnect += OnConnect;
        WebSocketNetwork.Instance.OnDisconnect += OnDisconnect;
    }
    /// <summary>
    /// connect to the server
    /// </summary>
    public void ConnectServer()
    {
#if UNITY_EDITOR || !UNITY_WEBGL
        if (NetConfig.Instance.NetType != NetType.WebSocket)
        {
            IPAddress ipAddress;
            if (!IPAddress.TryParse(NetConfig.Instance.ServerIP, out ipAddress) || NetConfig.Instance.TcpPort < 0 || NetConfig.Instance.TcpPort > 65535)
            {
                Debug.LogError("ip or port is wrong!");
                return;
            }
            client.ConnectServer(NetConfig.Instance.ServerIP, NetConfig.Instance.TcpPort);
        }
        else 
        {
            WebSocketNetwork.Instance.Connect(NetConfig.Instance.WsAddress);
        }
#else
        WebSocketNetwork.Instance.Connect(NetConfig.Instance.WsAddress);
#endif
    }
    /// <summary>
    /// disconnect to the server
    /// </summary>
    public void DisconnectServer()
    {
#if UNITY_EDITOR || !UNITY_WEBGL
        if (NetConfig.Instance.NetType != NetType.WebSocket)
        {
            client.Disconnect();
        }
        else 
        {
            WebSocketNetwork.Instance.Close();
        }
#else
           WebSocketNetwork.Instance.Close();
#endif
    }


    /// <summary>
    /// Send data to the server
    /// </summary>
    /// <param name="model"></param>
    internal void Send(DataModel model)
    {
#if UNITY_EDITOR || !UNITY_WEBGL

        if (NetConfig.Instance.NetType != NetType.WebSocket)
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
        else
        {
            WebSocketNetwork.Instance.Send(DataCodec.Encode(model));
        }
#else
        WebSocketNetwork.Instance.Send(DataCodec.Encode(model));
#endif
    }

    /// <summary>
    /// Receive server message callback
    /// </summary>
    /// <param name="data"></param>
    private void OnReceiveData(byte[] data)
    {
        if (ReceiveDataQueue.Count > 100) ReceiveDataQueue.Clear();
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

    public int GetDelayMS
    {
        get
        {
           if(connectResult||WebSocketNetwork.Instance.IsConnected)return NetDataHanlerCenter.Instance.DelayMS;
           return -1;
        }
    }
}
