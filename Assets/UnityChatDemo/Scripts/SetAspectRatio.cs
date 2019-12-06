using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetAspectRatio : MonoBehaviour {



    RawImage rawImage;
    RectTransform rect;
    AspectRatioFitter ratio;
    void Start () {
        rawImage= transform.GetComponent<RawImage>();
        rect = transform.GetComponent<RectTransform>();

        if (transform.GetComponent<AspectRatioFitter>() == null)
        {      
            gameObject.AddComponent<AspectRatioFitter>();
        }
        ratio = transform.GetComponent<AspectRatioFitter>();
        ratio.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
    }
    private void Update()
    {
        ratio.aspectRatio = (float)rawImage.texture.width / rawImage.texture.height;
    }
}
