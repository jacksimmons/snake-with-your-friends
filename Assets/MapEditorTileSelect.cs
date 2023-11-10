using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapEditorTileSelect : MonoBehaviour
{
    [SerializeField]
    private MapEditorPaintBehaviour m_painter;


    public void OnTileSelected(Tile tile)
    {
        m_painter.ChosenTilePaint = tile;
    }
}
