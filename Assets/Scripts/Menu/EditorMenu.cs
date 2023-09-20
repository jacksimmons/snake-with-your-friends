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
    private const float DISABLED_LAYER_OPACITY = 0.1f;

    [SerializeField]
    private Camera m_cam;
    [SerializeField]
    private MapCreatorPaintBehaviour m_painter;
    [SerializeField]
    private Tilemap m_groundLayer;
    [SerializeField]
    private Tilemap m_wallLayer;
    [SerializeField]
    private Tilemap m_objectLayer;

    public CreatorTool ToolInUse { get; private set; }

    private CreatorLayer m_currentLayer;
    public CreatorLayer CurrentLayer
    {
        get { return m_currentLayer; }
        private set
        {
            SetAllLayerOpacities(DISABLED_LAYER_OPACITY);
            switch (value)
            {
                case CreatorLayer.Ground:
                    m_painter.currentTilemap = m_groundLayer;
                    break;
                case CreatorLayer.Wall:
                    m_painter.currentTilemap = m_wallLayer;
                    break;
                case CreatorLayer.Object:
                    m_painter.currentTilemap = m_objectLayer;
                    break;
            }
            SetLayerOpacity(m_painter.currentTilemap, 1);
            m_currentLayer = value;
        }
    }

    private Vector3Int GridPos { get; set; }

    [SerializeField]
    private TileBase[] m_tiles;
    private int m_tileIndex;


    // Update is called once per frame
    private void Update()
    {
        Vector3Int currentGridPos = GetGridPos();
        if (currentGridPos != GridPos)
        {
            GridPos = currentGridPos;
            GetComponent<MapCreatorUIHandler>().UpdateGridPos(GridPos);
        }

        if (!Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetMouseButton(0))
            {
                m_painter.Draw(GridPos);
                ToolInUse = CreatorTool.Draw;
            }
            else if (Input.GetMouseButton(1))
            {
                m_painter.Erase(GridPos);
                ToolInUse = CreatorTool.Erase;
            }
            else
            {
                ToolInUse = CreatorTool.None;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
            CurrentLayer = CreatorLayer.Ground;
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            CurrentLayer = CreatorLayer.Wall;
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            CurrentLayer = CreatorLayer.Object;

        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            m_tileIndex--;
            if (m_tileIndex < 0)
                m_tileIndex = m_tiles.Length - 1;
        }
        else if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            m_tileIndex++;
            if (m_tileIndex > m_tiles.Length - 1)
                m_tileIndex = 0;
        }
        m_painter.selectedTile = m_tiles[m_tileIndex];
    }


    private Vector3Int GetGridPos()
    {
        Vector3 worldMousePos =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return m_painter.currentTilemap.WorldToCell(worldMousePos);
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
        SetLayerOpacity(m_objectLayer, opacity);
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
    }
}
