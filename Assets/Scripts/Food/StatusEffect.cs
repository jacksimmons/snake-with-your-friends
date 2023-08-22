using UnityEngine;

public class StatusEffect
{
    private static readonly float[] SPEED =
    {
        1f, // Lv0
        1.25f, // Lv1 +25%
        1.5f, // Lv2 +50%
        1.75f, // Lv3 +75%
        2f, // Lv4 +100%
        2.5f, // Lv5 +150%
    };

    public static float GetSpeedEffectByLevel(uint level)
    {
        if (level < SPEED.Length)
        {
            return SPEED[level];
        }
        Debug.LogError("Invalid SPEED level.");
        return SPEED[0];
    }
}