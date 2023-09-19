using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingIcon : MonoBehaviour
{
    private static LoadingIcon _instance;
    public static LoadingIcon Instance
    {
        get
        {
            if (_instance != null) return _instance;
            return _instance = GameObject.FindWithTag("LoadingIcon")
                .GetComponent<LoadingIcon>();
        }
    }

    void Awake()
    {
        // Only one loading icon can exist
        if (GameObject.FindWithTag("LoadingIcon") != gameObject)
            Destroy(gameObject);
    }

    private void Start()
    {
        SceneManager.activeSceneChanged -= OnSceneChange;
        SceneManager.activeSceneChanged += OnSceneChange;

        DontDestroyOnLoad(gameObject);
    }

    // Update rendering camera on scene change
    private void OnSceneChange(Scene prev, Scene next)
    {
        GameObject cam;
        if (cam = GameObject.FindWithTag("MainCamera"))
        {
            GetComponent<Canvas>().worldCamera = cam.GetComponent<Camera>();
        }
        Toggle(false);
    }

    public void Toggle(bool active)
    {
        transform.GetChild(0).gameObject.SetActive(active);
    }

    public void ShowLoadingSymbolUntil(Func<bool> check)
    {
        Toggle(true);
        StartCoroutine(
            Wait.WaitForConditionThen(
                check,
                0.1f,
                () => Toggle(false)
            )
        );
    }
}
