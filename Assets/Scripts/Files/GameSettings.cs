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
    public BitField FoodSettingsData;
    public MapData Map;


    public GameSettingsData(BitField foodSettingsData)
    {
        GameMode = EGameMode.SnakeRoyale;
        TimeToMove = GameSettings.DEFAULT_TIME_TO_MOVE;
        FriendlyFire = GameSettings.DEFAULT_FRIENDLY_FIRE;
        Map = new();

        FoodSettingsData = foodSettingsData;
    }


    public GameSettingsData(EGameMode mode, float timeToMove, bool friendlyFire, BitField foodSettingsData,
        MapData map)
    {
        GameMode = mode;
        TimeToMove = timeToMove;
        FriendlyFire = friendlyFire;

        FoodSettingsData = foodSettingsData;

        Map = map;
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


    public GameSettings()
    {
        Data = new(new(NUM_BYTES));
    }


    public GameSettings(GameSettings other)
    {
        Data = other.Data;
    }


    public GameSettings(GameSettingsData data)
    {
        Data = data;
    }


    public void Cache() { Saved = this; }
}