using System;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Extensions
{
    public static class Arrays
    {
        public static T[] SubArray<T>(T[] array, int offset)
        {
            return array.Skip(offset).ToArray();
        }

        public static T[] SubArray<T>(T[] array, int offset, int length)
        {
            return array.Skip(offset).Take(length).ToArray();
        }
    }

    public static class Bytes
    {
        public static byte[] ToBytes(string str)
        {
            return Encoding.ASCII.GetBytes(str.ToString());
        }


        public static byte[] ToBytes<T>(T data)
        {
            int size = Marshal.SizeOf(data);
            byte[] arr = new byte[size];

            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(data, ptr, true);
                Marshal.Copy(ptr, arr, 0, size);
            }
            catch
            {
                Debug.LogError("Was unable to convert to bytes.");
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return arr;
        }

        public static string FromBytes(byte[] data)
        {
            return Encoding.ASCII.GetString(data);
        }

        public static T FromBytes<T>(byte[] data)
        {
            T payload;

            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(data.Length);
                Marshal.Copy(data, 0, ptr, data.Length);
                payload = Marshal.PtrToStructure<T>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return payload;
        }
    }
}