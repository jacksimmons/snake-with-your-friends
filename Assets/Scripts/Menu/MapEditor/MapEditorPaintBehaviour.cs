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


    private bool CheckIfTileAtPos(Vector3Int pos)
    {
        return currentTilemap.HasTile(pos);
    }


    public void Draw(Vector3Int pos)
    {
        // Only increment tile count if not an overwrite && a tile is selected
        if (!currentTilemap.GetTile(pos) && ChosenTilePaint)
            m_UI.ChangeTileCount(true);

        print(currentTilemap.name);
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

        if (MapEditor.GridObjDict.IsPositionEmpty(pos))
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
        ob.DontDestroyOnExplosion();
        ob.Type = (byte)Array.IndexOf(m_loader.Objects, ChosenObjectPaint);

        if (go.transform.childCount == 2) // Teleporter
        {
            go.transform.GetChild(1).position += Vector3.right;
        }

        MapEditor.GridObjDict.AddObject(pos, go);
        m_UI.ChangeObjectCount(true);
    }


    public void EraseObject(Vector3Int pos)
    {
        if (MapEditor.GridObjDict.IsEmpty())
            return;

        GameObject removed = MapEditor.GridObjDict.RemoveObject(pos);
        Destroy(removed);
        m_UI.ChangeObjectCount(false);
    }


    public void FillObject(Vector3Int start, bool draw)
    {
        StartCoroutine(FillCoro(start, 
            draw ? MapEditor.GridObjDict.IsPositionEmpty : (Vector3Int pos) => !MapEditor.GridObjDict.IsPositionEmpty(pos),
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
