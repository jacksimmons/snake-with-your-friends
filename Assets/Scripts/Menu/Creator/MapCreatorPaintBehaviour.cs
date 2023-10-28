using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapCreatorPaintBehaviour : MonoBehaviour
{
    public Tile selectedTile;
    public ETileType selectedType;

    public GameObject selectedObject;

    [SerializeField]
    public Tilemap currentTilemap;
    [SerializeField]
    private GameObject objectLayer;

    private Dictionary<Vector3Int, GameObject> m_objectMapping = new();

    public const int MAX_OBJECTS = 100;
    public int NumObjects { get; private set; } = 0;

    public const int MAX_FILL_DEPTH = 1000;
    private int currentFillDepth = 0;
    private Queue<Vector3Int> fillQueue = new();


    public void LoadChildrenIntoMapping(Transform parent)
    {
        foreach (Transform transform in parent)
        {
            m_objectMapping.Add(
            new((int)transform.localPosition.x, 
                (int)transform.localPosition.y),
            transform.gameObject);
            NumObjects++;
        }
    }


    private bool CheckIfTileAtPos(Vector3Int pos)
    {
        return currentTilemap.HasTile(pos);
    }

    private bool CheckIfObjectAtPos(Vector3Int pos)
    {
        return m_objectMapping.ContainsKey(pos);
    }


    public void Paint(ECreatorTool tool, Vector3Int pos)
    {
        switch (tool)
        {
            case ECreatorTool.Draw:
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


    public void Fill(Vector3Int start, bool draw)
    {
        StartCoroutine(FillCoro(start, 
            draw ? CheckIfTileAtPos : (Vector3Int pos) => !CheckIfTileAtPos(pos),
            draw ? Draw : Erase));
    }


    private IEnumerator FillCoro(Vector3Int start, Func<Vector3Int, bool> checkForElementAtPos, Action<Vector3Int> drawAction)
    {
        void TryAddToQueue(Vector3Int pos)
        {
            // Only add to the queue if it is 1. Not occupied in tilemap
            // 2. Not present in the queue.
            if (checkForElementAtPos(pos)) return;
            if (fillQueue.Contains(pos)) return;
            fillQueue.Enqueue(pos);
        }

        TryAddToQueue(start);

        while (fillQueue.Count > 0)
        {
            if (currentFillDepth >= MAX_FILL_DEPTH)
            {
                print("Depth reached!");
                break;
            }

            Vector3Int currentPos = fillQueue.Dequeue();

            if (!checkForElementAtPos(currentPos))
            {
                currentFillDepth++;
                drawAction(currentPos);
            }

            TryAddToQueue(currentPos + Vector3Int.up);
            TryAddToQueue(currentPos + Vector3Int.down);
            TryAddToQueue(currentPos + Vector3Int.left);
            TryAddToQueue(currentPos + Vector3Int.right);

            yield return new WaitForEndOfFrame();
        }

        currentFillDepth = 0;
        fillQueue.Clear();

        yield return null;
    }


    public void DrawObject(Vector3Int pos)
    {
        if (NumObjects >= MAX_OBJECTS)
            return;

        GameObject go = Instantiate(selectedObject, objectLayer.transform);

        // Add an offset for the object's actual position, equivalent to the tilemap's offset from
        // the origin.
        go.transform.localPosition = (Vector3)pos;
        go.GetComponent<SpriteRenderer>().sortingOrder = 0;

        EraseObject(pos);
        m_objectMapping[pos] = go;
        NumObjects++;
    }


    public void EraseObject(Vector3Int pos)
    {
        if (!CheckIfObjectAtPos(pos)) return;

        Destroy(m_objectMapping[pos]);
        m_objectMapping.Remove(pos);
        NumObjects--;
    }


    public void FillObject(Vector3Int start, bool draw)
    {
        StartCoroutine(FillCoro(start, 
            draw ? CheckIfObjectAtPos : (Vector3Int pos) => !CheckIfObjectAtPos(pos),
            draw ? DrawObject : EraseObject));
    }


    public MapObjectData[] GetObjectData()
    {
        GameObject[] objs = m_objectMapping.Values.ToArray();
        MapObjectData[] objData = new MapObjectData[m_objectMapping.Values.Count];
        for (int i = 0; i < objData.Length; i++)
        {
            ObjectBehaviour ob = objs[i].GetComponent<ObjectBehaviour>();
            objData[i] = new(ob.Type,
                (short)Mathf.FloorToInt(ob.transform.localPosition.x),
                (short)Mathf.FloorToInt(ob.transform.localPosition.y));
        }

        return objData;
    }
}
