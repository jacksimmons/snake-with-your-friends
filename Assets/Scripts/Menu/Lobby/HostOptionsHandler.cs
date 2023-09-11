using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostOptionsHandler : MonoBehaviour
{
    [SerializeField]
    private Slider m_speedSlider;
    [SerializeField]
    private TextMeshProUGUI m_speedLabel;
    [SerializeField]
    private TextMeshProUGUI m_speedVerbose;

    [SerializeField]
    private Toggle m_friendlyFireToggle;
    [SerializeField]
    private TextMeshProUGUI m_friendlyFireLabel;
    [SerializeField]
    private TextMeshProUGUI m_friendlyFireVerbose;

    [SerializeField]
    private GameObject[] m_powerupToggleContainers;

    public static GameSettings SavedGameSettings { get; private set; }
    public GameSettings CurrentGameSettings { get; private set; }

    private void Start()
    {
        m_speedSlider.onValueChanged.AddListener(OnSpeedSliderUpdate);
        m_speedSlider.value = 30;

        m_friendlyFireToggle.onValueChanged.AddListener(OnFriendlyFireTogglePressed);
        OnFriendlyFireTogglePressed(true);

        foreach (GameObject go in m_powerupToggleContainers)
        {
            EFoodType foodType = go.GetComponent<PowerupToggleContainer>().foodType;
            go.GetComponentInChildren<Toggle>().onValueChanged.AddListener((pressed) => OnPowerupTogglePressed(pressed, foodType));
        }

        SavedGameSettings = new();
        CurrentGameSettings = new();
    }

    public void OnSpeedSliderUpdate(float value)
    {
        m_speedVerbose.text = string.Format($"Snakes move every {1 - (float)value / 60:F2} seconds");
        m_speedLabel.text = string.Format($"Speed ({value})");

        CurrentGameSettings.PlayerSpeed = (int)value;
    }

    public void OnFriendlyFireTogglePressed(bool pressed)
    {
        string onOrOff = pressed ? "ON" : "OFF";
        m_friendlyFireVerbose.text = string.Format($"Self-inflicted damage is {onOrOff}");
        m_friendlyFireLabel.text = string.Format($"Friendly Fire ({onOrOff})");

        CurrentGameSettings.FriendlyFire = pressed;
    }

    public void OnPowerupTogglePressed(bool pressed, EFoodType food)
    {
        if (!pressed)
            CurrentGameSettings.DisableFood(food);
        else
            CurrentGameSettings.EnableFood(food);
    }

    public void OnClose()
    {
        SavedGameSettings = new(CurrentGameSettings);
    }
}
