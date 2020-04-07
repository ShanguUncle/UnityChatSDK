
using UnityEngine;
using UnityEngine.UI;

public class HoloChatPanl : MonoBehaviour {

    private void OnEnable()
    {
        if (UnityChatSDK.Instance.VideoType==VideoType.CustomTexture && ChatDataHandler.Instance.ChatType != ChatType.Audio)
            HoloCaptureManager.Instance.StartCapture();
    }
    private void OnDisable()
    {
        if (UnityChatSDK.Instance.VideoType == VideoType.CustomTexture && ChatDataHandler.Instance.ChatType != ChatType.Audio)
            HoloCaptureManager.Instance.StopCapture();
    }

}
