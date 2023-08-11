using System;
using System.Collections;
using UnityEngine;

public class Wait
{
    public static IEnumerator WaitThen(float seconds, Action then)
    {
        yield return new WaitForSeconds(seconds);
        then();
    }

    public static IEnumerator WaitForConditionThen(Func<bool> getCondition, Action then, YieldInstruction waitTime)
    {
        while (getCondition() == false)
        {
            yield return waitTime;
        }
        then();
        yield return null;
    }

    /// <summary>
    /// Uses a findObj function to attempt to locate the object every waitTime, and then calls the "then" action
    /// with the found object as a parameter.
    public static IEnumerator WaitForObjectThen(Func<GameObject> findObj, Action<GameObject> then, YieldInstruction waitTime)
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