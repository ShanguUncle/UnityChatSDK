using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
/// <summary>
/// UnityChatSDK parameter settings
/// </summary>
public class UnityChatSet: MonoBehaviour {

    //peer audio playback volume size
    public float AudioVolume= 1f;

    //Local microphone volume size scale value
    public float MicVolumeScale= 1f;
	   
    public VideoType VideoType = VideoType.DeviceCamera;

    //video resolution
    public VideoResolution VideoResolution = VideoResolution._180P;
    //video compression quality
    public VideoQuality VideoQuality = VideoQuality.Middle;
    //video refresh rate
    [Range(5,25)]
    public int Framerate = 15;
    //Echo cancellation
    public bool EchoCancellation;
    [Tooltip("check video frame static")]
    public bool EnableDetection;
    //Video and audio frame synchronization
    public bool EnableSync;

    /// <summary>
    /// You need set the unity camera when choosing to capture by Unity Camera
    /// </summary>
    public Camera CaptureCamera;

    IEnumerator Start()
    {
        if (UnityChatSDK.Instance == null) gameObject.AddComponent<UnityChatSDK>();
        yield return new WaitUntil(() => UnityChatSDK.Instance != null);
        Application.targetFrameRate = 60;
        InitAudio();
        InitVideo(); 
    }
    //初始化音频
    void InitAudio() 
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
#endif
        UnityChatSDK.Instance.AudioVolume=AudioVolume;
		UnityChatSDK.Instance.MicVolumeScale= MicVolumeScale;
        UnityChatSDK.Instance.AudioThreshold= 0.002f;
        UnityChatSDK.Instance.AudioFrequency = 8000;
        UnityChatSDK.Instance.AudioSample = 2;
        UnityChatSDK.Instance.AudioLatency = 500;
        UnityChatSDK.Instance.EchoCancellation = EchoCancellation;
        //初始化音频(麦克风Index)
        UnityChatSDK.Instance.InitMic(0);
        print("InitAudio OK");
    }

    //初始化视频
    void InitVideo() 
    {
        UnityChatSDK.Instance.Framerate = Framerate;
        UnityChatSDK.Instance.EnableDetection = EnableDetection;
        UnityChatSDK.Instance.EnableSync = EnableSync;
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera)) 
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
#endif
        //Initialize video (camera Index)
        UnityChatSDK.Instance.InitVideo(0);
        UnityChatSDK.Instance.SetVideoQuality(VideoQuality);
        UnityChatSDK.Instance.SetResolution(VideoResolution);
        switch (VideoType)
        {
            case VideoType.DeviceCamera:
                SetVideoCaptureType(VideoType.DeviceCamera, null);
                SetFrontCam();
                break;
            case VideoType.UnityCamera:
                SetVideoCaptureType(VideoType.UnityCamera, CaptureCamera);
                break;
            case VideoType.CustomTexture:
                SetVideoCaptureType(VideoType.CustomTexture, null);
                break;
            default:
                break;
        }
        print("InitVideo OK [" + "VideoRes:" + VideoResolution + ",Quality:" + VideoQuality
            + ",Framerate:" + Framerate+"]");
    }

    public void SetVideoCaptureType(VideoType type, Camera captureCamera)
    {
        VideoType = type;
        bool result= UnityChatSDK.Instance.SetVideoCaptureType(type, captureCamera);
        if (result == false)
        {
            print("SetVideoCaptureType Failed!");
        }
    }
    public void OnResolutionValueChanged(Dropdown dp) 
    {
        SetResolution((VideoResolution)dp.value);
    }
    public void SetResolution(VideoResolution r) 
    {
        VideoResolution = r;
        UnityChatSDK.Instance.SetResolution(r);
        InitVideo();
    }
    public void OnVideoQualityValueChanged(Dropdown dp) 
    {
        SetVideoQuality((VideoQuality)dp.value);
    }
    public void SetVideoQuality(VideoQuality q)  
    {
        VideoQuality = q;
        UnityChatSDK.Instance.SetVideoQuality(q);
    }

    public void SetAudioEnable(Toggle tog)
    {
        print("audioEnable:"+ tog.isOn);
        UnityChatSDK.Instance.SetAudioEnable(tog.isOn);
    }
    public void SetVideoEnable(Toggle tog)
    {
        print("videoEnable:" + tog.isOn);
        UnityChatSDK.Instance.SetVideoEnable(tog.isOn);
    }
    /// <summary>
    /// Switch camera when the number of device cameras available>2
    /// </summary>
    public void SwitchCam() 
    {
        if (VideoType == VideoType.DeviceCamera) 
        {
            UnityChatSDK.Instance.SwitchCam();
        }
    }
    public void SetFrontCam() 
    {
        if (VideoType == VideoType.DeviceCamera)
        {
            bool result = UnityChatSDK.Instance.SetCamFrontFacing();
            print("SetFrontCam:" + result);
        }
    }
    /// <summary>
    /// Set the video capture type to the video captured by device camera
    /// </summary>
    public void SetDeciveCam()
    {
        SetVideoCaptureType(VideoType.DeviceCamera, CaptureCamera);
    }
    /// <summary>
    /// Set the video capture type to the video rendered by Unity Camera
    /// </summary>
    public void SetUnityCam()
    {
        SetVideoCaptureType(VideoType.UnityCamera, CaptureCamera);
    }
    /// <summary>
    /// Set customTexture capture type ,send video by "UpdateCustomTexture" API of UnityChatSDK
    /// </summary>
    public void SetCustomTexture()
    {
        SetVideoCaptureType(VideoType.CustomTexture, CaptureCamera);
    }
}
