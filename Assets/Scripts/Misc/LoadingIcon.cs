using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingIcon : MonoBehaviour
{
    public static LoadingIcon Instance
    {
        get
        {
            return GameObject.FindWithTag("LoadingIcon")
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
                new WaitForEndOfFrame(),
                () => Toggle(false)
            )
        );
    }

    public void LoadSceneWithLoadingSymbol(string sceneName)
    {
        Toggle(true);
        StartCoroutine(
            Wait.WaitForLoadSceneThen(
                sceneName,
                new WaitForEndOfFrame(),
                () => Toggle(false)
            )
        );
    }
}
