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


    /// <summary>
    /// Listener called when a button which loads a map-load scenario (a menu filled with maps to load)
    /// is pressed.
    /// </summary>
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


    /// <summary>
    /// Listener called when "Back" is pressed in a map-load scenario.
    /// </summary>
    public void OnBackButtonPressed()
    {
        m_loadPanel.SetActive(false);
    }


    /// <summary>
    /// Listener called when a map is selected to be loaded in a map-load scenario.
    /// Appears in the Lobby and Editor menus.
    /// </summary>
    public void OnLoadMapPressed(string filename)
    {
        // If we are in the editor, load the map locally.
        if (m_map != null)
        {
            m_map.LoadMapFromFile(filename);
            OnBackButtonPressed();
        }

        // If we are not in the editor, put the map into GameSettings for when the lobby starts.
        else
        {
            GameSettings.Saved.Data.Map = Saving.LoadFromFile<MapData>($"Maps/{filename}");
            Saving.SaveToFile(GameSettings.Saved, "GameSettings.dat");
        }
    }
}
