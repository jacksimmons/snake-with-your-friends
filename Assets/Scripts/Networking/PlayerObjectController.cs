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
    [SyncVar(hook = nameof(OnPlayerReadyUpdate))] public bool ready;

    private CustomNetworkManager _manager;
    private CustomNetworkManager Manager
    {
        get
        {
            if (_manager != null) { return _manager; }
            return _manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }


    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
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
        Manager.players.Add(this);
        LobbyController.instance.UpdateLobbyName();
        LobbyController.instance.UpdatePlayerList();
    }

    /// <summary>
    /// Called when stopping as a client.
    /// </summary>
    public override void OnStopClient()
    {
        Manager.players.Remove(this);
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
            LobbyController.instance.UpdatePlayerList();
        }
    }

    [Command]
    private void CmdSetPlayerReady()
    {
        this.OnPlayerReadyUpdate(this.ready, !this.ready);
    }

    public void ChangeReady()
    {
        if (isOwned)
        {
            CmdSetPlayerReady();
        }
    }

    public void OnPlayerReadyUpdate(bool oldValue, bool newValue)
    {
        if (isServer)
        {
            this.ready = newValue;
        }
        if (isClient)
        {
            LobbyController.instance.UpdatePlayerList();
        }
    }

    public void CanStartGame(string sceneName)
    {
        if (isOwned)
        {
            CmdCanStartGame(sceneName);
        }
    }

    [Command]
    public void CmdCanStartGame(string sceneName)
    {
        Manager.StartGame(sceneName);
    }
}
