using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using ChatNetWork;
using ChatProtocol;
using System.Text;

/// <summary>
/// TCP network handler center
/// </summary>
public class NetDataHanlerCenter : MonoBehaviour {

    IHandler mySqlHandler;
    IHandler imHandler;
    IHandler messageHandler;
    public static NetDataHanlerCenter Instance;
    private void Awake()
    {
        Instance = this;

        mySqlHandler = gameObject.AddComponent<MySqlHandler>();
        imHandler = gameObject.AddComponent<IMHandler>();
        messageHandler= gameObject.AddComponent<MessageHandler>();
    }
    void Start ()
    {

    }
	void Update ()
    {
        if(ChatNetworkManager.Instance.ReceiveDataQueue.Count > 0)
        {
            byte[] data = ChatNetworkManager.Instance.ReceiveDataQueue.Dequeue();
            HandlerData(data);
        }	    
	}
    void HandlerData(byte[] data)
    {
        DataModel model = DataCodec.Decode(data);

        switch (model.Type)
        {
            case ChatProtocolType.TYPE_NONE:
                print("NONE:" + Encoding.UTF8.GetString(model.Message));
                break;
            case ChatProtocolType.TYPE_MYSQL:
                mySqlHandler.MessageReceive(model);
                break;
            case ChatProtocolType.TYPE_IM:
                imHandler.MessageReceive(model);
                break;
            case ChatProtocolType.TYPE_MESSAGE:
                messageHandler.MessageReceive(model);
                break;
        }
    }
}
