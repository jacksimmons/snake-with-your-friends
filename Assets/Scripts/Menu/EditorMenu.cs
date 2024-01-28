using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.U2D.Path.GUIFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;


public class EditorMenu : MonoBehaviour
{
    private const float DISABLED_LAYER_OPACITY = 0.2f;

    private const float PAN_SPEED = 0.01f;
    private Vector2 m_panDirection = Vector2.zero;

    private const float ZOOM_START = 1f;
    private float m_zoom = ZOOM_START;
    private const float ZOOM_SPEED = 1f;
    private const float ZOOM_MIN = 0.1f;
    private const float ZOOM_MAX = 10f;

    private bool m_isDrawing = false;
    private bool m_isErasing = false;

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

    private ECreatorTool m_toolInUse = ECreatorTool.Brush;
    public ECreatorTool ToolInUse
    {
        get { return m_toolInUse; }
        private set
        {
            m_toolInUse = value;
            m_UI.UpdateSelectedTool(m_toolInUse.ToString());
            m_UI.DisableChosenObjectBox();
        }
    }

    private ECreatorLayer m_currentLayer = ECreatorLayer.Ground;
    public ECreatorLayer CurrentLayer
    {
        get { return m_currentLayer; }
        set
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
    private Vector3Int SelectedGridPos { get; set; }

    private bool m_objectMode = false;

    [SerializeField]
    public MapLoader Map;

    private MapEditorUIHandler m_UI;
    [SerializeField]
    private MapEditorPaintBehaviour m_painter;

    private MapControls controls;


    private void Awake()
    {
        controls = new();
        controls.Edit.Pan.performed += ctx =>
            m_panDirection = ctx.ReadValue<Vector2>();
        controls.Edit.Pan.canceled += ctx =>
            m_panDirection = Vector2.zero;

        Camera.main.orthographicSize = ZOOM_START;
        controls.Edit.Zoom.performed += ctx =>
        {
            m_zoom -= ctx.ReadValue<float>() * ZOOM_SPEED;
            m_zoom = Mathf.Clamp(m_zoom, ZOOM_MIN, ZOOM_MAX);

            Camera.main.orthographicSize = m_zoom;
            m_UI.UpdateZoom(ZOOM_START / m_zoom);
        };

        controls.Edit.Draw.performed += ctx =>
        {
            m_isDrawing = true;
            HandlePaintInput();
        };
        controls.Edit.Draw.canceled += ctx => m_isDrawing = false;

        controls.Edit.Erase.performed += ctx =>
        {
            m_isErasing = true;
            HandlePaintInput();
        };
        controls.Edit.Erase.canceled += ctx => m_isErasing = false;

        controls.Edit.ChangeTool.performed += ctx =>
        {
            if (ctx.ReadValue<float>() > 0)
                ToolInUse = Extensions.Next(ToolInUse);
            else
                ToolInUse = Extensions.Prev(ToolInUse);
        };

        controls.Edit.Tool_Brush.performed += ctx => ToolInUse = ECreatorTool.Brush;
        controls.Edit.Tool_Fill.performed += ctx => ToolInUse = ECreatorTool.Fill;
        controls.Edit.Tool_Pick.performed += ctx => ToolInUse = ECreatorTool.Pick;

        controls.Edit.Rotate.performed += ctx =>
        {
            float dir = ctx.ReadValue<float>();
            MapEditor.GridObjDict.PickObject(SelectedGridPos).transform.Rotate(Vector3.forward * dir * 90);
        };

        controls.Edit.Enable();
    }


    private void Start()
    {
        m_UI = GetComponent<MapEditorUIHandler>();
    }


    // Update is called once per frame
    private void Update()
    {
        if (m_panDirection != Vector2.zero)
        {
            Camera.main.transform.position += Camera.main.orthographicSize * PAN_SPEED * (Vector3)m_panDirection;
        }

        Vector3Int currentGridPos = GetGridPos();
        if (currentGridPos != GridPos)
        {
            GridPos = currentGridPos;
            m_UI.UpdateGridPos(GridPos);

            HandlePaintInput();
        }

        Map.HandleBackgroundInput();
        HandleSelectInput();
    }


    private Vector3Int GetGridPos()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return m_painter.currentTilemap.WorldToCell(mouseWorld);
    }


    private void HandlePaintInput()
    {
        // If the pointer is over UI, we don't want it to draw, that would be annoying.
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        Action<Vector3Int> draw = m_objectMode ? m_painter.DrawObject : m_painter.Draw;
        Action<Vector3Int> erase = m_objectMode ? m_painter.EraseObject : m_painter.Erase;
        Action<Vector3Int, bool> fill = m_objectMode ? m_painter.FillObject : m_painter.Fill;

        if (m_isDrawing)
        {
            switch (ToolInUse)
            {
                case ECreatorTool.Brush:
                    draw(GridPos);
                    break;
                case ECreatorTool.Fill:
                    fill(GridPos, true);
                    break;
                case ECreatorTool.Pick:
                    SelectedGridPos = GridPos;
                    m_UI.EnableChosenObjectBox(GridPos);
                    break;
            }
        }
        else if (m_isErasing)
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


    private void HandleSelectInput()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            SelectedGridPos += Vector3Int.up;
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


    public void OverwriteMap()
    {
        if (Map.CurrentFilename == null)
        {
            // ! Not implemented
        }
        SaveMapToFile(Map.CurrentFilename);
    }


    public void SaveMapToFile(string name)
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
                    MapTileData? mtd = GetTileData(i, j, sprite);


                    if (mtd != null)
                        data.Add(mtd.Value);
                }
            }

            return data.ToArray();
        }

        MapTileData? GetTileData(int x, int y, Sprite sprite)
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

        string savePath = $"Maps/{name}.map";
        //int numDuplicates = 0;

        //// If {chosenName}.prefab exists, Increase numDuplicates until we find an unused filename of the format
        //// "{chosenName} [numDuplicates].prefab" that hasn't been taken in the maps folder.
        //while (true)
        //{
        //    savePath = $"Maps/{m_savedName}";
        //    if (numDuplicates > 0)
        //        savePath += $"({numDuplicates})";
        //    savePath += ".map";

        //    if (File.Exists(savePath))
        //    {
        //        numDuplicates++;
        //        continue;
        //    }
        //    break;
        //}

        MapTileData[] groundData = GetTileDataArray(m_groundLayer);
        MapTileData[] wallData = GetTileDataArray(m_wallLayer);
        MapObjectData[] objData = MapEditor.GridObjDict.GetObjectData();
        MapData map = new(groundData, wallData, objData, Map.BackgroundIndex);

        Saving.SaveToFile(map, savePath);
        m_UI.UINameText = name;

        EventSystem.current.SetSelectedGameObject(null);

        SetLayerOpacity(m_groundLayer, ground_a);
        SetLayerOpacity(m_wallLayer, wall_a);
    }
}
