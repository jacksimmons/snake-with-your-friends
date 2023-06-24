using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;

public class PlayerObjectController : NetworkBehaviour
{
    // Player data
    [SyncVar] public int connectionID;
    [SyncVar] public int playerNo;
    [SyncVar] public ulong playerSteamID;
    [SyncVar(hook = nameof(OnPlayerNameUpdate))] public string playerName;

    private CustomNetworkManager _manager;
    private CustomNetworkManager Manager
    {
        get
        {
            if (_manager != null) { return _manager; }
            return _manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }

    /// <summary>
    /// Called when starting as a host.
    /// </summary>
    public override void OnStartAuthority()
    {
        CmdSetPlayerName(SteamFriends.GetPersonaName());
        gameObject.name = "LocalGamePlayer";
        LobbyController.instance.FindLocalPlayer();
        LobbyController.instance.UpdateLobbyName();
    }

    /// <summary>
    /// Called when starting as a client.
    /// </summary>
    public override void OnStartClient()
    {
        Manager._players.Add(this);
        LobbyController.instance.UpdateLobbyName();
        LobbyController.instance.UpdatePlayerList();
    }

    /// <summary>
    /// Called when stopping as a client.
    /// </summary>
    public override void OnStopClient()
    {
        Manager._players.Remove(this);
        LobbyController.instance.UpdatePlayerList();
    }

    [Command]
    private void CmdSetPlayerName(string playerName)
    {
        this.OnPlayerNameUpdate(this.playerName, playerName);
    }

    public void OnPlayerNameUpdate(string oldValue, string newValue)
    {
        if (isServer)
        {
            this.playerName = newValue;
        }
        if (isClient)
        {
        }
    }
}
