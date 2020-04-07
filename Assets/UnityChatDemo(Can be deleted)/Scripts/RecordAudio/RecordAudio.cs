using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordAudio : MonoBehaviour {

    /// <summary>
    /// Maximum recording time, timeout automatically stop
    /// </summary>
    public int LimitRecordTime=60;

    public void StartRecordAudio()
    {
        UnityChatSDK.Instance.StartRecordAudio(LimitRecordTime, OnFininshed);
    }
    public void StopRecordAudio()
    {
        UnityChatSDK.Instance.StopRecordAudio();
    }

    byte[] audioData;
    public void OnFininshed(byte[] recordData)
    {
        audioData = recordData;
    }
    public void PlayRecordAudio()
    {
        if (audioData != null)
            UnityChatSDK.Instance.PlayRecordAudio(audioData);
    }
}
