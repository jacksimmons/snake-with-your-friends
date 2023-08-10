using System;
using UnityEngine;

[Serializable]
public class Settings
{
    public float menuVolume;
    public float sfxVolume;
    public int resX, resY, resHz;
    public bool fullscreen;

    public Settings(float menuVolume, float sfxVolume, int resX, int resY, int resHz, bool fullscreen)
    {
        this.menuVolume = menuVolume;
        this.sfxVolume = sfxVolume;
        this.resX = resX;
        this.resY = resY;
        this.resHz = resHz;
        this.fullscreen = fullscreen;
    }
}