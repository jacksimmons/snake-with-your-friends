using System.Runtime.InteropServices;
using UnityEngine;

public static class Vectors
{
    public static Vector2 mod(Vector2 vec, int mod)
    {
        return new Vector2(vec.x % mod, vec.y % mod);
    }

    public static bool Approximately(Vector2 v1, Vector2 v2)
    {
        return Mathf.Approximately(v1.x, v2.x) && Mathf.Approximately(v1.y, v2.y);
    }

    // The next two methods are used to compare the only component of the first vector to the same component of the second
    // Only the first vector must have only one component
    public static bool OnlyComponentOfFirstGT(Vector2 v1, Vector2 v2)
    {
        if (v1.x == 0)
        {
            return Mathf.Abs(v1.y) > Mathf.Abs(v2.y);
        }

        return Mathf.Abs(v1.x) > Mathf.Abs(v2.x);
    }

    public static bool OnlyComponentOfFirstGTE(Vector2 v1, Vector2 v2)
    {
        if (v1.x == 0)
        {
            return Mathf.Abs(v1.y) >= Mathf.Abs(v2.y);
        }

        return Mathf.Abs(v1.x) >= Mathf.Abs(v2.x);
    }

    // https://answers.unity.com/questions/661383/whats-the-most-efficient-way-to-rotate-a-vector2-o.html
    public static Vector2 Rotate(Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float _x = v.x;
        float _y = v.y;

        v.x = (cos * _x) - (sin * _y);
        v.y = (sin * _x) + (cos * _y);

        return v;
    }
}