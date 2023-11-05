using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapEditorUIHandler : MonoBehaviour
{
    private EditorMenu m_editor;

    [SerializeField]
    private MapEditorPaintBehaviour m_painter;

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
    private Coroutine m_mouseZoomHideCoroutine = null;

    [SerializeField]
    private Image m_tileIcon;
    [SerializeField]
    private Image m_objectIcon;
    [SerializeField]
    private TextMeshProUGUI m_objectCount;

    [SerializeField]
    private TextMeshProUGUI m_helpLabel;


    private void Start()
    {
        m_editor = GetComponent<EditorMenu>();
    }


    private void Update()
    {
        m_toolValue.text = m_editor.ToolInUse.ToString();
        m_layerValue.text = m_editor.CurrentLayer.ToString();
        m_objectCount.text = $"({m_painter.NumObjects}/{MapEditorPaintBehaviour.MAX_OBJECTS})";
    }


    private void BrieflyShowUIElement(GameObject elementGO, ref Coroutine hideCoroutine)
    {
        // Reset the hide timer (for successive UI element shows, we don't want it to flicker)
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        void ToggleElem(bool on)
        {
            elementGO.SetActive(on);
        }

        // Show, then hide after a while
        ToggleElem(true);
        hideCoroutine = StartCoroutine(Wait.WaitThen(1, () => ToggleElem(false)));
    }


    public void UpdateGridPos(Vector3Int gridPos)
    {
        m_mouseCoordsXValue.text = $"{gridPos.x}";
        m_mouseCoordsYValue.text = $"{gridPos.y}";
    }


    public void UpdateZoom(float value)
    {
        BrieflyShowUIElement(m_mouseZoomValue.transform.parent.gameObject, ref m_mouseZoomHideCoroutine);
        m_mouseZoomValue.text = $"{value:F2}x";
    }


    public void UpdateTileIcon(Sprite sprite)
    {
        m_tileIcon.sprite = sprite;
    }


    public void UpdateObjectIcon(GameObject go)
    {
        m_objectIcon.sprite = go.GetComponent<SpriteRenderer>().sprite;
    }


    public void ToggleTileUI(bool toggle)
    {
        m_tileIcon.transform.parent.gameObject.SetActive(toggle);
    }


    public void ToggleObjectUI(bool toggle)
    {
        m_objectIcon.transform.parent.gameObject.SetActive(toggle);
    }


    public void ToggleHelpLabel()
    {
        m_helpLabel.enabled = !m_helpLabel.enabled;
    }
}
