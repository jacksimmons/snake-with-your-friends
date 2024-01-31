using Steamworks;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : SceneTransitionHandler
{
    [SerializeField]
    private Button[] m_buttonsRequiringAuth;
    [SerializeField]
    private Button[] m_buttonsRequiringOnline;
    [SerializeField]
    private Button m_retryButton;
    [SerializeField]
    private GameObject m_networkManager;
    [SerializeField]
    private GameObject m_transitionSnakeSpawner;


    protected override void Start()
    {
        base.Start();

        if (Settings.Saved == null)
            LoadSettings();

        if (SaveData.Saved == null)
            Saving.LoadFromFile<SaveData>("SaveData.dat");

        TestSteamConnection();
        if (!GameObject.Find("NetworkManager"))
        {
            GameObject go = Instantiate(m_networkManager);
            go.name = "NetworkManager";
        }

        if (!GameObject.Find("TransitionSnakeSpawner"))
        {
            GameObject go = Instantiate(m_transitionSnakeSpawner);
            go.name = "TransitionSnakeSpawner";
        }
    }

    private void LoadSettings()
    {
        Saving.LoadFromFile<Settings>("Settings.dat");

        GameObject audioParent = GameObject.FindWithTag("AudioHandler");
        audioParent.transform.Find("ClickHandler").GetComponent<AudioSource>().volume = Settings.Saved.menuVolume;
        audioParent.transform.Find("ButtonPressHandler").GetComponent<AudioSource>().volume = Settings.Saved.menuVolume;
        audioParent.transform.Find("EatHandler").GetComponent<AudioSource>().volume = Settings.Saved.sfxVolume;

        FullScreenMode fsm = Settings.GetWindowMode(Settings.Saved.Fullscreen, Settings.Saved.Borderless);
        Screen.SetResolution(Settings.Saved.resX, Settings.Saved.resY, fsm, Settings.Saved.resHz);

        //print($"Resolution: {Settings.Saved.resX}x{Settings.Saved.resY}@{Settings.Saved.resHz}");
        //print($"Fullscreen: {Settings.Saved.Fullscreen} Borderless: {Settings.Saved.Borderless}");
        //print($"Volume: MENU[{Settings.Saved.menuVolume}], SFX[{Settings.Saved.sfxVolume}]");
    }

    public void TestSteamConnection()
    {
        if (!SteamManager.Initialized)
        {
            ToggleButtons(m_buttonsRequiringAuth, false);
            m_retryButton.gameObject.SetActive(true);
        }
        else if (!SteamUser.BLoggedOn())
        {
            ToggleButtons(m_buttonsRequiringOnline, false);
            m_retryButton.gameObject.SetActive(true);
        }
        else
        {
            m_retryButton.gameObject.SetActive(false);
        }
    }

    private void ToggleButtons(Button[] buttonArray, bool interactable)
    {
        foreach (Button button in buttonArray)
            button.interactable = interactable;
    }

    public void OnNoFriendsButtonPressed()
    {
        LoadingIcon.Instance.Toggle(true);
        Steam.Instance.HostLobby(singleplayer: true);
    }

    public void OnCreateLobbyButtonPressed()
    {
        if (!SteamUser.BLoggedOn())
        {
            print("Not online!");
            return;
        }

        LoadingIcon.Instance.Toggle(true);
        Steam.Instance.HostLobby(singleplayer: false);
    }

    public void OnJoinLobbyButtonPressed()
    {
        if (!SteamUser.BLoggedOn())
        {
            print("Not online!");
            return;
        }

        LoadScene("JoinMenu");
    }

    public void OnSettingsButtonPressed()
    {
        LoadScene("SettingsMenu");
    }

    public void OnEditorButtonPressed()
    {
        LoadScene("EditorMenu");
    }

    public void OnRetryButtonPressed()
    {
        Chungus.ClearDontDestroyOnLoad();
        ReloadScene("MainMenu");
    }

    public void Quit()
    {
        LoadingIcon.Instance.Toggle(true);
        Application.Quit();

        Steam.Instance.GiveAchievement("ACH_TEST");
    }
}
