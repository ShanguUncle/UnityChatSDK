using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeskVideoModeDemo : MonoBehaviour
{
    private void Start()
    {
        ChatManager.Instance.OnChatAcceptgDg += OnOnAccpet;
        ChatManager.Instance.OnChatHangDg += OnChatHang;
    }

    bool isChatting;
    private void OnOnAccpet()
    {
        if (isDeskMode)
            UnityChatSDK.Instance.StartDeskCapture();
        isChatting = true;
    }

    private void OnChatHang()
    {
        if (isDeskMode)
            UnityChatSDK.Instance.StoptDeskCapture();
        isChatting = false;
        isDeskMode = false;
        FindObjectOfType<UnityChatSet>().SetDeciveCam();
    }

    bool isDeskMode;
    public void DeskCall()
    {
        isDeskMode = true;
        ChatUIManager.Instance.VideoCall();
        FindObjectOfType<UnityChatSet>().SetCustomTexture();
    }

    float lastTime;
   
    private void Update()
    {
        if (isDeskMode && isChatting && Time.time- lastTime>0.04f) 
        {
            lastTime = Time.time;
            UnityChatSDK.Instance.UpdateCustomTexture(UnityChatSDK.Instance.GetDeskTexture());
        }
    }
}
