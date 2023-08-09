using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButton : MonoBehaviour
{
    public void OnMainMenuButtonPressed()
    {
        GameObject.Find("NetworkManager").GetComponent<CustomNetworkManager>().StopClient();
        SceneManager.LoadScene("MainMenu");
    }
}
