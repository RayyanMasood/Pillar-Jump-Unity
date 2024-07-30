using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamProj : MonoBehaviour
{
    //Cache the camera
    private Camera cam;

    //This is the scaling factor for each axis
    [SerializeField]
    private Vector2 scale = Vector2.one;

    //Before any objects are drawn
    void OnPreCull()
    {
        //Find and store the attached camera if we don't already have it cached
        if (!cam)
            cam = GetComponent<Camera>();

        //These functions create projection matrices for both orthographic and perspective projection. Choose between them based on the camera's settings
        Matrix4x4 proj;
        if (cam.orthographic)
            proj = Matrix4x4.Ortho(-cam.orthographicSize * scale.x, cam.orthographicSize * scale.x, -cam.orthographicSize * scale.y, cam.orthographicSize * scale.y, cam.nearClipPlane, cam.farClipPlane);
        else
            proj = Matrix4x4.Perspective(cam.fieldOfView, scale.x / scale.y, cam.nearClipPlane, cam.farClipPlane);

        //Set the camera's projection matrix
        cam.projectionMatrix = proj;
    }

    void OnDisable()
    {
        //*Important* Reset the camera's projection matrix when this is disabled/the scene ends or things will start to break
        if (cam)
            cam.ResetProjectionMatrix();
    }
}
