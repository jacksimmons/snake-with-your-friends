using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class MapEditorTileIcon : MonoBehaviour
{
    [SerializeField]
    private Tile m_tile;


    // Start is called before the first frame update
    void Start()
    {
        if (m_tile == null)
            return;

        Image image = GetComponent<Image>();
        image.sprite = m_tile.sprite;
    }


    public void OnButtonPressed()
    {
        GetComponentInParent<MapEditorTileSelect>().OnTileSelected(m_tile);
    }
}
