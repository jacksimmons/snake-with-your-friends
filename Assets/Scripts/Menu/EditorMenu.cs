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
    private Vector3 m_mousePos;

    // Update is called once per frame
    void Update()
    {
        if (m_mousePos != Input.mousePosition)
        {
            m_mousePos = Input.mousePosition;
            m_mouseCoordsXValue.text = $"{(int)m_mousePos.x}";
            m_mouseCoordsYValue.text = $"{(int)m_mousePos.y}";
        }

        if (Input.GetMouseButtonDown(0))
        {
            m_painter.Draw(new((int)m_mousePos.x, (int)m_mousePos.y));
        }
    }
}
