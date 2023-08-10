using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour
{
    [SerializeField]
    private GameObject m_playerObject;

    private void Start()
    {
        GameObject go = Instantiate(m_playerObject);
        go.name = "LocalPlayerObject";
        SceneManager.LoadScene("Game");
        StartCoroutine(Wait.WaitForObject(() => GameObject.FindWithTag("MainCamera"),
            (GameObject obj) => { go.GetComponentInChildren<GameBehaviour>().OnPlayerReady(); },
            new WaitForEndOfFrame()));
    }
}
