
using UnityEngine;
using UnityEngine.UI;

public class HoloChatPanel : MonoBehaviour {

    private void OnEnable()
    {
        if (UnityChatSDK.Instance.VideoType==VideoType.CustomTexture)
            HoloCaptureManager.Instance.StartCapture();
    }
    private void OnDisable()
    {
        if (UnityChatSDK.Instance.VideoType == VideoType.CustomTexture)
            HoloCaptureManager.Instance.StopCapture();
    }

}
