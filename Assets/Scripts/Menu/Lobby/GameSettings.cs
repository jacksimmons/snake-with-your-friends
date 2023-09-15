using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSettings
{
    public static GameSettings Saved = new();

    public const int LOWEST_COUNTER_MAX = 1;
    public const int DEFAULT_COUNTER_MAX = 20;
    public const bool DEFAULT_FRIENDLY_FIRE = true;

    public int CounterMax { get; set; }
    public bool FriendlyFire { get; set; }

    public List<EFoodType> DisabledFoods { get; private set; }

    public GameSettings()
    {
        CounterMax = DEFAULT_COUNTER_MAX;
        FriendlyFire = DEFAULT_FRIENDLY_FIRE;

        DisabledFoods = new List<EFoodType>();
    }

    // Copy constructor
    public GameSettings(GameSettings other)
    {
        CounterMax = other.CounterMax;
        FriendlyFire = other.FriendlyFire;

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