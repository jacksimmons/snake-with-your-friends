using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSettings
{
    public int PlayerSpeed { get; set; }
    public bool FriendlyFire { get; set; }

    private List<EFoodType> m_disabledFoods;

    public GameSettings()
    {
        PlayerSpeed = 30;
        FriendlyFire = true;

        m_disabledFoods = new List<EFoodType>();
    }

    // Copy constructor
    public GameSettings(GameSettings other)
    {
        PlayerSpeed = other.PlayerSpeed;
        FriendlyFire = other.FriendlyFire;

        m_disabledFoods = other.m_disabledFoods;
    }

    public void EnableFood(EFoodType disabledFood)
    {
        if (!m_disabledFoods.Contains(disabledFood)) { return; }
        m_disabledFoods.Remove(disabledFood);
    }

    public void DisableFood(EFoodType disabledFood)
    {
        if (m_disabledFoods.Contains(disabledFood)) { return; }
        m_disabledFoods.Add(disabledFood);
    }
}