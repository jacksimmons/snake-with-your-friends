using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEditorObjectSelect : MonoBehaviour
{
    [SerializeField]
    private MapEditorPaintBehaviour m_painter;


    public void OnObjectSelected(GameObject go)
    {
        m_painter.ChosenObjectPaint = go;
    }
}
