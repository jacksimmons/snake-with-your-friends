using Mirror;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEditor.Experimental.RestService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SteamLobby : MonoBehaviour
{
    public static SteamLobby instance;

    // Constants
    private const string HOST_ADDRESS_KEY = "hostAddress";

    // Mirror
    private CustomNetworkManager _manager;

    // Steam IDs
    public ulong SteamID { get; private set; } = 0;
    public ulong LobbyID { get; private set; } = 0;
    public Dictionary<CSteamID, string> LobbyPlayerData { get; private set; } = new();

    // Callbacks/Callresults
    protected Callback<LobbyChatUpdate_t> lobbyChatUpdate;
    protected Callback<LobbyDataUpdate_t> lobbyDataUpdate;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    private CallResult<LobbyEnter_t> _lobbyEnter;
    private CallResult<LobbyCreated_t> _lobbyCreated;


    private void Awake()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("SteamManager is not initialised!");
            return;
        }

        if (instance == null)
        {
            instance = this; 
        }

        _manager = GetComponent<CustomNetworkManager>();

        SteamID = (ulong)SteamUser.GetSteamID();

        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
        _lobbyEnter = CallResult<LobbyEnter_t>.Create(OnLobbyEnter);
        _lobbyCreated = CallResult<LobbyCreated_t>.Create(OnLobbyCreated);
    }


    public void HostLobby()
    {
        SteamAPICall_t handle = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, _manager.maxConnections);
        _lobbyCreated.Set(handle);
        _lobbyEnter.Set(handle);

        //foreach (GameObject go in GameObject.FindGameObjectsWithTag("PlayerInput"))
        //{
        //    if (go.name == "SpeedSlider")
        //    {
        //        Slider slider = go.GetComponent<Slider>();
        //        _counter.thresholdSeconds = slider.value;
        //    }
        //}
    }


    // Callbacks
    /// <summary>
    /// Called when a user joins through the friends list.
    /// </summary>
    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t result)
    {
        print("Request to join lobby.");
        SteamAPICall_t handle = SteamMatchmaking.JoinLobby(result.m_steamIDLobby);
        _lobbyEnter.Set(handle);
    }


    // Callresults
    /// <summary>
    /// Called when attempting to create a lobby.
    /// </summary>
    private void OnLobbyCreated(LobbyCreated_t result, bool bIOFailure)
    {
        if (bIOFailure || (result.m_eResult != EResult.k_EResultOK))
        {
            Debug.LogError("Failed to create lobby.");
            return;
        }


        bool success = SteamMatchmaking.SetLobbyData(new CSteamID(result.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName() + "'s lobby")
                    && SteamMatchmaking.SetLobbyData(new CSteamID(result.m_ulSteamIDLobby), HOST_ADDRESS_KEY, SteamUser.GetSteamID().ToString());
        LobbyID = result.m_ulSteamIDLobby;

        if (success)
            print("Successfully set the name of the lobby.");
        else
            print("Was unable to set the name of the lobby.");

        _manager.StartHost();
    }

    /// <summary>
    /// Called when attempting to enter a lobby.
    /// </summary>
    private void OnLobbyEnter(LobbyEnter_t result, bool bIOFailure)
    {
        if (bIOFailure || result.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
        {
            print("Failed to enter lobby.");
            return;
        }

        // LobbyID should only be set if we aren't the owner.
        // A different ID is sent here if we are the host.
        if (LobbyID == 0)
        {
            LobbyID = result.m_ulSteamIDLobby;
        }

        // Determine if we are a client
        if (NetworkServer.active)
        {
            print("I am the host.");
            return;
        }

        // If we are a client then start client.
        print("I am not the host.");
        _manager.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(LobbyID), HOST_ADDRESS_KEY);
        _manager.StartClient();

        print("Entered lobby successfully.");
    }

    private void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
    {
        CSteamID affects = (CSteamID)callback.m_ulSteamIDUserChanged;
        CSteamID changer = (CSteamID)callback.m_ulSteamIDMakingChange;

        string affectsName = SteamFriends.GetFriendPersonaName(
            (CSteamID)callback.m_ulSteamIDUserChanged);
        string changerName = SteamFriends.GetFriendPersonaName(
            (CSteamID)callback.m_ulSteamIDMakingChange);

        uint stateChange = callback.m_rgfChatMemberStateChange;
        switch (stateChange)
        {
            case 1 << 0:
                print(affectsName + " entered.");
                break;
            case 1 << 1:
                print(affectsName + " left.");
                break;
            case 1 << 2:
                print(affectsName + " disconnected.");
                break;
            case 1 << 3:
                print(changerName + " kicked " + affects);
                break;
            case 1 << 4:
                print(changer + " banned " + affects);
                break;
            default:
                print("[OnLobbyChatUpdate] Something...happened?");
                break;
        }
    }
}
