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
public class GameSettings : ICached
{
    public static GameSettings Saved = null;

    public const float DEFAULT_TIME_TO_MOVE = 0.6f;
    public const bool DEFAULT_FRIENDLY_FIRE = true;

    public EGameMode GameMode;
    public int GameSize;
    public float TimeToMove;
    public bool FriendlyFire;

    public List<EFoodType> DisabledFoods;

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

    public void Cache() { Saved = this; }

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