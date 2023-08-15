using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

// Chungus always persists. Nobody like chungus.
// A singleton class which contains always-available (global) methods.

// Provides assurance (*) that the singleton is always defined after Awake.
public class Chungus : MonoBehaviour
{
    // Fields
    private string[] dontDestroyThese =
    {
        "Chungus",
        "Loading"
    };
    public Settings settings = null;
    public GameObject LoadingObj { get; private set; } = null;

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
                _instance.LoadingObj = GameObject.Find("Loading");
                _instance.LoadingObj.SetActive(false);
                DontDestroyOnLoad(_instance.gameObject);
                DontDestroyOnLoad(_instance.LoadingObj);
            }
            return _instance;
        }
    }

    public static void ToggleLoadingSymbol(bool show)
    {
        Instance.LoadingObj.SetActive(show);
    }

    public static void ShowLoadingSymbolUntil(Func<bool> check)
    {
        ToggleLoadingSymbol(true);
        Instance.StartCoroutine(
            Wait.WaitForConditionThen(
                check,
                new WaitForEndOfFrame(),
                () => ToggleLoadingSymbol(false)
            )
        );
    }

    public static void LoadSceneWithLoadingSymbol(string sceneName)
    {
        ToggleLoadingSymbol(true);
        Instance.StartCoroutine(
            Wait.WaitForLoadSceneThen(
                sceneName,
                new WaitForEndOfFrame(),
                () => ToggleLoadingSymbol(false)
            )
        );
    }

    public static void LoadSettings()
    {
        string dest = Application.persistentDataPath + "/settings.dat";
        FileStream fs;

        if (File.Exists(dest))
        {
            fs = File.OpenRead(dest);
            BinaryFormatter bf = new BinaryFormatter();
            Instance.settings = (Settings)bf.Deserialize(fs);
        }
        else fs = File.Create(dest);
        fs.Close();

        GameObject audioParent = GameObject.FindWithTag("AudioHandler");
        audioParent.transform.Find("ClickHandler").GetComponent<AudioSource>().volume = Instance.settings.menuVolume;
        audioParent.transform.Find("ButtonPressHandler").GetComponent<AudioSource>().volume = Instance.settings.menuVolume;
        audioParent.transform.Find("EatHandler").GetComponent<AudioSource>().volume = Instance.settings.sfxVolume;

        FullScreenMode fsm = Settings.GetWindowMode(Instance.settings.fullscreen, Instance.settings.borderless);
        Screen.SetResolution(Instance.settings.resX, Instance.settings.resY, fsm, Instance.settings.resHz);

        print($"Resolution: {Instance.settings.resX}x{Instance.settings.resY}@{Instance.settings.resHz}");
        print($"Fullscreen: {Instance.settings.fullscreen} Borderless: {Instance.settings.borderless}");
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
