using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UnityChatSDK参数设置
/// </summary>
public class UnityChatSet: MonoBehaviour {

    //声音音量衰减参数
    public  float AudioVolume= 1f;
    //声音采集阈值
    public  float AudioThreshold= 0.01f;
    //音视频分辨率
    public VideoRes VideoResolution= VideoRes._360P; 
    //音视频压缩质量
    public VideoQuality VideoQuality = VideoQuality.Middle;
    //视频刷新率
    [Range(5,20)]
    public int Framerate = 15;
    [Tooltip("check video frame static")]
    public bool EnableDetection; //检测通话视频是否静帧
    /// <summary>
    /// 当选择采集Unity Camera的画面时，选择要采集的unity的摄像机
    /// </summary>
    public Camera CapCamera;
    /// <summary>
    /// 聊天对象的视频画面显示
    /// </summary>
    public RawImage ChatPeerRawImage;
    /// <summary>
    /// 本地采集画面的回显
    /// </summary>
    public RawImage SelfRawImage;
    IEnumerator Start()
    {
        yield return new WaitUntil(() => UnityChatSDK.Instance != null);
        yield return new WaitUntil(() => ChatDataHandler.Instance != null);
        yield return new WaitUntil(() => UdpSocketManager._instance != null);
        yield return new WaitForSeconds(2);
        InitMic();
        InitVideo();
        SetDeciveCam();
    }
    //初始化音频
    void InitMic() 
    {
        UnityChatSDK.Instance.AudioVolume=AudioVolume;
        UnityChatSDK.Instance.AudioThreshold=AudioThreshold;
        UnityChatSDK.Instance.audioFrequency = 8000;
        //初始化音频
        UnityChatSDK.Instance.InitMic();
        print("初始化音频OK");

    }
    //初始化视频
    void InitVideo() 
    {
        UnityChatSDK.Instance.VideoRes = VideoResolution;
        UnityChatSDK.Instance.VideoQuality = VideoQuality;
        UnityChatSDK.Instance.Framerate = Framerate;
        UnityChatSDK.Instance.EnableDetection = EnableDetection;
        //初始化视频
        UnityChatSDK.Instance.InitVideo();

        print("streamSDK init OK!" + "--videoRes:" + UnityChatSDK.Instance.VideoRes + "--quality:" + UnityChatSDK.Instance.VideoQuality
            + "--Framerate:" + UnityChatSDK.Instance.Framerate);
    }
    /// <summary>
    /// 选择要采集的视频类型（注：未注册不支持Unity Camera）
    /// </summary>
    /// <param name="type">  DeviceCamera是外表摄像头的画面 UnityCamera是Unity Camera渲染的画面</param>
    /// <param name="captureCam"></param>
    public void SetVideoCaptureType(VideoType type, Camera captureCam)
    {
        UnityChatSDK.Instance.ChatPeerRawImage = ChatPeerRawImage;
        UnityChatSDK.Instance.SelfRawImage = SelfRawImage;

        UnityChatSDK.Instance.SetVideoCaptureType(type, captureCam);
    }
    /// <summary>
    /// 当外部可用摄像头的数量>2时，如手机端前后摄像头,改变要捕捉的外部摄像头
    /// </summary>
    public void ChangeDeviceCam()
    {
        UnityChatSDK.Instance.ChangeCam();
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
        SetVideoCaptureType(VideoType.UnityCamera, CapCamera);
    }

}
