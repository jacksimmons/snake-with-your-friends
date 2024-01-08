using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapEditorSelectGrid : MonoBehaviour
{
    [SerializeField]
    private MapLoader m_map;
    [SerializeField]
    private bool m_isObjectGrid;

    // Start is called before the first frame update
    void Start()
    {
        int icons = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform row = transform.GetChild(i);
            for (int j = 0; j < row.childCount; j++)
            {
                Transform icon = row.GetChild(j);

                if (m_isObjectGrid)
                {
                    if (icons >= m_map.Objects.Length) return;
                    icon.GetComponent<MapEditorObjectIcon>().Setup(m_map.Objects[icons]);
                }
                else
                {
                    if (icons >= m_map.Tiles.Length) return;
                    icon.GetComponent<MapEditorTileIcon>().Setup(m_map.Tiles[icons]);
                }
                icons++;
            }
        }
    }
}
