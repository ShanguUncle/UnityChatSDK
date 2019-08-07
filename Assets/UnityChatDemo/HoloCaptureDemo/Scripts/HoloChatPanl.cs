using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoloChatPanl : MonoBehaviour {

    private void OnEnable()
    {
        if (UnityChatSDK.Instance.VideoType==VideoType.CustomMode && ChatDataHandler.Instance.ChatType != ChatType.Audio)
            HoloCaptureManager.Instance.StartCapture();
    }
    private void OnDisable()
    {
        if (UnityChatSDK.Instance.VideoType == VideoType.CustomMode && ChatDataHandler.Instance.ChatType != ChatType.Audio)
            HoloCaptureManager.Instance.StopCapture();
    }

}
