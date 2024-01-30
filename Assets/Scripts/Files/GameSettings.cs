using Mirror;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;


[Serializable]
public struct GameSettingsData
{
    public EGameMode GameMode;
    public float TimeToMove;
    public bool FriendlyFire;
    public byte[] FoodSettingsData;
    public MapData Map;


    public GameSettingsData(GameSettingsData settings)
    {
        GameMode = settings.GameMode;
        TimeToMove = settings.TimeToMove;
        FriendlyFire = settings.FriendlyFire;
        FoodSettingsData = settings.FoodSettingsData;
        Map = settings.Map;
    }
}


[Serializable]
public class GameSettings : ICached
{
    public const float DEFAULT_TIME_TO_MOVE = 0.6f;
    public const bool DEFAULT_FRIENDLY_FIRE = true;
    private const int NUM_BYTES = 2;

    public static GameSettings Saved;
    public GameSettingsData Data;

    private BitField m_foodSettings = new(NUM_BYTES);


    public GameSettings()
    {
        Data = new();
    }


    public GameSettings(GameSettings other)
    {
        Data = other.Data;
    }


    public GameSettings(GameSettingsData data)
    {
        Data = data;
        m_foodSettings = new(data.FoodSettingsData);
    }


    public void SetFoodBit(EFoodType food, bool value)
    {
        m_foodSettings.SetBit((int)food, value);
        Data.FoodSettingsData = m_foodSettings.Data;
    }


    public bool GetFoodBit(EFoodType food)
    {
        return m_foodSettings.GetBit((int)food);
    }


    public void Cache() { Saved = this; }
}