using Mirror;
using Steamworks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class GameBehaviour : NetworkBehaviour
{
    [SerializeField]
    private Tile _lightTile;
    [SerializeField]
    private Tile _darkTile;
    [SerializeField]
    private Tile _wallTile;

    [SerializeField]
    private GameObject _gameOverTemplate;

    [SerializeField]
    private GameObject[] _foodTemplates;
    [SerializeField]
    private GameObject _menuSelectTemplate;

    private static Tilemap s_groundTilemap;
    private static Tilemap s_wallTilemap;

    [SerializeField]
    private Vector2 _spawnPoint;

    public enum EWorldSize : int
    {
        Lobby = 10,
        Small = 20,
        Medium = 40,
        Large = 60,
        Massive = 80
    }
    public EWorldSize GroundSize { get; private set; } = EWorldSize.Lobby;

    public enum EClientMode : int
    {
        Offline,
        Online
    }
    public EClientMode ClientMode { get; private set; } = EClientMode.Online;

    // An array of child indices for objects (all objects in this go under the Objects game object parent)
    public GameObject[] Objects { get; private set; }

    Vector2Int bl = Vector2Int.zero;

    // Soft limit is preferred, but if it is too small, the hard limit is used (1 tile).
    // The minimum ratio between the distance between two snakes, and the WORLD_SIZE, before an inner square must be established.
    private const float SOFT_MIN_DIST_WORLD_SIZE_RATIO = 0.2f;
    private const float HARD_MIN_DIST = 2f;

    private CustomNetworkManager _manager;
    private CustomNetworkManager Manager
    {
        get
        {
            if (_manager != null) { return _manager; }
            return _manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }

    private static int s_numPlayersReady = 0;

    [Client]
    public void OnGameSceneLoaded(string name)
    {
        if (name != "Game")
            return;

        if (isOwned)
        {
            ClientLoadGame();
            CmdReady();
        }
    }

    [Client]
    private void ClientLoadGame()
    {
        s_groundTilemap = CreateAndReturnTilemap(gridName: "Ground", hasCollider: false);
        s_wallTilemap = CreateAndReturnTilemap(gridName: "Wall", hasCollider: true);

        CreateGroundTilemap(ref s_groundTilemap, bl);
        CreateWallTilemap(ref s_wallTilemap, bl);

        PlayerMovementController player = GameObject.Find("LocalPlayerObject").GetComponent<PlayerMovementController>();

        GameObject cam = GameObject.FindWithTag("MainCamera");
        cam.GetComponent<CamBehaviour>().Player = player;
    }

    [Client]
    private Tilemap CreateAndReturnTilemap(string gridName, bool hasCollider)
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
        // Bounds are an inner square of the 51x51 wall bounds starting at 0,0
        BoundsInt bounds = new(
            (Vector3Int)(bl + Vector2Int.one),
            (Vector3Int)((int)GroundSize * Vector2Int.one) + Vector3Int.forward);
        Tile[] tiles = new Tile[(int)GroundSize * (int)GroundSize];
        for (int i = 0; i < (int)GroundSize; i++)
        {
            for (int j = 0; j < (int)GroundSize; j++)
            {
                if (i % 2 == 0)
                {
                    // Even row -> starts with light (i.e. Even cols are light)
                    if (j % 2 == 0)
                        tiles[(int)GroundSize * i + j] = _lightTile;
                    else
                        tiles[(int)GroundSize * i + j] = _darkTile;
                }
                else
                {
                    // Odd row -> starts with dark (i.e. Odd cols are light)
                    if (j % 2 == 0)
                        tiles[(int)GroundSize * i + j] = _darkTile;
                    else
                        tiles[(int)GroundSize * i + j] = _lightTile;
                }
            }
        }
        groundTilemap.SetTilesBlock(bounds, tiles);
    }

    [Client]
    private void CreateWallTilemap(ref Tilemap wallTilemap, Vector2Int bl)
    {
        // This square is (int)GroundSize + 2 squared, since it is one bigger on each side of the x and y edges of the inner square
        BoundsInt bounds = new(
            (Vector3Int)bl,
            (Vector3Int)(((int)GroundSize + 2) * Vector2Int.one) + Vector3Int.forward);
        Tile[] tiles = new Tile[((int)GroundSize + 2) * ((int)GroundSize + 2)];
        for (int i = 0; i < (int)GroundSize + 2; i++)
        {
            for (int j = 0; j < (int)GroundSize + 2; j++)
            {
                if (i == 0 || i == (int)GroundSize + 1)
                {
                    // We are on the top or bottom row, so guaranteed placement of wall
                    tiles[((int)GroundSize + 2) * i + j] = _wallTile;
                }
                else if (j == 0 || j == (int)GroundSize + 1)
                {
                    // We are on the leftmost or rightmost column, so place wall
                    tiles[((int)GroundSize + 2) * i + j] = _wallTile;
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
        s_numPlayersReady++;

        if (s_numPlayersReady == Manager.Players.Count)
        {
            ServerLoadGame();
        }
    }

    [Server]
    private void ServerLoadGame()
    {
        PlacePlayers(depth: 1, playersStartIndex: 0, bl);

        List<Vector2> positions = new(Manager.Players.Count);
        List<float> rotation_zs = new(Manager.Players.Count);
        for (int i = 0; i < Manager.Players.Count; i++)
        {
            positions.Add(Manager.Players[i].transform.position);
            rotation_zs.Add(Manager.Players[i].transform.rotation.eulerAngles.z);
        }
        PlacePlayersClientRpc(positions, rotation_zs);
        ActivateLocalPlayerClientRpc();

        Objects = new GameObject[(int)GroundSize * (int)GroundSize];
        GenerateStartingFood();
    }

    [Server]
    public void PlacePlayers(int depth, int playersStartIndex, Vector2Int bl)
    {
        // Outer snakes (along the walls)
        // Calculate the maximum distance between snakes.
        // If this distance is too small, spawn inner snakes.

        int playersCount = 0;
        if (Manager.Players.Count - playersStartIndex > 0)
        {
            playersCount = Manager.Players.Count - playersStartIndex;
        }
        List<PlayerObjectController> players = Manager.Players.GetRange(playersStartIndex, playersCount);

        float minDist = (int)GroundSize * SOFT_MIN_DIST_WORLD_SIZE_RATIO;
        if (minDist < HARD_MIN_DIST)
            minDist = HARD_MIN_DIST;

        Vector3 BL = s_groundTilemap.CellToWorld((Vector3Int)(bl + (depth + 1) * Vector2Int.one));
        Vector3 BR = s_groundTilemap.CellToWorld((Vector3Int)(bl + new Vector2Int((int)GroundSize - depth + 1, depth + 1)));
        Vector3 TL = s_groundTilemap.CellToWorld((Vector3Int)(bl + new Vector2Int(depth + 1, (int)GroundSize - depth + 1)));
        Vector3 TR = s_groundTilemap.CellToWorld((Vector3Int)(bl + ((int)GroundSize - depth + 1) * Vector2Int.one));

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
                if (newDepth >= (int)GroundSize / 2)
                {
                    Debug.LogError("The players do not fit in the map provided.");
                }
                else
                {
                    PlacePlayers(newDepth, playersStartIndex + 4, bl);
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
            PlayerObjectController poc = Manager.Players[i];
            poc.transform.SetPositionAndRotation(positions[i], Quaternion.Euler(Vector3.forward * rotation_zs[i]));
        }
    }

    [ClientRpc]
    private void ActivateLocalPlayerClientRpc()
    {
        PlayerMovementController pmc = GameObject.Find("LocalPlayerObject").GetComponent<PlayerMovementController>();
        pmc.bodyPartContainer.SetActive(true);
    }

    [Server]
    private void GenerateStartingFood()
    {
        for (int i = 0; i < Manager.Players.Count; i++)
        {
            GenerateFood();
        }
    }

    [Server]
    private void GenerateFood()
    {
        int objectPos = Random.Range(0, Objects.Length);

        // Overwrite Objects[objectPos] with -1 (if there are any vacancies)
        // This effectively acts as a test to see if there are any vacancies,
        // which also happens to locate the vacancy, while leaving its value
        // as -1.
        objectPos = AddObjectToGrid(objectPos, null);
        if (objectPos == -1)
        {
            // No vacancies.
            return;
        }

        int foodIndex = Random.Range(0, _foodTemplates.Length);
        Vector2 foodPos = new((objectPos % (int)GroundSize) + (bl.x + 1.5f), (objectPos / (int)GroundSize) + (bl.y + 1.5f));
        
        GameObject obj = Instantiate(_foodTemplates[foodIndex], foodPos, Quaternion.Euler(Vector3.forward * 0), GameObject.Find("Objects").transform);
        obj.GetComponent<GridObject>().gridPos.Value = objectPos;
        NetworkServer.Spawn(obj);

        AddObjectToGrid(objectPos, obj);
    }

    /// <summary>
    /// Checks if index `objectPos` is not -1 in Objects, if so it recursively
    /// searches for a valid index.
    /// </summary>
    /// <returns>The final position of the object, or -1 if no vacancies in Objects.</returns>
    [Server]
    public int AddObjectToGrid(int objectPos, GameObject obj)
    {
        if (Objects[objectPos] != null)
        {
            // If there already is an object at given pos, try to put
            // the object on the first different free slot in the array.
            for (int i = 0; (i < Objects.Length) && (i != objectPos); i++)
            {
                if (Objects[i] == null)
                {
                    Objects[i] = obj;
                    return i;
                }
            }

            Debug.LogError("Grid filled with objects!");
            return -1;
        }
        Objects[objectPos] = obj;
        return objectPos;
    }

    /// <summary>
    /// First gets every client to delete the object, THEN removes it from
    /// the objects array. Because the array is a SyncVar, clients would lose
    /// reference to it otherwise.
    /// </summary>
    [Command]
    public void CmdRemoveObjectFromGrid(int objectPos)
    {
        GameObject go = Objects[objectPos];

        if (go == null)
        {
            Debug.LogError("GameObject was null!");
            return;
        }

        NetworkServer.UnSpawn(go);
        NetworkServer.Destroy(go);
        Objects[objectPos] = null;

        GenerateFood();
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

        GameObject gameOver = GameObject.Find("Canvas").transform.Find("GameOver").gameObject;
        gameOver.SetActive(active);

        if (active)
        {
            bool online = !Manager.singleplayer;
            gameOver.transform.Find("OnlineButton").gameObject.SetActive(online);
            gameOver.transform.Find("OfflineButton").gameObject.SetActive(!online);
            gameOver.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = "Score: " + score.ToString();
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
