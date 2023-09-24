using Mirror;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Tilemaps;
using static UnityEngine.ParticleSystem;
using Random = UnityEngine.Random;

public class GameBehaviour : NetworkBehaviour
{
    public static readonly string[] GAME_SCENES =
    { "Game" };
    public static readonly string[] GAME_MODES =
    { "SnakeRoyale", "Puzzle" };

    // --- Puzzle
    public int availablePuzzles = 0;

    // Static variables
    private static int numPlayersReady = 0;
    // An array of child indices for objects (all objects in this go under the s_objects game object parent)
    public static GameObject[] s_objects { get; protected set; }

    protected static Tilemap s_groundTilemap;
    protected static Tilemap s_wallTilemap;

    // Templates
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


    private void OnEnable()
    {
        if (!isOwned) return;

        // If this is the host object
        if (NetworkServer.active)
        {
            numPlayersReady = 0;
        }
    }

    [Client]
    public void OnGameSceneLoaded(string name)
    {
        if (!isOwned) return;

        // Wait to receive the host's GameSettings by RPC
        StartCoroutine(Wait.WaitForConditionThen(
        () => GameSettings.Saved != null,
        0.1f,
        () => 
        {
            if (GameSettings.Saved.GameMode == EGameMode.Puzzle)
            {
                OnGameSceneLoaded_Puzzle();
            }
            else
            {
                ClientLoadTilemaps();
            }

            EnableLocalPlayerMovement();
            CmdReady();
        }));
    }


    // Enables all player object movement components
    [Client]
    private void EnableLocalPlayerMovement()
    {
        foreach (var player in CustomNetworkManager.Instance.Players)
        {
            PlayerMovement pm = player.PM;
            pm.enabled = true;

            if (!pm.isOwned) continue;
            GameObject cam = GameObject.FindWithTag("MainCamera");
            cam.GetComponent<CamBehaviour>().Player = pm;
        }
    }

    [Client]
    private void OnGameSceneLoaded_Puzzle()
    {
        byte puzzleLevel = SaveData.Saved.PuzzleLevel;

        GameObject puzzle = Instantiate(Resources.Load<GameObject>($"Puzzles/Puzzle{puzzleLevel}"));
        puzzle.name = $"Puzzle{puzzleLevel}";

        s_groundTilemap = puzzle.transform.Find("Ground").GetComponent<Tilemap>();
        s_wallTilemap = puzzle.transform.Find("Wall").GetComponent<Tilemap>();
    }


    [Client]
    protected void ClientLoadTilemaps()
    {
        s_groundTilemap = CreateAndReturnTilemap(gridName: "Ground", hasCollider: false);
        s_wallTilemap = CreateAndReturnTilemap(gridName: "Wall", hasCollider: true);

        CreateGroundTilemap(ref s_groundTilemap, Vector2Int.zero);
        CreateWallTilemap(ref s_wallTilemap, Vector2Int.zero);
    }


    [Client]
    protected Tilemap CreateAndReturnTilemap(string gridName, bool hasCollider)
    {
        GameObject gridObject = new GameObject(gridName);
        gridObject.AddComponent<Grid>();

        GameObject tilemapObject = new GameObject("Tilemap");
        tilemapObject.AddComponent<Tilemap>();
        tilemapObject.AddComponent<TilemapRenderer>();

        if (hasCollider)
        {
            tilemapObject.AddComponent<TilemapCollider2D>();
            tilemapObject.GetComponent<TilemapCollider2D>().isTrigger = true;
            tilemapObject.AddComponent<DeathTrigger>();
        }

        tilemapObject.transform.parent = gridObject.transform;

        Tilemap tilemap = tilemapObject.GetComponent<Tilemap>();

        return tilemap;
    }


    [Client]
    private void CreateGroundTilemap(ref Tilemap groundTilemap, Vector2Int bl)
    {
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
        groundTilemap.SetTilesBlock(bounds, tiles);
    }


    [Client]
    private void CreateWallTilemap(ref Tilemap wallTilemap, Vector2Int bl)
    {
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

        wallTilemap.SetTilesBlock(bounds, tiles);
    }


    [Command]
    private void CmdReady()
    {
        // Needs to be static, as every GameBehaviour calling this command will have its OWN
        // s_numPlayersReady incremented otherwise.
        numPlayersReady++;
        if (numPlayersReady == CustomNetworkManager.Instance.Players.Count)
        {
            ServerLoadGame();
        }
    }


    [Server]
    private void ServerLoadGame()
    {
        int groundSize = GameSettings.Saved.GameSize;

        s_objects = new GameObject[groundSize * groundSize];

        if (GameSettings.Saved.GameMode == EGameMode.Puzzle)
        {
            ServerLoadGame_Puzzle();
        }
        else if (GameSettings.Saved.GameMode == EGameMode.SnakeRoyale)
        {
            ServerLoadGame_SnakeRoyale();
        }
        GameObject.FindWithTag("HUD").GetComponent<PlayerHUDElementsHandler>().LoadHUD();
    }
    private void ServerLoadGame_Puzzle()
    {
        PlacePlayers();
    }
    private void ServerLoadGame_SnakeRoyale()
    {
        PlacePlayers();

        // Unload food items which were removed in settings
        for (int i = 0; i < _foodTemplates.Count; i++)
        {
            GameObject food = _foodTemplates[i];
            if (GameSettings.Saved.DisabledFoods.Contains(food.GetComponent<FoodObject>().food))
            {
                _foodTemplates.Remove(food);
                i--;
            }
        }

        List<Vector2> positions = new(CustomNetworkManager.Instance.Players.Count);
        List<float> rotation_zs = new(CustomNetworkManager.Instance.Players.Count);
        for (int i = 0; i < CustomNetworkManager.Instance.Players.Count; i++)
        {
            positions.Add(CustomNetworkManager.Instance.Players[i].transform.position);
            rotation_zs.Add(CustomNetworkManager.Instance.Players[i].transform.rotation.eulerAngles.z);
        }
        PlacePlayersClientRpc(positions, rotation_zs);
        ActivatePlayersClientRpc();

        GenerateStartingFood();
    }


    [Server]
    public void PlacePlayers()
    {
        if (GameSettings.Saved.GameMode == EGameMode.Puzzle)
        {
            PlacePlayers_Puzzle();
        }
        else if (GameSettings.Saved.GameMode == EGameMode.SnakeRoyale)
        {
            PlacePlayers_SnakeRoyale(depth: 1, playersStartIndex: 0, Vector2Int.zero);
        }
    }
    [Server]
    private void PlacePlayers_Puzzle()
    {
        Transform puzzleStartPoints = GameObject.FindWithTag("PuzzleStart").transform;
        PlayerMovement pm = GetComponentInParent<PlayerMovement>();
        pm.FreeMovement = true;
        pm.TimeToMove = 0.5f;

        for (int i = 0; i < pm.BodyParts.Count; i++)
        {
            Transform startPoint = puzzleStartPoints.GetChild(i);
            float rot = startPoint.rotation.eulerAngles.z;
            pm.BodyParts[i].Position = startPoint.position;
            pm.BodyParts[i].Direction = Extensions.Vectors.Rotate(Vector2.up, rot);
            pm.BodyParts[i].RegularAngle = rot;
        }

        pm.startingDirection = pm.BodyParts[0].Position - pm.BodyParts[1].Position;
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
            players[i].transform.position = corners[i % 4]
                + (Vector3)(Vector2.one * directions[i % 4] * s_groundTilemap.cellSize / 2);

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


    [ClientRpc]
    public void PlacePlayersClientRpc(List<Vector2> positions, List<float> rotation_zs)
    {
        if (positions.Count != rotation_zs.Count)
        {
            Debug.LogError("Positions and rotations have mismatching lengths!");
            return;
        }

        for (int i = 0; i < positions.Count; i++)
        {
            PlayerObjectController poc = CustomNetworkManager.Instance.Players[i];
            poc.transform.SetPositionAndRotation(positions[i], Quaternion.Euler(Vector3.forward * rotation_zs[i]));
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
    }


    [Server]
    private void GenerateStartingFood()
    {
        for (int i = 0; i < CustomNetworkManager.Instance.Players.Count; i++)
        {
            GenerateFood();
        }
    }


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
