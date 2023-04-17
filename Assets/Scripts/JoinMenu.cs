using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class JoinMenu : MonoBehaviour
{
	[SerializeField]
	private GameObject _contentOutput;

	[SerializeField]
	private GameObject _lobbyButtonTemplate;

	private CallResult<LobbyMatchList_t> m_LobbyMatchList;
	private CallResult<LobbyEnter_t> m_LobbyEnter;

	private void Awake()
	{
		if (SteamManager.Initialized)
		{
			m_LobbyMatchList = CallResult<LobbyMatchList_t>.Create(OnLobbyMatchList);
			m_LobbyEnter = CallResult<LobbyEnter_t>.Create(OnLobbyEnter);
		}
	}

	private void Start()
	{
		if (SteamManager.Initialized)
		{
			string name = SteamFriends.GetPersonaName();
			Debug.Log(name);

			SteamAPICall_t handle = SteamMatchmaking.RequestLobbyList();
			m_LobbyMatchList.Set(handle);
		}
	}

	public void OnBackPressed()
	{
		SceneManager.LoadScene("MainMenu");
	}

	public void OnLobbyJoinPressed(TextMeshProUGUI idField)
	{
		uint id;
		uint.TryParse(idField.text, out id);
		id--;

		print(id);

		SteamAPICall_t handle = SteamMatchmaking.JoinLobby((CSteamID)id);
		m_LobbyEnter.Set(handle);
	}

	private void OnLobbyMatchList(LobbyMatchList_t result, bool bIOFailure)
	{
		print("HI");
		uint count = result.m_nLobbiesMatching;
		for (int i = 0; i < count; i++)
		{
			CSteamID lobby_id = SteamMatchmaking.GetLobbyByIndex(i);
			GameObject newElement = Instantiate(_lobbyButtonTemplate, _contentOutput.transform);
			TextMeshProUGUI[] tmps = newElement.GetComponentsInChildren<TextMeshProUGUI>();

			string name = SteamMatchmaking.GetLobbyData(lobby_id, "name");
			int lobbyType;
			int.TryParse(SteamMatchmaking.GetLobbyData(lobby_id, "LOBBY_TYPE"), out lobbyType);
			int numPlayers = SteamMatchmaking.GetNumLobbyMembers(lobby_id);

			if (i == 2)
			{
				for (int j = 0; j < SteamMatchmaking.GetLobbyDataCount(lobby_id); j++)
				{
					string pchKey;
					string pchValue;
					SteamMatchmaking.GetLobbyDataByIndex(lobby_id, i, out pchKey, 500, out pchValue, 500);
					print(pchKey);
					print(pchValue);
				}
			}
			// Odd indices are labels
			tmps[0].text = (i+1).ToString();
			tmps[2].text = name;
			tmps[4].text = numPlayers.ToString();

			string lobbyTypeName;
			switch (lobbyType)
			{
				case 0:
					lobbyTypeName = "Private";
					break;
				case 1:
					lobbyTypeName = "Friends";
					break;
				case 2:
					lobbyTypeName = "Public";
					break;
				case 3:
					lobbyTypeName = "Invisible";
					break;
				default:
					lobbyTypeName = "Unknown";
					break;
			}
			tmps[6].text = lobbyTypeName;
		}
	}

	private void OnLobbyEnter(LobbyEnter_t result, bool bIOFailure)
	{

	}
}
