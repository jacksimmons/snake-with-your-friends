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
    public static readonly string[] GAME_MODES =
    { "SnakeRoyale", "Puzzle" };

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

    public enum EWorldSize : int
    {
        Lobby = 10,
        Small = 20,
        Medium = 40,
        Large = 60,
        Massive = 80
    }
    public EWorldSize GroundSize { get; private set; } = EWorldSize.Massive;


    protected virtual void Start()
    {
        // If this is the host object
        if (NetworkServer.active)
        {
            numPlayersReady = 0;
        }
    }


    [Client]
    public virtual void OnGameSceneLoaded(string name)
    {
        if (Array.IndexOf(GAME_SCENES, name) != -1)
            return;

        if (isOwned)
        {
            ClientLoadGame();
            CmdReady();
        }
    }

    [Client]
    protected virtual void ClientLoadGame()
    {
        if ((s_groundTilemap = GameObject.FindWithTag("GroundTilemap").GetComponent<Tilemap>()) != null)
        {
            s_wallTilemap = GameObject.FindWithTag("WallTilemap").GetComponent<Tilemap>();
            return;
        }

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
    protected void CreateGroundTilemap(ref Tilemap groundTilemap, Vector2Int bl)
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
    protected void CreateWallTilemap(ref Tilemap wallTilemap, Vector2Int bl)
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
    protected virtual void CmdReady()
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
    protected virtual void ServerLoadGame() { }


    [Server]
    public virtual void PlacePlayers(int depth, int playersStartIndex, Vector2Int bl) { }


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
    protected void ActivateLocalPlayerClientRpc()
    {
        PlayerMovement pm = GameObject.Find("LocalPlayerObject").GetComponent<PlayerMovement>();
        pm.bodyPartContainer.SetActive(true);
    }


    /// <summary>
    /// Checks if index `objectPos` is not -1 in s_objects, if so it recursively
    /// searches for a valid index.
    /// </summary>
    /// <returns>The final position of the object, or -1 if no vacancies in s_objects.</returns>
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
    public virtual void CmdRemoveFood(int objPos)
    {
        RemoveObjectFromGrid(objPos);
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
