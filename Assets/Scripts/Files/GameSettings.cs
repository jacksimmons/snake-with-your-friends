using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSettings
{
    public static GameSettings Saved = new();

    public const float DEFAULT_TIME_TO_MOVE = 0.6f;
    public const bool DEFAULT_FRIENDLY_FIRE = true;

    public string GameMode { get; set; }

    public float TimeToMove { get; set; }
    public bool FriendlyFire { get; set; }

    public List<EFoodType> DisabledFoods { get; private set; }

    public GameSettings()
    {
        TimeToMove = DEFAULT_TIME_TO_MOVE;
        FriendlyFire = DEFAULT_FRIENDLY_FIRE;
        GameMode = "SnakeRoyale";

        DisabledFoods = new List<EFoodType>();
    }

    // Copy constructor
    public GameSettings(GameSettings other)
    {
        TimeToMove = other.TimeToMove;
        FriendlyFire = other.FriendlyFire;
        GameMode = other.GameMode;

        DisabledFoods = other.DisabledFoods;
    }

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