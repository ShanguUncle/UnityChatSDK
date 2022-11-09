using System;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityWebSocket;

public class WebSocketNetwork : MonoBehaviour
{
    public static WebSocketNetwork Instance;
    private void Awake()
    {
        Instance = this;
    }
    WebSocket webSocket;
    public bool IsConnected { get; private set; }
    public Action<bool> OnConnect { get; internal set; }
    public Action OnDisconnect { get; internal set; }

    public void Connect(string address)
    {
        if (IsConnected) return;

        webSocket = new WebSocket(address);


        webSocket.OnOpen += OnOpen;
        webSocket.OnMessage += OnMessageReceived;
        webSocket.OnClose += OnClosed;
        webSocket.OnError += OnError;

        webSocket.ConnectAsync();

    }

    void OnOpen(object sender, EventArgs e)
    {
        IsConnected = true;
        OnConnect?.Invoke(true);
        Debug.Log("OnOpen");
    }

    void OnClosed(object sender, CloseEventArgs e)
    {
        IsConnected = false;
        OnDisconnect?.Invoke();
        if (webSocket != null)
        {
            webSocket.OnOpen -= OnOpen;
            webSocket.OnMessage -= OnMessageReceived;
            webSocket.OnClose -= OnClosed;
            webSocket.OnError -= OnError;
        }
        webSocket = null;
        Debug.Log(string.Format("OnClosed: StatusCode: {0}, Reason: {1}", e.Code, e.Reason));

    }

    /// <summary>
    /// Called when an error occured on client side
    /// </summary>
    void OnError(object sender, ErrorEventArgs e)
    {
        OnConnect?.Invoke(false);
        Debug.Log(string.Format("OnError: <color=red>{0}</color>", e.Message));
        Close();
    }


    public void Send(byte[] data)
    {
        if (!IsConnected) return;

        webSocket.SendAsync(data);
    }

    public void OnSendCompleted(bool success)
    {
        //Debug.Log("OnSend:" + success);
    }


    /// <summary>
    /// Called when we received a text message from the server
    /// </summary>
    void OnMessageReceived(object sender, MessageEventArgs e)
    {
        try
        {
            if (e.IsBinary)
            {
                //Debug.Log(string.Format("Receive IsBinary length: {0}", e.RawData.Length));
                OnReceiveData(e.RawData);
            }
            else if (e.IsText)
            {
                //Debug.Log(string.Format("Receive IsText : {0}", e.Data));
            }
        }
        catch (Exception ex)
        {
            Debug.Log(string.Format("<color=red>websocket error: {0}</color>", ex.Message));
        }
    }

    public Queue<byte[]> ReceiveDataQueue = new Queue<byte[]>();

    private void OnReceiveData(byte[] data)
    {
        if (ReceiveDataQueue.Count > 100) ReceiveDataQueue.Clear();
        lock (ReceiveDataQueue)
        {
            ReceiveDataQueue.Enqueue(data);
        }
    }
    void OnDestroy()
    {
        Close();
    }
    private void OnApplicationQuit()
    {
        Close();
    }
    public void Close()
    {
        if (webSocket != null && webSocket.ReadyState != WebSocketState.Closed)
        {
            webSocket.CloseAsync();
        }
        IsConnected = false;
        webSocket = null;
    }

}
