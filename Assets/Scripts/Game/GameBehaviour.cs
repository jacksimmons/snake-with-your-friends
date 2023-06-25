using Extensions;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameBehaviour : MonoBehaviour
{
    [SerializeField]
    private Tile _lightTile;
    [SerializeField]
    private Tile _darkTile;
    [SerializeField]
    private Tile _wallTile;

    private bool[] _objects;

    [SerializeField]
    private GameObject[] _foodTemplates;
    [SerializeField]
    private GameObject _menuSelectTemplate;

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
    private EWorldSize groundSize = EWorldSize.Lobby;

    public enum EWorldMode : int
    {
        None,
        Lobby,
        Offline,
        Online
    }
    public EWorldMode WorldMode = EWorldMode.Online;

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


    void Start()
    {
        SetGameOverScreenActivity(false);

        // Defaults every value to false.
        _objects = new bool[(int)groundSize * (int)groundSize];

        SetupGame();
        GenerateStartingFood();
    }


    private void GenerateStartingFood()
    {
        for (int i = 0; i < Manager.players.Count; i++)
        {
            AddAndInstantiateObjectToGrid(Random.Range(0, _objects.Length), _foodTemplates[Random.Range(0, _foodTemplates.Length)]);
        }
    }


    public void GenerateFood()
    {
        AddAndInstantiateObjectToGrid(Random.Range(0, _objects.Length), _foodTemplates[Random.Range(0, _foodTemplates.Length)]);
    }


    public void SetupGame()
    {
        PlayerMovementController player = GameObject.Find("LocalGamePlayer").GetComponent<PlayerMovementController>();
        GameObject cam = GameObject.FindWithTag("MainCamera");
        cam.GetComponent<CamBehaviour>().SetupCamera(player);

        Tilemap gameGT = CreateAndReturnTilemap(gridName: "Ground", hasCollider: false);
        Tilemap gameWT = CreateAndReturnTilemap(gridName: "Wall", hasCollider: true);

        CreateGroundTilemap(gameGT, bl);
        CreateWallTilemap(gameWT, bl);

        if (WorldMode == EWorldMode.Online || WorldMode == EWorldMode.Offline)
            PlacePlayers(depth: 1, Manager.players, gameGT, bl);
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


    void PlacePlayers(int depth, List<PlayerObjectController> remainingPlayers, Tilemap groundTilemap, Vector2Int bl)
    {
        // Outer snakes (along the walls)
        // Calculate the maximum distance between snakes.
        // If this distance is too small, spawn inner snakes.

        float minDist = (int)groundSize * SOFT_MIN_DIST_WORLD_SIZE_RATIO;
        if (minDist < HARD_MIN_DIST)
            minDist = HARD_MIN_DIST;

        Vector3 BL = groundTilemap.CellToWorld((Vector3Int)(bl + (depth + 1) * Vector2Int.one));
        Vector3 BR = groundTilemap.CellToWorld((Vector3Int)(bl + new Vector2Int((int)groundSize - depth + 1, depth + 1)));
        Vector3 TL = groundTilemap.CellToWorld((Vector3Int)(bl + new Vector2Int(depth + 1, (int)groundSize - depth + 1)));
        Vector3 TR = groundTilemap.CellToWorld((Vector3Int)(bl + ((int)groundSize - depth + 1) * Vector2Int.one));

        Vector3[] corners = { BL, BR, TL, TR };

        for (int i = 0; i < remainingPlayers.Count; i++)
        {
            Manager.players[i].transform.position = corners[i % 4] + (Vector3)(Vector2.one * groundTilemap.cellSize / 2);
            if (i % 4 == 0 && i < remainingPlayers.Count - 1)
            {
                int newDepth = depth + (int)Mathf.Floor(minDist);
                print(newDepth);
                if (newDepth >= (int)groundSize / 2)
                {
                    throw new System.Exception("The players do not fit in the map provided.");
                }
                else
                {
                    PlacePlayers(newDepth, remainingPlayers.GetRange(4, remainingPlayers.Count - 4), groundTilemap, bl);
                }
            }
        }
    }


    /// <summary>
    /// Checks if index `objectPos` is true in objects, if so it recursively
    /// searches for a valid index.
    /// </summary>
    /// <param name="objectPos"></param>
    /// <returns>The final position of the object.</returns>
    public int AddToGrid(int objectPos)
    {
        if (_objects[objectPos])
        {
            // If there already is an object at given pos, try to put
            // the object on the first different free slot in the array.
            for (int i = 0; (i < _objects.Length) && (i != objectPos); i++)
            {
                // Won't cause an infinite recursion due to the check
                // which ensures this block won't be entered again.
                if (!_objects[i])
                {
                    return AddToGrid(i);
                }
            }

            Debug.LogError("Grid filled with objects!");
            return -1;
        }
        _objects[objectPos] = true;
        return objectPos;
    }


    GameObject AddAndInstantiateObjectToGrid(int objectPos, GameObject obj)
    {
        // Overwrite objectPos with the selected objectPos
        objectPos = AddToGrid(objectPos);
        if (objectPos == -1)
            return null;
        else
        {
            Vector2 vectorPos = new Vector2((objectPos % (int)groundSize) + (bl.x + 1.5f), (objectPos / (int)groundSize) + (bl.y + 1.5f));
            Instantiate(obj, (Vector3)vectorPos, obj.transform.rotation);
            obj.GetComponent<FoodBehaviour>().gridPos = objectPos;
            _objects[objectPos] = true;
            return obj;
        }
    }


    public void RemoveFromGrid(int objectPos)
    {
        _objects[objectPos] = false;
    }


    void CreateTeleportingMenuPair(
        string text1, string text2,
        Vector3 from, Vector3 to)
    {
        GameObject menuSelect = Instantiate(_menuSelectTemplate);

        Teleporter teleporter = menuSelect.GetComponentInChildren<Teleporter>();

        teleporter.A.transform.position = from;
        teleporter.A.GetComponentInChildren<TextMeshProUGUI>().text = text1;
        teleporter.B.transform.position = to;
        teleporter.B.GetComponentInChildren<TextMeshProUGUI>().text = text2;
    }


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
