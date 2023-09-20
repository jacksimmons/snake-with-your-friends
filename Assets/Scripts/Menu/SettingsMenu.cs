using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    private GameObject audioHandler;

    // Back/save buttons
    [SerializeField]
    private Button backButton;
    [SerializeField]
    private Button saveButton;
    [SerializeField]
    private Button noSaveButton;

    // Settings sliders
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
    private Toggle borderlessToggle;
    private bool borderlessValue;

    [SerializeField]
    private Toggle motionSicknessToggle;
    private bool motionSicknessValue;

    //[SerializeField]
    //private TextMeshProUGUI brightnessLabel;
    //[SerializeField]
    //private Slider brightnessSlider;

    private void Start()
    {
        audioHandler = GameObject.FindWithTag("AudioHandler");

        menuVolumeSlider.value = Chungus.Instance.settings.menuVolume * 100;
        menuVolumeValue = menuVolumeSlider.value;
        menuVolumeSlider.onValueChanged.AddListener(SetMenuVolume);

        sfxVolumeSlider.value = Chungus.Instance.settings.sfxVolume * 100;
        sfxVolumeValue = sfxVolumeSlider.value;
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        resolutions = Screen.resolutions;
        for (int i = 0; i < resolutions.Length; i++)
        {
            var res = resolutions[i];
            resDropdown.options.Add(new TMP_Dropdown.OptionData(res.ToString()));

            Resolution currentRes = new Resolution();
            currentRes.width = Screen.width;
            currentRes.height = Screen.height;
            currentRes.refreshRate = Screen.currentResolution.refreshRate;
            if (ResolutionEquals(res, currentRes))
            {
                resDropdown.itemText.text = res.ToString();
                resDropdown.value = i;
            }
        }
        // Dropdown value can be changed in the above for-if statement - add listener after so res doesn't get changed
        resDropdown.onValueChanged.AddListener(SetResolution);

        fullscreenToggle.isOn = Screen.fullScreen;
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        borderlessValue = Chungus.Instance.settings.Borderless;
        borderlessToggle.isOn = borderlessValue;
        borderlessToggle.onValueChanged.AddListener(SetBorderless);

        motionSicknessValue = Chungus.Instance.settings.HelpMotionSickness;
        motionSicknessToggle.isOn = motionSicknessValue;
        motionSicknessToggle.onValueChanged.AddListener(SetHelpMotionSickness);
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
        AudioSource clickTest = audioHandler.transform.Find("ClickHandler").GetComponent<AudioSource>();
        AudioSource buttonTest = audioHandler.transform.Find("ButtonPressHandler").GetComponent<AudioSource>();
        clickTest.volume = volume / 100;
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
        Resolution resolution = resolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRate);
    }

    private void SetFullscreen(bool fullscreen)
    {
        Screen.fullScreenMode = Settings.GetWindowMode(fullscreen, borderlessValue);
    }

    private void SetBorderless(bool borderless)
    {
        borderlessValue = borderless;
        Screen.fullScreenMode = Settings.GetWindowMode(Screen.fullScreen, borderless);
    }

    private void SetHelpMotionSickness(bool motionSickness)
    {
        motionSicknessValue = motionSickness;
    }

    public void SaveSettingsToFile()
    {
        Settings settings = new Settings(
            menuVolumeValue / 100,
            sfxVolumeValue / 100,
            Screen.width, Screen.height, Screen.currentResolution.refreshRate,
            Screen.fullScreen,
            borderlessValue,
            motionSicknessValue
        );

        Saving.SaveToFile(settings, "Settings.dat");
    }
}
