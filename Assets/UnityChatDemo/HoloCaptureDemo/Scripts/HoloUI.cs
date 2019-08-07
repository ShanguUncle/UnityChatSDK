using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoloUI : MonoBehaviour {

    public Tagalong Tagalong;
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
		
	}
    public void Lock()
    {
        Tagalong.enabled = false;
    }
    public void UnLock()
    {
        Tagalong.enabled = true;
    }
}
