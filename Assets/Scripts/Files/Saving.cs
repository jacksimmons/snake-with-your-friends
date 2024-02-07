using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using UnityEngine;


public interface ICached
{
    /// <summary>
    /// Saves the instance as the .Saved static member variable for its class.
    /// </summary>
    void Cache();
}


public static class Saving
{
    /// <summary>
    /// Serialises objects and saves them to a given file location.
    /// Also calls .Cache() on the object beforehand if it : ICached.
    /// </summary>
    public static void SaveToFile<T>(T serializable, string filename)
    {
        if (LoadingIcon.Instance)
            LoadingIcon.Instance.Toggle(true);

        if (serializable is ICached cached)
            cached.Cache();

        string dest = Application.persistentDataPath + "/" + filename;
        if (!File.Exists(dest)) File.Create(dest).Close();

        // If the provided object is null, delete the file.
        if (serializable == null)
        {
            File.Delete(dest);
            return;
        }

        string json = JsonUtility.ToJson(serializable, true);
        File.WriteAllText(dest, json);

        if (LoadingIcon.Instance)
            LoadingIcon.Instance.Toggle(false);
    }

    /// <summary>
    /// Deserialises a serialised object stored in a file.
    /// Calls .Cache() on the object if it : ICached.
    /// </summary>
    public static T LoadFromFile<T>(string filename) where T : new()
    {
        if (LoadingIcon.Instance)
            LoadingIcon.Instance.Toggle(true);

        string dest = Application.persistentDataPath + "/" + filename;

        if (File.Exists(dest))
            return LoadAndCache<T>(File.ReadAllText(dest));
        else
        {
            // Restart the function after creating a new T save
            SaveToFile(new T(), filename);
            Debug.LogWarning("File does NOT exist! Returning empty object");
            return LoadFromFile<T>(filename);
        }
    }


    /// <summary>
    /// Deserialised an object from the Resources folder.
    /// </summary>
    /// <typeparam name="T">The object type to deserialise into.</typeparam>
    /// <param name="filename">The local filename (Resources/{filename}).</param>
    /// <returns>The deserialised object.</returns>
    public static T LoadFromResources<T>(string filename) where T : new()
    {
        if (LoadingIcon.Instance)
            LoadingIcon.Instance.Toggle(true);

        TextAsset ta = Resources.Load<TextAsset>(filename);
        if (ta == null)
        {
            Debug.LogError($"No resource {filename} exists!");
        }

        return LoadAndCache<T>(ta.text);
    }


    public static T LoadAndCache<T>(string json)
    {
        T val = (T)JsonUtility.FromJson(json, typeof(T));

        if (val is ICached cached)
            cached.Cache();

        if (LoadingIcon.Instance)
            LoadingIcon.Instance.Toggle(false);
        return val;
    }
}