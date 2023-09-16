using System;
using UnityEngine;

public enum Comparison
{
    None,
    LessThan,
    LessThanOrEqualTo,
    GreaterThan,
    GreaterThanOrEqualTo
}

// A class for comparing two Vector2s with different comparisons for each component.
[Serializable]
public class Vector2Comparison
{
    public Vector2 Value;
    public Comparison xComparison;
    public Comparison yComparison;

    public bool CompareVector2(Vector2 other)
    {
        return CompareComponent(Value.x, other.x, xComparison)
               &&
               CompareComponent(Value.y, other.y, yComparison);
    }

    public bool CompareComponent(float component, float other, Comparison comparison)
    {
        switch (comparison)
        {
            case Comparison.None: return true;
            case Comparison.LessThan:
                if (!(other < component)) return false;
                break;
            case Comparison.GreaterThan:
                if (!(other > component)) return false;
                break;
            case Comparison.LessThanOrEqualTo:
                if (!(other <= component)) return false;
                break;
            case Comparison.GreaterThanOrEqualTo:
                if (!(other >= component)) return false;
                break;
        }
        return true;
    }
}