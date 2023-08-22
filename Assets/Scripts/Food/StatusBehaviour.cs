using System.Collections.Generic;
using UnityEngine;

public class StatusBehaviour : MonoBehaviour
{
    private Transform m_statusUI;
    private Dictionary<string, GameObject> m_positiveStatusIcons;

    private void Start()
    {
        m_statusUI = GameObject.FindWithTag("StatusUI").transform;

        m_positiveStatusIcons = new();

        // Store the status icons in a dictionary, then disable them
        foreach (Transform child in m_statusUI.Find("Positive"))
        {
            GameObject childObj = child.gameObject;
            m_positiveStatusIcons.Add(childObj.name, childObj);
            childObj.SetActive(false);
        }
    }

    public void EnableSpeedIcon(uint level)
    {
        m_positiveStatusIcons["Speed" + level.ToString()].SetActive(true);
    }

    public void DisableSpeedIcon(uint level)
    {
        m_positiveStatusIcons["Speed" + level.ToString()].SetActive(false);
    }

    public void DisableAllSpeedIcons()
    {
        for (uint i = 1; i <= 5; i++)
        {
            DisableSpeedIcon(i);
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