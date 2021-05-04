using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LiveManager : MonoBehaviour {

    public RawImage CamRawImage;
    WebCamTexture cameraTexture;

    Texture2D TextureLocal;

    bool living;

    public void StartLive()
    {
        UnityChatSDK.Instance.ChatType = ChatType.Video;
        UnityChatSDK.Instance.StartCapture();

        living = true;

        TextureLocal = new Texture2D(Screen.width, Screen.height, TextureFormat.BGRA32, false);
        StartCoroutine(Record());
    }
    public void StopLive()
    {
        UnityChatSDK.Instance.StopCapture();
        living = false;
    }
    IEnumerator Record() 
    {
        while (living) 
        {
            yield return new WaitForEndOfFrame();
            TextureLocal.ReadPixels(new Rect(0.0f, 0.0f, Screen.width, Screen.height), 0, 0, false);
            TextureLocal.Apply();
            UnityChatSDK.Instance.UpdateCustomTexture(TextureLocal);
            yield return new WaitForSeconds(1f / UnityChatSDK.Instance.Framerate);
        }
    }
    public void OpenCam()
    {
        CamRawImage.transform.gameObject.SetActive(true);
        if (cameraTexture != null && cameraTexture.isPlaying)
        {
            cameraTexture.Stop();
        }
        cameraTexture = new WebCamTexture(640, 360, 30);
        cameraTexture.Play();
        CamRawImage.texture = cameraTexture;
    }
    public void CloseCam()
    {
        if (cameraTexture != null && cameraTexture.isPlaying)
        {
            cameraTexture.Stop();
        }
        CamRawImage.transform.gameObject.SetActive(false);
    }

    //decode data when receive living data
    public void DecodeVideoData(VideoPacket videoPacket)
    {
            UnityChatSDK.Instance.DecodeVideoData(videoPacket);
    }
    public void DecodeAudiooData(AudioPacket audioPacket)  
    {
            UnityChatSDK.Instance.DecodeAudioData(audioPacket);
    }
    private void Update()
    {
        //test==
        //todo send audio and video via your network refer to chatDataHandler.cs
        if (living)
        {
            VideoPacket videoPacket = UnityChatSDK.Instance.GetVideo();
            AudioPacket audioPacket = UnityChatSDK.Instance.GetAudio();

            if (videoPacket != null && videoPacket.Data != null)
            {
                DecodeVideoData(videoPacket);
            }
            if (audioPacket != null && audioPacket.Data != null)
            {
                DecodeAudiooData(audioPacket);
            }
        }
        //==test
    }
}
