// Simple Bitfield.
// Any bool value used with this defaults to false.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[Serializable]
public class BitField
{
    // Can store any number of bytes, where each byte stores 8 booleans.
    // Saves memory for lots of objects with lots of booleans, but has a CPU overhead for r/w.
    public byte[] Data { get; private set; }


    // [Length * 8]-bit bitfield.
    public BitField(int length)
    {
        Data = new byte[length];
    }


    // [data.Length * 8]-bit bitfield.
    public BitField(byte[] data)
    {
        Data = data;
    }


    /// <summary>
    /// Checks if a bit is in the bounds of the bitfield. If not, logs an error to the console.
    /// </summary>
    /// <param name="index">The bit index</param>
    private void CheckBit(int index)
    {
        if (index >= 0 && index < Data.Length * 8) return;

        Debug.LogError($"Invalid bit {index} is out of bounds for BitField of length {Data.Length}B");
    }


    public void SetBit(int index, bool value)
    {
        CheckBit(index);

        // Index // 8 gives which byte
        int byteIndex = index / 8;
        // Index mod 8 gives which bit in the byte, use this to make a mask
        int bitMask = 1 << index % 8;

        if (byteIndex >= Data.Length)
            Debug.LogError($"Invalid bit {index} => byte index {byteIndex}/{Data.Length}.");
        else if (byteIndex < 0)
            Debug.LogError($"Invalid bit {index} => byte index {byteIndex}. Negative bit.");

        Data[byteIndex] = (byte)(value ? (Data[byteIndex] | bitMask) : (Data[byteIndex] & ~bitMask));
    }

    public bool GetBit(int index)
    {
        CheckBit(index);

        // Index // 8 gives which byte
        int byteIndex = index / 8;
        // Index mod 8 gives which bit in the byte, use this to make a mask
        int bitMask = 1 << index % 8;

        return (Data[byteIndex] & bitMask) != 0;
    }
}