using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EditorPaintBehaviour : MonoBehaviour
{
    public TileBase m_selectedTile;

    [SerializeField]
    public Tilemap groundTilemap;


    public void Draw(Vector3Int pos)
    {
        print("hi");
        groundTilemap.SetTile(pos, m_selectedTile);
        print(pos);
    }
}
