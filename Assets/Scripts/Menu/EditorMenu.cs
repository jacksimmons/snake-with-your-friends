using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;


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
    private TMP_InputField m_saveInfo;
    private string m_savedName = null;

    public ECreatorTool ToolInUse { get; private set; }

    private ECreatorLayer m_currentLayer;
    public ECreatorLayer CurrentLayer
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
                case ECreatorLayer.Ground:
                    SetLayerToTilemap(m_groundLayer);
                    break;
                case ECreatorLayer.Wall:
                    SetLayerToTilemap(m_wallLayer);
                    break;
                case ECreatorLayer.Object:
                    SetLayerToObject();
                    break;
            }
            m_currentLayer = value;
        }
    }

    private Vector3Int GridPos { get; set; }

    [SerializeField]
    private Tile[] m_tiles = new Tile[3];
    private int m_tileIndex;

    [SerializeField]
    private GameObject[] m_objects;
    private int m_objectIndex;

    private bool m_objectMode = false;


    private void Start()
    {
        LoadMapFromFile();
    }


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
            CurrentLayer = ECreatorLayer.Ground;
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            CurrentLayer = ECreatorLayer.Wall;
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            CurrentLayer = ECreatorLayer.Object;
    }


    private void HandleTileInput()
    {
        HandleToolInput();
        HandleClickInput(false);

        if (Input.GetKeyDown(KeyCode.LeftBracket))
            ChangeIndex(ref m_tileIndex, m_tiles.Length, -1);
        else if (Input.GetKeyDown(KeyCode.RightBracket))
            ChangeIndex(ref m_tileIndex, m_tiles.Length, 1);

        m_painter.selectedTile = m_tiles[m_tileIndex];
        m_painter.selectedType = (ETileType)m_tileIndex;
        GetComponent<MapCreatorUIHandler>().UpdateTileIcon(m_painter.selectedTile.sprite);
    }


    private void HandleToolInput()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToolInUse = ECreatorTool.Draw;
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToolInUse = ECreatorTool.Fill;
        }
    }


    private void HandleClickInput(bool objMode)
    {
        Action<Vector3Int> draw = objMode ? m_painter.DrawObject : m_painter.Draw;
        Action<Vector3Int> erase = objMode ? m_painter.EraseObject : m_painter.Erase;
        Action<Vector3Int, bool> fill = objMode ? m_painter.FillObject : m_painter.Fill;

        if (Input.GetMouseButton(0))
        {
            switch (ToolInUse)
            {
                case ECreatorTool.Draw:
                    draw(GridPos);
                    break;
                case ECreatorTool.Fill:
                    fill(GridPos, true);
                    break;
            }
        }
        if (Input.GetMouseButton(1))
        {
            switch (ToolInUse)
            {
                case ECreatorTool.Draw:
                    erase(GridPos);
                    break;
                case ECreatorTool.Fill:
                    fill(GridPos, false);
                    break;
            }
        }
    }


    private void HandleObjectInput()
    {
        HandleToolInput();
        HandleClickInput(true);

        if (Input.GetKeyDown(KeyCode.LeftBracket))
            ChangeIndex(ref m_objectIndex, m_objects.Length, -1);
        else if (Input.GetKeyDown(KeyCode.RightBracket))
            ChangeIndex(ref m_objectIndex, m_objects.Length, 1);

        m_painter.selectedObject = m_objects[m_objectIndex];
        GetComponent<MapCreatorUIHandler>().UpdateObjectIcon(m_painter.selectedObject);
    }


    private void ChangeIndex(ref int index, int length, int increment)
    {
        index += increment;
        if (index < 0)
            index = length - 1;
        else if (index > length - 1)
            index = 0;
    }


    private float GetLayerOpacity(Tilemap tilemap)
    {
        return tilemap.color.a;
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
        MapTileData[] GetTileDataArray(Tilemap tilemap)
        {
            List<MapTileData> data = new();
            for (int i = tilemap.cellBounds.xMin; i <= tilemap.cellBounds.xMax; i++)
            {
                for (int j = tilemap.cellBounds.yMin; j <= tilemap.cellBounds.yMax; j++)
                {
                    Vector3Int pos = new(i, j, 0);
                    Sprite sprite = tilemap.GetSprite(pos);
                    MapTileData mtd = GetTileData(i, j, sprite);

                    if (mtd != null)
                        data.Add(mtd);
                }
            }

            Debug.Log(data.Count);
            return data.ToArray();
        }

        MapTileData GetTileData(int x, int y, Sprite sprite)
        {
            if (sprite == null) return null;

            // Find index of matching tile; this is the ETileType in int form.
            for (int k = 0; k < m_tiles.Length; k++)
            {
                if (m_tiles[k].sprite == sprite)
                {
                    return new((short)x, (short)y, (ETileType)k);
                }
            }

            return null;
        }

        if (!Directory.Exists(Application.persistentDataPath + "/Maps"))
            Directory.CreateDirectory(Application.persistentDataPath + "/Maps");

        float ground_a = GetLayerOpacity(m_groundLayer);
        float wall_a = GetLayerOpacity(m_wallLayer);
        SetAllLayerOpacities(1);

        string savePath;
        int numDuplicates = 0;
        if (m_savedName != m_saveInfo.text)
        {
            // If {chosenName}.prefab exists, Increase numDuplicates until we find an unused filename of the format
            // "{chosenName} [numDuplicates].prefab" that hasn't been taken in the maps folder.
            while (true)
            {
                savePath = "Maps/";
                if (numDuplicates > 0)
                    savePath += $"({numDuplicates})";
                savePath += ".map";

                if (File.Exists(savePath))
                {
                    numDuplicates++;
                    continue;
                }
                break;
            }
        }
        else
        {
            savePath = "Maps/" + m_saveInfo.text + ".map";
        }

        MapTileData[] groundData = GetTileDataArray(m_groundLayer);
        MapTileData[] wallData = GetTileDataArray(m_wallLayer);
        MapObjectData[] objData = m_painter.GetObjectData();

        MapData map = new(groundData, wallData, objData);

        Saving.SaveToFile(map, savePath);

        m_savedName = m_saveInfo.text;

        EventSystem.current.SetSelectedGameObject(null);
        if (numDuplicates > 0)
            m_saveInfo.text += $"({numDuplicates})";

        SetLayerOpacity(m_groundLayer, ground_a);
        SetLayerOpacity(m_wallLayer, wall_a);
    }


    public void LoadMapFromFile()
    {
        MapData map = Saving.LoadFromFile<MapData>("Maps/.map");

        MapTileData[] groundData = map.groundData;
        MapTileData[] wallData = map.wallData;
        MapObjectData[] objectData = map.objectData;

        void AddTileToTilemap(MapTileData tile, Tilemap tilemap)
        {
            tilemap.SetTile(new Vector3Int(tile.x, tile.y, 0), m_tiles[(int)tile.type]); 
        }

        void AddObjectToObjmap(MapObjectData obj, GameObject objmap)
        {
            GameObject go = Instantiate(m_objects[(int)obj.type], objmap.transform);
            go.transform.localPosition = new(obj.x, obj.y);
            go.transform.localRotation = Quaternion.Euler(Vector3.forward * obj.rotation);
        }

        for (int i = 0; i < groundData.Length; i++) { AddTileToTilemap(groundData[i], m_groundLayer); }
        for (int i = 0; i < wallData.Length; i++) { AddTileToTilemap(wallData[i], m_wallLayer); }
        for (int i = 0; i < objectData.Length; i++) { AddObjectToObjmap(objectData[i], m_objectLayer); }

        m_painter.LoadChildrenIntoMapping(m_objectLayer.transform);
    }
}
