using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEditor;

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


    // Lobby Methods

    /// <summary>
    /// Called when starting as a host.
    /// Used in SinglePlayer or MultiPlayer mode.
    /// </summary>
    public override void OnStartAuthority()
    {
        CmdSetPlayerName(SteamFriends.GetPersonaName());
        gameObject.name = "LocalPlayerObject";
        LobbyController.instance.FindLocalPlayer();
        LobbyController.instance.UpdateLobbyName();
    }

    /// <summary>
    /// Called when starting as a client.
    /// Used in MultiPlayer mode.
    /// </summary>
    public override void OnStartClient()
    {
        Manager.players.Add(this);
        LobbyController.instance.UpdateLobbyName();
        LobbyController.instance.UpdatePlayerList();
    }

    /// <summary>
    /// Called when stopping as a client.
    /// Used in MultiPlayer mode.
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

    public void TryToggleReady()
    {
        if (isOwned)
        {
            CmdSetPlayerReady();
        }
    }

    [Command]
    private void CmdSetPlayerReady()
    {
        this.OnPlayerReadyUpdate(this.ready, !this.ready);
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

    /// <summary>
    /// Starts the game, if our client has authority over this object.
    /// This check is in place, because this function is called even for objects
    /// we don't have authority over.
    /// </summary>
    /// <param name="sceneName">The name of the scene (of a game) to load.</param>
    public void TryStartGame(string sceneName)
    {
        if (isOwned)
        {
            CmdStartGame(sceneName);
        }
    }

    [Command]
    public void CmdStartGame(string sceneName)
    {
        Manager.StartGame(sceneName);
    }


    // In-Game Methods
    public void TryUpdateBodyParts(List<BodyPart> bodyParts)
    {
        if (isOwned)
        {
            List<BodyPartData> bodyPartDatas = new();
            foreach (BodyPart part in bodyParts)
            {
                bodyPartDatas.Add(BodyPart.ToData(part));
            }
            CmdUpdateBodyParts(bodyPartDatas, playerSteamID);
        }
    }

    [Command]
    public void CmdUpdateBodyParts(List<BodyPartData> bodyPartDatas, ulong victimPlayerSteamID)
    {
        OnBodyPartUpdate(bodyPartDatas, victimPlayerSteamID);
    }

    [ClientRpc]
    public void OnBodyPartUpdate(List<BodyPartData> bodyPartDatas, ulong victimPlayerSteamID)
    {
        if (playerSteamID == victimPlayerSteamID && !isOwned)
        {
            PlayerMovementController playerMovementController = GetComponent<PlayerMovementController>();
            playerMovementController.BodyParts.Clear();
            for (int i = 0; i < bodyPartDatas.Count; i++)
            {
                playerMovementController.BodyParts.Add(
                    BodyPart.FromData(
                        bodyPartDatas[i],
                        playerMovementController.bodyPartContainer.transform.GetChild(i)
                    )
                );
            }
        }
    }
}
