using System;
using UnityEngine;

[Serializable]
public class Settings
{
    public float menuVolume;
    public float sfxVolume;
    public int resX, resY, resHz;

    private BitField bf;
    public bool Fullscreen
    {
        get { return bf.GetBit(0); }
        set { bf.SetBit(0, value); }
    }
    public bool Borderless
    {
        get { return bf.GetBit(1); }
        set { bf.SetBit(1, value); }
    }

    // Unimplemented
    public bool HelpMotionSickness
    {
        get { return bf.GetBit(2); }
        set { bf.SetBit(2, value); }
    }

    public Settings(float menuVolume, float sfxVolume, int resX, int resY, int resHz,
        bool fullscreen, bool borderless, bool helpMotionSickness)
    {
        this.menuVolume = menuVolume;
        this.sfxVolume = sfxVolume;
        this.resX = resX;
        this.resY = resY;
        this.resHz = resHz;

        bf = new();
        Fullscreen = fullscreen;
        Borderless = borderless;
        HelpMotionSickness = helpMotionSickness;
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