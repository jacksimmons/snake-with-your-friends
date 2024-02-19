using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    private TMP_Text menuVolumeLabel;
    [SerializeField]
    private Slider menuVolumeSlider;
    private float menuVolumeValue;

    [SerializeField]
    private TMP_Text sfxVolumeLabel;
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

        SetMenuVolume(Settings.Saved.menuVolume);
        menuVolumeSlider.onValueChanged.AddListener(SetMenuVolume);

        SetSFXVolume(Settings.Saved.sfxVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        resolutions = Screen.resolutions;

        // Put the resolutions into the dropdown
        for (int i = 0; i < resolutions.Length; i++)
        {
            var res = resolutions[i];
            resDropdown.options.Add(new TMP_Dropdown.OptionData(res.ToString()));

            if (ResolutionEquals(res, Res.ToResolution(Settings.Saved.resolution)))
            {
                resDropdown.itemText.text = res.ToString();
                resDropdown.value = i;
            }
        }

        // Dropdown value can be changed in the above for-if statement - add listener after so res doesn't get changed
        resDropdown.onValueChanged.AddListener(SetResolution);

        fullscreenToggle.isOn = Screen.fullScreen;
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        borderlessValue = Settings.Saved.Borderless;
        borderlessToggle.isOn = borderlessValue;
        borderlessToggle.onValueChanged.AddListener(SetBorderless);

        motionSicknessValue = Settings.Saved.HelpMotionSickness;
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
        clickTest.volume = volume / 100f;
        buttonTest.volume = volume / 100f;
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
        print(Screen.currentResolution);
        Settings settings = new Settings(
            menuVolumeValue,
            sfxVolumeValue,
            new(Screen.currentResolution),
            Screen.fullScreen,
            borderlessValue,
            motionSicknessValue
        );

        Saving.SaveToFile(settings, "Settings.json");
    }
}
