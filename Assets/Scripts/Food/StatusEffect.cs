using System;
using Unity.Mathematics;
using UnityEngine;

public class SpeedEffect
{
    private static readonly float[] SPEED_NEGATIVE =
{
        1f, // Lv0
        0.8f, // Lv1 -20%
        0.6f, // Lv2 -40%
        0.4f, // Lv3 -60%
        0.2f, // Lv4 -80%
        0f, // Lv5 -100% (This acts as a frozen effect)
    };

    private static readonly float[] SPEED_POSITIVE =
    {
        1f, // Lv0
        1.25f, // Lv1 +25%
        1.5f, // Lv2 +50%
        1.75f, // Lv3 +75%
        2f, // Lv4 +100%
        2.5f, // Lv5 +150%
    };

    private static float GetSpeedMultFromLevel(int level, float[] speedArray)
    {
        if (level < speedArray.Length)
        {
            return speedArray[level];
        }
        Debug.LogError("Invalid speed level.");
        return speedArray[0];
    }

    public static float GetSpeedMultFromSignedLevel(int level)
    {
        if (level >= 0)
            return GetSpeedMultFromLevel(level, SPEED_POSITIVE);
        else
            return GetSpeedMultFromLevel(-level, SPEED_NEGATIVE);
    }
}