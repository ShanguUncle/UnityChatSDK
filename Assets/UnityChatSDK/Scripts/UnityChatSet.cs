using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
/// <summary>
/// UnityChatSDK参数设置
/// </summary>
public class UnityChatSet: MonoBehaviour {

    //声音音量播放大小
    public  float AudioVolume= 1f;
	
	//麦克风音量大小
	public  float MicVolumeScale= 1f;
	   
    public VideoType VideoType = VideoType.DeviceCamera;

    //音视频分辨率
    public VideoResolution VideoResolution = VideoResolution._360P; 
    //音视频压缩质量
    public VideoQuality VideoQuality = VideoQuality.Middle;
    //视频刷新率
    [Range(5,25)]
    public int Framerate = 20;

    public bool EchoCancellation;

    [Tooltip("check video frame static")]
    public bool EnableDetection; //检测通话视频是否静帧
    //视频和音频帧同步
    public bool EnableSync;
    /// <summary>
    /// 当选择采集Unity Camera的画面时，选择要采集的unity的摄像机
    /// </summary>
    public Camera CaptureCamera;
    /// <summary>
    /// 聊天对象的视频画面显示
    /// </summary>
    public RawImage[] ChatPeerRawImage;
    /// <summary>
    /// 本地采集画面的回显
    /// </summary>
    public RawImage SelfRawImage;

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
        //初始化视频(摄像头Index)
        UnityChatSDK.Instance.InitVideo(0);
        UnityChatSDK.Instance.SetVideoQuality(VideoQuality);
        UnityChatSDK.Instance.SetResolution(VideoResolution);
        switch (VideoType)
        {
            case VideoType.DeviceCamera:
                SetVideoCaptureType(VideoType.DeviceCamera, null);
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
        UnityChatSDK.Instance.SetSelfRawImage(SelfRawImage);

        print("InitVideo OK [" + "VideoRes:" + VideoResolution + ",Quality:" + VideoQuality
            + ",Framerate:" + Framerate+"]");
    }
    /// <summary>
    /// 选择要采集的视频类型（注：未注册不支持Unity Camera）
    /// </summary>
    /// <param name="type">  DeviceCamera是外表摄像头的画面 UnityCamera是Unity Camera渲染的画面</param>
    /// <param name="captureCamera"></param>
    public void SetVideoCaptureType(VideoType type, Camera captureCamera)
    {
        UnityChatSDK.Instance.SetVideoCaptureType(type, captureCamera);
    }
    public void OnResolutionValueChanged(Dropdown dp) 
    {
        SetResolution((VideoResolution)dp.value);
    }
    public void SetResolution(VideoResolution r) 
    {
        VideoResolution = r;
        UnityChatSDK.Instance.SetResolution(r);
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

    bool audioEnable;
    public void SetAudioEnable()
    {
        print("audioEnable:"+ audioEnable);
        UnityChatSDK.Instance.SetAudioEnable(audioEnable);
        audioEnable = !audioEnable;
    }
    bool videoEnable; 
    public void SetVideoEnable()
    {
        print("videoEnable:" + videoEnable);
        UnityChatSDK.Instance.SetVideoEnable(videoEnable);
        videoEnable = !videoEnable;
    }
    /// <summary>
    /// 当外部可用摄像头的数量>2时，如手机端前后摄像头,改变要捕捉的外部摄像头
    /// </summary>
    public void SwitchCam() 
    {
        UnityChatSDK.Instance.SwitchCam();
    }
    public void SetFrontCam() 
    {
        bool result= UnityChatSDK.Instance.SetCamFrontFacing();
        print("SetFrontCam:"+result);
    }
    /// <summary>
    /// 设置视频采集类型为外部摄像头捕捉的画面
    /// </summary>
    public void SetDeciveCam()
    {
        SetVideoCaptureType(VideoType.DeviceCamera, null);
    }
    /// <summary>
    /// 设置视频采集类型为Unity Camera渲染的画面
    /// </summary>
    public void SetUnityCam()
    {
        SetVideoCaptureType(VideoType.UnityCamera, CaptureCamera);
    }
}
