using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using static GameBehaviour;

public class CustomNetworkManager : NetworkManager
{
    [SerializeField]
    private PlayerObjectController _playerPrefab;

    // Players
    public List<PlayerObjectController> Players { get; private set; } = new List<PlayerObjectController>();

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "LobbyMenu")
        {
            PlayerObjectController playerInstance = Instantiate(_playerPrefab);
            playerInstance.connectionID = conn.connectionId;
            playerInstance.playerNo = Players.Count + 1;
            playerInstance.playerSteamID = (ulong)SteamMatchmaking.GetLobbyMemberByIndex(new CSteamID(SteamLobby.instance.LobbyID), Players.Count);

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

        GameObject lpo = GameObject.Find("LocalPlayerObject");
        GameBehaviour gb = lpo.GetComponentInChildren<GameBehaviour>();

        gb.OnGameSceneLoaded(SceneManager.GetActiveScene().name);
    }
}