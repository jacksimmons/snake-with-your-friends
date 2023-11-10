using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapEditorObjectIcon : MonoBehaviour
{
    [SerializeField]
    private GameObject m_object;

    // Start is called before the first frame update
    void Start()
    {
        Image image = GetComponent<Image>();
        image.sprite = m_object.GetComponent<SpriteRenderer>().sprite;
    }


    public void OnButtonPressed()
    {
        GetComponentInParent<MapEditorObjectSelect>().OnObjectSelected(m_object);
    }
}
