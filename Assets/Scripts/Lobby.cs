using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using System;

public class Lobby : MonoBehaviour
{
	ulong lobbyID = 0uL;
	List<string> playerList = new List<string>();

	protected Callback<LobbyCreated_t> m_LobbyCreated;
	protected Callback<LobbyEnter_t> m_LobbyEnter;
	protected Callback<LobbyChatUpdate_t> m_LobbyChatUpdate;

	private void Awake()
	{
		DontDestroyOnLoad(this);
		if (SteamManager.Initialized)
		{
			m_LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
			m_LobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
			m_LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
		}
	}

	// Start is called before the first frame update
	void Start()
    {
		if (SteamManager.Initialized)
		{
			CreateLobby();
		}
    }

	void CreateLobby()
	{
		SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, cMaxMembers: 4);
	}

	void JoinLobby()
	{
		//SteamMatchmaking.JoinLobby
	}

	// Callbacks
	void OnLobbyCreated(LobbyCreated_t pCallback)
	{
		switch (pCallback.m_eResult)
		{
			case EResult.k_EResultOK:
				print("Lobby created successfully.");
				break;
			default:
				print("Failed to create lobby.");
				break;
		}
	}

	// We (attempted) joined the lobby
	void OnLobbyEnter(LobbyEnter_t pCallback)
	{
		lobbyID = pCallback.m_ulSteamIDLobby;
	}

	// A user has joined, left, disconnected, etc.
	void OnLobbyChatUpdate(LobbyChatUpdate_t pCallback)
	{
		string affects = SteamFriends.GetFriendPersonaName(
			new CSteamID(pCallback.m_ulSteamIDUserChanged));
		string changer = SteamFriends.GetFriendPersonaName(
			new CSteamID(pCallback.m_ulSteamIDMakingChange));

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
}
