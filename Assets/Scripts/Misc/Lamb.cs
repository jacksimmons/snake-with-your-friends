using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class that always persists in DontDestroyOnLoad.
public class Lamb : MonoBehaviour
{
    public void ClearDontDestroyOnLoad()
    {
        foreach (var root in gameObject.scene.GetRootGameObjects())
        {
            if (root != gameObject && root.name != "SteamManager")
                Destroy(root);
        }
    }
}
