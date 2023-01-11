using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Network type, you can customize TCP, UDP, P2P, Webrct, Unet, Photon... The case demo uses UDP
/// </summary>
public enum NetType { TcpStream, UdpStream, WebSocket };// UdpP2P
/// <summary>
/// Server ip configuration
/// </summary>
public class NetConfig : MonoBehaviour {

    public static NetConfig Instance;

    public NetType NetType;
    //Server ip and port
    public string ServerIP;
    public int TcpPort = 6650;
    public int UdpPort=6680;
    public int WsPort=6660;
    public bool isSecure;
    public string DomainName= "xxx.com";
    public string WsAddress
    {
        get
        {
            if (isSecure)
            {
                return string.Format("wss://{0}:{1}/default", DomainName, WsPort);
            }
            else 
            {
                return string.Format("ws://{0}:{1}/default", ServerIP, WsPort);
            }
        }
    }


    public GameObject NetPanl;
    public InputField ServerIPInputField;

    public string IpKey = "IpKey";
    private void Awake()
    {
        Instance=this;
        if (PlayerPrefs.HasKey(IpKey))
        {
            ServerIP = PlayerPrefs.GetString(IpKey);
            print("PlayerPrefs get ip:" + ServerIP);
        }
    }
    void Start ()
    {
        ServerIPInputField.text = ServerIP;
        ChatNetworkManager.Instance.OnConnectResultAction += OnConnect;
        Connect();
    }

    private void OnConnect(bool result)
    {
        if (result) 
        {
            PlayerPrefs.SetString(IpKey, ServerIP);
            PlayerPrefs.Save();
        }
    }

    public void Connect()
    {
        ServerIP = ServerIPInputField.text;
        ChatNetworkManager.Instance.ConnectServer();
    }
}
