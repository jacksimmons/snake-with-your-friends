using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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
        FileStream fs;

        if (File.Exists(dest)) fs = File.OpenWrite(dest);
        else fs = File.Create(dest);

        // If the provided object is null, delete the file.
        if (serializable == null)
        {
            File.Delete(dest);
            return;
        }

        BinaryFormatter bf = new();
        bf.Serialize(fs, serializable);
        fs.Close();

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
        {
            FileStream fs = File.OpenRead(dest);
            fs.Position = 0;

            BinaryFormatter bf = new();

            T val = (T)bf.Deserialize(fs);
            fs.Close();

            if (val is ICached cached)
                cached.Cache();

            if (LoadingIcon.Instance)
                LoadingIcon.Instance.Toggle(false);
            return val;
        }
        else
        {
            // Restart the function after creating a new T save
            SaveToFile(new T(), filename);
            Debug.LogWarning("File does NOT exist! Returning empty object");
            return LoadFromFile<T>(filename);
        }
    }
}