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
    IHandler videoDataHandler;
    public static NetDataHanlerCenter Instance;
    private void Awake()
    {
        Instance = this;

        mySqlHandler = gameObject.AddComponent<MySqlHandler>();
        imHandler = gameObject.AddComponent<IMHandler>();
        messageHandler= gameObject.AddComponent<MessageHandler>();
        videoDataHandler = gameObject.AddComponent<VideoDataHandler>();
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
            case ChatProtocolType.TYPE_CHATDATA:
                videoDataHandler.MessageReceive(model);
                break;
            case ChatProtocolType.TYPE_OTHER:
                HandlerOther(model.Request,model.Message);
                break;
        }
    }

    public int DelayMS { get; private set; }
    void HandlerOther(byte req,byte[]message) 
    {
        switch (req)
        {
            case OtherProtocol.OP_NONE:
                break;
            case OtherProtocol.OP_HEART:
                TimeSpan heartDelay = new TimeSpan(DateTime.UtcNow.Ticks - BitConverter.ToInt64(message, 0));
                DelayMS = heartDelay.Milliseconds;
                break;
            case OtherProtocol.OP_QUEUE_NONE:
                print("QUEUE_NONE");
                break;
            case OtherProtocol.OP_QUEUE_WAIT:
                print("QUEUE_WAIT:" + BitConverter.ToInt32(message,0));
                MessageManager.Instance.ShowMessage("In Queue..."+ BitConverter.ToInt32(message, 0));
                break;
            case OtherProtocol.OP_QUEUE_SUCCESS:
                print("QUEUE_SUCCESS");
                MessageManager.Instance.ShowMessage("QUEUE_SUCCESS");
                break;
        }
    }
}
