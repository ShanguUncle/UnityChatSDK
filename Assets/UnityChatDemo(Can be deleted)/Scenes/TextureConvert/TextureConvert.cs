using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureConvert : MonoBehaviour
{
    public UnityChatSDK.ConvertType ConvertType;
    public Texture SoureTexture;//need read/write enable
    public RawImage ConverRawImage;

    public void OnToggleChanged(int type)  
    {
        ConvertType = (UnityChatSDK.ConvertType)type;
    }
    public void Convert() 
    { 
        ConverRawImage.texture = UnityChatSDK.Instance.ConvertTexture(SoureTexture,TextureFormat.BGRA32, ConvertType);
    }
}
