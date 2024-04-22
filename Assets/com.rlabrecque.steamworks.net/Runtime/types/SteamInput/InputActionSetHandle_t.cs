// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2022 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// This file is automatically generated.
// Changes to this file will be reverted when you update Steamworks.NET

#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS

namespace Steamworks
{
    [System.Serializable]
    public struct InputActionSetHandle_t : System.IEquatable<InputActionSetHandle_t>, System.IComparable<InputActionSetHandle_t>
    {
        public ulong m_InputActionSetHandle;

        public InputActionSetHandle_t(ulong value)
        {
            m_InputActionSetHandle = value;
        }

        public override string ToString()
        {
            return m_InputActionSetHandle.ToString();
        }

        public override bool Equals(object other)
        {
            return other is InputActionSetHandle_t && this == (InputActionSetHandle_t)other;
        }

        public override int GetHashCode()
        {
            return m_InputActionSetHandle.GetHashCode();
        }

        public static bool operator ==(InputActionSetHandle_t x, InputActionSetHandle_t y)
        {
            return x.m_InputActionSetHandle == y.m_InputActionSetHandle;
        }

        public static bool operator !=(InputActionSetHandle_t x, InputActionSetHandle_t y)
        {
            return !(x == y);
        }

        public static explicit operator InputActionSetHandle_t(ulong value)
        {
            return new InputActionSetHandle_t(value);
        }

        public static explicit operator ulong(InputActionSetHandle_t that)
        {
            return that.m_InputActionSetHandle;
        }

        public bool Equals(InputActionSetHandle_t other)
        {
            return m_InputActionSetHandle == other.m_InputActionSetHandle;
        }

        public int CompareTo(InputActionSetHandle_t other)
        {
            return m_InputActionSetHandle.CompareTo(other.m_InputActionSetHandle);
        }
    }
}

#endif // !DISABLESTEAMWORKS
