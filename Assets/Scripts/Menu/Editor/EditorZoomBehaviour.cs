using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorZoomBehaviour : MonoBehaviour
{
    [SerializeField]
    private EditorMenu m_editor;

    private const float MOUSE_ZOOM_MULT = 10f;

    private const float MOUSE_ZOOM_MIN = 15f;
    private const float MOUSE_ZOOM_MAX = 100f;

    // Note that these should have different starting values
    private float previousMouseZoom = 0f;
    private float currentMouseZoom = 1f;


    private void Update()
    {
        currentMouseZoom += Input.mouseScrollDelta.y * MOUSE_ZOOM_MULT;

        // Mouse zoom has not changed
        if (currentMouseZoom == previousMouseZoom) return;

        currentMouseZoom = Mathf.Clamp(currentMouseZoom, MOUSE_ZOOM_MIN, MOUSE_ZOOM_MAX);
        Camera.main.fieldOfView = currentMouseZoom;

        Camera.main.orthographicSize = currentMouseZoom;

        m_editor.UpdateZoom(currentMouseZoom);
        previousMouseZoom = currentMouseZoom;
    }
}
