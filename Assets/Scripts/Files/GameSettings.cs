using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum EGameMode
{
    SnakeRoyale,
    Puzzle
}


/// <summary>
/// Struct form of the below class, used to transfer data between clients.
/// </summary>
[Serializable]
public readonly struct FoodSettingsData
{
    public readonly int FoodsEnabled;

    public FoodSettingsData(FoodSettings settings)
    {
        FoodsEnabled = settings.FoodsEnabled.Data;
    }
}


[Serializable]
public class FoodSettings
{
    public BitField FoodsEnabled { get; private set; }

    public FoodSettings()
    {
        FoodsEnabled = new(int.MaxValue); // All enabled
    }

    public FoodSettings(FoodSettingsData data)
    {
        FoodsEnabled = new(data.FoodsEnabled);
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
    public readonly FoodSettingsData FoodSettings;

    public GameSettingsData(GameSettings settings)
    {
        GameMode = settings.GameMode;
        GameSize = settings.GameSize;
        TimeToMove = settings.TimeToMove;
        FriendlyFire = settings.FriendlyFire;
        FoodSettings = new(settings.foodSettings);
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
    }


    public GameSettings(GameSettingsData data)
    {
        TimeToMove = data.TimeToMove;
        FriendlyFire = data.FriendlyFire;
        GameMode = data.GameMode;
        GameSize = data.GameSize;

        foodSettings = new(data.FoodSettings);
    }

    public void Cache() { Saved = this; }
}