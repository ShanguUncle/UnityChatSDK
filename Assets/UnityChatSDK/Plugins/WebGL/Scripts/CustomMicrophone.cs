using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WebglMic.Plugins.Native
{
	public sealed class CustomMicrophone
	{
		private static float[] _SamplesArrayBuffer = new float[0];

		public static event Action RecordStartedEvent;
		public static event Action RecordEndedEvent;
		/// <summary>
		/// Works only in WebGL! Sends chunk of recorded data.
		/// </summary>
		public static event Action<float[]> RecordStreamDataEvent;
		/// <summary>
		/// Works only in WebGL! Fire when permission to microphone was changed.
		/// </summary>
		public static event Action<bool> PermissionStateChangedEvent;

		static CustomMicrophone()
		{
#if UNITY_WEBGL //&& !UNITY_EDITOR
			WebglMicrophone.Microphone.RecordStartedEvent += RecordStartedEventHandler;
			WebglMicrophone.Microphone.RecordEndedEvent += RecordEndedEventHandler;
			WebglMicrophone.Microphone.RecordStreamDataEvent += RecordStreamDataEventHandler;
			WebglMicrophone.Microphone.PermissionStateChangedEvent += PermissionStateChangedEventHandler;
#endif
		}

		public static string[] devices
		{
#if UNITY_WEBGL //&& !UNITY_EDITOR
			get { return WebglMicrophone.Microphone.Instance.GetMicrophoneDevices(); }
#else
			get { return Microphone.devices; }
#endif
		}

		public static AudioClip Start(string deviceName, bool loop, int lengthSec, int frequency)
		{
#if UNITY_WEBGL //&& !UNITY_EDITOR
			string deviceId = WebglMicrophone.Microphone.Instance.GetDeviceIdByName(deviceName);
			return WebglMicrophone.Microphone.Instance.Start(deviceId, loop, lengthSec, frequency);
#else
			var clip = Microphone.Start(deviceName, loop, lengthSec, frequency);
			RecordStartedEvent?.Invoke();
			return clip;
#endif
		}

		public static bool IsRecording(string deviceName)
		{
#if UNITY_WEBGL //&& !UNITY_EDITOR
			string deviceId = WebglMicrophone.Microphone.Instance.GetDeviceIdByName(deviceName);
			return WebglMicrophone.Microphone.Instance.IsRecording(deviceId);
#else
			return Microphone.IsRecording(deviceName);
#endif
		}

		public static void GetDeviceCaps(string deviceName, out int minFreq, out int maxfreq)
		{
#if UNITY_WEBGL //&& !UNITY_EDITOR
			string deviceId = WebglMicrophone.Microphone.Instance.GetDeviceIdByName(deviceName);
			WebglMicrophone.Microphone.Instance.GetDeviceCaps(deviceId, out minFreq, out maxfreq);
#else
			Microphone.GetDeviceCaps(deviceName, out minFreq, out maxfreq);
#endif
		}

		public static int GetPosition(string deviceName)
		{
#if UNITY_WEBGL //&& !UNITY_EDITOR
			string deviceId = WebglMicrophone.Microphone.Instance.GetDeviceIdByName(deviceName);
			return WebglMicrophone.Microphone.Instance.GetPosition(deviceId);
#else
			return Microphone.GetPosition(deviceName);
#endif
		}

		public static void End(string deviceName, Action callback = null)
		{
#if UNITY_WEBGL //&& !UNITY_EDITOR
			WebglMicrophone.Microphone.Instance.RegisterEndRecordCallback(callback);
			string deviceId = WebglMicrophone.Microphone.Instance.GetDeviceIdByName(deviceName);
			WebglMicrophone.Microphone.Instance.End(deviceId);
#else
			Microphone.End(deviceName);
			RecordEndedEvent?.Invoke();
			callback?.Invoke();
#endif
		}

		public static bool HasConnectedMicrophoneDevices()
		{
#if UNITY_WEBGL //&& !UNITY_EDITOR
			return WebglMicrophone.Microphone.Instance.HasConnectedMicrophoneDevices();
#else
			return Microphone.devices.Length > 0;
#endif
		}

		public static void RequestMicrophonePermission()
		{
			if (!HasMicrophonePermission())
			{
#if UNITY_ANDROID
				UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
#elif UNITY_IOS
				Application.RequestUserAuthorization(UserAuthorization.Microphone);
#elif UNITY_WEBGL //&& !UNITY_EDITOR
				WebglMicrophone.Microphone.Instance.RequestPermission();
#endif
			}
		}

		public static bool HasMicrophonePermission()
		{
#if UNITY_ANDROID
			return UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone);
#elif UNITY_IOS
			return Application.HasUserAuthorization(UserAuthorization.Microphone);
#elif UNITY_WEBGL //&& !UNITY_EDITOR
			return WebglMicrophone.Microphone.Instance.HasUserAuthorizedPermission();
#else
			return true;
#endif
		}

		public static bool GetRawData(ref float[] output, AudioClip source = null)
		{
#if UNITY_WEBGL //&& !UNITY_EDITOR
			output = WebglMicrophone.Microphone.Instance.GetRawData();
			return true;
#else
			if (source == null)
				return false;
			// this is the very slow function. please dont use it every frame
			source.GetData(output, 0);
			return true;
#endif
		}

		public static void RefreshMicrophoneDevices()
		{
#if UNITY_WEBGL //&& !UNITY_EDITOR
			WebglMicrophone.Microphone.Instance.RefreshMicrophoneDevices();
#endif
		}

		/// <summary>
		/// Detect voice based on threshold
		/// </summary>
		/// <param name="data">input bytes data</param>
		/// <param name="averageVoiceLevel">ref value of current voice level</param>
		/// <param name="threshold">threshold filter</param>
		/// <returns></returns>
		public static bool IsVoiceDetected(float[] samples, ref float averageVoiceLevel, double threshold = 0.02d)
		{
			return IsVoiceDetectedProcess(samples, ref averageVoiceLevel, threshold);
		}

		/// <summary>
		/// Detect voice based on threshold
		/// </summary>
		/// <param name="data">input bytes data</param>
		/// <param name="averageVoiceLevel">ref value of current voice level</param>
		/// <param name="threshold">threshold filter</param>
		/// <returns></returns>
		public static bool IsVoiceDetected(string deviceName, AudioClip audioClip, ref float averageVoiceLevel, double threshold = 0.02d)
		{
			if (IsRecording(deviceName) && audioClip != null && audioClip)
			{
				if (_SamplesArrayBuffer.Length != audioClip.samples)
				{
					Array.Resize(ref _SamplesArrayBuffer, audioClip.samples);
				}

				if (GetRawData(ref _SamplesArrayBuffer, audioClip))
				{
					int position = GetPosition(deviceName);

					int amount = audioClip.frequency;

					if (position >= amount)
					{
						int startIndex = position - amount;
						int count = amount;

						if (startIndex + count >= _SamplesArrayBuffer.Length)
						{
							count = _SamplesArrayBuffer.Length - startIndex;
						}

						float[] samplesChunk = new float[count];
						for (int i = 0; i < samplesChunk.Length; i++)
						{
							samplesChunk[i] = _SamplesArrayBuffer[startIndex + i];
						}
						return IsVoiceDetected(samplesChunk, ref averageVoiceLevel, threshold);
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Convert float array of RAW samples into bytes array
		/// </summary>
		/// <param name="samples"></param>
		/// <returns></returns>
		public static byte[] FloatToByte(float[] samples)
		{
			short[] intData = new short[samples.Length];

			byte[] bytesData = new byte[samples.Length * 2];

			for (int i = 0; i < samples.Length; i++)
			{
				intData[i] = (short)(samples[i] * 32767);
				byte[] byteArr = System.BitConverter.GetBytes(intData[i]);
				byteArr.CopyTo(bytesData, i * 2);
			}

			return bytesData;
		}

		/// <summary>
		/// Converts list of bytes to float array by using 32767 rescale factor
		/// </summary>
		/// <param name="bytesData"></param>
		/// <returns></returns>
		public static float[] ByteToFloat(byte[] bytesData)
		{
			int length = bytesData.Length / 2;
			float[] samples = new float[length];

			for (int i = 0; i < length; i++)
				samples[i] = (float)BitConverter.ToInt16(bytesData, i * 2) / 32767;

			return samples;
		}

		public static AudioClip MakeCopy(string name, int recordingTime, int frequency, AudioClip sourceClip)
		{
			float[] array = new float[recordingTime * frequency];
			if (CustomMicrophone.GetRawData(ref array, sourceClip))
			{
				AudioClip audioClip = AudioClip.Create(name, recordingTime * frequency, 1, frequency, false);
				audioClip.SetData(array, 0);

				return audioClip;
			}

			return null;
		}

		/// <summary>
		/// Filters data based on threshold
		/// </summary>
		/// <param name="data">input bytes data</param>
		/// <param name="averageVoiceLevel">ref value of current voice level</param>
		/// <param name="threshold">threshold filter</param>
		/// <returns></returns>
		private static bool IsVoiceDetectedProcess(float[] samples, ref float averageVoiceLevel, double threshold = 0.02d)
		{
			bool detected = false;
			double sumTwo = 0;
			double tempValue;

			for (int index = 0; index < samples.Length; index++)
			{
				tempValue = samples[index];

				sumTwo += tempValue * tempValue;

				if (tempValue > threshold)
					detected = true;
			}

			sumTwo /= samples.Length;

			averageVoiceLevel = (averageVoiceLevel + (float)sumTwo) / 2f;

			if (detected || sumTwo > threshold)
				return true;
			else
				return false;
		}

#if UNITY_WEBGL //&& !UNITY_EDITOR
		private static void RecordStartedEventHandler()
        {
			RecordStartedEvent?.Invoke();
		}

		private static void RecordEndedEventHandler()
		{
			RecordEndedEvent?.Invoke();
		}

		private static void RecordStreamDataEventHandler(float[] data)
		{
			RecordStreamDataEvent?.Invoke(data);
		}

		private static void PermissionStateChangedEventHandler(bool state)
		{
			PermissionStateChangedEvent?.Invoke(state);
		}
#endif
	}
}