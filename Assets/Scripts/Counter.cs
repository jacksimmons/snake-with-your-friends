using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Counter : MonoBehaviour
{
    public float Cnt { get; private set; } = 0;
    public bool Paused { get; set; } = true;
    [SerializeField]
    public float thresholdSeconds = 0f;

    public Dictionary<CSteamID, Dictionary<string, float>> PlayerCounters { get; private set; } = new();

    [SerializeField]
    private GameObject _listener;

    private void FixedUpdate()
    {
        if (!Paused && _listener && (thresholdSeconds > 0))
        {
            Increment();
        }
    }

    public void Increment()
    {
        Cnt += Time.fixedDeltaTime;
        if (Cnt >= thresholdSeconds)
        {
            Cnt = 0;
            _listener.SendMessage("OnCounterThresholdReached");
        }

        foreach (var kvp in PlayerCounters)
        {
            kvp.Value["cnt"] += Time.fixedDeltaTime;
            if (kvp.Value["cnt"] >= kvp.Value["threshold_seconds"])
            {
                kvp.Value["cnt"] = 0;
                _listener.SendMessage("OnCustomCounterThresholdReached", kvp.Key);
            }
        }
    }

    public void Reset()
    {
        Cnt = 0;
    }

    public void SetListener(GameObject listener)
    {
        _listener = listener;
    }

    public void AddPlayerCounter(CSteamID player, float movementSpeed, float cntStart)
    {
        // movementSpeed is a divisor for thresholdSeconds
        PlayerCounters.Add(player,
        new Dictionary<string, float>
        {
            { "threshold_seconds", thresholdSeconds / movementSpeed },
            { "cnt", cntStart },
        });
    }

    public void RemovePlayerCounter(CSteamID player)
    {
        PlayerCounters.Remove(player);
    }
}