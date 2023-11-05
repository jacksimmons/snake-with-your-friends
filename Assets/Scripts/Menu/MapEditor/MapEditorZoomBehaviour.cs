using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapEditorZoomBehaviour : MonoBehaviour
{
    private const float MOUSE_ZOOM_MIN = 1f;
    private const float MOUSE_ZOOM_MAX = 10f;

    private float startingMouseZoom;

    // Note that these should have different starting values
    private float previousMouseZoom;
    private float currentMouseZoom = 1f;

    private MapEditorUIHandler m_UI;


    private void Start()
    {
        m_UI = GetComponent<MapEditorUIHandler>();

        previousMouseZoom = currentMouseZoom = startingMouseZoom = Camera.main.orthographicSize;
    }


    private void Update()
    {
        currentMouseZoom -= Input.mouseScrollDelta.y;

        // Mouse zoom has not changed
        if (currentMouseZoom == previousMouseZoom) return;

        currentMouseZoom = Mathf.Clamp(currentMouseZoom, MOUSE_ZOOM_MIN, MOUSE_ZOOM_MAX);
        Camera.main.fieldOfView = currentMouseZoom;

        Camera.main.orthographicSize = currentMouseZoom;
        m_UI.UpdateZoom(startingMouseZoom / currentMouseZoom);

        previousMouseZoom = currentMouseZoom;
    }
}

