using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetAspectRatio : MonoBehaviour
{



    RawImage rawImage;
    AspectRatioFitter ratio;
    void Start()
    {
        rawImage = transform.GetComponent<RawImage>();

        if (transform.GetComponent<AspectRatioFitter>() == null)
        {
            gameObject.AddComponent<AspectRatioFitter>();
        }
        ratio = transform.GetComponent<AspectRatioFitter>();
        ratio.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
    }
    private void Update()
    {
        if (rawImage.texture != null)
            ratio.aspectRatio = (float)rawImage.texture.width / rawImage.texture.height;
    }
}
