using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 消息提示管理
/// </summary>
public class MessageManager : MonoBehaviour
{
    public static MessageManager _instance;
    public GameObject MessgageIamge;
    public Text MessgageText;
    public GameObject SelectBox;
    public Text WarnText;
    Action yesAction;
    Action noAction;
    private void Awake()
    {
        _instance = this;
    }
    void Start()
    {

    }
    /// <summary>
    /// 显示提示消息
    /// </summary>
    /// <param name="mes">提示语</param>
    /// <param name="showTime">显示时间</param>
    public void ShowMessage(string mes, float showTime = 2)
    {
        MessgageIamge.gameObject.SetActive(true);
        MessgageText.text = mes;
        Invoke("HideText", showTime);
    }
    void HideText()
    {
        MessgageText.text = "";
        MessgageIamge.gameObject.SetActive(false);
    }

    /// <summary>
    /// 显示提示选择框
    /// </summary>
    /// <param name="warn">提示语</param>
    /// <param name="yes">选择YES时回调的方法</param>
    /// <param name="no">选择NO时回调的方法</param>
    public void ShowSelectBox(string warn, Action yes, Action no)
    {
        SelectBox.SetActive(true);
        WarnText.text = warn;
        yesAction = yes;
        noAction = no;
    }
    public void Yes()
    {
        yesAction.Invoke();
        WarnText.text = "";
        SelectBox.SetActive(false);
    }
    public void No()
    {
        noAction.Invoke();
        WarnText.text = "";
        SelectBox.SetActive(false);
    }

}
