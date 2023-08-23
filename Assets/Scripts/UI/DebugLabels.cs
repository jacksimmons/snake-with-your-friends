using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLabels : MonoBehaviour
{
    public void Debug_AddBodyPart()
    {
        GameObject localPlayerObj = GameObject.Find("LocalPlayerObject");
        if (localPlayerObj == null) return;
        localPlayerObj.GetComponent<PlayerMovement>().QAddBodyPart();
    }

    public void Debug_RemoveBodyPart()
    {
        GameObject localPlayerObj = GameObject.Find("LocalPlayerObject");
        if (localPlayerObj == null) return;
        localPlayerObj.GetComponent<PlayerMovement>().QRemoveBodyPart();
    }
}
