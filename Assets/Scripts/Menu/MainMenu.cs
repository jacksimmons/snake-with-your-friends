using Steamworks;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : SceneTransitionHandler
{
    private const int VERSION_MAJOR = 1;
    private const int VERSION_MINOR = 0;


    [SerializeField]
    private Button[] m_buttonsRequiringAuth;
    [SerializeField]
    private Button[] m_buttonsRequiringOnline;
    [SerializeField]
    private Button m_retryButton;
    [SerializeField]
    private GameObject m_networkManager;


    protected override void Start()
    {
        base.Start();

        if (true)
            ResetSettings();
        LoadAllSettings();
        TestSteamConnection();

        if (!GameObject.Find("NetworkManager"))
        {
            GameObject go = Instantiate(m_networkManager);
            go.name = "NetworkManager";
        }
    }


    private void LoadAllSettings()
    {
        if (Settings.Saved == null)
            Saving.LoadFromFile<Settings>("Settings.dat");
        if (SaveData.Saved == null)
            Saving.LoadFromFile<SaveData>("SaveData.dat");
        if (OutfitSettings.Saved == null)
            Saving.LoadFromFile<OutfitSettings>("OutfitSettings.dat");
        if (GameSettings.Saved == null)
            Saving.LoadFromFile<GameSettings>("GameSettings.dat");
    }


    public void ResetSettings()
    {
        if (Settings.Saved != null)
            Saving.SaveToFile<Settings>(null, "Settings.dat");
        if (SaveData.Saved != null)
            Saving.SaveToFile<SaveData>(null, "SaveData.dat");
        if (OutfitSettings.Saved != null)
            Saving.SaveToFile<OutfitSettings>(null, "OutfitSettings.dat");
        if (GameSettings.Saved != null)
            Saving.SaveToFile<GameSettings>(null, "GameSettings.dat");
        LoadAllSettings();
    }


    private void LoadSettings()
    {
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
