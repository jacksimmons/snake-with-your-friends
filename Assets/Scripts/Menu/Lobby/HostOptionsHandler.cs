using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostOptionsHandler : MonoBehaviour
{
    // Player will move every (FPS - speed) frames
    private int m_speed;
    private bool m_selfDmg;
    private Dictionary<EFoodType, bool> m_powerups;

    [SerializeField]
    private Slider m_speedSlider;
    [SerializeField]
    private TextMeshProUGUI m_speedLabel;
    [SerializeField]
    private TextMeshProUGUI m_speedVerbose;

    [SerializeField]
    private Toggle m_selfDmgToggle;
    [SerializeField]
    private TextMeshProUGUI m_selfDmgLabel;
    [SerializeField]
    private TextMeshProUGUI m_selfDmgVerbose;

    [SerializeField]
    private GameObject[] m_powerupToggles;

    private void Start()
    {
        m_speedSlider.onValueChanged.AddListener(OnSpeedSliderUpdate);
        m_speedSlider.value = 30;

        m_selfDmgToggle.onValueChanged.AddListener(OnSelfDmgTogglePressed);
        OnSelfDmgTogglePressed(true);

        m_powerups = new()
        {
            { EFoodType.Apple, false },
            { EFoodType.Orange, false },
            { EFoodType.Banana, false },
            { EFoodType.Dragonfruit, false },
            { EFoodType.Balti, false },
            { EFoodType.Brownie, false },
            { EFoodType.Drumstick, false },
            { EFoodType.Booze, false },
        };

        foreach (GameObject go in m_powerupToggles)
        {
            EFoodType foodType = go.GetComponent<PowerupToggle>().foodType;
            go.GetComponentInChildren<Toggle>().onValueChanged.AddListener((pressed) => OnPowerupTogglePressed(pressed, foodType));
        }
    }

    public void OnSpeedSliderUpdate(float value)
    {
        m_speedVerbose.text = string.Format($"Snakes move every {1 - (float)value / 60:F2} seconds");
        m_speedLabel.text = string.Format($"Speed ({value})");
        m_speed = (int)value;
    }

    public void OnSelfDmgTogglePressed(bool pressed)
    {
        string onOrOff = pressed ? "ON" : "OFF";
        m_selfDmgVerbose.text = string.Format($"Self-inflicted damage is {onOrOff}");
        m_selfDmgLabel.text = string.Format($"Friendly Fire ({onOrOff})");
        m_selfDmg = pressed;
    }

    public void OnPowerupTogglePressed(bool pressed, EFoodType food)
    {
        m_powerups[food] = pressed;
    }

    public void OnClose()
    {
    }
}
