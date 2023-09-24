using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEditor;

public class PlayerObjectController : NetworkBehaviour
{
    [SerializeField]
    private PlayerMovement m_playerMovement;
    public PlayerMovement PM
    {
        get { return m_playerMovement; }
    }

    [SerializeField]
    private GameObject m_bodyPartTemplate;

    // Player data
    // SyncVars - Can only be changed on the server, and clients receive these changes.
    // Hooks - Client functions are executed when the SyncVar updates, acting like a ClientRPC.
    [SyncVar] public int connectionID;
    [SyncVar] public int playerNo;
    [SyncVar] public ulong playerSteamID;
    [SyncVar(hook = nameof(OnPlayerNameUpdate))] public string playerName;
    [SyncVar(hook = nameof(OnPlayerReadyUpdate))] public bool ready;
    [SyncVar(hook = nameof(OnPlayerHostUpdate))] public bool isHost = false;

    private CustomNetworkManager _manager;
    private CustomNetworkManager Manager
    {
        get
        {
            if (_manager != null) { return _manager; }
            return _manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }

    private PlayerHUDElement m_playerOnHUD = null;
    public PlayerHUDElement PlayerOnHUD
    {
        get
        {
            if (m_playerOnHUD) return m_playerOnHUD;

            PlayerHUDElementsHandler hud = GameObject.FindWithTag("HUD").GetComponent<PlayerHUDElementsHandler>();

            // This will be NULL if the HUD hasn't loaded yet.
            return hud.GetHUDElementOrNull(playerSteamID);
        }
    }


    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Lobby Methods
    /// <summary>
    /// Called when starting a player that we have authority over.
    /// </summary>
    public override void OnStartAuthority()
    {
        CmdSetPlayerName(SteamFriends.GetPersonaName());
        gameObject.name = "LocalPlayerObject";

        LobbyMenu.instance.UpdateLobbyName();

        if (NetworkServer.active)
            isHost = true;
        else
            CmdRequestGameSettings(playerSteamID);

        GetComponentInChildren<GameBehaviour>().enabled = true;
    }

    /// <summary>
    /// Called when starting a player that we don't have authority over.
    /// </summary>
    public override void OnStartClient()
    {
        Manager.AddPlayer(this);
        LobbyMenu.instance.UpdateLobbyName();
        LobbyMenu.instance.UpdatePlayerList();

        GetComponentInChildren<GameBehaviour>().enabled = true;
    }

    [Command]
    private void CmdRequestGameSettings(ulong id)
    {
        ReceiveHostSettingsClientRpc(id, GameSettings.Saved);
    }


    [ClientRpc]
    private void ReceiveHostSettingsClientRpc(ulong id, GameSettings settings)
    {
        if (playerSteamID != id) return;

        GameSettings.Saved = new(settings);

        print(GameSettings.Saved.TimeToMove);
    }

    /// <summary>
    /// Called when stopping as a client.
    /// Used in MultiPlayer mode.
    /// </summary>
    public override void OnStopClient()
    {
        Manager.RemovePlayer(this);
        LobbyMenu.instance.UpdatePlayerList();
    }

    [Command]
    private void CmdSetPlayerName(string playerName) { OnPlayerNameUpdate(this.playerName, playerName); }

    public void OnPlayerNameUpdate(string oldValue, string newValue)
    {
        if (isServer)
        {
            this.playerName = newValue;
        }
        if (isClient)
        {
            LobbyMenu.instance.UpdatePlayerList();
        }
    }

    [Command]
    public void CmdSetPlayerReady() { OnPlayerReadyUpdate(ready, !ready); }

    public void OnPlayerReadyUpdate(bool oldValue, bool newValue)
    {
        if (isServer)
        {
            ready = newValue;
        }
        if (isClient)
        {
            LobbyMenu.instance.UpdatePlayerList();
        }
    }

    public void OnPlayerHostUpdate(bool oldValue, bool newValue)
    {
        
    }

    [Command]
    public void CmdStartGame()
    {
        Manager.StartGame("Game");
    }

    
    /// <summary>
    /// Converts all Body Parts into BodyPartDatas, ready to be sent across the network.
    /// </summary>
    public void UpdateBodyParts()
    {
        if (!isOwned) return;

        List<BodyPartData> bodyPartDatas = new();
        foreach (BodyPart part in PM.BodyParts)
        {
            bodyPartDatas.Add(BodyPart.ToData(part));
        }

        if (PlayerOnHUD) // Patient update - can wait if the HUD isn't ready.
            PlayerOnHUD.SetNumParts(bodyPartDatas.Count);
        CmdUpdateBodyParts(bodyPartDatas, playerSteamID);
    }

    [Command]
    public void CmdUpdateBodyParts(List<BodyPartData> bodyPartDatas, ulong victimPlayerSteamID)
    {
        ClientUpdateBodyParts(bodyPartDatas, victimPlayerSteamID);
    }

    [ClientRpc]
    public void ClientUpdateBodyParts(List<BodyPartData> bodyPartDatas, ulong victimPlayerSteamID)
    {
        StartCoroutine(Wait.WaitForConditionThen(() => PM.enabled, 0.1f, () =>
        {
            if (playerSteamID == victimPlayerSteamID && !isOwned)
            {
                PM.BodyParts.Clear();
                for (int i = 0; i < bodyPartDatas.Count; i++)
                {
                    Transform bodyPartParent = PM.bodyPartContainer.transform;
                    int diff = bodyPartDatas.Count - bodyPartParent.childCount;

                    // Ensure all clients have the same number of BodyPart gameobjects (it doesn't
                    // matter which gameobject we remove when diff < 0)
                    // This seems computationally less expensive than destroying and reconstructing
                    // all the necessary gameobjects every move frame.

                    if (diff > 0)
                    {
                        for (int _j = 0; _j < diff; _j++)
                        {
                            Instantiate(m_bodyPartTemplate, bodyPartParent);
                        }
                    }
                    else if (diff < 0)
                    {
                        for (int _j = 0; _j > diff; _j--)
                        {
                            Destroy(bodyPartParent.GetChild(bodyPartParent.childCount - 2).gameObject);
                        }
                    }

                    BodyPart newBP = BodyPart.FromData(
                        bodyPartDatas[i],
                        PM.bodyPartContainer.transform.GetChild(i)
                    );

                    PM.BodyParts.Add(newBP);
                }
            }
        }));
    }

    public void LogDeath()
    { 
        CmdLogDeath(Manager.Players.IndexOf(this));
        if (PlayerOnHUD)
            PlayerOnHUD.AppearDead();
    }

    [Command]
    private void CmdLogDeath(int index) { LogDeathClientRpc(index); }

    [ClientRpc]
    private void LogDeathClientRpc(int index) { Manager.KillPlayer(index); }

    [ClientRpc]
    public void RpcDisableComponents()
    {
        GameBehaviour game = transform.Find("Game").GetComponent<GameBehaviour>();
        game.enabled = false;

        PlayerMovement pm = GetComponent<PlayerMovement>();
        pm.bodyPartContainer.SetActive(false);
        pm.enabled = false;
    }
}
