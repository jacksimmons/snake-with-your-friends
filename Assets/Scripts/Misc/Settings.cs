using System;
using UnityEngine;

[Serializable]
public class Settings
{
    public float menuVolume;
    public float sfxVolume;
    public int resX, resY, resHz;
    public bool fullscreen;
    public bool borderless;

    public Settings(float menuVolume, float sfxVolume, int resX, int resY, int resHz,
        bool fullscreen, bool borderless)
    {
        this.menuVolume = menuVolume;
        this.sfxVolume = sfxVolume;
        this.resX = resX;
        this.resY = resY;
        this.resHz = resHz;
        this.fullscreen = fullscreen;
        this.borderless = borderless;
    }

    public static FullScreenMode GetWindowMode(bool fullscreen, bool borderless)
    {
        if (fullscreen)
        {
            if (borderless)
                return FullScreenMode.FullScreenWindow;
            else
                return FullScreenMode.ExclusiveFullScreen;
        }
        else
            return FullScreenMode.Windowed;
    }
}