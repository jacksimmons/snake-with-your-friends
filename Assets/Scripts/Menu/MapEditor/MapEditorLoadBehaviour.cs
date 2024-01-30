using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MapEditorLoadBehaviour : MonoBehaviour
{
    [SerializeField]
    private GameObject m_loadPanel;
    [SerializeField]
    private GameObject m_mapPreviewContainer;
    [SerializeField]
    private GameObject m_mapPreviewTemplate;

    [SerializeField]
    private MapLoader m_map;


    public void OnLoadButtonPressed()
    {
        m_loadPanel.SetActive(true);

        foreach (Transform child in m_mapPreviewContainer.transform)
        {
            Destroy(child.gameObject);
        }

        string folder = Application.persistentDataPath + "/Maps";
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        else
        {
            string[] filepaths = Directory.GetFiles(folder, "*.map", SearchOption.TopDirectoryOnly);
            foreach (string filepath in filepaths)
            {
                string name = Path.GetFileName(filepath);
                AddMapToPanel(filepath, name);
            }
        }
    }


    private void AddMapToPanel(string filepath, string name)
    {
        GameObject newMap = Instantiate(m_mapPreviewTemplate, m_mapPreviewContainer.transform);
        MapPreviewBehaviour map = newMap.GetComponent<MapPreviewBehaviour>();
        map.transform.Find("LoadButton").GetComponent<Button>().onClick.AddListener(() => OnLoadMapPressed(name));

        long fileSize = new FileInfo(filepath).Length;
        DateTime lastModified = File.GetLastWriteTime(filepath);

        map.SetName(name);
        map.SetFileSize(fileSize);
        map.SetLastModified(lastModified);
    }


    public void OnBackButtonPressed()
    {
        m_loadPanel.SetActive(false);
    }


    public void OnLoadMapPressed(string filename)
    {
        // If there is a map, load the map locally
        if (m_map != null)
        {
            m_map.LoadMapFromFile(filename);
            OnBackButtonPressed();
        }

        // If there is not a map, save the map to GameSettings for future use in networking
        else
        {
            GameSettings.Saved.Data.Map = Saving.LoadFromFile<MapData>($"Maps/{filename}");
            Saving.SaveToFile(GameSettings.Saved, "GameSettings.dat");
        }
    }
}
