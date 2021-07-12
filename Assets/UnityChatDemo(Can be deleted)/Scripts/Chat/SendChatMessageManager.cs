using ChatProto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
public class SendChatMessageManager : MonoBehaviour {

    public static SendChatMessageManager Instance;

    public InputField MessageTextInputField;

    public GameObject OnReceiveTextPanel;
    public Text ReceiveText;

    public GameObject OnReceivePicPanel;
    public RawImage ReceiveImage;

    public GameObject OnReceiveVoicePanel;

    public Text ReceicePicUserText;
    public Text ReceiceVoiceUserText;

    public delegate void OnReciveMessage(MessageInfo info);
    public OnReciveMessage OnReciveMessageDg;
    public enum MessageType
    {
        None,
        Text,
        Pic,
        Voice,
        File,
        Mark,
        Other,
    }
    private void Awake()
    {
        Instance = this;
    }
    void Start ()
    {

    }
    public void OnMessage(MessageInfo info)
    {
        SoundManager._instance.PlayEffect("message");

        switch ((MessageType)info.Type)
        {
            case MessageType.None:
                break;
            case MessageType.Text:
                OnReciveText(info);
                break;
            case MessageType.Pic:
                OnRecivePic(info);
                break;
            case MessageType.Voice:
                OnReciveVoice(info);
                break;
            case MessageType.File:
                break;
            default:
                break;
        }
        OnReciveMessageDg?.Invoke(info);
    }

    private void OnReciveText(MessageInfo info)
    {
        OnReceiveTextPanel.SetActive(true);

        UserInfo user = ChatManager.Instance.OnlineUserList.Find((UserInfo u) => { return u.UserID == info.UserID; });
        ReceiveText.text += "<color=#00A6FF>" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " [receive] </color>" + "\n";
        ReceiveText.text += user.UserName + "(" + user.UserID + "):" + Encoding.UTF8.GetString(info.MessageData.ToByteArray()) + "\n";

        ReceiveText.GetComponent<RectTransform>().sizeDelta = new Vector2(0, ReceiveText.preferredHeight);
        ReceiveText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, ReceiveText.preferredHeight);
    }

    private void OnRecivePic(MessageInfo info)
    {
        UserInfo user = ChatManager.Instance.OnlineUserList.Find((UserInfo u) => { return u.UserID == info.UserID; });
        ReceicePicUserText.text = user.UserName;

        byte[] data = info.MessageData.ToByteArray();
        OnReceivePicPanel.SetActive(true);
        Texture2D t = new Texture2D(BitConverter.ToInt32(new byte[4] { data[0], data[1], data[2], data[3] },0),
            BitConverter.ToInt32(new byte[4] { data[4], data[5], data[6], data[7] },0));
        byte[] pic = new byte[data.Length-8];
        Buffer.BlockCopy(data,8, pic,0, pic.Length);
        ImageConversion.LoadImage(t, pic, false);
        ReceiveImage.texture = t;
    }

    byte[] receiveVoiceData;
    private void OnReciveVoice(MessageInfo info)
    {
        UserInfo user = ChatManager.Instance.OnlineUserList.Find((UserInfo u) => { return u.UserID == info.UserID; });
        ReceiceVoiceUserText.text = user.UserName;

        receiveVoiceData = info.MessageData.ToByteArray();
        OnReceiveVoicePanel.SetActive(true);
    }
    public void PlayReceiveVoice()
    {
        if(receiveVoiceData!=null)
            UnityChatSDK.Instance.PlayRecordAudio(receiveVoiceData);
    }

    public void SendText() 
    {
        if (string.IsNullOrEmpty(MessageTextInputField.text))
        {
            MessageManager.Instance.ShowMessage("message is null!");
            return;
        }

        ReceiveText.text += "<color=#00A600>" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " [send] </color>" + "\n";
        ReceiveText.text += ChatManager.Instance.UserName + "(" + ChatManager.Instance.UserID + "):" + MessageTextInputField.text + "\n";

        MainUIManager.Instance.SendMessage((int)MessageType.Text,Encoding.UTF8.GetBytes(MessageTextInputField.text));
    }

    public ToggleGroup PicTogGrp;
    public void SendPic()
    {
        Toggle tog= PicTogGrp.ActiveToggles().First();
        if (tog!=null)
        {
            Texture2D t = tog.GetComponent<Image>().sprite.texture;
            if (t == null) 
            {
                MessageManager.Instance.ShowMessage("pic is null!");
                return;
            }

            byte[] width = BitConverter.GetBytes(t.width);
            byte[] height = BitConverter.GetBytes(t.height);
            byte[] picData = t.EncodeToPNG();

            byte[] data = new byte[picData.Length+8];
            Buffer.BlockCopy(width,0, data,0,4);
            Buffer.BlockCopy(height, 0, data,4, 4);
            Buffer.BlockCopy(picData, 0, data, 8, picData.Length);

            MainUIManager.Instance.SendMessage((int)MessageType.Pic,data);
        }
        else
        {
            MessageManager.Instance.ShowMessage("please select pic!");
        }
    }
    public void SendVoice() 
    {
        if (sendVoiceData != null)
        {
            MainUIManager.Instance.SendMessage((int)MessageType.Voice, sendVoiceData);
            sendVoiceData = null;
        }
        else
        {
            MessageManager.Instance.ShowMessage("please record voice!");
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
