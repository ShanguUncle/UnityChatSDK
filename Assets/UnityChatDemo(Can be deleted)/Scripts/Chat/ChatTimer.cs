using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatTimer : MonoBehaviour
{

    public Text TimeText;

    bool isStart;
    DateTime chatTime;
    private void Awake()
    {

    }
    void Start()
    {

    }
    private void OnEnable()
    {
        StartTime();
    }
    private void OnDisable()
    {
        StopTime();
    }
    public void StartTime()
    {
        isStart = true;
        chatTime = DateTime.Now;
        StartCoroutine(StartTimeing());
    }
    IEnumerator StartTimeing()
    {
        while (isStart)
        {
            yield return new WaitForSeconds(1);
            TimeText.text = (DateTime.Now - chatTime).ToString(@"hh\:mm\:ss");
        }
    }
    /// <summary>
    /// 停止计时
    /// </summary>
    public void StopTime()
    {
        isStart = false;
        TimeText.text = "";
    }
}
