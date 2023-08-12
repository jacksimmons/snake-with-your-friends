using Steamworks;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private Button m_noFriendsButton;
    [SerializeField]
    private Button m_createButton;
    [SerializeField]
    private Button m_joinButton;
    [SerializeField]
    private Button m_refreshButton;
    [SerializeField]
    private GameObject m_networkManager;

    private void Start()
    {
        // Ensures Chungus instance
        Chungus.LoadSettings();

        TestSteamConnection();
        if (!GameObject.Find("NetworkManager"))
        {
            GameObject go = Instantiate(m_networkManager);
            go.name = "NetworkManager";
        }
    }

    public void TestSteamConnection()
    {
        if (!SteamManager.Initialized)
        {
            m_noFriendsButton.interactable = false;
            m_createButton.interactable = false;
            m_joinButton.interactable = false;
            m_refreshButton.gameObject.SetActive(true);
        }
        else if (!SteamUser.BLoggedOn())
        {
            m_noFriendsButton.interactable = true;
            m_createButton.interactable = false;
            m_joinButton.interactable = false;
            m_refreshButton.gameObject.SetActive(true);
        }
        else
        {
            m_noFriendsButton.interactable = true;
            m_createButton.interactable = true;
            m_joinButton.interactable = true;
            m_refreshButton.gameObject.SetActive(false);
        }
    }

    public void OnNoFriendsButtonPressed()
    {
        GameObject go = GameObject.Find("NetworkManager");

        if (go != null)
            go.GetComponent<SteamLobby>().HostLobby(singleplayer: true);
    }

    public void OnCreateLobbyButtonPressed()
    {
        if (!SteamUser.BLoggedOn())
        {
            print("Not online!");
            return;
        }

        Chungus.ShowLoadingSymbolUntil(() => SceneManager.GetActiveScene().name == "LobbyMenu");

        GameObject go = GameObject.Find("NetworkManager");

        if (go != null)
            go.GetComponent<SteamLobby>().HostLobby(singleplayer: false);
    }

    public void OnJoinLobbyButtonPressed()
    {
        if (!SteamUser.BLoggedOn())
        {
            print("Not online!");
            return;
        }

        Chungus.LoadSceneWithLoadingSymbol("JoinMenu");
    }

    public void OnSettingsButtonPressed()
    {
        Chungus.LoadSceneWithLoadingSymbol("SettingsMenu");
    }

    public void Restart()
    {
        // Will enable the loading symbol
        Chungus.ToggleLoadingSymbol(true);
        Chungus.ClearDontDestroyOnLoad();

        // Will enable then disable the loading symbol
        Chungus.LoadSceneWithLoadingSymbol("MainMenu");
    }

    public void Quit()
    {
        Chungus.ShowLoadingSymbolUntil(() => false);
        Application.Quit();
    }
}
