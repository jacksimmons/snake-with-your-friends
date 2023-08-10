using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

// Chungus always persists. Nobody like chungus.
public class Chungus : MonoBehaviour
{
    public Settings m_settings = null;

    private void Start()
    {
        // There can only be one
        name = "Blungus";
        if (GameObject.Find("Chungus") != null)
            Destroy(gameObject);
        name = "Chungus";

        string dest = Application.persistentDataPath + "/settings.dat";
        FileStream fs;

        if (File.Exists(dest))
        {
            fs = File.OpenRead(dest);
            BinaryFormatter bf = new BinaryFormatter();
            m_settings = (Settings)bf.Deserialize(fs);
            LoadSettings();
        }
        else fs = File.Create(dest);
        fs.Close();
    }

    public void LoadSettings()
    {
        GameObject audioParent = GameObject.FindWithTag("AudioHandler");
        audioParent.transform.Find("ClickHandler").GetComponent<AudioSource>().volume = m_settings.menuVolume;
        audioParent.transform.Find("ButtonPressHandler").GetComponent<AudioSource>().volume = m_settings.menuVolume;
        audioParent.transform.Find("EatHandler").GetComponent<AudioSource>().volume = m_settings.sfxVolume;

        Screen.SetResolution(m_settings.resX, m_settings.resY, m_settings.fullscreen, m_settings.resHz);

        print($"Resolution: {m_settings.resX}x{m_settings.resY}@{m_settings.resHz}");
        print($"Fullscreen: {m_settings.fullscreen}");
        print($"Volume: MENU[{m_settings.menuVolume}], SFX[{m_settings.sfxVolume}]");
    }

    public void ClearDontDestroyOnLoad()
    {
        foreach (var root in gameObject.scene.GetRootGameObjects())
        {
            if (root != gameObject && root.name != "SteamManager")
                Destroy(root);
        }
    }
}
