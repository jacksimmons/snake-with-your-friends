using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class EditorMenu : MonoBehaviour
{
    [SerializeField]
    private Camera m_cam;
    [SerializeField]
    private EditorPaintBehaviour m_painter;

    [SerializeField]
    private TextMeshProUGUI m_mouseCoordsXValue;
    [SerializeField]
    private TextMeshProUGUI m_mouseCoordsYValue;
    [SerializeField]
    private TextMeshProUGUI m_mouseZoomValue;

    private Vector3Int m_gridPos;


    // Update is called once per frame
    private void Update()
    {
        Vector3Int currentGridPos = GetGridPos();
        if (currentGridPos != m_gridPos)
        {
            m_gridPos = currentGridPos;
            m_mouseCoordsXValue.text = $"{m_gridPos.x}";
            m_mouseCoordsYValue.text = $"{m_gridPos.y}";
        }

        if (Input.GetMouseButtonDown(0))
        {
            m_painter.Draw(m_gridPos);
        }
    }


    private Vector3Int GetGridPos()
    {
        Vector3 worldMousePos =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return m_painter.groundTilemap.WorldToCell(worldMousePos);
    }


    public void UpdateZoom(float value)
    {
        print(value);
        m_mouseZoomValue.text = $"{value:F2}x";
    }
}
