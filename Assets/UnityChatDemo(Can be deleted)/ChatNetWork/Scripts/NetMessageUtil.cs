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
    private void Awake()
    {
        _instance = this;

        mySqlHandler = gameObject.AddComponent<MySqlHandler>();
        imHandler = gameObject.AddComponent<IMHandler>();
        receiveDataList = new Queue<byte[]>();
        NetWorkManager.Instance.DataReceiveEvent += AddReceiveData;
    }
    void Start ()
    {

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
