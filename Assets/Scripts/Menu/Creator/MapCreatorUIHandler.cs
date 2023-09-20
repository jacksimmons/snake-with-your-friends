using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class MapCreatorUIHandler : MonoBehaviour
{
    private EditorMenu m_editor;
    [SerializeField]
    private TextMeshProUGUI m_toolValue;
    [SerializeField]
    private TextMeshProUGUI m_layerValue;
    [SerializeField]
    private TextMeshProUGUI m_mouseCoordsXValue;
    [SerializeField]
    private TextMeshProUGUI m_mouseCoordsYValue;
    [SerializeField]
    private TextMeshProUGUI m_mouseZoomValue;


    private void Start()
    {
        m_editor = GetComponent<EditorMenu>();
    }


    private void Update()
    {
        m_toolValue.text = m_editor.ToolInUse.ToString();
        m_layerValue.text = m_editor.CurrentLayer.ToString();
    }


    public void UpdateGridPos(Vector3Int gridPos)
    {
        m_mouseCoordsXValue.text = $"{gridPos.x}";
        m_mouseCoordsYValue.text = $"{gridPos.y}";
    }


    public void UpdateZoom(float value)
    {
        m_mouseZoomValue.text = $"{value:F2}x";
    }
}
