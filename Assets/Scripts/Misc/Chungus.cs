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

    public static void ClearDontDestroyOnLoad()
    {
        foreach (var root in Instance.gameObject.scene.GetRootGameObjects())
        {
            if (!Instance.dontDestroyThese.Contains(root.name))
                Destroy(root);
        }
    }
}
