using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviour
{
    ulong lobbyId = 0;

    protected Callback<LobbyChatUpdate_t> m_LobbyChatUpdate;
    protected Callback<LobbyDataUpdate_t> m_LobbyDataUpdate;

    private CallResult<LobbyEnter_t> m_LobbyEnter;
    private CallResult<LobbyCreated_t> m_LobbyCreated;

    // Start is called before the first frame update
    private void Awake()
    {
        if (SteamManager.Initialized)
        {
            m_LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
            m_LobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
            m_LobbyEnter = CallResult<LobbyEnter_t>.Create(OnLobbyEnter);
            m_LobbyCreated = CallResult<LobbyCreated_t>.Create(OnLobbyCreated);

            DontDestroyOnLoad(this);
        }
    }

    public void OnBackPressed()
    {
        if (lobbyId != 0)
        {
            SteamMatchmaking.LeaveLobby((CSteamID)lobbyId);
            print("Left the lobby.");
        }
        SceneManager.LoadScene("MainMenu");
    }

    public void CreateLobby()
    {
        SteamAPICall_t handle = SteamMatchmaking.CreateLobby(
            ELobbyType.k_ELobbyTypePublic, cMaxMembers: 4);
        m_LobbyCreated.Set(handle);
    }

    public void JoinLobby(CSteamID id)
    {
        SteamMatchmaking.JoinLobby(id);
    }

    // Callbacks
    private void OnLobbyCreated(LobbyCreated_t result, bool bIOFailure)
    {
        switch (result.m_eResult)
        {
            case EResult.k_EResultOK:
                print("Lobby created successfully.");
                break;
            default:
                print("Failed to create lobby.");
                return;
        }

        bool success = SteamMatchmaking.SetLobbyData(
            (CSteamID)result.m_ulSteamIDLobby,
            "name",
            SteamFriends.GetPersonaName() + "'s lobby");
        if (success)
        {
            print("Yay set name!");
        }
        else
            print("Nay didn't set name...");
    }

    private void OnLobbyEnter(LobbyEnter_t result, bool bIOFailure)
    {
        if (result.m_EChatRoomEnterResponse ==
            (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
        {
            lobbyId = result.m_ulSteamIDLobby;
            print("Joined lobby successfully.");

            print("Number of members: " + SteamMatchmaking.GetNumLobbyMembers((CSteamID)lobbyId).ToString());
        }
        else
        {
            print(result.m_EChatRoomEnterResponse);
        }
    }

    // A user has joined, left, disconnected, etc.
    private void OnLobbyChatUpdate(LobbyChatUpdate_t pCallback)
    {
        string affects = SteamFriends.GetFriendPersonaName(
            (CSteamID)(pCallback.m_ulSteamIDUserChanged));
        string changer = SteamFriends.GetFriendPersonaName(
            (CSteamID)(pCallback.m_ulSteamIDMakingChange));

        uint bf_stateChange = pCallback.m_rgfChatMemberStateChange;
        switch (bf_stateChange)
        {
            case 1 << 0:
                // Entered
                break;
            case 1 << 1:
                // Left
                break;
            case 1 << 2:
                // DCd
                break;
            case 1 << 3:
                // Kicked
                break;
            case 1 << 4:
                // Banned
                break;
            default:
                // ???
                break;
        }
    }

    private void OnLobbyDataUpdate(LobbyDataUpdate_t pCallback)
    {
        if (pCallback.m_bSuccess == 1)
            print("Data changed for " + pCallback.m_ulSteamIDMember.ToString() + " successfully.");
        else
            print("Data was unable to be changed for " + pCallback.m_ulSteamIDMember.ToString());
    }

    public Dictionary<string, string> GetLobbyDebug()
    {
        Dictionary<string, string> lobbyValues;
        if (SteamManager.Initialized)
        {
            lobbyValues = new Dictionary<string, string>
            {
                { "Steam Name", SteamFriends.GetPersonaName() },
                { "Steam State", SteamFriends.GetPersonaState().ToString().Substring(15) },
                { "Lobby ID", lobbyId == 0 ? "False" : lobbyId.ToString() },
                { "Lobby Name", lobbyId == 0 ? "False" : SteamMatchmaking.GetLobbyData((CSteamID)lobbyId, "name") }
            };
        }
        else
            lobbyValues = new Dictionary<string, string>();

        return lobbyValues;
    }
}