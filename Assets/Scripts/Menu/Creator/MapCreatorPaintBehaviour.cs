using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapCreatorPaintBehaviour : MonoBehaviour
{
    public TileBase selectedTile;

    [SerializeField]
    public Tilemap currentTilemap;


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
}
