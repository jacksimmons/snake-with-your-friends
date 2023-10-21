using Mirror;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class GameBehaviour : NetworkBehaviour
{
    public static readonly string[] GAME_SCENES =
    { "Game" };

    // --- Puzzle
    public int availablePuzzles = 0;

    // An array of child indices for objects (all objects in this go under the s_objects game object parent)
    public static GameObject[] s_objects { get; protected set; }

    protected static Tilemap s_groundTilemap;
    protected static Tilemap s_wallTilemap;

    // Templates
    [SerializeField]
    private GameObject _mapTemplate;

    [SerializeField]
    private Tile _lightTile;
    [SerializeField]
    private Tile _darkTile;
    [SerializeField]
    private Tile _wallTile;

    [SerializeField]
    private GameObject _gameOverTemplate;

    [SerializeField]
    private GameObject _menuSelectTemplate;

    [SerializeField]
    private Vector2 _spawnPoint;

    // Soft limit is preferred, but if it is too small, the hard limit is used (1 tile).
    // The minimum ratio between the distance between two snakes, and the WORLD_SIZE, before an inner square must be established.
    private const float SOFT_MIN_DIST_WORLD_SIZE_RATIO = 0.2f;
    private const float HARD_MIN_DIST = 2f;

    [SerializeField]
    private List<GameObject> _foodTemplates = new();

    // SERVER VARIABLES --------------------
    // These are only accurate on the server; hence do NOT use them on a client.
    // They are static because they belong to no particular GameBehaviour.
    // How "loaded" the game currently is for the furthest behind player.
    public enum LoadingStage
    {
        Unloaded,
        SceneLoaded,
        GameSettingsSynced,
        MapLoaded,
        PlayerScriptsEnabled,
        GameStarted,
    }

    private static LoadingStage serverPlayersLoadingStage;
    private static int serverNumPlayersReady;


    private void OnEnable()
    {
        if (!isOwned) return;

        if (NetworkServer.active)
        {
            serverNumPlayersReady = 0;
            serverPlayersLoadingStage = LoadingStage.Unloaded;
        }
    }


    // LOADING STAGES ----------------------
    /// <summary>
    /// Increments the number of players that are ready in this stage.
    /// All players must be ready before the loading stage is incremented.
    /// </summary>
    [Command]
    private void CmdOnReady()
    {
        serverNumPlayersReady++;

        print($"Ready: {serverNumPlayersReady}/{CustomNetworkManager.Instance.numPlayers}");
        if (serverNumPlayersReady >= CustomNetworkManager.Instance.numPlayers)
        {
            serverNumPlayersReady = 0;

            serverPlayersLoadingStage++;
            RpcLoadingStageUpdate(serverPlayersLoadingStage);
        }

        if (numPlayersReady != 0) return;
        // ^ Following code executes directly after the last readier, every handshake

        switch (playersLoadingStage)
        {
            case LoadingStage.PlayerScriptsEnabled:
                ServerSetupGame();
                break;
        }
    }

    /// <summary>
    /// The progression through game loading is handled through a series of handshakes.
    /// Each handshake increments the loading stage and calls this function.
    /// </summary>
    [ClientRpc]
    private void RpcLoadingStageUpdate(LoadingStage newValue)
    {
        if (!isOwned) return;

        print(newValue.ToString());

        switch (newValue)
        {
            case LoadingStage.Unloaded:
                break;
            case LoadingStage.SceneLoaded:
                CmdRequestGameSettings(transform.parent.gameObject);
                break;
            case LoadingStage.GameSettingsSynced:
                CmdRequestMap(transform.parent.gameObject);
                break;
            case LoadingStage.MapLoaded:
                EnablePlayerScripts();
                break;
        }
    }

    [Client]
    public void OnGameSceneLoaded(string name)
    {
        if (!isOwned) return;

        CmdOnReady();
    }
    // ------------------------------------


    // GAME SETTINGS HANDSHAKE ------------
    [Command]
    private void CmdRequestGameSettings(GameObject player)
    {
        NetworkIdentity netIdentity = player.GetComponent<NetworkIdentity>();
        RpcReceiveGameSettings(netIdentity.connectionToClient, new(GameSettings.Saved));
    }


    [TargetRpc]
    private void RpcReceiveGameSettings(NetworkConnectionToClient _, GameSettingsData data)
    {
        if (!isOwned) return;
        GameSettings.Saved = new(data);
        CmdOnReady();
    }
    // ------------------------------------


    // MAP HANDSHAKE ----------------------
    [Command]
    private void CmdRequestMap(GameObject player)
    {
        GameObject map;
        if (GameSettings.Saved.GameMode == EGameMode.Puzzle)
        {
            int puzzleLevel = SaveData.Saved.PuzzleLevel;

            map = Instantiate(Resources.Load<GameObject>($"Puzzles/Puzzle{puzzleLevel}"));

            NetworkServer.Spawn(map);
        }
        else
        {
            map = Instantiate(_mapTemplate);

            SetupGroundTilemap(map, Vector2Int.zero);
            SetupWallTilemap(map, Vector2Int.zero);

            NetworkServer.Spawn(map);
        }

        map.name = "Map";

        NetworkIdentity netIdentity = player.GetComponent<NetworkIdentity>();
        RpcReceiveMap(netIdentity.connectionToClient, map);
    }


    [TargetRpc]
    private void RpcReceiveMap(NetworkConnectionToClient _, GameObject map)
    {
        s_groundTilemap = map.transform.Find("Ground").GetComponentInChildren<Tilemap>();
        s_wallTilemap = map.transform.Find("Wall").GetComponentInChildren<Tilemap>();
        CmdOnReady();
    }
    // ------------------------------------


    // PLAYER SCRIPT ENABLE HANDSHAKE -----
    [Client]
    private void EnablePlayerScripts()
    {
        foreach (var player in CustomNetworkManager.Instance.Players)
        {
            PlayerMovement pm = player.PM;
            pm.enabled = true;

            if (!pm.isOwned) continue;
            Camera cam = Camera.main;
            cam.GetComponent<CamBehaviour>().Player = pm;
        }

        CmdOnReady();
    }
    // ------------------------------------


    // (Server) GAME SETUP HANDSHAKE ------
    [Server]
    private void ServerSetupGame()
    {
        // --- Food ---
        int groundSize = GameSettings.Saved.GameSize;
        s_objects = new GameObject[groundSize * groundSize];

        // Unload food items which were removed in settings
        for (int i = 0; i < _foodTemplates.Count; i++)
        {
            GameObject food = _foodTemplates[i];
            if (!GameSettings.Saved.foodSettings.GetFoodEnabled(food.GetComponent<FoodObject>().food))
            {
                _foodTemplates.Remove(food);
                i--;
            }
        }

        GenerateStartingFood();

        // --- Players ---
        PlacePlayers();
        ActivatePlayersClientRpc();

        // IF Snake Royale
        //List<Vector2> positions = new(CustomNetworkManager.Instance.Players.Count);
        //List<float> rotation_zs = new(CustomNetworkManager.Instance.Players.Count);
        //for (int i = 0; i < CustomNetworkManager.Instance.Players.Count; i++)
        //{
        //    positions.Add(CustomNetworkManager.Instance.Players[i].transform.position);
        //    rotation_zs.Add(CustomNetworkManager.Instance.Players[i].transform.rotation.eulerAngles.z);
        //}
        //PlacePlayersClientRpc(positions, rotation_zs);
    }

    [Server]
    public void PlacePlayers()
    {
        if (GameSettings.Saved.GameMode == EGameMode.SnakeRoyale)
        {
            PlacePlayers_SnakeRoyale(depth: 1, playersStartIndex: 0, Vector2Int.zero);
        }
    }
    [Server]
    private void PlacePlayers_SnakeRoyale(int depth, int playersStartIndex, Vector2Int bl)
    {
        // Outer snakes (along the walls)
        // Calculate the maximum distance between snakes.
        // If this distance is too small, spawn inner snakes.

        int groundSize = GameSettings.Saved.GameSize;

        int playersCount = 0;
        if (CustomNetworkManager.Instance.Players.Count - playersStartIndex > 0)
        {
            playersCount = CustomNetworkManager.Instance.Players.Count - playersStartIndex;
        }
        List<PlayerObjectController> players = CustomNetworkManager.Instance.Players.GetRange(playersStartIndex, playersCount);

        float minDist = groundSize * SOFT_MIN_DIST_WORLD_SIZE_RATIO;
        if (minDist < HARD_MIN_DIST)
            minDist = HARD_MIN_DIST;

        Vector3 BL = s_groundTilemap.CellToWorld((Vector3Int)(bl + (depth + 1) * Vector2Int.one));
        Vector3 BR = s_groundTilemap.CellToWorld((Vector3Int)(bl + new Vector2Int(groundSize - depth + 1, depth + 1)));
        Vector3 TL = s_groundTilemap.CellToWorld((Vector3Int)(bl + new Vector2Int(depth + 1, groundSize - depth + 1)));
        Vector3 TR = s_groundTilemap.CellToWorld((Vector3Int)(bl + (groundSize - depth + 1) * Vector2Int.one));

        Vector3[] corners = { BL, BR, TL, TR };
        Vector2[] directions = { Vector2.one, new Vector2(-1, 1), new Vector2(1, -1), -Vector2.one };

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].transform.position == Vector3.zero)
            {
                players[i].transform.position = corners[i % 4]
                + (Vector3)(Vector2.one * directions[i % 4] * s_groundTilemap.cellSize / 2);
            }

            // If i were 0 then it might enter this, causing -4 as length to be provided (in the PlacePlayers line).
            if (i != 0 && i % 4 == 0 && i < players.Count - 1)
            {
                int newDepth = depth + (int)Mathf.Floor(minDist);
                if (newDepth >= groundSize / 2)
                {
                    Debug.LogError("The players do not fit in the map provided.");
                }
                else
                {
                    PlacePlayers_SnakeRoyale(newDepth, playersStartIndex + 4, bl);
                }
            }
        }
    }

    [Server]
    private void GenerateStartingFood()
    {
        for (int i = 0; i < CustomNetworkManager.Instance.Players.Count; i++)
        {
            GenerateFood();
        }
    }

    [ClientRpc]
    private void ActivatePlayersClientRpc()
    {
        foreach (var player in CustomNetworkManager.Instance.Players)
        {
            PlayerMovement pm = player.PM;
            pm.bodyPartContainer.SetActive(true);
        }

        CmdOnReady();
    }
    // ------------------------------------


    // Additional Functions ---------------
    [Client]
    private void SetupGroundTilemap(GameObject map, Vector2Int bl)
    {
        Tilemap tilemap = map.transform.Find("Ground").GetComponentInChildren<Tilemap>();

        int groundSize = GameSettings.Saved.GameSize;

        // Bounds are an inner square of the 51x51 wall bounds starting at 0,0
        BoundsInt bounds = new(
            (Vector3Int)(bl + Vector2Int.one),
            (Vector3Int)(groundSize * Vector2Int.one) + Vector3Int.forward);
        Tile[] tiles = new Tile[groundSize * groundSize];
        for (int i = 0; i < groundSize; i++)
        {
            for (int j = 0; j < groundSize; j++)
            {
                if (i % 2 == 0)
                {
                    // Even row -> starts with light (i.e. Even cols are light)
                    if (j % 2 == 0)
                        tiles[groundSize * i + j] = _lightTile;
                    else
                        tiles[groundSize * i + j] = _darkTile;
                }
                else
                {
                    // Odd row -> starts with dark (i.e. Odd cols are light)
                    if (j % 2 == 0)
                        tiles[groundSize * i + j] = _darkTile;
                    else
                        tiles[groundSize * i + j] = _lightTile;
                }
            }
        }
        tilemap.SetTilesBlock(bounds, tiles);
    }


    [Client]
    private void SetupWallTilemap(GameObject map, Vector2Int bl)
    {
        Tilemap tilemap = map.transform.Find("Wall").GetComponentInChildren<Tilemap>();

        int groundSize = GameSettings.Saved.GameSize;

        // This square is (int)GroundSize + 2 squared, since it is one bigger on each side of the x and y edges of the inner square
        BoundsInt bounds = new(
            (Vector3Int)bl,
            (Vector3Int)((groundSize + 2) * Vector2Int.one) + Vector3Int.forward);
        Tile[] tiles = new Tile[(groundSize + 2) * (groundSize + 2)];
        for (int i = 0; i < groundSize + 2; i++)
        {
            for (int j = 0; j < groundSize + 2; j++)
            {
                if (i == 0 || i == groundSize + 1)
                {
                    // We are on the top or bottom row, so guaranteed placement of wall
                    tiles[(groundSize + 2) * i + j] = _wallTile;
                }
                else if (j == 0 || j == groundSize + 1)
                {
                    // We are on the leftmost or rightmost column, so place wall
                    tiles[(groundSize + 2) * i + j] = _wallTile;
                }
            }
        }

        tilemap.SetTilesBlock(bounds, tiles);
    }



    //[ClientRpc]
    //public void PlacePlayersClientRpc(List<Vector2> positions, List<float> rotation_zs)
    //{
    //    if (positions.Count != rotation_zs.Count)
    //    {
    //        Debug.LogError("Positions and rotations have mismatching lengths!");
    //        return;
    //    }

    //    for (int i = 0; i < positions.Count; i++)
    //    {
    //        PlayerObjectController poc = CustomNetworkManager.Instance.Players[i];
    //        poc.transform.SetPositionAndRotation(positions[i], Quaternion.Euler(Vector3.forward * rotation_zs[i]));
    //    }
    //}


    [Server]
    private void GenerateFood()
    {
        // If no foods are enabled, quick exit.
        // This is not necessarily erroneous, as players can disable all foods.
        if (_foodTemplates.Count == 0)
        {
            Debug.LogWarning("No foods");
            return;
        }

        int objectPos = Random.Range(0, s_objects.Length);

        // Overwrite s_objects[objectPos] with -1 (if there are any vacancies)
        // This effectively acts as a test to see if there are any vacancies,
        // which also happens to locate the vacancy, while leaving its value
        // as -1.
        objectPos = AddObjectToGrid(objectPos, null);
        if (objectPos == -1)
        {
            // No vacancies.
            return;
        }

        int foodIndex = Random.Range(0, _foodTemplates.Count);
        int groundSize = GameSettings.Saved.GameSize;

        Vector2 foodPos = new((objectPos % groundSize) + (1.5f), (objectPos / groundSize) + (1.5f));

        GameObject obj = Instantiate(_foodTemplates[foodIndex], foodPos, Quaternion.Euler(Vector3.forward * 0));
        obj.GetComponent<GridObject>().gridPos = objectPos;

        if (AddObjectToGrid(objectPos, obj) != -1)
        {
            NetworkServer.Spawn(obj);
        }
    }


    /// <summary>
    /// Finds the first free slot in s_objects(null slot), populates it with obj, and returns the
    /// index. Linear Search = O(n)
    [Server]
    public int AddObjectToGrid(GameObject obj)
    {
        // Linear search for the first empty slot
        int objPos = 0;
        while (objPos < s_objects.Length)
        {
            if (s_objects[objPos] == null)
            {
                s_objects[objPos] = obj;
                return objPos;
            }
            objPos++;
        }
        Debug.LogError("Grid filled with objects!");
        return -1;
    }


    /// <summary>
    /// Checks the given index to see if it is free. If not, searches every slot until it finds a
    /// free slot. Then populates the slot with obj, returns the index of the slot. Recursive.
    /// </summary>
    [Server]
    public int AddObjectToGrid(int objectPos, GameObject obj)
    {
        if (s_objects[objectPos] != null)
        {
            // If there already is an object at given pos, try to put
            // the object on the first different free slot in the array.
            for (int i = 0; (i < s_objects.Length) && (i != objectPos); i++)
            {
                if (s_objects[i] == null)
                {
                    s_objects[i] = obj;
                    return i;
                }
            }

            Debug.LogError("Grid filled with objects!");
            return -1;
        }
        s_objects[objectPos] = obj;
        return objectPos;
    }


    [Server]
    public void RemoveObjectFromGrid(int objectPos)
    {
        GameObject go = s_objects[objectPos];

        if (go == null)
        {
            Debug.LogError("GameObject was null!");
            return;
        }

        NetworkServer.UnSpawn(go);
        NetworkServer.Destroy(go);
        s_objects[objectPos] = null;
    }


    [Command]
    public void CmdRemoveFood(int objPos)
    {
        RemoveObjectFromGrid(objPos);

        if (GameSettings.Saved.GameMode == EGameMode.SnakeRoyale)
        {
            GenerateFood();
        }
    }


    //void CreateTeleportingMenuPair(
    //    string text1, string text2,
    //    Vector3 from, Vector3 to)
    //{
    //    GameObject menuSelect = Instantiate(_menuSelectTemplate);

    //    Teleporter teleporter = menuSelect.GetComponentInChildren<Teleporter>();

    //    teleporter.A.transform.position = from;
    //    teleporter.A.GetComponentInChildren<TextMeshProUGUI>().text = text1;
    //    teleporter.B.transform.position = to;
    //    teleporter.B.GetComponentInChildren<TextMeshProUGUI>().text = text2;
    //}


    private void SetGameOverScreenActivity(bool active, int score = 0)
    {
        if (!isOwned)
            return;

        GameObject canvas = GameObject.Find("Canvas");
        Transform bg = canvas.transform.GetChild(0);
        GameObject gameOver = bg.Find("GameOver").gameObject;
        GameObject spectateUI = bg.Find("SpectateUI").gameObject;
        gameOver.SetActive(active);

        if (active)
        {
            bool online = !CustomNetworkManager.Instance.singleplayer;
            gameOver.transform.Find("OnlineButton").gameObject.SetActive(online);
            gameOver.transform.Find("OfflineButton").gameObject.SetActive(!online);
            gameOver.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = "Score: " + score.ToString();

            // Spectate the next player in the list
            spectateUI.SetActive(true);
            spectateUI.GetComponent<SpectateBehaviour>().GetFirstTarget();
        }
    }

    public void OnGameOver(int score)
    {
        if (!isOwned)
            return;

        SetGameOverScreenActivity(true, score);
    }

    public void OnGameOverDecision()
    {
        if (!isOwned)
            return;

        SetGameOverScreenActivity(false);
    }
}
