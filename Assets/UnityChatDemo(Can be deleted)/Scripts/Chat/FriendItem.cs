using ChatProto;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class FriendItem : MonoBehaviour
{

    public string FriendName;
    public int FriendID;
    public UserInfo UserInfo;
    private Toggle.ToggleEvent onValueChanged;

    void Start()
    {
        gameObject.GetComponent<Toggle>().onValueChanged.AddListener(OnValueChanged);
    }
    void OnValueChanged(bool isOn)
    {
        if (isOn)
        {
            MainUIManager.Instance.SelectedFriendList.Add(UserInfo);
        }
        else
        {
            MainUIManager.Instance.SelectedFriendList.Remove(UserInfo);
        }
    }
}
