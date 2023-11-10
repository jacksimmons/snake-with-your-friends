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
    public string UIToolText
    {
        set
        {
            m_toolValue.text = value;
        }
    }

    [SerializeField]
    private TextMeshProUGUI m_layerValue;
    public string UILayerText
    {
        set
        {
            m_layerValue.text = value;
        }
    }

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
    private GameObject m_tileSelectPanel;
    
    [SerializeField]
    private Image m_objectIcon;
    [SerializeField]
    private GameObject m_objectSelectPanel;


    [SerializeField]
    private TextMeshProUGUI m_objectCount;
    public string UIObjectCountText
    {
        set
        {
            m_layerValue.text = value;
        }
    }

    [SerializeField]
    private TextMeshProUGUI m_helpLabel;

    [SerializeField]
    private TMP_InputField m_saveInfo;
    public string ChosenMapName
    {
        get
        {
            return m_saveInfo.text;
        }
        set
        {
            m_saveInfo.text = value;
        }
    }

    [SerializeField]
    private GameObject m_selectedObjectPanel;
    [SerializeField]
    private TextMeshProUGUI m_selectedObjectNameLabel;
    [SerializeField]
    private TextMeshProUGUI m_selectedObjectPosLabel;
    [SerializeField]
    private TextMeshProUGUI m_selectedObjectIDLabel;


    private void Start()
    {
        m_editor = GetComponent<EditorMenu>();
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


    public void UpdateObjectCountLabel()
    {
        m_objectCount.text = $"({MapEditor.GridObjDict.NumObjects}/{GridObjectDictionary.MAX_OBJECTS})";
    }


    public void ToggleTileUI(bool toggle)
    {
        m_tileIcon.transform.parent.gameObject.SetActive(toggle);
        m_tileSelectPanel.SetActive(toggle);
    }


    public void ToggleObjectUI(bool toggle)
    {
        m_objectIcon.transform.parent.gameObject.SetActive(toggle);
        m_objectSelectPanel.SetActive(toggle);
    }


    public void ToggleHelpLabel()
    {
        m_helpLabel.enabled = !m_helpLabel.enabled;
    }


    public void ToggleSelectedObjectPanel(bool toggle)
    {
        m_selectedObjectPanel.SetActive(toggle);
    }


    public void UpdateSelectedObjectPanel(Vector3 position, string name)
    {
        m_selectedObjectPanel.transform.position = position;
        m_selectedObjectNameLabel.text = name;
        m_selectedObjectPosLabel.text = $"{position.x}, {position.y}, {position.z}";
        m_selectedObjectIDLabel.text = "Not implemented...";
    }
}
