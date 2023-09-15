using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Wait
{
    public static IEnumerator WaitThen(float seconds, Action then=null)
    {
        yield return new WaitForSeconds(seconds);
        then?.Invoke();
        yield return null;
    }

    public static IEnumerator WaitForConditionThen(Func<bool> getCondition, YieldInstruction waitTime, Action then=null)
    {
        while (!getCondition())
        {
            yield return waitTime;
        }
        then?.Invoke();
        yield return null;
    }

    /// <summary>
    /// Uses a findObj function to attempt to locate the object every waitTime, and then calls the "then" action
    /// with the found object as a parameter.
    public static IEnumerator WaitForObjectThen<T>(Func<T> findObj, YieldInstruction waitTime, Action<T> then=null)
        where T : class
    {
        T obj = null;
        while (obj == null)
        {
            obj = findObj();
            yield return waitTime;
        }
        then?.Invoke(obj);
        yield return null;
    }

    public static IEnumerator WaitForLoadSceneThen(string sceneName, YieldInstruction waitTime, Action then=null)
    {
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
        while (load.isDone)
        {
            yield return waitTime;
        }
        then?.Invoke();
        yield return null;
    }
}