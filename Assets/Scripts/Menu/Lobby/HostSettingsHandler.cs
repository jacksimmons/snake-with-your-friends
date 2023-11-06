using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostSettingsHandler : MonoBehaviour
{
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
    private Slider m_mapSizeSlider;
    [SerializeField]
    private TextMeshProUGUI m_mapSizeLabel;

    [SerializeField]
    private GameObject[] m_powerupToggleContainers;

    private GameSettings m_currentGameSettings;


    private void Start()
    {
        if (GameSettings.Saved == null)
            m_currentGameSettings = new GameSettings();
        else
            m_currentGameSettings = GameSettings.Saved;

        m_speedSlider.value = m_currentGameSettings.TimeToMove;
        m_speedLast = m_speedSlider.value;
        UpdateSpeedLabels();

        m_friendlyFireToggle.isOn = m_currentGameSettings.FriendlyFire;
        m_friendlyFireToggle.onValueChanged.AddListener(OnFriendlyFireTogglePressed);
        SetFriendlyFireLabel(m_friendlyFireToggle.isOn);

        m_gameModeDropdown.onValueChanged.AddListener(OnGameModeUpdate);

        m_mapSizeSlider.value = m_currentGameSettings.GameSize;
        m_mapSizeSlider.onValueChanged.AddListener(OnMapSizeUpdate);
        SetMapSizeLabel(m_mapSizeSlider.value);

        foreach (GameObject go in m_powerupToggleContainers)
        {
            EFoodType foodType = go.GetComponent<PowerupToggleContainer>().foodType;
            go.GetComponentInChildren<Toggle>().onValueChanged.AddListener((pressed) => OnPowerupTogglePressed(pressed, foodType));
            go.GetComponentInChildren<Toggle>().isOn = m_currentGameSettings.foodSettings.GetFoodEnabled(foodType);
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

        m_currentGameSettings.TimeToMove = m_speedSlider.value;
    }


    public void OnMapSizeUpdate(float value)
    {
        SetMapSizeLabel(value);
        m_currentGameSettings.GameSize = (int)value;
    }
    private void SetMapSizeLabel(float value)
    {
        m_mapSizeLabel.text = $"Map Size ({(int)value})";
    }


    public void OnFriendlyFireTogglePressed(bool pressed)
    {
        SetFriendlyFireLabel(pressed);
        m_currentGameSettings.FriendlyFire = pressed;
    }
    private void SetFriendlyFireLabel(bool pressed)
    {
        string onOrOff = pressed ? "ON" : "OFF";
        m_friendlyFireLabel.text = $"Friendly Fire ({onOrOff})";
    }


    public void OnGameModeUpdate(int index)
    {
        m_currentGameSettings.GameMode = 
        (EGameMode)Enum.GetValues(typeof(EGameMode)).GetValue(index);
    }


    public void OnPowerupTogglePressed(bool pressed, EFoodType food)
    {
        m_currentGameSettings.foodSettings.SetFoodEnabled(food, pressed);
    }


    /// <summary>
    /// When host Settings is closed, save GameSettings to file, making it the player's new default.
    /// </summary>
    public void OnClose()
    {
        Saving.SaveToFile(m_currentGameSettings, "GameSettings.dat");
    }
}
