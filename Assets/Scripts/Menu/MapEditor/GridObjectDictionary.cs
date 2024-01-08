using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// A dictionary handler which controls the number of entries, and has full control over what
/// happens when an object is requested to be added or removed.
/// Does not handle Destroy events.
/// </summary>
public sealed class GridObjectDictionary
{
    public const int MAX_OBJECTS = 200;
    public int NumObjects { get; private set; } = 0;

    /// <summary>
    /// The core data structure of the class.
    /// </summary>
    private Dictionary<Vector3Int, GameObject> m_ds = new();


    /// <summary>
    /// Handles adding/overwriting an object at a grid position.
    /// </summary>
    /// <param name="objGridPos">The to-be-added object's position on the grid.</param>
    /// <param name="obj">Object to add.</param>
    /// <returns>`true` if object was added, `false` if not.</returns>
    public bool AddObject(Vector3Int objGridPos, GameObject obj, bool overwrite = true)
    {
        if (m_ds.ContainsKey(objGridPos))
        {
            // Overwrite existing object.
            if (overwrite)
                m_ds[objGridPos] = obj;
            return false;
        }

        m_ds.Add(objGridPos, obj);
        NumObjects++;
        return true;
    }


    /// <summary>
    /// Returns an object from the data structure by its key.
    /// </summary>
    /// <param name="objGridPos">The selected object's position on the grid.</param>
    /// <returns>The selected object, or null if there was no object at `objGridPos`.</returns>
    public GameObject PickObject(Vector3Int objGridPos)
    {
        if (!m_ds.ContainsKey(objGridPos))
            return null;
        return m_ds[objGridPos];
    }


    /// <summary>
    /// Removes an object from the data structure by its key.
    /// </summary>
    /// <param name="objGridPos">The to-be-removed object's position on the grid.</param>
    /// <returns>The removed object, or null if one was not removed.</returns>
    public GameObject RemoveObject(Vector3Int objGridPos)
    {
        GameObject toBeRemoved = PickObject(objGridPos);
        if (toBeRemoved == null)
            return null;

        m_ds.Remove(objGridPos);
        NumObjects--;
        return toBeRemoved;
    }


    public void ClearObjects()
    {
        NumObjects = 0;
        m_ds.Clear();
    }


    public void AddChildObjects(Transform parent)
    {
        foreach (Transform transform in parent)
        {
            m_ds.Add(
            new((int)transform.localPosition.x,
                (int)transform.localPosition.y),
            transform.gameObject);
            NumObjects++;
        }
    }


    public bool IsFull()
    {
        return (NumObjects >= MAX_OBJECTS);
    }


    public bool IsEmpty()
    {
        return (NumObjects <= 0);
    }


    public bool IsPositionEmpty(Vector3Int gridPos)
    {
        return m_ds.ContainsKey(gridPos);
    }


    public MapObjectData[] GetObjectData()
    {
        GameObject[] objs = m_ds.Values.ToArray();
        MapObjectData[] objData = new MapObjectData[NumObjects];
        for (int i = 0; i < NumObjects; i++)
        {
            ObjectBehaviour ob = objs[i].GetComponent<ObjectBehaviour>();
            objData[i] = new(ob.Type,
                (short)Mathf.FloorToInt(ob.transform.localPosition.x),
                (short)Mathf.FloorToInt(ob.transform.localPosition.y),
                ob.transform.localRotation.z
            );
        }

        return objData;
    }
}