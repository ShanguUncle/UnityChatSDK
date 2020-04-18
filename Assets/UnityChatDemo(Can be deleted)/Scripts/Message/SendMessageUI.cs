using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
public class SendMessageUI : MonoBehaviour {

    public Text[] ReceiveUserNameText;
    public InputField SendTextInputField;
    public RawImage SendPicImage;
    public Texture2D[] PicArray;
    public GameObject SendTextPanl;
    public GameObject SendPicPanl;
    public GameObject SendVoicePanl;

    public GameObject ReceiveTextPanl;
    public Text ReceiveText;
    public GameObject ReceivePicPanl;
    public RawImage ReceiveImage;
    public GameObject ReceiveVoicePanl;
    void Start ()
    {
        ChatSendMessageManager.Instance.OnReciveText += OnReciveText;
        ChatSendMessageManager.Instance.OnRecivePic += OnRecivePic;
        ChatSendMessageManager.Instance.OnReciveVoice += OnReciveVoice;
    }

    private void OnReciveText(byte[] data)
    {
        ReceiveTextPanl.SetActive(true);
        ReceiveText.text = Encoding.UTF8.GetString(data);
        ReceiveUserNameText[0].text = ChatSendMessageManager.Instance.MessagePeerName;
        SoundManager._instance.PlayEffect("message");
    }

    private void OnRecivePic(byte[] data)
    {
        ReceivePicPanl.SetActive(true);
        Texture2D t = new Texture2D(BitConverter.ToInt32(new byte[4] { data[0], data[1], data[2], data[3] },0),
            BitConverter.ToInt32(new byte[4] { data[4], data[5], data[6], data[7] },0));
        byte[] pic = new byte[data.Length-8];
        Buffer.BlockCopy(data,8, pic,0, pic.Length);
        ImageConversion.LoadImage(t, pic, false);
        ReceiveImage.texture = t;
        ReceiveUserNameText[1].text = ChatSendMessageManager.Instance.MessagePeerName;
        SoundManager._instance.PlayEffect("message");
    }
    byte[] receiveVoiceData;
    private void OnReciveVoice(byte[] data)
    {
        receiveVoiceData = data;
        ReceiveVoicePanl.SetActive(true);
        ReceiveUserNameText[2].text = ChatSendMessageManager.Instance.MessagePeerName;
        SoundManager._instance.PlayEffect("message");
    }
    public void PlayReceiveVoice()
    {
        if(receiveVoiceData!=null)
            UnityChatSDK.Instance.PlayRecordAudio(receiveVoiceData);
    }
    // Update is called once per frame
    void Update () {
		
	}

    public void SendText()
    {
        if (!string.IsNullOrEmpty(SendTextInputField.text))
        {
            ChatSendMessageManager.Instance.SendPeerMessage(Encoding.UTF8.GetBytes(SendTextInputField.text), ChatSendMessageManager.MessageType.Text);
            SendTextInputField.text = "";
            SendTextPanl.SetActive(false);
        }
        else
        {
            MessageManager._instance.ShowMessage("please input text!");
        }

    }
    int picOrder = -1;
    public void SelectPic(int order)
    {
        picOrder = order;
        SendPicImage.texture = PicArray[order];
    }
    public void SendPic()
    {
        if (picOrder>=0)
        {

            byte[] width = BitConverter.GetBytes(PicArray[picOrder].width);
            byte[] height = BitConverter.GetBytes(PicArray[picOrder].height);
            byte[] picData = PicArray[picOrder].EncodeToPNG();

            byte[] data = new byte[picData.Length+8];
            Buffer.BlockCopy(width,0, data,0,4);
            Buffer.BlockCopy(height, 0, data,4, 4);
            Buffer.BlockCopy(picData, 0, data, 8, picData.Length);

            ChatSendMessageManager.Instance.SendPeerMessage(data, ChatSendMessageManager.MessageType.Pic);
            SendPicImage.texture = null;
            picOrder = -1;
            SendPicPanl.SetActive(false);
        }
        else
        {
            MessageManager._instance.ShowMessage("please select pic!");
        }
    }
    public void SendVoice() 
    {
        if (sendVoiceData != null)
        {
            ChatSendMessageManager.Instance.SendPeerMessage(sendVoiceData,ChatSendMessageManager.MessageType.Voice);
            sendVoiceData = null;
            SendVoicePanl.SetActive(false);
        }
        else
        {
            MessageManager._instance.ShowMessage("please record voice!");
        }
          
    }
    public void StartRecordVoice()
    {
        UnityChatSDK.Instance.StartRecordAudio(60, OnFininshed);
    }
    byte[] sendVoiceData;
    public void OnFininshed(byte[] recordData)
    {
        sendVoiceData = recordData;
        print("OnFininshed recordVoice");
    }
    public void EndRecordVoice() 
    {
        UnityChatSDK.Instance.StopRecordAudio();
    }
    public void PlayRecordAudio()
    {
        if (sendVoiceData != null)
            UnityChatSDK.Instance.PlayRecordAudio(sendVoiceData);
    }
}
