using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using System;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviour
{
	static ulong lobbyId = 0;

	static protected Callback<LobbyEnter_t> m_LobbyEnter;
	static protected Callback<LobbyChatUpdate_t> m_LobbyChatUpdate;

	static private CallResult<LobbyCreated_t> m_LobbyCreated;

	// Start is called before the first frame update
	void Start()
	{
		if (SteamManager.Initialized)
		{
			m_LobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
			m_LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
			m_LobbyCreated = CallResult<LobbyCreated_t>.Create(OnLobbyCreated);

			CreateLobby();
		}
	}

	static public void OnBackPressed()
	{
		if (lobbyId != 0)
		{
			SteamMatchmaking.LeaveLobby((CSteamID)lobbyId);
			print("Left the lobby.");
		}
		SceneManager.LoadScene("MainMenu");
	}

	static void CreateLobby()
	{
		SteamAPICall_t handle = SteamMatchmaking.CreateLobby(
			ELobbyType.k_ELobbyTypePublic, cMaxMembers: 4);
		m_LobbyCreated.Set(handle);
	}

	// Callbacks
	static void OnLobbyCreated(LobbyCreated_t result, bool bIOFailure)
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
	}

	// A user has joined, left, disconnected, etc.
	static void OnLobbyChatUpdate(LobbyChatUpdate_t pCallback)
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

	static void OnLobbyEnter(LobbyEnter_t pCallback)
	{
		if (pCallback.m_EChatRoomEnterResponse ==
			(uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
		{
			lobbyId = pCallback.m_ulSteamIDLobby;
			print("Joined lobby successfully.");
		}
	}

	static public Dictionary<string, string> GetLobbyDebug()
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