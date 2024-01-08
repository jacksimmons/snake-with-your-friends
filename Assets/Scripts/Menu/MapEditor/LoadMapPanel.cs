using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadMapPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject m_mapsContainer;
    private RectTransform m_rt;
    [SerializeField]
    private TextMeshProUGUI m_pageLabel;

    private const int MAX_MAPS_ONSCREEN = 5;
    private int m_mapIndexOnLeft = 0;


    private void Start()
    {
        m_rt = m_mapsContainer.GetComponent<RectTransform>();

        UpdatePageLabel(1, m_rt.childCount);
    }


    private void UpdatePageLabel(int left, int max)
    {
        int right = left + MAX_MAPS_ONSCREEN;
        if (right > max)
            right = max;

        m_pageLabel.text = $"{left}-{right} of {max}";
    }


    public void OnArrowPressed(bool right)
    {
        int dir;
        if (right)
            dir = -1;
        else
            dir = +1;

        int numMaps = m_rt.childCount;
        if (numMaps == 0)
        {
            Debug.LogWarning("No maps!");
            return;
        }

        // Add a map's width onto the position in given direction to make the next/prev map hug the LHS
        Vector3 newPos = m_rt.localPosition + dir * Vector3.right * m_rt.GetChild(0).GetComponent<RectTransform>().rect.width;

        // Map index decreases as dir increases - pressing right means maps go left, so -dir => index increase.
        int newMapIndexOnLeft = m_mapIndexOnLeft - dir;

        if (newMapIndexOnLeft >= 0 && newMapIndexOnLeft <= numMaps - MAX_MAPS_ONSCREEN)
        {
            m_rt.localPosition = newPos;
            m_mapIndexOnLeft = newMapIndexOnLeft;

            UpdatePageLabel(m_mapIndexOnLeft + 1, numMaps);
        }
    }
}
