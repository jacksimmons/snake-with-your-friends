using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;


public enum CreatorTool
{
    None,
    Draw,
    Erase
}

public enum CreatorLayer
{
    Ground,
    Wall,
    Object
}

public class EditorMenu : MonoBehaviour
{
    private const float DISABLED_LAYER_OPACITY = 0.2f;

    [SerializeField]
    private Camera m_cam;
    [SerializeField]
    private MapCreatorPaintBehaviour m_painter;
    [SerializeField]
    private Tilemap m_groundLayer;
    [SerializeField]
    private Tilemap m_wallLayer;
    [SerializeField]
    private GameObject m_objectLayer;

    [SerializeField]
    private TextMeshProUGUI m_saveInfo;

    public CreatorTool ToolInUse { get; private set; }

    private CreatorLayer m_currentLayer;
    public CreatorLayer CurrentLayer
    {
        get { return m_currentLayer; }
        private set
        {
            void SetLayerToTilemap(Tilemap tilemap)
            {
                m_objectMode = false;

                m_painter.currentTilemap = tilemap;
                SetLayerOpacity(m_painter.currentTilemap, 1);

                GetComponent<MapCreatorUIHandler>().ToggleTileUI(true);
                GetComponent<MapCreatorUIHandler>().ToggleObjectUI(false);
            }

            void SetLayerToObject()
            {
                m_objectMode = true;

                GetComponent<MapCreatorUIHandler>().ToggleTileUI(false);
                GetComponent<MapCreatorUIHandler>().ToggleObjectUI(true);
            }

            SetAllLayerOpacities(DISABLED_LAYER_OPACITY);
            switch (value)
            {
                case CreatorLayer.Ground:
                    SetLayerToTilemap(m_groundLayer);
                    break;
                case CreatorLayer.Wall:
                    SetLayerToTilemap(m_wallLayer);
                    break;
                case CreatorLayer.Object:
                    SetLayerToObject();
                    break;
            }
            m_currentLayer = value;
        }
    }

    private Vector3Int GridPos { get; set; }

    [SerializeField]
    private Tile[] m_tiles;
    private int m_tileIndex;

    [SerializeField]
    private GameObject[] m_objects;
    private int m_objectIndex;

    private bool m_objectMode = false;


    // Update is called once per frame
    private void Update()
    {
        Vector3Int currentGridPos = GetGridPos();
        if (currentGridPos != GridPos)
        {
            GridPos = currentGridPos;

            GetComponent<MapCreatorUIHandler>().UpdateGridPos(GridPos);
        }

        HandleLayerInput();

        if (!m_objectMode)
            HandleTileInput();
        else
            HandleObjectInput();
    }


    private Vector3Int GetGridPos()
    {
        Vector3 worldMousePos =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return m_painter.currentTilemap.WorldToCell(worldMousePos);
    }


    private void HandleLayerInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            CurrentLayer = CreatorLayer.Ground;
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            CurrentLayer = CreatorLayer.Wall;
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            CurrentLayer = CreatorLayer.Object;
    }


    private void HandleTileInput()
    {
        int mouse = HandleMouseInput();
        if (mouse == 0)
            m_painter.Draw(GridPos);
        else if (mouse == 1)
            m_painter.Erase(GridPos);

        if (Input.GetKeyDown(KeyCode.LeftBracket))
            ChangeIndex(ref m_tileIndex, m_tiles.Length, -1);
        else if (Input.GetKeyDown(KeyCode.RightBracket))
            ChangeIndex(ref m_tileIndex, m_tiles.Length, 1);

        m_painter.selectedTile = m_tiles[m_tileIndex];
        GetComponent<MapCreatorUIHandler>().UpdateTileIcon(m_painter.selectedTile.sprite);
    }


    private void HandleObjectInput()
    {
        int mouse = HandleMouseInput();
        if (mouse == 0)
            m_painter.DrawObject(GridPos);
        else if (mouse == 1)
            m_painter.EraseObject(GridPos);

        if (Input.GetKeyDown(KeyCode.LeftBracket))
            ChangeIndex(ref m_objectIndex, m_objects.Length, -1);
        else if (Input.GetKeyDown(KeyCode.RightBracket))
            ChangeIndex(ref m_objectIndex, m_objects.Length, 1);
    
        m_painter.selectedObject = m_objects[m_objectIndex];
        GetComponent<MapCreatorUIHandler>().UpdateObjectIcon(m_painter.selectedObject);
    }


    private int HandleMouseInput()
    {
        if (!Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetMouseButton(0))
            {
                ToolInUse = CreatorTool.Draw;
                return 0;
            }
            else if (Input.GetMouseButton(1))
            {
                ToolInUse = CreatorTool.Erase;
                return 1;
            }
            else
            {
                ToolInUse = CreatorTool.None;
            }
        }

        return -1;
    }


    private void ChangeIndex(ref int index, int length, int increment)
    {
        index += increment;
        if (index < 0)
            index = length - 1;
        else if (index > length - 1)
            index = 0;
    }


    private void SetLayerOpacity(Tilemap tilemap, float opacity)
    {
        Color color = tilemap.color;
        color.a = opacity;
        tilemap.color = color;
    }


    private void SetAllLayerOpacities(float opacity)
    {
        SetLayerOpacity(m_groundLayer, opacity);
        SetLayerOpacity(m_wallLayer, opacity);
    }


    public void SaveMapToFile()
    {
        if (!Directory.Exists("Assets/Prefabs/Maps"))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Maps");
        }

        SetAllLayerOpacities(1);
        PrefabUtility.SaveAsPrefabAsset(m_groundLayer.transform.parent.parent.gameObject,
            "Assets/Prefabs/Maps/Map.prefab");

        long fileLength = 0;
        try
        {
            FileInfo mapFileInfo = new FileInfo("Assets/Prefabs/Maps/Map.prefab");
            fileLength = mapFileInfo.Length;

            m_saveInfo.text = $"Map.prefab ({fileLength / 1_000_000}MB)";
        }
        catch
        {
            m_saveInfo.text = $"Map.prefab (Unknown Size)";
        }
    }
}
