using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Class used for scenes where Lobby is not present
/// </summary>
public class BackButton : MonoBehaviour
{
    public void OnBackButtonPressed()
    {
        try
        {
            Destroy(GameObject.FindWithTag("Lobby"));
        }
        catch { }
        SceneManager.LoadScene("MainMenu");
    }
}
