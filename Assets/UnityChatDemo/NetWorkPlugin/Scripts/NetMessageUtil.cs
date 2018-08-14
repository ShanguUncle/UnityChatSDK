using UnityEngine;
using System.Collections;
using Protocol;
using System.Collections.Generic;
using System;
using NetWorkPlugin;

/// <summary>
/// TCP网络数据处理管理
/// </summary>
public class NetMessageUtil : MonoBehaviour {

    IHandler mySqlHandler;
    IHandler imHandler;
    Queue<byte[]> receiveDataList;
    public static NetMessageUtil _instance;
    public bool Isdisconnected { get; set; }
    private void Awake()
    {
        _instance = this;
    }
    void Start ()
    {
        mySqlHandler = gameObject.AddComponent<MySqlHandler>();
        imHandler = gameObject.AddComponent<IMHandler>();
        receiveDataList = new Queue<byte[]>();
        //注册服务器断开连接回调
        NetWorkManager.Instance.ServerDisConnectEvent += OnServerDisConnect;
        //注册收到服务器数据回调
        NetWorkManager.Instance.ReceiveDataEvent += AddReceiveData;
    }
    void OnServerDisConnect()
    {
        Isdisconnected = true;
    }
    public void AddReceiveData(byte[] data) 
    {
        receiveDataList.Enqueue(data);
    }
	void Update ()
    {
        if(receiveDataList.Count > 0)
        {
            ProtocolDataModel model = pdmDecode(receiveDataList.Dequeue()); 
            StartCoroutine("MessageReceive", model);
        }	    
	}
     ProtocolDataModel pdmDecode(byte[] value)
    {
        ProtocolDataModel pdm = new ProtocolDataModel();
        pdm.Type = value[0];
        pdm.Request = value[1];
        if (value.Length > 2)
        {
            byte[] message = new byte[value.Length - 2];
            Buffer.BlockCopy(value, 2, message, 0, value.Length - 2);
            pdm.Message = message;
        }
        return pdm;
    }
    void MessageReceive(ProtocolDataModel pdm) {
        switch (pdm.Type) {
            case ProtocolType.TYPE_NONE:
                break; 
            case ProtocolType.TYPE_MYSQL:
                mySqlHandler.MessageReceive(pdm);
                break;
            case ProtocolType.TYPE_IM:
                imHandler.MessageReceive(pdm);
                break;
        }
    }
}
