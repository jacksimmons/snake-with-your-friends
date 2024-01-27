using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class FoodSettings
{
    public BitField FoodsEnabled { get; private set; }

    public FoodSettings()
    {
        // Create a bitfield with valid length
        FoodsEnabled = new(Enum.GetValues(typeof(EFoodType)).Length);
    }

    public FoodSettings(BitField foodsEnabled)
    {
        // Copy by value
        FoodsEnabled = new(foodsEnabled.Data);
    }

    public void SetFoodEnabled(EFoodType type, bool val)
    {
        FoodsEnabled.SetBit((int)type, val);
    }

    public bool GetFoodEnabled(EFoodType type)
    {
        return FoodsEnabled.GetBit((int)type);
    }
}


[Serializable]
public readonly struct GameSettingsData
{
    public readonly EGameMode GameMode;
    public readonly int GameSize;
    public readonly float TimeToMove;
    public readonly bool FriendlyFire;
    public readonly FoodSettings FoodSettings;
    public readonly MapData Map;

    public GameSettingsData(GameSettings settings)
    {
        GameMode = settings.GameMode;
        GameSize = settings.GameSize;
        TimeToMove = settings.TimeToMove;
        FriendlyFire = settings.FriendlyFire;
        FoodSettings = settings.foodSettings;
        Map = settings.Map;
    }
}


[Serializable]
public class GameSettings : ICached
{
    public static GameSettings Saved = null;

    public const float DEFAULT_TIME_TO_MOVE = 0.6f;
    public const bool DEFAULT_FRIENDLY_FIRE = true;

    public EGameMode GameMode;
    public int GameSize;
    public float TimeToMove;
    public bool FriendlyFire;
    public MapData Map;

    public FoodSettings foodSettings;

    public GameSettings()
    {
        TimeToMove = DEFAULT_TIME_TO_MOVE;
        FriendlyFire = DEFAULT_FRIENDLY_FIRE;
        GameMode = EGameMode.SnakeRoyale;
        GameSize = 10;

        foodSettings = new FoodSettings();
    }

    // Copy constructor
    public GameSettings(GameSettings other)
    {
        TimeToMove = other.TimeToMove;
        FriendlyFire = other.FriendlyFire;
        GameMode = other.GameMode;
        GameSize = other.GameSize;

        foodSettings = other.foodSettings;
        Map = other.Map;
    }


    public GameSettings(GameSettingsData data)
    {
        TimeToMove = data.TimeToMove;
        FriendlyFire = data.FriendlyFire;
        GameMode = data.GameMode;
        GameSize = data.GameSize;

        foodSettings = data.FoodSettings;
        Map = data.Map;
    }

    public void Cache() { Saved = this; }
}