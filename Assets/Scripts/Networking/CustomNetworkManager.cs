using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.Linq;

public class CustomNetworkManager : NetworkManager
{
    [SerializeField]
    private PlayerObjectController _playerPrefab;

    // Players
    public List<PlayerObjectController> Players { get; private set; } = new();
    public List<PlayerObjectController> AlivePlayers { get; private set; } = new();

    public bool singleplayer = false;

    private readonly string[] GAME_SCENES = { "Game" };
    private readonly string[] LOBBY_SCENES = { "LobbyMenu" };

    private static CustomNetworkManager _instance;
    public static CustomNetworkManager Instance
    {
        get
        {
            if (_instance != null) { return _instance; }
            return _instance = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }


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

    public void RemovePlayer(int index) { RemovePlayer(Players[index]); }

    public void KillPlayer(PlayerObjectController player)
    {
        AlivePlayers.Remove(player);
        if (AlivePlayers.Count == 1)
        {
            print("Game Over! " + AlivePlayers[0].playerName + " wins the game.");
            AlivePlayers[0].CmdEndGame();
        }
    }

    // Note: Player must be in AlivePlayers for this to work. Error handling TBA.
    public void KillPlayer(int index) { KillPlayer(Players[index]); }

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

    [Server]
    public void StartGame(string sceneName)
    {
        ServerChangeScene(sceneName);
    }

    // Called once a scene is fully loaded by our client, after being requested to by the server.
    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();

        string sceneName = SceneManager.GetActiveScene().name;

        if (GAME_SCENES.Contains(sceneName))
        {
            GameObject lpo = GameObject.Find("LocalPlayerObject");
            GameBehaviour gb = lpo.GetComponentInChildren<GameBehaviour>();

            gb.OnGameSceneLoaded(sceneName);

            return;
        }

        if (LOBBY_SCENES.Contains(sceneName))
        {
            GameObject.Find("LobbyController").GetComponent<LobbyMenu>()
                .UpdatePlayerList();

            // Reset every Player Object, so that the game can run again
            foreach (PlayerObjectController poc in Players)
            {
                Transform bodyParts = poc.transform.Find("BodyParts");
                while (bodyParts.childCount > 0)
                {
                    DestroyImmediate(bodyParts.GetChild(0).gameObject);
                }

                // Disable PlayerMovement so OnEnable is called later
                poc.GetComponent<PlayerMovement>().enabled = false;
            }
        }
    }

    [Server]
    public void EndGame()
    {
        ServerChangeScene("LobbyMenu");
    }
}