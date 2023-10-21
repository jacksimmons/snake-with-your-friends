// Simple Bitfield.
// Any bool value used with this defaults to false.
using System;
using System.Collections;

[Serializable]
public class BitField
{
    // 32 bit integer (32 fields)
    // Undefined behaviour if !(0 <= index <= 31)
    public int Data { get; private set; }

    public BitField(int data) { Data = data; }
    public BitField() { Data = 0; }

    public void SetBit(int index, bool value)
    {
        int mask = 1 << index;
        Data = value ? (int)(Data | mask) : (int)(Data & ~mask);
    }

    public bool GetBit(int index)
    {
        int mask = 1 << index;
        return (Data & mask) != 0;
    }
}