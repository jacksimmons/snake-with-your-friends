using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Extensions
{
    //https://stackoverflow.com/questions/642542/how-to-get-next-or-previous-enum-value-in-c-sharp
    public static T Next<T>(this T src) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int next = Array.IndexOf<T>(Arr, src) + 1;
        return (Arr.Length == next) ? Arr[0] : Arr[next];
    }

    public static T Prev<T>(this T src) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int prev = Array.IndexOf<T>(Arr, src) - 1;
        return (-1 == prev) ? Arr[Arr.Length - 1] : Arr[prev];
    }

    public static void ChangeIndex(ref int index, int length, int increment)
    {
        index += increment;
        if (index < 0)
            index = length - 1;
        else if (index > length - 1)
            index = 0;
    }

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

        /// <summary>
        /// Converts multi-directional analog stick input into 4-direction DPad input.
        /// Truncates the angled vector into a cardinal (NESW) direction vector.
        /// </summary>
        /// <param name="stickValue">Input in stick form (any vector)</param>
        /// <returns>Input in DPad form (cardinal vector)</returns>
        public static Vector2 StickToDPad(Vector2 stickValue)
        {
            if (Mathf.Abs(stickValue.x) >= Mathf.Abs(stickValue.y))
                return new(Mathf.RoundToInt(stickValue.x), 0);
            else
                return new(0, Mathf.RoundToInt(stickValue.y));
        }
    }

    //public static class Arrays
    //{
    //    public static T[] SubArray<T>(T[] array, int offset)
    //    {
    //        return array.Skip(offset).ToArray();
    //    }

    //    public static T[] SubArray<T>(T[] array, int offset, int length)
    //    {
    //        return array.Skip(offset).Take(length).ToArray();
    //    }
    //}
}