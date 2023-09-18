using System;
using System.Collections.Generic;
using UnityEngine;

public enum EGameMode
{
    SnakeRoyale,
    Puzzle
}

[Serializable]
public class GameSettings : ICached
{
    public static GameSettings Saved = new();

    public const float DEFAULT_TIME_TO_MOVE = 0.6f;
    public const bool DEFAULT_FRIENDLY_FIRE = true;

    public EGameMode GameMode { get; set; }
    public int GameSize { get; set; }

    public float TimeToMove { get; set; }
    public bool FriendlyFire { get; set; }

    public List<EFoodType> DisabledFoods { get; private set; }

    public GameSettings()
    {
        TimeToMove = DEFAULT_TIME_TO_MOVE;
        FriendlyFire = DEFAULT_FRIENDLY_FIRE;
        GameMode = EGameMode.SnakeRoyale;
        GameSize = 10;

        DisabledFoods = new List<EFoodType>();
    }

    // Copy constructor
    public GameSettings(GameSettings other)
    {
        TimeToMove = other.TimeToMove;
        FriendlyFire = other.FriendlyFire;
        GameMode = other.GameMode;
        GameSize = other.GameSize;

        DisabledFoods = other.DisabledFoods;
    }

    public void Cache() { Saved = new(this); }

    public void EnableFood(EFoodType disabledFood)
    {
        if (!DisabledFoods.Contains(disabledFood)) { return; }
        DisabledFoods.Remove(disabledFood);
    }

    public void DisableFood(EFoodType disabledFood)
    {
        if (DisabledFoods.Contains(disabledFood)) { return; }
        DisabledFoods.Add(disabledFood);
    }
}