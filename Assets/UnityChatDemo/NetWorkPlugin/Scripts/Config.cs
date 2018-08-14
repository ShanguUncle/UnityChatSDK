using NetWorkPlugin;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 服务器地址配置
/// </summary>
public class Config : MonoBehaviour {

    public static Config _instance;

    //TCP信令服务器地址及端口
    public string SipServerIP;
    public int SipServerPort; 
    //UDP视音频流服务器地址及端口
    public string ChatStreamServerIP; 
    public int ChatStreamServerPort;
     
    public GameObject NetPanl;

    public InputField SipServerIPInputField;
    public InputField SipServerPortInputField;
    public InputField ChatStreamServeIPInputField;
    public InputField ChatStreamServerPortInputField;

    private void Awake()
    {
        _instance=this;

    }
    void Start ()
    {     
     
    }
    public void Connect()
    {
        try
        {
            SipServerIP = SipServerIPInputField.text;
            SipServerPort = int.Parse(SipServerPortInputField.text);

            ChatStreamServerIP = ChatStreamServeIPInputField.text;
            ChatStreamServerPort = int.Parse(ChatStreamServerPortInputField.text);
        }
        catch (System.Exception e)
        {
            print("config error:"+e.Message);
            MessageManager._instance.ShowMessage("输入有误！");
            return;
        }

        NetManager._instance.Connect();
        Invoke("cheakRes", 2);
    }
    void cheakRes()
    {
        if (NetWorkManager.Instance.IsConnect())
        {
            NetPanl.SetActive(false);
        }
    }
}
