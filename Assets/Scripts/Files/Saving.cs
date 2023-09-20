using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


public interface ICached
{
    // Caching happens on Saves and Loads, can be used to save a static instance to reduce file use
    void Cache();
}


public static class Saving
{
    /// <summary>
    /// Serialises objects and saves them to a given file location.
    /// </summary>
    public static void SaveToFile<T>(T serializable, string filename) where T : class, ICached
    {
        LoadingIcon.Instance.Toggle(true);

        if (serializable != null)
            serializable.Cache();

        string dest = Application.persistentDataPath + "/" + filename;
        FileStream fs;

        if (File.Exists(dest)) fs = File.OpenWrite(dest);
        else fs = File.Create(dest);

        BinaryFormatter bf = new();
        bf.Serialize(fs, serializable);
        fs.Close();

        Debug.Log(filename);

        LoadingIcon.Instance.Toggle(false);
    }

    /// <summary>
    /// Deserialises saved objects into usable objects.
    /// Returns plain "object" type, so casting is necessary.
    /// </summary>
    public static T LoadFromFile<T>(string filename) where T : class, ICached, new()
    {
        LoadingIcon.Instance.Toggle(true);

        string dest = Application.persistentDataPath + "/" + filename;
        FileStream fs;

        if (File.Exists(dest))
        {
            Debug.Log(dest);
            fs = File.OpenRead(dest);
            BinaryFormatter bf = new();

            LoadingIcon.Instance.Toggle(false);

            T val = (T)bf.Deserialize(fs);
            fs.Close();

            if (val != null)
                val.Cache();
            return val;
        }
        else
        {
            // Restart the function after creating a new T save
            SaveToFile(new T(), filename);
            return LoadFromFile<T>(filename);
        }
    }
}