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

    public void Start()
    {
        TestSteamConnection();
        if (!GameObject.Find("NetworkManager"))
        {
            GameObject go = Instantiate(m_networkManager);
            go.name = "NetworkManager";
        }
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

    public void OnNoFriendsButtonPressed()
    {
        GameObject go = GameObject.Find("NetworkManager");

        if (go != null)
            go.GetComponent<SteamLobby>().HostLobby();
    }

    public void OnCreateLobbyButtonPressed()
    {
        if (!SteamUser.BLoggedOn())
        {
            print("Not online!");
            return;
        }

        GameObject go = GameObject.Find("NetworkManager");

        if (go != null)
            go.GetComponent<SteamLobby>().HostLobby();
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
