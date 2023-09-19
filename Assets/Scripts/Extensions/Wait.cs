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

    public static IEnumerator WaitForConditionThen(Func<bool> getCondition, float secondsToWait,
        Action then=null)
    {
        while (!getCondition())
        {
            yield return new WaitForSeconds(secondsToWait);
        }
        then?.Invoke();
        yield return null;
    }

    /// <summary>
    /// Uses a findObj function to attempt to locate the object every waitTime, and then calls the "then" action
    /// with the found object as a parameter.
    public static IEnumerator WaitForObjectThen<T>(Func<T> findObj, float secondsToWait,
        Action<T> then=null) where T : class
    {
        T obj = null;
        while (obj == null)
        {
            obj = findObj();
            yield return new WaitForSeconds(secondsToWait);
        }
        then?.Invoke(obj);
        yield return null;
    }


    public static IEnumerator LoadSceneThenWait(string sceneName, 
        Func<bool> conditionToActivate, float secondsToWait)
    {
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
        load.allowSceneActivation = false;

        while (!load.isDone)
        {
            if (load.progress >= 0.9f)
            {
                if (conditionToActivate())
                    load.allowSceneActivation = true;
            }

            yield return new WaitForSeconds(secondsToWait);
        }

        yield return null;
    }
}