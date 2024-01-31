using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class MapEditorPaintBehaviour : MonoBehaviour
{
    private Tile _chosenTilePaint;
    public Tile ChosenTilePaint
    {
        get { return _chosenTilePaint; }
        set
        {
            _chosenTilePaint = value;
            m_UI.UpdateTileIcon(_chosenTilePaint.sprite);
        }
    }

    private GameObject _chosenObjectPaint;
    public GameObject ChosenObjectPaint
    {
        get { return _chosenObjectPaint; }
        set
        {
            _chosenObjectPaint = value;
            m_UI.UpdateObjectIcon(_chosenObjectPaint);
        }
    }

    [SerializeField]
    public Tilemap currentTilemap;

    [SerializeField]
    private MapEditorUIHandler m_UI;
    [SerializeField]
    private MapLoader m_loader;

    public const int MAX_FILL_DEPTH = 100;
    private Queue<Vector3Int> fillQueue = new();

    // Object IDs which can only have one present object
    private List<int> oneOnlyObjIds = new();
    // Object IDs which can only have one present object, which have already been placed
    private List<int> existingOneOnlyObjIds = new();


    private void Start()
    {
        for (int i = 0; i < m_loader.Objects.Length; i++)
        {
            if (m_loader.Objects[i].TryGetComponent<SpawnPointBehaviour>(out _))
            {
                oneOnlyObjIds.Add(i);
            }
        }
    }


    private bool CheckIfTileAtPos(Vector3Int pos)
    {
        return currentTilemap.HasTile(pos);
    }


    public void Draw(Vector3Int pos)
    {
        // Only increment tile count if not an overwrite && a tile is selected
        if (!currentTilemap.GetTile(pos) && ChosenTilePaint)
            m_UI.ChangeTileCount(true);

        currentTilemap.SetTile(pos, ChosenTilePaint);
    }


    public void Erase(Vector3Int pos)
    {
        // Only decrement tile count if an overwrite
        if (currentTilemap.GetTile(pos))
            m_UI.ChangeTileCount(false);

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

        int currentFillDepth = 0;
        while (fillQueue.Count > 0)
        {
            if (currentFillDepth >= MAX_FILL_DEPTH)
            {
                print($"Max fill depth reached. {MAX_FILL_DEPTH}");
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

        fillQueue.Clear();
        yield return null;
    }


    public void DrawObject(Vector3Int pos)
    {
        if (MapEditor.GridObjDict.IsFull())
            return;

        if (!MapEditor.GridObjDict.IsPositionEmpty(pos))
            return;

        // No object was chosen for painting
        if (!ChosenObjectPaint)
            return;

        GameObject go = Instantiate(ChosenObjectPaint, m_loader.ObjectLayer.transform);

        // Add an offset for the object's actual position, equivalent to the tilemap's offset from
        // the origin.
        go.transform.localPosition = (Vector3)pos;

        // Ensure object won't be destroyed even if it disappears
        ObjectBehaviour ob = go.GetComponent<ObjectBehaviour>();
        ob.ObjId = (byte)Array.IndexOf(m_loader.Objects, ChosenObjectPaint);

        // If the obj id matches a one-only object type
        if (oneOnlyObjIds.Contains(ob.ObjId))
        {
            // If there is already one object of obj id type, destroy the obj and exit early
            if (existingOneOnlyObjIds.Contains(ob.ObjId))
            {
                Destroy(go);
                return;
            }

            existingOneOnlyObjIds.Add(ob.ObjId);
        }

        ob.DontDestroyOnExplosion();

        if (go.transform.childCount == 2) // Teleporter
        {
            go.transform.GetChild(1).position += Vector3.right;
        }

        MapEditor.GridObjDict.AddObject(pos, go);
        m_UI.ChangeObjectCount(true);
    }


    public void EraseObject(Vector3Int pos)
    {
        if (MapEditor.GridObjDict.IsPositionEmpty(pos))
            return;

        GameObject removed = MapEditor.GridObjDict.RemoveObject(pos);

        byte objId = removed.GetComponent<ObjectBehaviour>().ObjId;

        // Remove the "exists" flag for the given objId, if applicable
        // Note: Don't need to check oneOnlyObjIds, as if it is in existingOneOnlyObjIds it must also be in oneOnlyObjIds.
        if (existingOneOnlyObjIds.Contains(objId))
        {
            existingOneOnlyObjIds.Remove(objId);
        }

        Destroy(removed);
        m_UI.ChangeObjectCount(false);
    }


    public void FillObject(Vector3Int start, bool draw)
    {
        StartCoroutine(FillCoro(start, 
            draw ? (Vector3Int pos) => !MapEditor.GridObjDict.IsPositionEmpty(pos) : MapEditor.GridObjDict.IsPositionEmpty,
            draw ? DrawObject : EraseObject));
    }


    //private void MoveSelectedObject(Vector3Int dir)
    //{
    //    Vector3Int oldObjGridPos = currentTilemap.WorldToCell(selectedObject.transform.position);
    //    Vector3Int newObjGridPos = oldObjGridPos + dir;

    //    if (MapEditor.GridObjDict.AddObject(newObjGridPos, selectedObject))
    //    {
    //        MapEditor.GridObjDict.RemoveObject(oldObjGridPos);
    //    }

    //    selectedObject.transform.position += dir;
    //}
}
