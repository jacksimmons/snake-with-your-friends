using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    private GameObject audioHandler;

    [SerializeField]
    private TextMeshProUGUI menuVolumeLabel;
    [SerializeField]
    private Slider menuVolumeSlider;
    private float menuVolumeValue;

    [SerializeField]
    private TextMeshProUGUI sfxVolumeLabel;
    [SerializeField]
    private Slider sfxVolumeSlider;
    private float sfxVolumeValue;

    private Resolution[] resolutions;
    [SerializeField]
    private TMP_Dropdown resDropdown;

    [SerializeField]
    private Toggle fullscreenToggle;

    [SerializeField]
    private TextMeshProUGUI brightnessLabel;
    [SerializeField]
    private Slider brightnessSlider;

    private void Start()
    {
        audioHandler = GameObject.FindWithTag("AudioHandler");

        // Disable clicking sound for testing purposes
        audioHandler.transform.Find("ClickHandler").GetComponent<AudioSource>().volume = 0;

        menuVolumeSlider.onValueChanged.AddListener(SetMenuVolume);
        menuVolumeSlider.value = audioHandler.transform.Find("ButtonPressHandler").GetComponent<AudioSource>().volume * 100;

        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        sfxVolumeSlider.value = audioHandler.transform.Find("EatHandler").GetComponent<AudioSource>().volume * 100;

        resolutions = Screen.resolutions;
        resDropdown.onValueChanged.AddListener(SetResolution);
        for (int i = 0; i < resolutions.Length; i++)
        {
            var res = resolutions[i];
            resDropdown.options.Add(new TMP_Dropdown.OptionData(res.ToString()));
            if (ResolutionEquals(res, Screen.currentResolution))
            {
                resDropdown.itemText.text = res.ToString();
                resDropdown.value = i;
            }
        }

        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        fullscreenToggle.isOn = Screen.fullScreen;

        brightnessSlider.onValueChanged.AddListener(SetBrightness);
        brightnessSlider.value = Screen.brightness * 100;
    }

    private bool ResolutionEquals(Resolution res1, Resolution res2)
    {
        if (res1.width == res2.width)
            if (res1.height == res2.height)
                if (res1.refreshRate == res2.refreshRate)
                    return true;
        return false;
    }

    private void SetMenuVolume(float volume)
    {
        menuVolumeLabel.text = "Menu Volume: " + volume;

        // Testing sound
        AudioSource buttonTest = audioHandler.transform.Find("ButtonPressHandler").GetComponent<AudioSource>();
        buttonTest.volume = volume / 100;
        buttonTest.Play();

        menuVolumeValue = volume;
    }

    private void SetSFXVolume(float volume)
    {
        sfxVolumeLabel.text = "SFX Volume: " + volume;

        // Testing sound
        AudioSource eatTest = audioHandler.transform.Find("EatHandler").GetComponent<AudioSource>();
        eatTest.volume = volume / 100;
        eatTest.Play();

        sfxVolumeValue = volume;
    }

    private void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen, res.refreshRate);
    }

    private void SetFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
    }

    private void SetBrightness(float brightness)
    {
    }

    public void SaveAndQuit()
    {
        // Update all non-testing sounds
        audioHandler.transform.Find("ClickHandler").GetComponent<AudioSource>().volume = menuVolumeValue;
    }
}
