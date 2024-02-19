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


    public PlayerObjectController GetPlayer(ulong steamID)
    {
        foreach (var player in Instance.Players)
        {
            if (player.playerSteamID == steamID)
                return player;
        }
        Debug.LogError("No player with that ID was found!");
        return null;
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

        bool gameOver = true;
        string gameOverMessage = "";

        if (AlivePlayers.Count == 1)
            gameOverMessage = $"Game Over! {AlivePlayers[0].playerName} wins the game.";
        else if (AlivePlayers.Count == 0)
            gameOverMessage = $"Game Over! All players died - noone wins the game.";
        else
            gameOver = false;

        if (gameOver && NetworkServer.active)
        {
            print(gameOverMessage);
            StartCoroutine(Wait.WaitThen(3f, ServerEndGame));
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

            CSteamID lobbyID = new(Steam.Instance.LobbyID);
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
            GameBehaviour.Instance.OnGameSceneLoaded(sceneName);
            return;
        }

        if (LOBBY_SCENES.Contains(sceneName))
        {
            GameObject.Find("LobbyMenu").GetComponent<LobbyMenu>()
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
    public void ServerEndGame()
    {
        GameBehaviour.Instance.enabled = false;
        GameObject.Find("LocalPlayerObject").GetComponent<PlayerObjectController>().RpcDisableComponents();
        ServerChangeScene("LobbyMenu");
    }
}