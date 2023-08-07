using Steamworks;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private Button m_createLobbyButton;
    [SerializeField]
    private Button m_joinLobbyButton;
    [SerializeField]
    private Button m_retryNetworkButton;

    void Start()
    {
        TestSteamConnection();
    }

    public void TestSteamConnection()
    {
        if (!SteamManager.Initialized || !SteamUser.BLoggedOn())
        {
            m_createLobbyButton.interactable = false;
            m_joinLobbyButton.interactable = false;
        }
        else
        {
            m_createLobbyButton.interactable = true;
            m_joinLobbyButton.interactable = true;
        }
    }

    public void OnNoFriendsPressed()
    {
        SceneManager.LoadScene("Game");
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
