using WebglMic.Plugins.Native;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif
public class UnityChatSdkProxy : MonoBehaviour
{
    public static UnityChatSdkProxy Instance;
    private void Awake()
    {
        Instance = this;
    }


    int TimeSecond = 3;
    bool isRecordAudio;
    int lastMicPosition;
    float[] audioRecordData;

    private void Start()
    {
   
    }
    public void RequestUserPermission()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);
            }
#endif
        }
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            try
            {
                CustomMicrophone.RequestMicrophonePermission();
                CustomMicrophone.RefreshMicrophoneDevices();

                if (!Application.HasUserAuthorization(UserAuthorization.WebCam)) Application.RequestUserAuthorization(UserAuthorization.WebCam);
            }
            catch (Exception)
            {

            }
        }
        else
        {
            if (!Application.HasUserAuthorization(UserAuthorization.Microphone)) Application.RequestUserAuthorization(UserAuthorization.Microphone);
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam)) Application.RequestUserAuthorization(UserAuthorization.WebCam);
        }
    }

    public void StartCaptureWebGL() 
    {
        if (Application.platform != RuntimePlatform.WebGLPlayer) return;

        if (isRecordAudio) return;

        if (!CustomMicrophone.HasMicrophonePermission()) 
        {
            CustomMicrophone.RequestMicrophonePermission();
        }

        if (CustomMicrophone.devices.Length == 0) return;

        CustomMicrophone.Start(CustomMicrophone.devices[0], true, TimeSecond, UnityChatSDK.Instance.AudioFrequency);

        isRecordAudio = true;
        lastMicPosition = 0;
 
        StartCoroutine(Record());
        print("StartCaptureWebGL");
    }
    IEnumerator Record()
    {
        yield return new WaitForSecondsRealtime(0.05f);
        while (isRecordAudio)
        {
            yield return null;
            //yield return new WaitForEndOfFrame();
            RecordMicLocal();
        }
    }
    void RecordMicLocal()
    {
        if (!isRecordAudio) return;

        int micPosition = CustomMicrophone.GetPosition(CustomMicrophone.devices[0]);

        int audioLength = micPosition - lastMicPosition;

        if (audioLength < 0) audioLength = UnityChatSDK.Instance.AudioFrequency * TimeSecond - lastMicPosition + micPosition;

        if (audioLength < UnityChatSDK.Instance.AudioFrequency / UnityChatSDK.Instance.AudioSample)
        {
            return;
        }

        audioRecordData = new float[audioLength];

        float[] array = new float[0];
        CustomMicrophone.GetRawData(ref array);
        List<float> buffer = new List<float>();
        if (lastMicPosition > micPosition)
        {
            buffer.AddRange(array.ToList().GetRange(lastMicPosition, array.Length - lastMicPosition));
            buffer.AddRange(array.ToList().GetRange(0, micPosition));
        }
        else
        {
            buffer.AddRange(array.ToList().GetRange(lastMicPosition, micPosition - lastMicPosition));
        }
        audioRecordData = buffer.ToArray();

        if (audioRecordData == null || audioRecordData.Length != audioLength) return;

        if (UnityChatSDK.Instance.MicVolumeScale != 1)
        {
            for (int index = 0; index < audioRecordData.Length; ++index)
                audioRecordData[index] = audioRecordData[index] *= UnityChatSDK.Instance.MicVolumeScale;
        }


        UnityChatSDK.Instance.SetAudio(lastMicPosition, audioRecordData);

        lastMicPosition = micPosition;
    }

    public void StopCaptureWebGL()
    {
        if (Application.platform != RuntimePlatform.WebGLPlayer) return;

        if (!isRecordAudio) return;

        isRecordAudio = false;
        if (!CustomMicrophone.IsRecording(null)) return;
        if (CustomMicrophone.devices.Length == 0) return;
        CustomMicrophone.End(CustomMicrophone.devices[0]);
        print("StopCaptureWebGL");
    }

    internal void SetAudioEnable(bool isOn)
    {
        if (isOn)
        {
            StartCaptureWebGL();
        }
        else
        {
            StopCaptureWebGL();
        }
    }
}
