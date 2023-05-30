using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is static after Start in MainMenu.
/// </summary>
public class AudioHandler : MonoBehaviour
{
    private static AudioSource _clickAudioSource;
    public static AudioSource _buttonPressAudioSource;

    private void Awake()
    {
        // If an AudioHandler already exists (i.e. returned to Main Menu), this one is
        // not needed.
        if (GameObject.FindWithTag("AudioHandler"))
            Destroy(gameObject);
        tag = "AudioHandler";
    }

    void Start()
    {
        DontDestroyOnLoad(this);
        _clickAudioSource = transform.Find("ClickHandler").GetComponent<AudioSource>();
        _buttonPressAudioSource = transform.Find("ButtonPressHandler").GetComponent<AudioSource>();
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _clickAudioSource.Play();
        }
    }

    public void OnButtonPressed()
    {
        _buttonPressAudioSource.Play();
    }
}
