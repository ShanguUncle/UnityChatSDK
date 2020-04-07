using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 通讯计时
/// </summary>
public class TimeKeeping : MonoBehaviour
{
    public static TimeKeeping _instance;
    public bool IsStart{ get; private set; }
    public float CurTime { get; private set; }
    public Text TimeText;

    private void Awake()
    {
        _instance = this;
    }
    void Start ()
    {
    }
	
	void FixedUpdate ()
    {
        if (!IsStart) return;
        CurTime += Time.deltaTime;
	    TranMinite(CurTime);
	}

    /// <summary>
    /// 开始计时
    /// </summary>
    public void StartTime()
    {
        IsStart = true;
        CurTime = 0;
    }
    /// <summary>
    /// 停止计时
    /// </summary>
    public void StopTime()
    {
        IsStart = false;
        TimeText.text = "";
    }

    private void TranMinite(float f)
    {
        int min = (int) (f/60);
        string mins = "";
        if (min < 10) { mins = "0" + min.ToString(); } else { mins= min.ToString(); }
        string secs = "";
        int sec = ((int)f) % 60;
        if (sec < 10) { secs = "0" + sec.ToString(); } else { secs = sec.ToString(); }
        TimeText.text = mins+":"+secs;
    }
}
