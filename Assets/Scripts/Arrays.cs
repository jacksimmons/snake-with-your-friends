using System.Linq;

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