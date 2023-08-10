using Steamworks;
using System.Collections.Generic;
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
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "LobbyMenu")
        {
            PlayerObjectController playerInstance = Instantiate(_playerPrefab);
            playerInstance.connectionID = conn.connectionId;
            playerInstance.playerNo = players.Count + 1;
            playerInstance.playerSteamID = (ulong)SteamMatchmaking.GetLobbyMemberByIndex(new CSteamID(SteamLobby.instance.LobbyID), players.Count);

            NetworkServer.AddPlayerForConnection(conn, playerInstance.gameObject);
        }
        else if (scene.name == "OfflineGame")
        {
            PlayerObjectController playerInstance = Instantiate(_playerPrefab);
            playerInstance.connectionID = conn.connectionId;
            playerInstance.playerNo = players.Count + 1;
            playerInstance.playerSteamID = 0;

            NetworkServer.AddPlayerForConnection(conn, playerInstance.gameObject);
        }
    }

    public void StartGame(string sceneName)
    {
        ServerChangeScene(sceneName);
    }

    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        StartCoroutine(WaitForLoad.WaitForObject(() => GameObject.Find("LocalPlayerObject"),
            (GameObject obj) => obj.GetComponentInChildren<GameBehaviour>().OnServerChangeScene(newSceneName),
            new WaitForSeconds(0.1f))
        );
    }
}