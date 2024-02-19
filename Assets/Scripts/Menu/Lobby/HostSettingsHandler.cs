using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostSettingsHandler : MonoBehaviour
{
    [SerializeField]
    private LobbyMenu m_lobbyMenu;

    [SerializeField]
    private Slider m_speedSlider;
    [SerializeField]
    private TextMeshProUGUI m_speedLabel;
    [SerializeField]
    private TextMeshProUGUI m_speedVerbose;
    private float m_speedLast;

    [SerializeField]
    private Toggle m_friendlyFireToggle;
    [SerializeField]
    private TextMeshProUGUI m_friendlyFireLabel;

    [SerializeField]
    private TMP_Dropdown m_gameModeDropdown;

    [SerializeField]
    private GameObject[] m_powerupToggleContainers;


    private void Start()
    {
        m_speedSlider.value = GameSettings.Saved.Data.TimeToMove;
        m_speedLast = m_speedSlider.value;
        UpdateSpeedLabels();

        m_friendlyFireToggle.isOn = GameSettings.Saved.Data.FriendlyFire;
        m_friendlyFireToggle.onValueChanged.AddListener(OnFriendlyFireTogglePressed);
        SetFriendlyFireLabel(m_friendlyFireToggle.isOn);

        m_gameModeDropdown.value = (int)GameSettings.Saved.Data.GameMode;
        m_gameModeDropdown.onValueChanged.AddListener(OnGameModeUpdate);

        foreach (GameObject go in m_powerupToggleContainers)
        {
            EFoodType foodType = go.GetComponent<PowerupToggleContainer>().foodType;
            go.GetComponentInChildren<Toggle>().onValueChanged.AddListener((pressed) => OnPowerupTogglePressed(pressed, foodType));
            go.GetComponentInChildren<Toggle>().isOn = GameSettings.Saved.FoodSettings.GetBit((int)foodType);
        }
    }


    private void Update()
    {
        if (m_speedSlider.value != m_speedLast)
        {
            UpdateSpeedLabels();
        }
    }


    public void UpdateSpeedLabels()
    {
        m_speedLast = m_speedSlider.value;

        m_speedVerbose.text = $"Snakes move every {m_speedSlider.value} seconds";

        string speed;
        if (m_speedSlider.value <= 0.25f) speed = ": BAG GUY";
        else if (m_speedSlider.value <= 0.5f) speed = ": FAST";
        else if (m_speedSlider.value <= 0.75f) speed = ": SLOW";
        else speed = ": SNAIL";

        m_speedLabel.text = $"Time Between Moves ({m_speedSlider.value}{speed})";

        GameSettings.Saved.Data.TimeToMove = m_speedSlider.value;
        m_lobbyMenu.SaveGameSettings();
    }


    public void OnFriendlyFireTogglePressed(bool pressed)
    {
        SetFriendlyFireLabel(pressed);
        GameSettings.Saved.Data.FriendlyFire = pressed;
        m_lobbyMenu.SaveGameSettings();
    }


    private void SetFriendlyFireLabel(bool pressed)
    {
        string onOrOff = pressed ? "ON" : "OFF";
        m_friendlyFireLabel.text = $"Friendly Fire ({onOrOff})";
        m_lobbyMenu.SaveGameSettings();
    }


    public void OnGameModeUpdate(int index)
    {
        GameSettings.Saved.Data.GameMode = 
        (EGameMode)Enum.GetValues(typeof(EGameMode)).GetValue(index);
        m_lobbyMenu.SaveGameSettings();
    }


    public void OnPowerupTogglePressed(bool pressed, EFoodType food)
    {
        GameSettings.Saved.FoodSettings.SetBit((int)food, pressed);
        m_lobbyMenu.SaveGameSettings();
    }
}
