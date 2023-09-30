using System;
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

    public const int MAX_OBJECTS = 100;
    public int numObjects { get; private set; } = 0;

    public const int MAX_FILL_DEPTH = 1000;
    private int currentFillDepth = 0;
    private Queue<Vector3Int> fillQueue = new();


    private bool CheckIfTileAtPos(Vector3Int pos)
    {
        return currentTilemap.HasTile(pos);
    }

    private bool CheckIfObjectAtPos(Vector3Int pos)
    {
        return m_objectMapping.ContainsKey(pos);
    }


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
        if (!CheckIfObjectAtPos(pos)) return;

        Destroy(m_objectMapping[pos]);
        m_objectMapping.Remove(pos);
        numObjects--;
    }


    public void FillObject(Vector3Int start, bool draw)
    {
        StartCoroutine(FillCoro(start, 
            draw ? CheckIfObjectAtPos : (Vector3Int pos) => !CheckIfObjectAtPos(pos),
            draw ? DrawObject : EraseObject));
    }
}
