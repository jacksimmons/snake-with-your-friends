using UnityEngine.Tilemaps;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public class MapLoader : MonoBehaviour
{
    [SerializeField]
    private Tilemap m_groundLayer;
    public Tilemap GroundLayer
    {
        get { return m_groundLayer; }
        private set { m_groundLayer = value; }
    }

    [SerializeField]
    private Tilemap m_wallLayer;
    public Tilemap WallLayer
    {
        get { return m_wallLayer; }
        private set { m_wallLayer = value; }
    }

    [SerializeField]
    private GameObject m_objectLayer;
    public GameObject ObjectLayer
    {
        get { return m_objectLayer; }
        private set { m_objectLayer = value; }
    }

    [SerializeField]
    private SpriteRenderer m_backgroundLayer = null;

    [SerializeField]
    private Tile[] m_tiles;
    public Tile[] Tiles { get { return m_tiles; } }

    [SerializeField]
    private GameObject[] m_objects;
    public GameObject[] Objects { get { return m_objects; } }

    [SerializeField]
    private Sprite[] m_backgrounds;
    public Sprite[] Backgrounds { get { return m_backgrounds; } }

    private int m_backgroundIndex = 0;
    public int BackgroundIndex { get { return m_backgroundIndex; } }

    [SerializeField]
    private MapEditorPaintBehaviour m_painter;
    [SerializeField]
    private MapEditorUIHandler m_UI;

    public string CurrentFilename { get; private set; } = null;


    public void LoadMapFromFile(string filename)
    {
        CurrentFilename = filename;
        MapData map = Saving.LoadFromFile<MapData>($"Maps/{filename}");
        LoadMap(map);
    }


    public void LoadMap(MapData map)
    {
        MapTileData[] groundData = map.groundData;
        MapTileData[] wallData = map.wallData;
        MapObjectData[] objectData = map.objectData;

        // Clear any existing tiles
        GroundLayer.ClearAllTiles();
        WallLayer.ClearAllTiles();

        // Clear any existing objects
        Transform objLayerParent = m_objectLayer.transform.parent;
        Destroy(ObjectLayer);
        ObjectLayer = Instantiate(new GameObject(), objLayerParent);
        ObjectLayer.name = "Object";
        ObjectLayer.transform.position = new Vector3(0.5f, 0.5f, 0);

        void AddTileToTilemap(MapTileData tile, Tilemap tilemap)
        {
            tilemap.SetTile(new Vector3Int(tile.x, tile.y, 0), m_tiles[(int)tile.type]);
        }

        void AddObjectToObjmap(MapObjectData obj, GameObject objmap)
        {
            GameObject go = Instantiate(Objects[(int)obj.type], objmap.transform);
            go.transform.localPosition = new(obj.x, obj.y);
            go.transform.localRotation = Quaternion.Euler(Vector3.forward * obj.rotation);
        }

        for (int i = 0; i < groundData.Length; i++) { AddTileToTilemap(groundData[i], m_groundLayer); }
        for (int i = 0; i < wallData.Length; i++) { AddTileToTilemap(wallData[i], m_wallLayer); }
        for (int i = 0; i < objectData.Length; i++) { AddObjectToObjmap(objectData[i], ObjectLayer); }
        m_backgroundIndex = map.bgIndex;
        UpdateBackgroundSprite();

        MapEditor.GridObjDict.ClearObjects();
        MapEditor.GridObjDict.AddChildObjects(ObjectLayer.transform);

        if (m_UI)
        {
            m_UI.UpdateTileCountLabel(new Tilemap[] { m_groundLayer, m_wallLayer });
            m_UI.UpdateObjectCountLabel();
        }
    }


    public void HandleBackgroundInput()
    {
        if (Input.GetKeyDown(KeyCode.Minus))
            Extensions.ChangeIndex(ref m_backgroundIndex, m_backgrounds.Length, -1);
        else if (Input.GetKeyDown(KeyCode.Equals))
            Extensions.ChangeIndex(ref m_backgroundIndex, m_backgrounds.Length, 1);
        else
            return;
        UpdateBackgroundSprite();
    }


    private void UpdateBackgroundSprite()
    {
        m_backgroundLayer.sprite = m_backgrounds[m_backgroundIndex];
    }
}