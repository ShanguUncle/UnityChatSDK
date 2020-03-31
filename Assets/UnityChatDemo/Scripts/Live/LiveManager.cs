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

    public bool Living;
    private void Awake()
    {
        Instance = this;
    }
    public void StartLive()
    {
        UnityChatSDK.Instance.ChatType = ChatType.AV;
        UnityChatSDK.Instance.StartCapture();

        Living = true;
    }
    public void StopLive()
    {
        UnityChatSDK.Instance.StopCpture();
        Living = false;
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
            LiveImage.texture= UnityChatSDK.Instance.DecodeVideoData(videoPacket);
    }
    public void DecodeAudiooData(AudioPacket audioPacket)  
    {
            UnityChatSDK.Instance.DecodeAudioData(audioPacket.Id,audioPacket);
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
            if (packet != null)
            {
                DecodeVideoData(packet);
            }
        }
        //==test
    }
}
