using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ChatSDKDevice : MonoBehaviour
{

#if UNITY_IPHONE
	[DllImport("__Internal")]
	private static extern void _forceToSpeaker();
	[DllImport("__Internal")]
	private static extern void _forceToHeadset();
#endif

    public static void ToggleSpeaker(bool isOn)
    {
#if UNITY_ANDROID
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject audioManager = activity.Call<AndroidJavaObject>("getSystemService", "audio");

            if (isOn)
            {
                audioManager.Call("setMode", 0);
                audioManager.Call("setSpeakerphoneOn", true);
            }
            else
            {
                audioManager.Call("setMode", 3);
                audioManager.Call("setSpeakerphoneOn", false);
            }

            int mode = audioManager.Call<Int32>("getMode");
            bool isSpeakers = audioManager.Call<Boolean>("isSpeakerphoneOn");

            Debug.Log("Speakers set to: " + isSpeakers + ",mode is " + mode);
        }
        catch (Exception e)
        {
            Debug.Log("ToggleSpeaker Error:" + e.Message);
        }
#endif

#if UNITY_IPHONE
	    try
        {
         if (isOn)
		{
			_forceToSpeaker();
		}
		else 
		{
			_forceToHeadset();
		}
        }
        catch (Exception e)
        {
            Debug.Log("ToggleSpeaker Error:" + e.Message);
        }
#endif

    }
}
