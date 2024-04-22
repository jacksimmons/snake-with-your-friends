using Steamworks;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles counters for player movement.
/// The listener is the parent of this object (transform.parent.gameObject).
/// </summary>
public class Counter : MonoBehaviour
{
    public float Cnt { get; private set; } = 0;
    public bool Paused { get; set; } = true;
    [SerializeField]
    public float thresholdSeconds;

    public Dictionary<CSteamID, Dictionary<string, float>> PlayerCounters { get; private set; } = new();

    private void FixedUpdate()
    {
        if (!Paused && transform.parent.gameObject && (thresholdSeconds > 0))
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
            transform.parent.gameObject.SendMessage("OnCounterThresholdReached");
        }

        foreach (var kvp in PlayerCounters)
        {
            kvp.Value["cnt"] += Time.fixedDeltaTime;
            if (kvp.Value["cnt"] >= kvp.Value["threshold_seconds"])
            {
                kvp.Value["cnt"] = 0;
                transform.parent.gameObject.SendMessage("OnCustomCounterThresholdReached", kvp.Key);
            }
        }
    }

    public void Reset()
    {
        Cnt = 0;
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