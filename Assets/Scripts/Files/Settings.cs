using System;
using UnityEngine;


[Serializable]
public struct Res
{
    public int x;
    public int y;
    public int hz;


    public Res(Resolution res)
    {
        x = res.width;
        y = res.height;
        hz = res.refreshRate;
    }


    public static Resolution ToResolution(Res res)
    {
        Resolution resolution = new();
        resolution.width = res.x;
        resolution.height = res.y;
        resolution.refreshRate = res.hz;

        return resolution;
    }
}


[Serializable]
public class Settings : ICached
{
    public static Settings Saved = null;

    public float menuVolume;
    public float sfxVolume;
    public Res resolution;

    private BitField bf = new(1);
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

    public Settings()
    {
        menuVolume = 1;
        sfxVolume = 1;
        resolution = new();

        Fullscreen = false;
        Borderless = true;
        HelpMotionSickness = false;
    }

    public Settings(Settings other)
    {
        menuVolume = other.menuVolume;
        sfxVolume = other.sfxVolume;
        resolution = other.resolution;

        bf = new(1);
        Fullscreen = other.Fullscreen;
        Borderless = other.Borderless;
        HelpMotionSickness = other.HelpMotionSickness;
    }

    public Settings(float menuVolume, float sfxVolume, Res resolution, bool fullscreen,
        bool borderless, bool helpMotionSickness)
    {
        this.menuVolume = menuVolume;
        this.sfxVolume = sfxVolume;
        this.resolution = resolution;

        bf = new(1);
        Fullscreen = fullscreen;
        Borderless = borderless;
        HelpMotionSickness = helpMotionSickness;
    }

    public void Cache() { Saved = new(this); }

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