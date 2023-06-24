using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class CustomNetworkManager : NetworkManager
{
    [SerializeField]
    private PlayerObjectController _playerPrefab;

    public List<PlayerObjectController> players { get; } = new List<PlayerObjectController>();


    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (SceneManager.GetActiveScene().name == "LobbyMenu")
        {
            PlayerObjectController playerInstance = Instantiate(_playerPrefab);
            playerInstance.connectionID = conn.connectionId;
            playerInstance.playerNo = players.Count + 1;
            playerInstance.playerSteamID = (ulong)SteamMatchmaking.GetLobbyMemberByIndex(new CSteamID(SteamLobby.instance.LobbyID), players.Count);

            NetworkServer.AddPlayerForConnection(conn, playerInstance.gameObject);
        }
    }
}
