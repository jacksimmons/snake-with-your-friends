using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum EGameMode
{
    SnakeRoyale,
    Puzzle
}


[Serializable]
public class FoodSettings
{
    public BitField FoodsEnabled { get; private set; }

    public FoodSettings()
    {
        FoodsEnabled = new BitField(int.MaxValue);
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

    public void Cache() { Saved = this; }
}