using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapEditorObjectIcon : MonoBehaviour
{
    private bool m_setup = false;
    private GameObject m_object;


    public void Setup(GameObject go)
    {
        m_object = go;

        Image image = GetComponent<Image>();

        SpriteRenderer sr = go.GetComponentInChildren<SpriteRenderer>();
        if (sr)
            image.sprite = sr.sprite;
        else
            Debug.LogError("No sprite renderer attached to this object or any of its children.");

        m_setup = true;
    }


    public void OnButtonPressed()
    {
        if (!m_setup)
        {
            Debug.LogWarning("Object select button pressed before assignment.");
            return;
        }

        GetComponentInParent<MapEditorObjectSelect>().OnObjectSelected(m_object);
    }
}
