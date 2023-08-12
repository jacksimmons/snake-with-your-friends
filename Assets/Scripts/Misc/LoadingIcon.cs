using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingIcon : MonoBehaviour
{
    void Awake()
    {
        // Only one loading icon can exist
        if (Chungus.Instance.LoadingObj != gameObject)
            Destroy(gameObject);
    }
}
