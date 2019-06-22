using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARManager : MonoBehaviour {

    public Camera VuforiaCamera;
    public Camera CaptureCamera;
    void Start () {
        VuforiaCamera.gameObject.SetActive(false);
    }
	
	void Update ()
    {
		
	}

    public void OpenARcamera()
    {
        UnityChatSDK.Instance.SetVideoCaptureType(VideoType.UnityCamera, CaptureCamera);
        VuforiaCamera.gameObject.SetActive(true);
        Invoke("Copy",2);
    }
    void Copy()
    {
        CaptureCamera.fieldOfView= VuforiaCamera.fieldOfView;
        CaptureCamera.farClipPlane = VuforiaCamera.farClipPlane;
        CaptureCamera.depth = VuforiaCamera.depth-1;
    }
    public void CloseARcamera()
    {
        VuforiaCamera.gameObject.SetActive(false);
    }
}
