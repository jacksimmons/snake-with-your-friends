// Simple Bitfield.
// Any bool value used with this defaults to false.
using System;

[Serializable]
public class BitField
{
    // 32 bit integer (32 fields)
    // Undefined behaviour if !(0 <= index <= 31)
    public int Data { get; private set; }

    // Set every bit to 0 by default
    public BitField(int data = 0) { this.Data = data; }

    public void SetBit(int index, bool value)
    {
        byte mask = (byte)(1 << index);
        Data = value ? (int)(Data | mask) : (int)(Data & ~mask);
    }

    public bool GetBit(int index)
    {
        byte mask = (byte)(1 << index);
        return (Data & mask) != 0;
    }
}