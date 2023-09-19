using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EditorPaintBehaviour : MonoBehaviour
{
    public TileBase m_selectedTile;

    [SerializeField]
    private Tilemap m_groundTilemap;


    public void Draw(Vector3Int pos)
    {
        print("hi");
        m_groundTilemap.SetTile(pos, m_selectedTile);
        print(m_groundTilemap.WorldToCell(pos));
    }
}
