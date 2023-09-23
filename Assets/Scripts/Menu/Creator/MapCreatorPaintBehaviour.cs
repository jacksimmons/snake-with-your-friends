using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapCreatorPaintBehaviour : MonoBehaviour
{
    public Tile selectedTile;
    public GameObject selectedObject;

    [SerializeField]
    public Tilemap currentTilemap;
    [SerializeField]
    private GameObject objectLayer;

    public Dictionary<Vector3Int, GameObject> m_objectMapping = new();

    public int numObjects { get; private set; } = 0;
    public const int MAX_OBJECTS = 100;


    public void Paint(CreatorTool tool, Vector3Int pos)
    {
        switch (tool)
        {
            case CreatorTool.Draw:
                Draw(pos);
                break;
        }
    }


    public void Draw(Vector3Int pos)
    {
        currentTilemap.SetTile(pos, selectedTile);
    }


    public void Erase(Vector3Int pos)
    {
        currentTilemap.SetTile(pos, null);
    }


    public void DrawObject(Vector3Int pos)
    {
        if (numObjects >= MAX_OBJECTS)
            return;

        GameObject go = Instantiate(selectedObject, objectLayer.transform);

        // Add an offset for the object's actual position, equivalent to the tilemap's offset from
        // the origin.
        go.transform.position = (Vector3)pos + new Vector3(0.5f, 0.5f, 0);
        go.GetComponent<SpriteRenderer>().sortingOrder = 0;

        EraseObject(pos);
        m_objectMapping[pos] = go;
        numObjects++;
    }


    public void EraseObject(Vector3Int pos)
    {
        if (m_objectMapping.ContainsKey(pos))
        {
            Destroy(m_objectMapping[pos]);
            m_objectMapping.Remove(pos);
            numObjects--;
        }
    }
}
