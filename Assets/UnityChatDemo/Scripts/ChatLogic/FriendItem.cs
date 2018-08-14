using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 在线用户类
/// </summary>
public class FriendItem : MonoBehaviour {

    public string FriendName;
    public int FriendID;
    void Start () {
		
	}
    public void Onclick()
    {
        ChatUIManager._instance.ShowSelectFriend(FriendName, FriendID);
    }
}
