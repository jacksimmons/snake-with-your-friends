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
    private Tilemap m_groundLayer;
    [SerializeField]
    private Tilemap m_wallLayer;
    [SerializeField]
    private GameObject m_objectLayer;
    [SerializeField]
    private GameObject m_backgroundLayer;

    private string m_savedName = null;

    private ECreatorTool m_toolInUse = ECreatorTool.None;
    public ECreatorTool ToolInUse
    {
        get { return m_toolInUse; }
        private set
        { 
            m_toolInUse = value;
            m_UI.UIToolText = m_toolInUse.ToString();
        }
    }

    private ECreatorLayer m_currentLayer = ECreatorLayer.None;
    public ECreatorLayer CurrentLayer
    {
        get { return m_currentLayer; }
        private set
        {
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
            m_UI.UILayerText = m_currentLayer.ToString();
        }
    }

    private Vector3Int GridPos { get; set; }

    private bool m_objectMode = false;

    [SerializeField]
    public MapLoader Map;

    private MapEditorUIHandler m_UI;
    [SerializeField]
    private MapEditorPaintBehaviour m_painter;


    private void Start()
    {
        m_UI = GetComponent<MapEditorUIHandler>();

        ToolInUse = ECreatorTool.Brush;
        CurrentLayer = ECreatorLayer.Ground;
    }


    // Update is called once per frame
    private void Update()
    {
        Vector3Int currentGridPos = GetGridPos();
        if (currentGridPos != GridPos)
        {
            GridPos = currentGridPos;
            m_UI.UpdateGridPos(GridPos);
        }

        HandleLayerInput();
        Map.HandleBackgroundInput();

        if (Input.GetKeyDown(KeyCode.LeftControl))
            ToolInUse = ECreatorTool.None;

        HandleToolInput();
        HandleClickInput(m_objectMode);
    }


    private Vector3Int GetGridPos()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return m_painter.currentTilemap.WorldToCell(mouseWorld);
    }


    private void HandleLayerInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            CurrentLayer = ECreatorLayer.Ground;
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            CurrentLayer = ECreatorLayer.Wall;
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            CurrentLayer = ECreatorLayer.Object;
        else
            return;
    }


    private void HandleToolInput()
    {
        if (Input.GetKeyDown(KeyCode.B))
            ToolInUse = ECreatorTool.Brush;
        else if (Input.GetKeyDown(KeyCode.F))
            ToolInUse = ECreatorTool.Fill;
        else if (Input.GetKeyDown(KeyCode.O))
            ToolInUse = ECreatorTool.SelectObject;
        else
            return;
    }


    private void HandleClickInput(bool objMode)
    {
        // If the pointer is over UI, we don't want it to draw, that would be annoying.
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        Action<Vector3Int> draw = objMode ? m_painter.DrawObject : m_painter.Draw;
        Action<Vector3Int> erase = objMode ? m_painter.EraseObject : m_painter.Erase;
        Action<Vector3Int, bool> fill = objMode ? m_painter.FillObject : m_painter.Fill;

        if (Input.GetMouseButton(0))
        {
            switch (ToolInUse)
            {
                case ECreatorTool.Brush:
                    m_painter.DeselectObject();
                    draw(GridPos);
                    break;
                case ECreatorTool.Fill:
                    m_painter.DeselectObject();
                    fill(GridPos, true);
                    break;
                case ECreatorTool.SelectObject:
                    m_painter.SelectObject(GridPos);
                    break;
            }
        }
        else if (Input.GetMouseButton(1))
        {
            switch (ToolInUse)
            {
                case ECreatorTool.Brush:
                    erase(GridPos);
                    break;
                case ECreatorTool.Fill:
                    fill(GridPos, false);
                    break;
            }
        }
    }


    void SetLayerToTilemap(Tilemap tilemap)
    {
        m_objectMode = false;

        m_painter.currentTilemap = tilemap;
        SetLayerOpacity(m_painter.currentTilemap, 1);

        m_UI.ToggleTileUI(true);
        m_UI.ToggleObjectUI(false);
    }

    void SetLayerToObject()
    {
        m_objectMode = true;

        m_UI.ToggleTileUI(false);
        m_UI.ToggleObjectUI(true);
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

            return data.ToArray();
        }

        MapTileData GetTileData(int x, int y, Sprite sprite)
        {
            if (sprite == null) return null;

            // Find index of matching tile; this is the ETileType in int form.
            for (int k = 0; k < Map.Tiles.Length; k++)
            {
                if (Map.Tiles[k].sprite == sprite)
                {
                    return new((short)x, (short)y, (ETileType)k);
                }
            }

            Debug.LogError("This tile cannot be saved - it is not in the tiles list!");
            return null;
        }

        if (!Directory.Exists(Application.persistentDataPath + "/Maps"))
            Directory.CreateDirectory(Application.persistentDataPath + "/Maps");

        float ground_a = GetLayerOpacity(m_groundLayer);
        float wall_a = GetLayerOpacity(m_wallLayer);
        SetAllLayerOpacities(1);

        string savePath;
        int numDuplicates = 0;
        if (m_savedName != m_UI.ChosenMapName)
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
            savePath = "Maps/" + m_UI.ChosenMapName + ".map";
        }

        MapTileData[] groundData = GetTileDataArray(m_groundLayer);
        MapTileData[] wallData = GetTileDataArray(m_wallLayer);
        MapObjectData[] objData = MapEditor.GridObjDict.GetObjectData();
        MapData map = new(groundData, wallData, objData, Map.BackgroundIndex);

        Saving.SaveToFile(map, savePath);

        m_savedName = m_UI.ChosenMapName;

        EventSystem.current.SetSelectedGameObject(null);
        if (numDuplicates > 0)
            m_UI.ChosenMapName += $"({numDuplicates})";

        SetLayerOpacity(m_groundLayer, ground_a);
        SetLayerOpacity(m_wallLayer, wall_a);
    }
}
