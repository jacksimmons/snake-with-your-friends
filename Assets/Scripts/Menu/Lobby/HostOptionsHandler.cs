using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    private GameSettings m_currentGameSettings = new();

    private void Start()
    {
        m_speedSlider.onValueChanged.AddListener(OnSpeedSliderUpdate);
        m_speedSlider.value = GameSettings.Saved.CounterMax;

        m_friendlyFireToggle.onValueChanged.AddListener(OnFriendlyFireTogglePressed);
        OnFriendlyFireTogglePressed(true);

        foreach (GameObject go in m_powerupToggleContainers)
        {
            EFoodType foodType = go.GetComponent<PowerupToggleContainer>().foodType;
            go.GetComponentInChildren<Toggle>().onValueChanged.AddListener((pressed) => OnPowerupTogglePressed(pressed, foodType));
            go.GetComponentInChildren<Toggle>().isOn = !GameSettings.Saved.DisabledFoods.Contains(foodType);
        }
    }

    public void OnSpeedSliderUpdate(float value)
    {
        m_speedVerbose.text = $"Snakes move every {(float)value / 60:F2} seconds";
        m_speedLabel.text = $"Movement Frequency ({value})";

        m_currentGameSettings.CounterMax = (int)value;
    }

    public void OnFriendlyFireTogglePressed(bool pressed)
    {
        string onOrOff = pressed ? "ON" : "OFF";
        m_friendlyFireVerbose.text = $"Self-inflicted damage is {onOrOff}";
        m_friendlyFireLabel.text = $"Friendly Fire ({onOrOff})";

        m_currentGameSettings.FriendlyFire = pressed;
    }

    public void OnPowerupTogglePressed(bool pressed, EFoodType food)
    {
        if (!pressed)
            m_currentGameSettings.DisableFood(food);
        else
            m_currentGameSettings.EnableFood(food);
    }

    /// <summary>
    /// When host options is closed, save GameSettings to file, making it the player's new default.
    /// </summary>
    public void OnClose()
    {
        GameSettings.Saved = new(m_currentGameSettings);

        SaveData.SaveToFile(GameSettings.Saved, "GameSettings.dat");
    }
}
