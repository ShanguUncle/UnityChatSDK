using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UDeskVideo : MonoBehaviour
{
    public void StartCapture()
    {
        UnityChatSDK.Instance.StartDeskCapture();
    }
    public void StoptCapture()
    {
        UnityChatSDK.Instance.StoptDeskCapture();
    }


    public RawImage Image;

   
    void FixedUpdate()
    {
        Texture2D t= UnityChatSDK.Instance.GetDeskTexture();      
        if (t != null)
        {
            if (Image != null) Image.texture = t;
        }
    }
}
