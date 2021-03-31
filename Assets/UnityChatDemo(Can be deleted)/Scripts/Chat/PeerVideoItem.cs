using ChatProto;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PeerVideoItem : MonoBehaviour
{
    void Start()
    {
        gameObject.GetComponent<Toggle>().onValueChanged.AddListener(OnPeerTogChanged);
        int id = transform.Find("RawImage").GetComponent<VideoTexure>().ID;
        UserInfo info = ChatManager.Instance.GetUserInfoById(id);
        transform.Find("Text").GetComponent<Text>().text= info.UserName;
    }

    void OnPeerTogChanged(bool isOn)
    {
        if (isOn)
        {
            ChatUIManager.Instance.SelectedPeerVideo.ID = int.Parse(gameObject.name);
        }
    }
}
