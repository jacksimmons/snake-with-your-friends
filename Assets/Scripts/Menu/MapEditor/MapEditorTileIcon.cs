using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class MapEditorTileIcon : MonoBehaviour
{
    private bool m_setup = false;
    private Tile m_tile;


    public void Setup(Tile tile)
    {
        m_tile = tile;

        Image image = GetComponent<Image>();
        image.sprite = tile.sprite;

        m_setup = true;
    }


    public void OnButtonPressed()
    {
        if (!m_setup)
        {
            Debug.LogWarning("Tile select button pressed before assignment.");
            return;
        }

        GetComponentInParent<MapEditorTileSelect>().OnTileSelected(m_tile);
    }
}
