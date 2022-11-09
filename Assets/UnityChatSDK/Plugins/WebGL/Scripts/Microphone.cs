using System.Runtime.InteropServices;
using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using AOT;

namespace WebglMic.WebglMicrophone
{
#if UNITY_WEBGL
    public class Microphone
    {
        public static event Action RecordStartedEvent;
        public static event Action RecordEndedEvent;
        public static event Action<float[]> RecordStreamDataEvent;
        public static event Action<bool> PermissionStateChangedEvent;

        public static bool Logging = true;

        public delegate void NativeWebGLCallback(string data);
        public delegate void NativeWebGLFloatCallback(IntPtr data, int length);

        #region __Internal

        [DllImport("__Internal")]
        private static extern int init(double version, int worklet);
        [DllImport("__Internal")]
        private static extern int devices(NativeWebGLCallback callback);
        [DllImport("__Internal")]
        private static extern void start(string deviceId, int frequency, NativeWebGLCallback callback);
        [DllImport("__Internal")]
        private static extern void end(string deviceId, NativeWebGLCallback callback);
        [DllImport("__Internal")]
        private static extern int isRecording(string deviceId);
        [DllImport("__Internal")]
        private static extern string getDeviceCaps(string deviceId);
        [DllImport("__Internal")]
        private static extern int isSupported();
        [DllImport("__Internal")]
        private static extern void requestPermission(NativeWebGLCallback callback);
        [DllImport("__Internal")]
        private static extern void isPermissionGranted(NativeWebGLCallback callback);
        [DllImport("__Internal")]
        private static extern void setRecordingBufferCallback(NativeWebGLFloatCallback callback);
        [DllImport("__Internal")]
        private static extern void getRecordingBuffer(NativeWebGLCallback callback);
       
        #endregion


        private static Microphone _Instance;
        public static Microphone Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new Microphone();

                return _Instance;
            }
        }

        private static List<Action> _EndRecordCallbacks = new List<Action>();

        private const int _AmountOfSamplesToUpdateAudioClip = 2048;

        private AudioClip _microphoneClip;
        private bool _loopRecording;
        private int _frequency;
        private float[] _audioDataArray;
        private CultureInfo _provider;
        private string[] _microphoneDevices;
        private bool _permissionGranted;
        private int _samplePosition;
        private int _setSamplesAmount;
        private MicrophoneDeviceInfo[] _microphoneDeviceInfos;

		public Microphone()
        {
            _microphoneDevices = new string[0];
            _microphoneDeviceInfos = new MicrophoneDeviceInfo[0];

#if UNITY_2021_1_OR_NEWER
            init(2021.1, 0);
#else
            init(0, 0);
#endif
            setRecordingBufferCallback(HandleNativeWebGLFloatCallback);
        }

        /// <summary>
        /// Start Recording with device.
        /// </summary>
        /// <param name="deviceName">The name of the device. (not uses)</param>
        /// <param name="loop">Indicates whether the recording should continue recording if lengthSec is reached, and wrap around and record from the beginning of the AudioClip.</param>
        /// <param name="lengthSec">Is the length of the AudioClip produced by the recording.</param>
        /// <param name="frequency">The sample rate of the AudioClip produced by the recording.</param>
        /// <returns>The function returns null if the recording fails to start.</returns>
        public AudioClip Start(string deviceId, bool loop, int lengthSec, int frequency)
        {
            if (IsRecording(deviceId))
                return _microphoneClip;

            Cleanup();

            _frequency = frequency;
            _loopRecording = loop;
            _microphoneClip = AudioClip.Create("Microphone", _frequency * lengthSec, 1, _frequency, false);
            _audioDataArray = new float[_frequency * lengthSec];

            start(deviceId, _frequency, HandleNativeWebGLCallback);

            return _microphoneClip;
        }

        /// <summary>
        /// Query if a device is currently recording.
        /// </summary>
        /// <param name="deviceName">The name of the device. (not uses)</param>
        /// <returns></returns>
        public bool IsRecording(string deviceId)
        {
            return isRecording(deviceId) == 1;
        }

        public void GetDeviceCaps(string deviceId, out int minFreq, out int maxfreq)
        {
            int[] array = JsonUtility.FromJson<ArrayObjectClass<int>>(getDeviceCaps(deviceId)).array;
            minFreq = array[0];
            maxfreq = array[1];
        }

        /// <summary>
        /// Get the position in samples of the recording. 
        /// </summary>
        /// <param name="deviceName">The name of the device (not uses)</param>
        /// <returns></returns>
        public int GetPosition(string deviceName)
        {
            return _samplePosition;
        }

        /// <summary>
        /// Stops recording.
        /// </summary>
        /// <param name="deviceName">The name of the device. (not uses)</param>
        public void End(string deviceId)
        {
            end(deviceId, HandleNativeWebGLCallback);
        }

        public bool HasConnectedMicrophoneDevices()
        {
            return GetMicrophoneDevices().Length > 0;
        }

        /// <summary>
        /// Returns a list of available microphone devices, identified by name
        /// </summary>
        /// <returns></returns>
        public string[] GetMicrophoneDevices()
        {
            return _microphoneDevices;
        }

        /// <summary>
        /// Requests permission for using media devices
        /// </summary>
        public void RequestPermission()
        {
            requestPermission(HandleNativeWebGLCallback);
        }

        public bool HasUserAuthorizedPermission()
        {
           // isPermissionGranted(HandleNativeWebGLCallback);
            return _permissionGranted;
        }

        public void RefreshMicrophoneDevices()
        {
            devices(HandleNativeWebGLCallback);
        }

        /// <summary>
        /// Returns RAW data (samples array) of an AudioClip; This is the full array of samples that could be not filled fully by audio stream.
        /// </summary>
        /// <returns></returns>
        public float[] GetRawData()
        {
            return _audioDataArray;
        }

        /// <summary>
        /// Cleanups service
        /// </summary>
        public void Dispose()
        {
            _Instance = null;
            Cleanup();
        }

        public string GetDeviceIdByName(string deviceName)
		{
            return Array.Find(_microphoneDeviceInfos, item => item.label == deviceName)?.deviceId;
        }

        public void RegisterEndRecordCallback(Action callback)
		{
            if(callback != null)
                _EndRecordCallbacks.Add(callback);
        }

        private void Cleanup()
        {
            if (_microphoneClip != null)
                MonoBehaviour.Destroy(_microphoneClip);
            _audioDataArray = null;
            _samplePosition = 0;
        }

        [MonoPInvokeCallback(typeof(NativeWebGLFloatCallback))]
        public static void HandleNativeWebGLFloatCallback(IntPtr data, int length)
        {
            float[] stream = new float[length];
            Marshal.Copy(data, stream, 0, length);
            
            Instance.WriteBufferFromMicrophoneHandler(stream);
            RecordStreamDataEvent?.Invoke(stream);
        }

        [MonoPInvokeCallback(typeof(NativeWebGLCallback))]
        public static void HandleNativeWebGLCallback(string json)
		{
            try
            {
                CallbackDataModel model = JsonUtility.FromJson<CallbackDataModel>(json);

                switch ((CallbackDataModelType)Enum.Parse(typeof(CallbackDataModelType), model.type))
                {
                    case CallbackDataModelType.devices:
                        {
                            if (model.status)
                            {
                                Instance._microphoneDeviceInfos = JsonUtility.FromJson<ArrayObjectClass<MicrophoneDeviceInfo>>(model.data).array;
                                Instance._microphoneDevices = Instance._microphoneDeviceInfos.Select(item => item.label).ToArray();

                                if (Instance._microphoneDeviceInfos.Length > 0)
                                {
                                    Instance._permissionGranted = Instance._microphoneDeviceInfos[0].isGrantedAccess;
                                }
                            }
                            else
                            {
                                if (Logging)
                                {
                                    UnityEngine.Debug.Log(model.data);
                                }
                            }
                        }
                        break;
                    case CallbackDataModelType.isPermissionGranted:
                        {
                            bool was = Instance._permissionGranted;
                            Instance._permissionGranted = model.status;

                            if (Logging)
                            {
                                if (!Instance._permissionGranted)
                                {
                                    UnityEngine.Debug.Log(model.data);
                                }
                            }

                            if(was != Instance._permissionGranted)
                                PermissionStateChangedEvent?.Invoke(Instance._permissionGranted);
                        }
                        break;
                    case CallbackDataModelType.requestPermission:
                        {
                            Instance._permissionGranted = model.status;
                            if (Logging)
                            {
                                UnityEngine.Debug.Log(model.data);
                            }
                        }
                        break;
                    case CallbackDataModelType.start:
                        {
                            if (Logging)
                            {
                                UnityEngine.Debug.Log(model.data);
                            }

                            Instance._setSamplesAmount = 0;

                            RecordStartedEvent?.Invoke();
                        }
                        break;
                    case CallbackDataModelType.end:
                        {
                            if (Logging)
                            {
                                UnityEngine.Debug.Log(model.data);
                            }

                            //refresh data when record ended
                            Instance._setSamplesAmount = 0;
                            if (Instance._microphoneClip != null && Instance._microphoneClip)
                            {
                                Instance._microphoneClip.SetData(Instance._audioDataArray, 0);
                            }

                            RecordEndedEvent?.Invoke();

                            foreach (var item in _EndRecordCallbacks)
                                item?.Invoke();
                            _EndRecordCallbacks.Clear();
                        }
                        break;
       //             case CallbackDataModelType.recordingBufferCallback:
       //                 {
       //                     if (model.status)
       //                     {
       //                         float[] array = JsonUtility.FromJson<ArrayObjectClass<float>>(model.data).array;
       //                         Instance.WriteBufferFromMicrophoneHandler(array);
       //                         RecordStreamDataEvent?.Invoke(array);
       //                     }
							//else
							//{
       //                         if (Logging)
       //                         {
       //                             UnityEngine.Debug.Log(model.data);
       //                         }
       //                     }
       //                 }
       //                 break;
                }
            }
            catch(Exception ex)
			{
                if (Logging)
                {
                    Debug.LogException(ex);
                }
			}
        }

        /// Event handler from JS library
        /// </summary>
        /// <param name="data"></param>
        private void WriteBufferFromMicrophoneHandler(float[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (!_loopRecording && _samplePosition == _audioDataArray.Length)
                    break;

                _audioDataArray[_samplePosition] = array[i];

                if (_loopRecording)
                {
                    _samplePosition = (int)Mathf.Repeat(_samplePosition + 1, _audioDataArray.Length);
                }
                else
                {
                    _samplePosition++;
                }
            }

            _setSamplesAmount += array.Length;

            if (_setSamplesAmount >= _AmountOfSamplesToUpdateAudioClip)
            {
                _setSamplesAmount = 0;

                if (_microphoneClip != null && _microphoneClip)
                {
                    _microphoneClip.SetData(_audioDataArray, 0);
                }
            }
        }

        [Serializable]
        private class ArrayObjectClass<T>
		{
            public T[] array;

#if UNITY_2018_4_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            public ArrayObjectClass()
            {
                array = new T[0];
            }
        }

        [Serializable]
        private class MicrophoneDeviceInfo
        {
            public string deviceId;
            public string kind;
            public string label;
            public string groupId;
            public bool isGrantedAccess;

#if UNITY_2018_4_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            public MicrophoneDeviceInfo()
            {
            }
#if UNITY_2018_4_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            public MicrophoneDeviceInfo(string deviceId, string kind, string label, string groupId, bool isGrantedAccess)
            {
                this.deviceId = deviceId;
                this.kind = kind;
                this.label = label;
                this.groupId = groupId;
                this.isGrantedAccess = isGrantedAccess;
            }
        }

        [Serializable]
        private class CallbackDataModel
		{
            public bool status;
            public string data;
            public string type;

#if UNITY_2018_4_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            public CallbackDataModel()
            {
            }

#if UNITY_2018_4_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            public CallbackDataModel(bool status, string data, string type)
            {
                this.status = status;
                this.data = data;
                this.type = type;
            }
        }

        private enum CallbackDataModelType
		{
            devices,
            isPermissionGranted,
            requestPermission,
            start,
            end,
            recordingBufferCallback
        }
    }
#endif
        }