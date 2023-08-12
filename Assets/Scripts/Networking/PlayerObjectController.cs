using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEditor;

public class PlayerObjectController : NetworkBehaviour
{
    [SerializeField]
    private PlayerMovementController m_pmc;
    [SerializeField]
    private GameObject m_bodyPartTemplate;

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
        Manager.Players.Add(this);
        LobbyController.instance.UpdateLobbyName();
        LobbyController.instance.UpdatePlayerList();
    }

    /// <summary>
    /// Called when stopping as a client.
    /// Used in MultiPlayer mode.
    /// </summary>
    public override void OnStopClient()
    {
        Manager.Players.Remove(this);
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
    public void CmdSetPlayerReady()
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

    [Command]
    public void CmdStartGame(string sceneName)
    {
        Manager.StartGame(sceneName);
    }

    // In-Game Methods
    public void UpdateBodyParts()
    {
        List<BodyPartData> bodyPartDatas = new();
        foreach (BodyPart part in m_pmc.BodyParts)
        {
            bodyPartDatas.Add(BodyPart.ToData(part));
        }
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
        if (playerSteamID == victimPlayerSteamID && !isOwned)
        {
            m_pmc.BodyParts.Clear();
            for (int i = 0; i < bodyPartDatas.Count; i++)
            {
                BodyPartData data = bodyPartDatas[i];
                print($"Body Part {i}: \n Position: {data.position} \n Direction: {data.direction}");
                print($"Corner Rot: {data.CornerAngle} \n Rot: {data.RegularAngle}");
                print($"Current Type: {data.CurrentType} \n Default Type: {data.DefaultType}");

                Transform bodyPartParent = m_pmc.bodyPartContainer.transform;
                int diff = bodyPartDatas.Count - bodyPartParent.childCount;
                while (diff > 0)
                {
                    Instantiate(m_bodyPartTemplate, bodyPartParent);
                    diff = bodyPartDatas.Count - bodyPartParent.childCount;
                }
                while (diff < 0)
                {
                    Destroy(bodyPartParent.GetChild(bodyPartParent.childCount).gameObject);
                    diff = bodyPartDatas.Count - bodyPartParent.childCount;
                }

                BodyPart newBP = BodyPart.FromData(
                    bodyPartDatas[i],
                    m_pmc.bodyPartContainer.transform.GetChild(i)
                );

                m_pmc.BodyParts.Add(newBP);
            }
        }
    }

    public void HandleBodyPartDeath(BodyPart bp, bool dead)
    {
        if (!isOwned)
            return;

        int ocIndex = Manager.Players.IndexOf(this);
        if (ocIndex == -1)
        {
            Debug.LogError("Couldn't find player in Manager.Players!");
            return;
        }
        int bpIndex = m_pmc.BodyParts.IndexOf(bp);
        if (bpIndex == -1)
        {
            Debug.LogError("Couldn't find BodyPart!");
            return;
        }
        CmdHandleBodyPartDeath(ocIndex, bpIndex, dead);
    }

    [Command]
    private void CmdHandleBodyPartDeath(int ocIndex, int bpIndex, bool dead)
    {
        PlayerObjectController poc = Manager.Players[ocIndex];
        poc.m_pmc.SetBodyPartDeadClientRpc(bpIndex, dead);
    }

    public void HandleDeath(bool dead)
    {
        if (!isOwned)
            return;

        int index = Manager.Players.IndexOf(this);
        if (index == -1)
        {
            Debug.LogError("Couldn't find player in Manager.Players!");
            return;
        }
        CmdHandleDeath(index, dead);
    }

    [Command]
    private void CmdHandleDeath(int index, bool dead)
    {
        PlayerObjectController poc = Manager.Players[index];
        poc.m_pmc.SetDeadClientRpc(dead);
    }
}
