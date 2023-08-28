using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class CustomNetworkManager : NetworkManager
{
    [SerializeField]
    private PlayerObjectController _playerPrefab;

    // Players
    public List<PlayerObjectController> Players { get; private set; } = new();
    public List<PlayerObjectController> AlivePlayers { get; private set; } = new();

    public bool singleplayer = false;


    public void AddPlayer(PlayerObjectController player)
    {
        Players.Add(player);
        AlivePlayers.Add(player);
    }

    public void RemovePlayer(PlayerObjectController player)
    {
        Players.Remove(player);
        KillPlayer(player);
    }

    public void KillPlayer(PlayerObjectController player)
    {
        AlivePlayers.Remove(player);
        if (AlivePlayers.Count == 1)
        {
            print("Game Over! " + player.playerName + " wins the game.");
        }
    }

    public void StartWithNoFriends()
    {
        singleplayer = true;
        StartHost();
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "LobbyMenu")
        {
            PlayerObjectController playerInstance = Instantiate(_playerPrefab);
            playerInstance.connectionID = conn.connectionId;
            playerInstance.playerNo = Players.Count + 1;

            CSteamID lobbyID = new(SteamLobby.instance.LobbyID);
            playerInstance.playerSteamID = 
                (ulong)SteamMatchmaking.GetLobbyMemberByIndex(lobbyID, Players.Count);

            NetworkServer.AddPlayerForConnection(conn, playerInstance.gameObject);
        }
    }

    public void StartGame(string sceneName)
    {
        ServerChangeScene(sceneName);
    }

    // Called once a scene is fully loaded by our client, after being requested to by the server.
    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != "Game")
        {
            return;
        }

        GameObject lpo = GameObject.Find("LocalPlayerObject");
        GameBehaviour gb = lpo.GetComponentInChildren<GameBehaviour>();

        gb.OnGameSceneLoaded("Game");
    }
}