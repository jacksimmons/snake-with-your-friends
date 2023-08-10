using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButton : MonoBehaviour
{
    public void OnMainMenuButtonPressed()
    {
        CustomNetworkManager cnm = GameObject.Find("NetworkManager").GetComponent<CustomNetworkManager>();
        cnm.StopClient();
        SceneManager.LoadScene("MainMenu");
    }
}
