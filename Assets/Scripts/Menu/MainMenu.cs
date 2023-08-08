using Steamworks;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private Button m_offlineButton;
    [SerializeField]
    private Button m_createButton;
    [SerializeField]
    private Button m_joinButton;
    [SerializeField]
    private Button m_refreshButton;

    public void Start()
    {
        TestSteamConnection();
    }

    public void Restart()
    {
        GameObject sm = GameObject.Find("SteamManager");
        if (sm != null) Destroy(sm);

        SceneManager.LoadScene("MainMenu");
    }

    public void TestSteamConnection()
    {
        if (!SteamManager.Initialized)
        {
            m_createButton.interactable = false;
            m_joinButton.interactable = false;
            m_refreshButton.gameObject.SetActive(true);
        }
        else
        {
            m_createButton.interactable = true;
            m_joinButton.interactable = true;
            m_refreshButton.gameObject.SetActive(false);
        }
    }

    public void OnJoinLobbyButtonPressed()
    {
        if (!SteamUser.BLoggedOn())
        {
            print("Not online!");
            return;
        }

        SceneManager.LoadScene("JoinMenu");
    }

    public void OnSettingsButtonPressed()
    {
        SceneManager.LoadScene("SettingsMenu");
    }
}
