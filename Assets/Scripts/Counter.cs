using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Counter : MonoBehaviour
{
    private static float cnt = 0;
    public bool Paused { get; set; } = true;
    [SerializeField]
    public float ThresholdSeconds { get; set; } = 0f;

    private Dictionary<CSteamID, Dictionary<string, float>> _playerCounters = new();

    [SerializeField]
    private GameObject _listener = null;

    private void FixedUpdate()
    {
        if (!Paused && _listener)
        {
            Increment();
        }
    }

    public void Increment()
    {
        cnt += Time.fixedDeltaTime;
        if (cnt >= ThresholdSeconds)
        {
            cnt = 0;
            _listener.SendMessage("OnCounterThresholdReached");
        }

        foreach (var kvp in _playerCounters)
        {
            kvp.Value["cnt"] += Time.fixedDeltaTime;
            if (kvp.Value["cnt"] >= kvp.Value["threshold_seconds"])
            {
                kvp.Value["cnt"] = 0;
                _listener.SendMessage("OnCounterThresholdReached", kvp.Key);
            }
        }
    }

    public void Reset()
    {
        cnt = 0;
    }

    public void SetListener(GameObject listener)
    {
        _listener = listener;
    }

    public void AddPlayerCounter(CSteamID player, float movementSpeed, float cntStart)
    {
        _playerCounters.Add(player,
        new Dictionary<string, float>
        {
            { "movement_speed", movementSpeed },
            { "cnt", cntStart },
        });
    }

    public void RemovePlayerCounter(CSteamID player)
    {
        _playerCounters.Remove(player);
    }
}