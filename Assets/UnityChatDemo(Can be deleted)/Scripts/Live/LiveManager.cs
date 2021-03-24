using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LiveManager : MonoBehaviour {

    public static LiveManager Instance;

    public RawImage CamRawImage;
    WebCamTexture cameraTexture;

    public RawImage LiveImage;

    Texture2D TextureLocal;

    public bool Living;


    private void Awake()
    {
        Instance = this;
    }
    public void StartLive()
    {
        UnityChatSDK.Instance.ChatType = ChatType.Video;
        UnityChatSDK.Instance.StartCapture();

        Living = true;

        TextureLocal = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
        StartCoroutine(RecordScreen());
    }
    public void StopLive()
    {
        UnityChatSDK.Instance.StopCapture();
        Living = false;
    }
    IEnumerator RecordScreen() 
    {
        while (Living) 
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
        if (LiveImage != null)
            LiveImage.texture = UnityChatSDK.Instance.DecodeVideoData(videoPacket);
    }
    public void DecodeAudiooData(AudioPacket audioPacket)  
    {
            UnityChatSDK.Instance.DecodeAudioData(audioPacket);
    }

    //todo send audio and video via your network refer to chatDataHandler.cs
    void SendAudio()
    {
        AudioPacket packet = UnityChatSDK.Instance.GetAudio();
    }
    void SendVideo()
    {
        VideoPacket packet = UnityChatSDK.Instance.GetVideo();
    }
    private void Update()
    {
        //test==
        if (Living)
        {
            VideoPacket packet = UnityChatSDK.Instance.GetVideo();
            if (packet!= null && packet.Data != null)
            {
                DecodeVideoData(packet);
            }
        }
        //==test
    }
}
