using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AspectCam : MonoBehaviour
{
    private void Start()
    {
        // Credit to http://gamedesigntheory.blogspot.com/2010/09/controlling-aspect-ratio-in-unity.html
        float targetAspect = 16 / 9;
        float windowAspect = Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Camera cam = gameObject.GetComponent<Camera>();
        Rect camRect = cam.rect;

        // Scale Height < Current Height => Add letterbox
        if (scaleHeight < 1.0f)
        {
            camRect.width = 1.0f;
            camRect.height = scaleHeight;
            camRect.x = 0;
            camRect.y = (1 - scaleHeight) / 2;
        }
        else // Add pillarbox
        {
            float scaleWidth = 1 / scaleHeight;

            camRect.width = scaleWidth;
            camRect.height = 1.0f;
            camRect.x = (1 - scaleWidth) / 2;
            camRect.y = 0;
        }

        cam.rect = camRect;
    }
}
