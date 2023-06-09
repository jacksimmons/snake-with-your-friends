using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using TMPro;
using UnityEditor.PackageManager.Requests;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Channels are used for different types:
// 0 - Update
// 1 - FixedUpdate
// 2 - Console Output

/// <summary>
/// Server-side lobby management.
/// </summary>
public class LocalLobby : MonoBehaviour
{
    public PlayerBehaviour Player { get; private set; }
    public readonly CSteamID id = CSteamID.Nil;

    [SerializeField]
    private Counter _counter;

    public void SetPlayerMovementSpeed(float value)
    {
        if (value != PlayerBehaviour.DEFAULT_MOVEMENT_SPEED && value != Player.MovementSpeed)
        {
            // Remove existing custom counter if there is one
            // Thus, custom counters are only cleaned up when the next custom counter is requested.
            if (_counter.PlayerCounters.Keys.Contains(id))
                _counter.RemovePlayerCounter(id);
            _counter.AddPlayerCounter(id, value, _counter.Cnt);
        }
        else if (value == PlayerBehaviour.DEFAULT_MOVEMENT_SPEED)
        {
            if (_counter.PlayerCounters.Keys.Contains(id))
                _counter.RemovePlayerCounter(id);
        }
    }

    private void OnCounterThresholdReached()
    {
        if (Player.MovementSpeed == PlayerBehaviour.DEFAULT_MOVEMENT_SPEED)
            Player.HandleMovementLoop();
    }

    /// <summary>
    /// Fitted to the same parameters as the local counter procedure for Lobby,
    /// but does not use the `id` param.
    /// </summary>
    private void OnCustomCounterThresholdReached(CSteamID _)
    {
        if (Player.MovementSpeed != PlayerBehaviour.DEFAULT_MOVEMENT_SPEED)
            Player.HandleMovementLoop();
    }
}