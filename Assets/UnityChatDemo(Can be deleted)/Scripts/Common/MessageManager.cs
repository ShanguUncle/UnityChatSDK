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
    public Transform Layout;
    public GameObject MessgageIamge;
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
        GameObject go=GameObject.Instantiate(MessgageIamge, Layout);
        go.SetActive(true);
        Text t = go.transform.Find("Text").GetComponent<Text>();
        t.text = mes;
        go.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(622,t.preferredHeight+10);
        Destroy(go, showTime);
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
