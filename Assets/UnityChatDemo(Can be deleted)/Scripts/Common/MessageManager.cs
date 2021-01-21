using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Message show manager
/// </summary>
public class MessageManager : MonoBehaviour
{
    public static MessageManager Instance;
    public GameObject MessgageIamge;
    public Text MessgageText;
    public GameObject SelectBox;
    public Text WarnText;
    Action yesAction;
    Action noAction;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {

    }
    /// <summary>
    /// Show message
    /// </summary>

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
    /// Show selection box
    /// </summary>
    /// <param name="warn">warn</param>
    /// <param name="yes">Callback method when YES is selected</param>
    /// <param name="no">Callback method when No is selected</param>
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
