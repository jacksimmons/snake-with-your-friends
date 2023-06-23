using Steamworks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateMenu : MonoBehaviour
{
    public int _speed = 0;

    [SerializeField]
    private Slider _speedSlider;
    [SerializeField]
    private TextMeshProUGUI _speedValue;

    private void Start()
    {
        _speed = (int)_speedSlider.value;
        _speedValue.text = "-" + _speedSlider.value.ToString();
    }

    public void OnSpeedSliderUpdate(float value)
    {
        _speedValue.text = "-" + value.ToString();
        _speed = (int)value;
    }
}
