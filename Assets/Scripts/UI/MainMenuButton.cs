using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButton : MonoBehaviour
{
    public void OnMainMenuButtonPressed()
    {
        GameObject nm = GameObject.Find("NetworkManager");
        if (nm != null)
        {
            CustomNetworkManager cnm = nm.GetComponent<CustomNetworkManager>();
            cnm.StopClient();
            cnm.StopHost();
        }

        LoadingIcon.Instance.Toggle(true);
        SceneManager.LoadScene("MainMenu");
    }
}
