using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchCameras : MonoBehaviour
{
    public RenderTexture dashTexture;
    public RenderTexture followTexture;

    public Camera dashCam;
    public Camera followCam;

    public RawImage otherCamImage;

    public void SwitchCamera()
    {
        if (dashCam.depth == -1)
        {
            dashCam.depth = -2;
            followCam.depth = -1;

            dashCam.targetTexture = dashTexture;
            followCam.targetTexture = null;
            otherCamImage.texture = dashTexture;
        }
        else
        {
            dashCam.depth = -1;
            followCam.depth = -1;

            dashCam.targetTexture = null;
            followCam.targetTexture = followTexture;
            otherCamImage.texture = followTexture;
        }
    }
}
