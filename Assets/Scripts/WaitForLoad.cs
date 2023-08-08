using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

using Object = UnityEngine.Object;

public class WaitForLoad
{
    /// <summary>
    /// Uses a findObj function to attempt to locate the object every waitTime, and then calls the "then" action
    /// with the found object as a parameter.
    /// </summary>
    /// <param name="findObj"></param>
    /// <param name="then"></param>
    /// <param name="waitTime"></param>
    /// <returns></returns>
    public static IEnumerator WaitForObject(Func<GameObject> findObj, Action<GameObject> then, YieldInstruction waitTime)
    {
        GameObject obj = null;
        while (obj == null)
        {
            obj = findObj();
            yield return waitTime;
        }
        then(obj);
        yield return null;
    }
}