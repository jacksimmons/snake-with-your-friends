using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

// Always persists. Nobody like chungus.
// A singleton class which contains always-available (global) methods.

// Provides assurance (*) that the singleton is always defined after Awake.
public class Chungus : MonoBehaviour
{
    // Fields
    private string[] dontDestroyThese =
    {
        "Chungus",
        "LoadingCanvas"
    };
    public Settings settings = null;

    // Singleton
    private static Chungus _instance;
    public static Chungus Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = new GameObject().AddComponent<Chungus>();
                _instance.name = "Chungus";
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }

    public static void LoadSettings()
    {
        Settings settings = SaveData.LoadFromFile<Settings>("Settings.dat");
        if (settings == null) return;

        Instance.settings = settings;

        GameObject audioParent = GameObject.FindWithTag("AudioHandler");
        audioParent.transform.Find("ClickHandler").GetComponent<AudioSource>().volume = Instance.settings.menuVolume;
        audioParent.transform.Find("ButtonPressHandler").GetComponent<AudioSource>().volume = Instance.settings.menuVolume;
        audioParent.transform.Find("EatHandler").GetComponent<AudioSource>().volume = Instance.settings.sfxVolume;

        FullScreenMode fsm = Settings.GetWindowMode(Instance.settings.Fullscreen, Instance.settings.Borderless);
        Screen.SetResolution(Instance.settings.resX, Instance.settings.resY, fsm, Instance.settings.resHz);

        print($"Resolution: {Instance.settings.resX}x{Instance.settings.resY}@{Instance.settings.resHz}");
        print($"Fullscreen: {Instance.settings.Fullscreen} Borderless: {Instance.settings.Borderless}");
        print($"Volume: MENU[{Instance.settings.menuVolume}], SFX[{Instance.settings.sfxVolume}]");
    }

    public static void ClearDontDestroyOnLoad()
    {
        foreach (var root in Instance.gameObject.scene.GetRootGameObjects())
        {
            if (!Instance.dontDestroyThese.Contains(root.name))
                Destroy(root);
        }
    }
}
