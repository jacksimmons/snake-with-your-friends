using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapPreviewBehaviour : MonoBehaviour
{
    [SerializeField]
    private Image m_thumbnail;
    [SerializeField]
    private TextMeshProUGUI m_name;
    [SerializeField]
    private TextMeshProUGUI m_fileSize;
    [SerializeField]
    private TextMeshProUGUI m_lastModified;


    public void SetThumbnail(Sprite sprite)
    {
        m_thumbnail.sprite = sprite;
    }


    public void SetName(string name)
    {
        m_name.text = name;
    }


    public void SetFileSize(long fileSize)
    {
        m_fileSize.text = fileSize.ToString() + "B";
    }


    public void SetLastModified(DateTime lastModified)
    {
        m_lastModified.text = lastModified.ToString();
    }
}

