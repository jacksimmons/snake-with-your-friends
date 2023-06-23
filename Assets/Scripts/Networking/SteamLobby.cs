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

    // Lobby objects
    [SerializeField]
    private TextMeshProUGUI _lobbyNameText;

    // Mirror
    private CustomNetworkManager _manager;

    // Steam IDs
    public CSteamID SteamID { get; private set; } = CSteamID.Nil;
    public CSteamID LobbyID { get; private set; } = CSteamID.Nil;
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

        SteamID = SteamUser.GetSteamID();

        lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
        lobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
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


    private void AddLobbyMember(CSteamID id)
    {
        string name = SteamFriends.GetFriendPersonaName(id);
        LobbyPlayerData.Add(id, name);
    }

    private void RemoveLobbyMember(CSteamID id)
    {
        LobbyPlayerData.Remove(id);
    }

    /// <summary>
    /// Should only be used when joining a lobby, to prevent reconstruction on every
    /// chat update event.
    /// </summary>
    private void AddAllLobbyMembers()
    {
        int numPlayers = SteamMatchmaking.GetNumLobbyMembers(LobbyID);
        for (int i = 0; i < numPlayers; i++)
        {
            CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(LobbyID, i);
            AddLobbyMember(memberId);
        }
    }


    // Callbacks
    /// <summary>
    /// Called when a user joins through the friends list.
    /// </summary>
    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t result)
    {
        print("Request to join lobby.");
        SteamMatchmaking.JoinLobby(result.m_steamIDLobby);
    }

    /// <summary>
    /// Called when a user has joined, left, disconnected, etc. Need to check if we are the new owner.
    /// </summary>
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
                AddLobbyMember(affects);
                break;
            case 1 << 1:
                print(affectsName + " left.");
                RemoveLobbyMember(affects);
                break;
            case 1 << 2:
                print(affectsName + " disconnected.");
                RemoveLobbyMember(affects);
                break;
            case 1 << 3:
                print(changerName + " kicked " + affects);
                RemoveLobbyMember(affects);
                break;
            case 1 << 4:
                print(changer + " banned " + affects);
                RemoveLobbyMember(affects);
                break;
            default:
                print("[OnLobbyChatUpdate] Something...happened?");
                break;
        }

        //IsOwner = Id == SteamMatchmaking.GetLobbyOwner(_lobbyId);
    }

    /// <summary>
    /// Only tells when a data update occurs, not what is updated.
    /// Therefore, this function updates all essential data, under a greedy philosophy.
    /// </summary>
    private void OnLobbyDataUpdate(LobbyDataUpdate_t callback)
    {
        if (callback.m_bSuccess != 1)
        {
            Debug.LogError("Data update failed.");
            return;
        }

        _lobbyNameText.text = SteamMatchmaking.GetLobbyData(LobbyID, "name");
    }


    // Callresults
    /// <summary>
    /// Called when attempting to create a lobby.
    /// </summary>
    private void OnLobbyCreated(LobbyCreated_t result, bool bIOFailure)
    {
        if (bIOFailure || result.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError("Failed to create lobby.");
            return;
        }

        LobbyID = (CSteamID)result.m_ulSteamIDLobby;
        print("Lobby [ID: " + LobbyID.ToString() + "] created successfully.");

        bool success = SteamMatchmaking.SetLobbyData(LobbyID, "name", SteamFriends.GetPersonaName() + "'s lobby");

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
        if (bIOFailure || result.m_EChatRoomEnterResponse == (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseError)
        {
            print("Failed to enter lobby.");
            return;
        }

        if (LobbyID == CSteamID.Nil)
            LobbyID = (CSteamID)result.m_ulSteamIDLobby;
        StartCoroutine(LoadLobby());
        print("Entered lobby successfully.");
    }

    private IEnumerator LoadLobby()
    {
        AsyncOperation loadLobbyMenuComplete = SceneManager.LoadSceneAsync("LobbyMenu");

        while (!loadLobbyMenuComplete.isDone)
        {
            yield return new WaitForSeconds(0.1f);
        }
        AddAllLobbyMembers();

        // Everyone
        _lobbyNameText.text = SteamMatchmaking.GetLobbyData(LobbyID, "name");

        if (NetworkServer.active)
        {
            print("I am the host.");
            yield break;
        }

        // Clients
        print("I am not the host.");
        _manager.networkAddress = SteamMatchmaking.GetLobbyData(LobbyID, HOST_ADDRESS_KEY);
        _manager.StartClient();

        yield break;
    }
}
