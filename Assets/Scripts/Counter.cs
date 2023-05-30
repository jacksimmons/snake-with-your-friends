using System;
using System.Collections;
using UnityEngine;

public class Counter : MonoBehaviour
{
    private static int cnt = 0;
    public bool Paused { get; set; } = true;
    [SerializeField]
    private int _threshold = 0;
    [SerializeField]
    private GameObject _listener = null;

    private void FixedUpdate()
    {
        if (!Paused && _listener)
            Increment();
    }

    public void Increment()
    {
        cnt++;
        if (cnt >= _threshold)
        {
            cnt = 0;
            _listener.SendMessage("OnCounterThresholdReached");
        }
    }

    public void Reset()
    {
        cnt = 0;
    }

    public void SetThreshold(int threshold)
    {
        _threshold = threshold;
    }

    public void SetListener(GameObject listener)
    {
        _listener = listener;
    }
}