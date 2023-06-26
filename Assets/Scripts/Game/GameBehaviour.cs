using Extensions;
using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameBehaviour : NetworkBehaviour
{
    [SerializeField]
    private Tile _lightTile;
    [SerializeField]
    private Tile _darkTile;
    [SerializeField]
    private Tile _wallTile;

    [SerializeField]
    private GameObject[] _foodTemplates;
    [SerializeField]
    private GameObject _menuSelectTemplate;

    private Tilemap _groundTilemap;
    private Tilemap _wallTilemap;

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

    [SerializeField]
    private static EWorldSize groundSize = EWorldSize.Lobby;

    public enum EWorldMode : int
    {
        None,
        Lobby,
        Offline,
        Online
    }

    [SerializeField]
    private EWorldMode WorldMode = EWorldMode.Online;

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

    // An array of child indices for objects (all objects in this go under the Objects game object parent)
    private List<int> _objects;


    void Start()
    {
        SetGameOverScreenActivity(false);

        SetupGame();

        if (isServer)
        {
            _objects = new((int)groundSize * (int)groundSize);
            // Sets every value to -1.
            for (int i = 0; i < _objects.Count; i++) { _objects[i] = -1; }
            GenerateStartingFood();
        }
    }

    public void GenerateStartingFood()
    {
        for (int i = 0; i < Manager.players.Count; i++)
        {
            GenerateFood();
        }
    }

    public void GenerateFood()
    {
        int objectPos = Random.Range(0, _objects.Count);

        // Overwrite _objects[objectPos] with -1 (if there are any vacancies)
        // This effectively acts as a test to see if there are any vacancies,
        // which also happens to locate the vacancy, while leaving its value
        // as -1.
        objectPos = AddToGrid(objectPos, -1);
        if (objectPos == -1)
        {
            // No vacancies.
            return;
        }

        int foodIndex = Random.Range(0, _foodTemplates.Length);
        Vector2 foodPos = new((objectPos % (int)groundSize) + (bl.x + 1.5f), (objectPos / (int)groundSize) + (bl.y + 1.5f));
        ClientCreateObject(_objects[objectPos], position: foodPos, rotation_z: 0, foodIndex);
    }

    [ClientRpc]
    public void ClientCreateObject(int siblingIndex, Vector2 position, float rotation_z, int foodIndex)
    {
        GameObject obj = _foodTemplates[foodIndex];
        Instantiate(obj, position, Quaternion.Euler(Vector3.forward * rotation_z), GameObject.Find("Objects").transform);
        obj.transform.SetSiblingIndex(siblingIndex);
    }

    public void SetupGame()
    {
        PlayerMovementController player = GameObject.Find("LocalGamePlayer").GetComponent<PlayerMovementController>();
        GameObject cam = GameObject.FindWithTag("MainCamera");
        cam.GetComponent<CamBehaviour>().SetupCamera(player);

        _groundTilemap = CreateAndReturnTilemap(gridName: "Ground", hasCollider: false);
        _wallTilemap = CreateAndReturnTilemap(gridName: "Wall", hasCollider: true);

        CreateGroundTilemap(_groundTilemap, bl);
        CreateWallTilemap(_wallTilemap, bl);

        if (WorldMode == EWorldMode.Online && isServer)
        {
            PlacePlayers(depth: 1, playersStartIndex: 0, bl);
            List<Vector2> positions = new(Manager.players.Count);
            List<float> rotation_zs = new(Manager.players.Count);
            for(int i = 0; i < Manager.players.Count; i++)
            {
                positions.Add(Manager.players[i].transform.position);
                rotation_zs.Add(Manager.players[i].transform.rotation.eulerAngles.z);
            }
            ClientPlacePlayers(positions, rotation_zs);
        }
    }

    Tilemap CreateAndReturnTilemap(string gridName, bool hasCollider)
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
            tilemapObject.AddComponent<WallTilemap>();
        }

        tilemapObject.transform.parent = gridObject.transform;

        Tilemap tilemap = tilemapObject.GetComponent<Tilemap>();

        return tilemap;
    }


    void CreateGroundTilemap(Tilemap groundTilemap, Vector2Int bl)
    {
        // Bounds are an inner square of the 51x51 wall bounds starting at 0,0
        BoundsInt bounds = new BoundsInt(
            (Vector3Int)(bl + Vector2Int.one),
            (Vector3Int)((int)groundSize * Vector2Int.one) + Vector3Int.forward);
        Tile[] tiles = new Tile[(int)groundSize * (int)groundSize];
        for (int i = 0; i < (int)groundSize; i++)
        {
            for (int j = 0; j < (int)groundSize; j++)
            {
                if (i % 2 == 0)
                {
                    // Even row -> starts with light (i.e. Even cols are light)
                    if (j % 2 == 0)
                        tiles[(int)groundSize * i + j] = _lightTile;
                    else
                        tiles[(int)groundSize * i + j] = _darkTile;
                }
                else
                {
                    // Odd row -> starts with dark (i.e. Odd cols are light)
                    if (j % 2 == 0)
                        tiles[(int)groundSize * i + j] = _darkTile;
                    else
                        tiles[(int)groundSize * i + j] = _lightTile;
                }
            }
        }
        groundTilemap.SetTilesBlock(bounds, tiles);
    }


    void CreateWallTilemap(Tilemap wallTilemap, Vector2Int bl)
    {
        // This square is (int)groundSize + 2 squared, since it is one bigger on each side of the x and y edges of the inner square
        BoundsInt bounds = new BoundsInt(
            (Vector3Int)bl,
            (Vector3Int)(((int)groundSize + 2) * Vector2Int.one) + Vector3Int.forward);
        Tile[] tiles = new Tile[((int)groundSize + 2) * ((int)groundSize + 2)];
        for (int i = 0; i < (int)groundSize + 2; i++)
        {
            for (int j = 0; j < (int)groundSize + 2; j++)
            {
                if (i == 0 || i == (int)groundSize + 1)
                {
                    // We are on the top or bottom row, so guaranteed placement of wall
                    tiles[((int)groundSize + 2) * i + j] = _wallTile;
                }
                else if (j == 0 || j == (int)groundSize + 1)
                {
                    // We are on the leftmost or rightmost column, so place wall
                    tiles[((int)groundSize + 2) * i + j] = _wallTile;
                }
            }
        }

        wallTilemap.SetTilesBlock(bounds, tiles);
    }

    public void PlacePlayers(int depth, int playersStartIndex, Vector2Int bl)
    {
        // Outer snakes (along the walls)
        // Calculate the maximum distance between snakes.
        // If this distance is too small, spawn inner snakes.

        int playersCount = 0;
        if (Manager.players.Count - playersStartIndex > 0)
        {
            playersCount = Manager.players.Count - playersStartIndex;
        }
        List<PlayerObjectController> players = Manager.players.GetRange(playersStartIndex, playersCount);

        float minDist = (int)groundSize * SOFT_MIN_DIST_WORLD_SIZE_RATIO;
        if (minDist < HARD_MIN_DIST)
            minDist = HARD_MIN_DIST;

        Vector3 BL = _groundTilemap.CellToWorld((Vector3Int)(bl + (depth + 1) * Vector2Int.one));
        Vector3 BR = _groundTilemap.CellToWorld((Vector3Int)(bl + new Vector2Int((int)groundSize - depth + 1, depth + 1)));
        Vector3 TL = _groundTilemap.CellToWorld((Vector3Int)(bl + new Vector2Int(depth + 1, (int)groundSize - depth + 1)));
        Vector3 TR = _groundTilemap.CellToWorld((Vector3Int)(bl + ((int)groundSize - depth + 1) * Vector2Int.one));

        Vector3[] corners = { BL, BR, TL, TR };
        Vector2[] directions = { Vector2.one, new Vector2(-1, 1), new Vector2(1, -1), -Vector2.one };

        for (int i = 0; i < players.Count; i++)
        {
            players[i].transform.position = corners[i % 4]
                + (Vector3)(Vector2.one * directions[i % 4] * _groundTilemap.cellSize / 2);

            // If i were 0 then it might enter this, causing -4 as length to be provided (in the PlacePlayers line).
            if (i != 0 && i % 4 == 0 && i < players.Count - 1)
            {
                int newDepth = depth + (int)Mathf.Floor(minDist);
                if (newDepth >= (int)groundSize / 2)
                {
                    throw new System.Exception("The players do not fit in the map provided.");
                }
                else
                {
                    PlacePlayers(newDepth, playersStartIndex + 4, bl);
                }
            }
        }
    }

    [ClientRpc]
    public void ClientPlacePlayers(List<Vector2> positions, List<float> rotation_zs)
    {
        if (positions.Count != rotation_zs.Count)
        {
            Debug.LogError("Positions and rotations have mismatching lengths!");
            return;
        }

        for (int i = 0; i < positions.Count; i++)
        {
            PlayerObjectController player = Manager.players[i];
            player.transform.position = positions[i];
            player.transform.rotation = Quaternion.Euler(Vector3.forward * rotation_zs[i]);
        }
    }

    /// <summary>
    /// Checks if index `objectPos` is not -1 in _objects, if so it recursively
    /// searches for a valid index.
    /// </summary>
    /// <returns>The final position of the object, or -1 if no vacancies in _objects.</returns>
    public int AddToGrid(int objectPos, int siblingIndex)
    {
        if (_objects[objectPos] != -1)
        {
            // If there already is an object at given pos, try to put
            // the object on the first different free slot in the array.
            for (int i = 0; (i < _objects.Count) && (i != objectPos); i++)
            {
                // Won't cause an infinite recursion due to the check
                // which ensures this block won't be entered again.
                if (_objects[i] != -1)
                {
                    return AddToGrid(i, siblingIndex);
                }
            }

            Debug.LogError("Grid filled with objects!");
            return -1;
        }
        _objects[objectPos] = siblingIndex;
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
        int siblingIndex = _objects[objectPos];
        if (siblingIndex == -1)
        {
            Debug.LogError("Sibling index == -1!");
            return;
        }
        ClientDestroyObjectAtGridPos(siblingIndex);
        _objects[objectPos] = -1;
    }

    /// <summary>
    /// Deletes an object at a given sibling index, if possible.
    /// </summary>
    [ClientRpc]
    public void ClientDestroyObjectAtGridPos(int siblingIndex)
    {
        GameObject objects = GameObject.Find("Objects");
        if (siblingIndex >= objects.transform.childCount)
        {
            Debug.LogError("Sibling index higher than Objects count!");
            return;
        }
        Destroy(objects.transform.GetChild(siblingIndex).gameObject);
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


    private void SetGameOverScreenActivity(bool active)
    {
        transform.Find("GameOver").gameObject.SetActive(active);
    }


    public void OnGameOver(int score)
    {
        SetGameOverScreenActivity(true);
        Transform gameOver = transform.Find("GameOver");
        gameOver.Find("Lobby").gameObject.SetActive(WorldMode == EWorldMode.Lobby);
        gameOver.Find("Online").gameObject.SetActive(WorldMode == EWorldMode.Online);
        gameOver.Find("Offline").gameObject.SetActive(WorldMode == EWorldMode.Offline);
        gameOver.Find("Score").GetComponent<TextMeshProUGUI>().text = "Score: " + score.ToString();
    }


    public void OnGameOverDecision()
    {
        SetGameOverScreenActivity(false);
    }
}
