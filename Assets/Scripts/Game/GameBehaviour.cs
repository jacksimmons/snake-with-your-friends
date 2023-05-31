using Extensions;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField]
    private GameObject[] _players;
    private bool[] _objects;

    [SerializeField]
    private GameObject[] _foodTemplates;
    [SerializeField]
    private GameObject _menuSelectTemplate;

    public enum E_WorldSize : int
    {
        LOBBY = 10,
        GAME_SMALL = 15,
        GAME_MEDIUM = 30,
        GAME_LARGE = 45
    }
    [SerializeField]
    private E_WorldSize groundSize;

    Vector2Int bl = Vector2Int.zero;

    // Soft limit is preferred, but if it is too small, the hard limit is used (1 tile).
    // The minimum ratio between the distance between two snakes, and the WORLD_SIZE, before an inner square must be established.
    private const float SOFT_MIN_DIST_WORLD_SIZE_RATIO = 0.2f;
    private const float HARD_MIN_DIST = 10f;

    void Start()
    {
        groundSize = E_WorldSize.LOBBY;

        // Defaults every value to false.
        _objects = new bool[(int)groundSize * (int)groundSize];

        GenerateStartingFood();
    }

    private void GenerateStartingFood()
    {
        for (int i = 0; i < _players.Length; i++)
        {
            AddObjectToGrid(Random.Range(0, _objects.Length), _foodTemplates[Random.Range(0, _foodTemplates.Length)]);
        }
    }

    public void GenerateFood()
    {
        AddObjectToGrid(Random.Range(0, _objects.Length), _foodTemplates[Random.Range(0, _foodTemplates.Length)]);
    }

    public void SetupGame(GameObject[] players, PlayerBehaviour localP1)
    {
        _players = players;
        GameObject cam = GameObject.FindWithTag("MainCamera");
        print("hi");
        cam.GetComponent<CamBehaviour>().SetupCamera(localP1);

        Tilemap gameGT = CreateAndReturnTilemap(gridName: "Ground", hasCollider: false);
        Tilemap gameWT = CreateAndReturnTilemap(gridName: "Wall", hasCollider: true);

        CreateGroundTilemap(gameGT, bl);
        CreateWallTilemap(gameWT, bl);
        
        PlaceSnakes(1, _players, gameGT, bl);
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
        print(bounds.size);
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

    void PlaceSnakes(int depth, GameObject[] remainingPlayers, Tilemap groundTilemap, Vector2Int bl)
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

        for (int i = 0; i < remainingPlayers.Length; i++)
        {
            _players[i].transform.position = corners[i % 4] + (Vector3)(Vector2.one * groundTilemap.cellSize / 2);
            if (i % 4 == 0 && i < remainingPlayers.Length - 1)
            {
                int newDepth = depth + (int)Mathf.Floor(minDist);
                print(newDepth);
                if (newDepth >= (int)groundSize / 2)
                {
                    throw new System.Exception("The players do not fit in the map provided.");
                }
                else
                {
                    PlaceSnakes(newDepth, Arrays.SubArray(remainingPlayers, 4), groundTilemap, bl);
                }
            }
        }
    }

    GameObject AddObjectToGrid(int objectPos, GameObject obj)
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
                    return AddObjectToGrid(i, obj);
                }
            }

            Debug.LogError("Grid filled with objects!");
            return null;
        }
        else
        {
            Vector2 vectorPos = new Vector2((objectPos % (int)groundSize) + (bl.x + 1.5f), (objectPos / (int)groundSize) + (bl.y + 1.5f));
            Instantiate(obj, (Vector3)vectorPos, obj.transform.rotation);
            obj.GetComponent<FoodBehaviour>().gridPos = objectPos;
            _objects[objectPos] = true;
            return obj;
        }
    }

    public void RemoveObjectFromGrid(int gridPos)
    {
        _objects[gridPos] = false;
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
}
