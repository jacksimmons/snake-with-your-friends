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

    [SyncVar]
    public int numPlayersInGame = 0;

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
    }

    public void StartGame(string sceneName)
    {
        ServerChangeScene(sceneName);
    }

    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        StartCoroutine(Wait.WaitForObjectThen(() => GameObject.Find("LocalPlayerObject"),
            (GameObject obj) =>
            {
                GameBehaviour gb = obj.GetComponentInChildren<GameBehaviour>();
                gb.OnServerChangeScene(newSceneName);
                numPlayersInGame++;
            },
            new WaitForSeconds(0.1f))
        );
    }
}