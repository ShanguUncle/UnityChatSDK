#if UNITY_EDITOR ||UNITY_WSA
using HoloCapture;
#endif

using System.Collections.Generic;
using UnityEngine;
public class HoloCaptureManager : MonoBehaviour {

    public static HoloCaptureManager Instance;
    private void Awake()
    {
        Instance = this;
    }
#if UNITY_EDITOR ||UNITY_WSA
    //https://docs.microsoft.com/zh-cn/windows/mixed-reality/locatable-camera

    public enum HoloCamFrame
    {
        Holo15,
        Holo30,

        HoloOne20,
        HoloOne24
    }
    public enum HoloResolution
    {
        Holo_896x504,
        Holo_1280x720,

        HoloOne_1344x756,
        HoloOne_1408x792,

        HoloTwo_424x240,
        HoloTwo_500x282,
        HoloTwo_640x360,
        HoloTwo_760x428,
        HoloTwo_960x540,
        HoloTwo_1128x636,
        HoloTwo_1920x1080,
        HoloTwo_2272x1278
    }

    public HoloResolution holoResolution;
    public HoloCamFrame holoFrame;
    HoloCapture.Resolution resolution;
    public bool EnableHolograms = true;
    public bool HorizontalMirror;
    [Range(0,1)]
    public float Opacity=0.9f;


    public Texture2D _videoTexture { get; set; }


    void Start()
    {
        Init();
    }
    public void Init()
    {
        switch (holoResolution)
        {
            case HoloResolution.Holo_896x504:
                resolution = new HoloCapture.Resolution(896, 504);
                break;
            case HoloResolution.Holo_1280x720:
                resolution = new HoloCapture.Resolution(1280,720);
                break;
            case HoloResolution.HoloOne_1344x756:
                resolution = new HoloCapture.Resolution(1344,756);
                break;
            case HoloResolution.HoloOne_1408x792:
                resolution = new HoloCapture.Resolution(1408,792);
                break;
            case HoloResolution.HoloTwo_424x240:
                resolution = new HoloCapture.Resolution(424,240);
                break;
            case HoloResolution.HoloTwo_500x282:
                resolution = new HoloCapture.Resolution(500,282);
                break;
            case HoloResolution.HoloTwo_640x360:
                resolution = new HoloCapture.Resolution(640,360);
                break;
            case HoloResolution.HoloTwo_760x428:
                resolution = new HoloCapture.Resolution(760,428);
                break;
            case HoloResolution.HoloTwo_960x540:
                resolution = new HoloCapture.Resolution(960,540);
                break;
            case HoloResolution.HoloTwo_1128x636:
                resolution = new HoloCapture.Resolution(1128,636);
                break;
            case HoloResolution.HoloTwo_1920x1080:
                resolution = new HoloCapture.Resolution(1920,1080);
                break;
            default:
                resolution = new HoloCapture.Resolution(896, 504);
                break;

        }
        int frame;
        switch (holoFrame)
        {
            case HoloCamFrame.Holo15:
                frame = 15;
                break;
            case HoloCamFrame.Holo30:
                frame = 30;
                break;
            case HoloCamFrame.HoloOne20:
                frame = 20;
                break;
            case HoloCamFrame.HoloOne24:
                frame = 24;
                break;
            default:
                frame = 15;
                break;
        }
        HoloCaptureHelper.Instance.Init(resolution, frame, true, EnableHolograms, Opacity, false,
UnityEngine.XR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr(), OnFrameSampleCallback);

        _videoTexture = new Texture2D(resolution.width, resolution.height, TextureFormat.BGRA32, false);
    }

    private void OnDestroy()
    {
        HoloCaptureHelper.Instance.Destroy();
    }

    bool isStartCaputure;
    public void StartCapture()
    {
        if (isStartCaputure) return;
        isStartCaputure = true;
        HoloCaptureHelper.Instance.StartCapture();
    }
    public void StopCapture()
    {
        if (!isStartCaputure) return;
        isStartCaputure = false;
        HoloCaptureHelper.Instance.StopCapture();
    }

    void OnFrameSampleCallback(VideoCaptureSample sample)
    {

        byte[] imageBytes = new byte[sample.dataLength];

        sample.CopyRawImageDataIntoBuffer(imageBytes);

        sample.Dispose();

        //图像水平是镜像的！
        UnityEngine.WSA.Application.InvokeOnAppThread(() =>
        {
            if (HorizontalMirror) {ImageMirror(imageBytes); }
            _videoTexture.LoadRawTextureData(imageBytes);
            _videoTexture.wrapMode = TextureWrapMode.Clamp;
            _videoTexture.Apply();
            UnityChatSDK.Instance.UpdateCustomTexture(_videoTexture);

        }, false);


    }
    void ImageMirror(byte[] imageBytes)
    {
        int PixelSize = 4;
        int width = resolution.width;
        int height = resolution.height;
        int Line = width * PixelSize;

        for (int i = 0; i < height; ++i)
        {
            for (int j = 0; j + 4 < Line / 2; j += 4)
            {
                Swap<byte>(ref imageBytes[Line * i + j], ref imageBytes[Line * i + Line - j - 4]);
                Swap<byte>(ref imageBytes[Line * i + j + 1], ref imageBytes[Line * i + Line - j - 3]);
                Swap<byte>(ref imageBytes[Line * i + j + 2], ref imageBytes[Line * i + Line - j - 2]);
                Swap<byte>(ref imageBytes[Line * i + j + 3], ref imageBytes[Line * i + Line - j - 1]);
            }
        }
    }
    void Swap<T>(ref T lhs, ref T rhs)
    {
        T temp;
        temp = lhs;
        lhs = rhs;
        rhs = temp;
    }
#else
    public void StartCapture()
    {
    }
    public void StopCapture()
    {
    }
#endif
}
