using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
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
    public string UILayerText
    {
        get { return m_layerValue.text; }
        set { m_layerValue.text = value; }
    }

    [SerializeField]
    private TextMeshProUGUI m_nameValue;
    public string UINameText
    {
        get { return m_nameValue.text; }
        set { m_nameValue.text = value; }
    }

    [SerializeField]
    private TextMeshProUGUI m_mouseCoordsValue;
    [SerializeField]
    private TextMeshProUGUI m_mouseZoomValue;

    [SerializeField]
    private Image m_tileIcon;
    [SerializeField]
    private GameObject m_tileSelectPanel;
    
    [SerializeField]
    private Image m_objectIcon;
    [SerializeField]
    private GameObject m_objectSelectPanel;

    [SerializeField]
    private TextMeshProUGUI m_objectCountLabel;
    private int m_objectCount;

    [SerializeField]
    private TextMeshProUGUI m_tileCountLabel;
    private int m_tileCount;

    [SerializeField]
    private TextMeshProUGUI m_helpLabel;

    [SerializeField]
    private GameObject m_chosenObjectBox;


    private void Start()
    {
        m_editor = GetComponent<EditorMenu>();

        UpdateObjectCount();
    }


    //private void BrieflyShowUIElement(GameObject elementGO, ref Coroutine hideCoroutine)
    //{
    //    // Reset the hide timer (for successive UI element shows, we don't want it to flicker)
    //    if (hideCoroutine != null)
    //    {
    //        StopCoroutine(hideCoroutine);
    //        hideCoroutine = null;
    //    }

    //    void ToggleElem(bool on)
    //    {
    //        elementGO.SetActive(on);
    //    }

    //    // Show, then hide after a while
    //    ToggleElem(true);
    //    hideCoroutine = StartCoroutine(Wait.WaitThen(1, () => ToggleElem(false)));
    //}


    public void UpdateGridPos(Vector3Int gridPos)
    {
        m_mouseCoordsValue.text = $"Position: ({gridPos.x}, {gridPos.y})";
    }


    public void UpdateZoom(float value)
    {
        m_mouseZoomValue.text = $"Zoom: {value:F2}x";
    }


    public void UpdateSelectedTool(string toolName)
    {
        m_toolValue.text = $"Tool: {toolName}";
    }


    public void UpdateTileIcon(Sprite sprite)
    {
        m_tileIcon.sprite = sprite;
    }


    public void UpdateObjectIcon(GameObject go)
    {
        SpriteRenderer sr = go.GetComponentInChildren<SpriteRenderer>();
        if (sr)
            m_objectIcon.sprite = sr.sprite;
        else
            Debug.LogError("GameObject and its first child (if it has one) have no sprite renderer component.");
    }


    public void ChangeObjectCount(bool add)
    {
        if (add) m_objectCount += 1;
        else m_objectCount -= 1;
        m_objectCountLabel.text = $"Object Count: ({m_objectCount}/{GridObjectDictionary.MAX_OBJECTS})";
    }


    public void UpdateObjectCount()
    {
        m_objectCount = MapEditor.GridObjDict.NumObjects - 1;
        ChangeObjectCount(true);
    }


    public void ChangeTileCount(bool add)
    {
        if (add) m_tileCount += 1;
        else m_tileCount -= 1;
        m_tileCountLabel.text = $"Tile Count: {m_tileCount}";
    }


    public void UpdateTileCount(Tilemap[] tilemaps)
    {
        m_tileCount = 0;
        foreach (Tilemap tilemap in tilemaps)
        {
            TileBase[] tiles = tilemap.GetTilesBlock(tilemap.cellBounds);
            foreach (TileBase tile in tiles)
            {
                if (tile)
                {
                    m_tileCount++;
                }
            }
        }

        m_tileCount--;
        ChangeTileCount(true);
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


    public void DisableChosenObjectBox()
    {
        m_chosenObjectBox.SetActive(false);
    }


    public void EnableChosenObjectBox(Vector3 position)
    {
        m_chosenObjectBox.SetActive(true);
        m_chosenObjectBox.transform.position = position + 0.5f * Vector3.one;
    }


    public void OnChangeLayerButtonPressed(bool right)
    {
        if (right)
            m_editor.CurrentLayer = Extensions.Next(m_editor.CurrentLayer);
        else
            m_editor.CurrentLayer = Extensions.Prev(m_editor.CurrentLayer);
    }
}
