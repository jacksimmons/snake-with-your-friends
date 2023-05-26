using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Server-side lobby management.
/// </summary>
public class Lobby : MonoBehaviour
{
    [SerializeField]
    private GameObject _lobbyEntryTemplate;

    private CSteamID lobbyId = new CSteamID();

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
        if ((ulong)lobbyId != 0)
        {
            SteamMatchmaking.LeaveLobby(lobbyId);
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
        SteamAPICall_t handle = SteamMatchmaking.JoinLobby(id);
        m_LobbyEnter.Set(handle);
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
        switch (result.m_EChatRoomEnterResponse)
        {
            case (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess:
                print("Joined lobby successfully.");
                StartCoroutine(LoadLobby());
                break;
            case (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseNotAllowed:
                print("Not allowed to join lobby.");
                break;
            case (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseError:
                print("An error occurred.");
                break;
            case (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseDoesntExist:
                print("This lobby no longer exists.");
                break;
        }
    }

    // A user has joined, left, disconnected, etc.
    private void OnLobbyChatUpdate(LobbyChatUpdate_t pCallback)
    {
        string affects = SteamFriends.GetFriendPersonaName(
            (CSteamID)(pCallback.m_ulSteamIDUserChanged));
        string changer = SteamFriends.GetFriendPersonaName(
            (CSteamID)(pCallback.m_ulSteamIDMakingChange));

        uint stateChange = pCallback.m_rgfChatMemberStateChange;
        switch (stateChange)
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
                { "Lobby ID", (ulong) lobbyId == 0 ? "False" : lobbyId.ToString() },
                { "Lobby Name", (ulong) lobbyId, "name") }
            };
        }
        else
            lobbyValues = new Dictionary<string, string>();

        return lobbyValues;
    }

    private IEnumerator LoadLobby()
    {
        SceneManager.LoadSceneAsync("LobbyMenu");

        while (SceneManager.GetActiveScene().name != "LobbyMenu")
        {
            yield return new WaitForSeconds(1);
        }

        // The "Content" child has this tag.
        GameObject content = GameObject.FindWithTag("LobbyPanel");

        TextMeshProUGUI[] tmps = entry.GetComponentsInChildren<TextMeshProUGUI>();

        int numPlayers = SteamMatchmaking.GetNumLobbyMembers(lobbyId);

        for (int i = 0; i < numPlayers; i++)
        {
            GameObject entry = Instantiate(_lobbyEntryTemplate, content.transform);

            CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, i);
            string name = SteamFriends.GetFriendPersonaName(memberId);

            tmps[0].text = i.ToString();
            tmps[1].text = name;
        }
        yield break;
    }
}