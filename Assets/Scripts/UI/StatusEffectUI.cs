using System.Collections.Generic;
using UnityEngine;

public class StatusEffectUI : MonoBehaviour
{
    private Transform m_statusUI;
    private Dictionary<string, GameObject> m_positiveStatusIcons;
    private Dictionary<string, GameObject> m_negativeStatusIcons;

    private void Start()
    {
        m_statusUI = GameObject.FindWithTag("StatusUI").transform;

        m_positiveStatusIcons = new();
        m_negativeStatusIcons = new();

        // Store the status icons in a dictionary, then disable them
        foreach (Transform child in m_statusUI.Find("Positive"))
        {
            GameObject childObj = child.gameObject;
            m_positiveStatusIcons.Add(childObj.name, childObj);
            childObj.SetActive(false);
        }
        foreach (Transform child in m_statusUI.Find("Negative"))
        {
            GameObject childObj = child.gameObject;
            m_negativeStatusIcons.Add(childObj.name, childObj);
            childObj.SetActive(false);
        }
    }

    public void ChangeIconActive(bool positive, string effect, int level, bool active)
    {
        if (positive)
            m_positiveStatusIcons[effect + level.ToString()].SetActive(active);
        else
            m_negativeStatusIcons[effect + level.ToString()].SetActive(active);
    }

    public void DisableAllSpeedIcons()
    {
        for (int i = 1; i <= 5; i++)
        {
            ChangeIconActive(true, "Fast", i, false);
            ChangeIconActive(false, "Slow", i, false);
        }
    }

    public void EnableShitIcon()
    {
        m_positiveStatusIcons["Poo"].SetActive(true);
    }

    public void DisableShitIcon()
    {
        m_positiveStatusIcons["Poo"].SetActive(false);
    }

    public void DisableAllIcons()
    {
        foreach (GameObject icon in m_positiveStatusIcons.Values)
        {
            icon.SetActive(false);
        }
    }
}