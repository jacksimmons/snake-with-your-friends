using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ForegroundBehaviour : MonoBehaviour
{
    private const int MAX_FG_OBJ = 50;

    [SerializeField]
    private GameObject _fgObjTemplate;

    public void AddToForeground(Sprite sprite)
    {
        if (transform.childCount < MAX_FG_OBJ)
        {
            Rect rect = transform.parent.GetComponent<RectTransform>().rect;
            float pos_x = Random.Range(0, rect.width);
            float pos_y = Random.Range(0, rect.height);
            GameObject fgObj = Instantiate(_fgObjTemplate, gameObject.transform);
            fgObj.transform.position = new Vector3(pos_x, pos_y);
            fgObj.GetComponent<Image>().sprite = sprite;
        }
    }
}