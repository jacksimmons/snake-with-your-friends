using Mirror;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class GameBehaviour : NetworkBehaviour
{
    private struct PlayerSpawnInfo
    {
        // Used as a check; ensures the list of players has not changed.
        public ulong SteamID;

        public Vector3 Position;
        public float Rotation_z;


        public PlayerSpawnInfo(ulong steamID, Vector3 position, float rotation_z)
        {
            SteamID = steamID;
            Position = position;
            Rotation_z = rotation_z;
        }
    }

    public enum EGameLoadStage
    {
        Unloaded,
        SceneLoaded,
        GameSettingsSynced,
        OutfitSettingsSynced,
        MapLoaded,
        PlayerScriptsEnabled,
        UIElementsEnabled,
        GameStarted,
    }

    /// <summary>
    /// ====================================
    /// Attribs ----------------------------
    /// ====================================
    /// </summary>
    private static GameBehaviour _instance;
    public static GameBehaviour Instance
    {
        get
        {
            if (!_instance)
            {
                GameObject lpo = GameObject.Find("LocalPlayerObject");
                if (!lpo)
                    return null;
                _instance = lpo.GetComponentInChildren<GameBehaviour>();
            }
            return _instance;
        }
    }

    public static readonly string[] GAME_SCENES =
    { "Game" };

    // --- Puzzle
    public int availablePuzzles = 0;

    // An array of food GameObjects (all foods in this go under the Objects game object parent)
    private static GameObject[] s_foods;

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
    private List<GameObject> _foodTemplates = new();

    // A list containing every food spawn point location (these are defined by the position of FoodSpawner objects)
    private static List<Vector2> s_foodSpawnPoints;


    // SERVER VARIABLES --------------------
    // These are only accurate on the server; hence do NOT use them on a client.
    // They are static because they belong to no particular GameBehaviour.
    // How "loaded" the game currently is for the furthest behind player.
    private static EGameLoadStage serverPlayersLoadingStage;
    private static int serverNumPlayersReady;


    // CLIENT VARIABLES --------------------
    private int m_numOutfitSettingsReceived;


    /// <summary>
    /// ====================================
    /// Methods ----------------------------
    /// ====================================
    /// </summary>
    private void OnEnable()
    {
        if (!isOwned) return;

        if (NetworkServer.active)
        {
            serverNumPlayersReady = 0;
            serverPlayersLoadingStage = EGameLoadStage.Unloaded;
            m_numOutfitSettingsReceived = 0;
        }
    }


    // LOADING STAGES ----------------------
    [Client]
    public void OnGameSceneLoaded(string name)
    {
        if (!isOwned) return;

        CmdOnReady();
    }


    [Command]
    private void CmdOnReady() { ServerOnReady(); }


    /// <summary>
    /// Increments the number of players that are ready in this stage.
    /// All players must be ready before the loading stage is incremented.
    /// </summary>
    [Server]
    private void ServerOnReady()
    {
        serverNumPlayersReady++;

        //print($"Stage: {serverPlayersLoadingStage}");
        //print($"Ready: {serverNumPlayersReady}/{CustomNetworkManager.Instance.numPlayers}");
        if (serverNumPlayersReady >= CustomNetworkManager.Instance.numPlayers)
            ServerOnAllReady();

        if (serverNumPlayersReady != 0) return;
        // ^ Following code executes directly after the last readier, every handshake

        switch (serverPlayersLoadingStage)
        {
            case EGameLoadStage.PlayerScriptsEnabled:
                ServerInitFood();
                ServerPlacePlayers();
                break;
        }
    }


    [Server]
    private void ServerOnAllReady()
    {
        serverNumPlayersReady = 0;

        serverPlayersLoadingStage++;
        RpcLoadingStageUpdate(serverPlayersLoadingStage);
    }


    /// <summary>
    /// The progression through game loading is handled through a series of handshakes.
    /// Each handshake increments the loading stage and calls this function.
    /// They are all called on the host GameBehaviour object, which exists on all clients.
    /// </summary>
    [ClientRpc]
    private void RpcLoadingStageUpdate(EGameLoadStage newValue)
    {
        // Navigate from Host's GB locally -> Your GB locally
        // Call commands from Your GB locally for authority
        // Some commands need a target; this is the gameObject
        // of Your GB locally.
        switch (newValue)
        {
            case EGameLoadStage.Unloaded:
                break;
            case EGameLoadStage.SceneLoaded:
                Instance.CmdRequestGameSettings(Instance.transform.parent.gameObject);
                break;
            case EGameLoadStage.GameSettingsSynced:
                Instance.CmdRequestMap(Instance.transform.parent.gameObject);
                break;
            case EGameLoadStage.OutfitSettingsSynced:
                Instance.CmdSendOutfitSettings(Instance.transform.parent.gameObject);
                break;
            case EGameLoadStage.MapLoaded:
                Instance.EnablePlayerScripts();
                break;
            case EGameLoadStage.PlayerScriptsEnabled:
                Instance.LoadUIElements();
                break;
            case EGameLoadStage.UIElementsEnabled:
                Instance.StartGame();
                break;
        }
    }
    // ------------------------------------


    // GAME SETTINGS HANDSHAKE ------------
    [Command]
    private void CmdRequestGameSettings(GameObject player)
    {
        NetworkIdentity netIdentity = player.GetComponent<NetworkIdentity>();
        RpcReceiveGameSettings(netIdentity.connectionToClient, GameSettings.Saved.Data);
    }


    [TargetRpc]
    private void RpcReceiveGameSettings(NetworkConnectionToClient _, GameSettingsData data)
    {
        if (!isOwned)
        {
            Debug.LogError("Client with authority was recipient of GameSettings.");
            return;
        }
        GameSettings.Saved = new(data);
        CmdOnReady();
    }
    // ------------------------------------


    // CUSTOMISATION HANDSHAKE ------------
    [Command]
    private void CmdSendOutfitSettings(GameObject player)
    {
        int playerNo = player.GetComponent<PlayerObjectController>().playerNo;
        RpcReceiveOutfitSettings(OutfitSettings.Saved.Data, playerNo);
    }


    [ClientRpc]
    private void RpcReceiveOutfitSettings(OutfitSettingsData data, int playerNo)
    {
        m_numOutfitSettingsReceived++;

        // Assign the settings to the player
        CustomNetworkManager.Instance.Players[playerNo-1].GetComponent<PlayerOutfit>().UpdateOutfit(data);

        if (m_numOutfitSettingsReceived == CustomNetworkManager.Instance.Players.Count)
            CmdOnReady();
    }


    [TargetRpc]
    //private void RpcReceiveGameSettings(NetworkConnectionToClient _, GameSettingsData data)
    //{
    //    if (!isOwned)
    //    {
    //        Debug.LogError("Client with authority was recipient of GameSettings.");
    //        return;
    //    }
    //    GameSettings.Saved = new(data);
    //    CmdOnReady();
    //}
    // ------------------------------------


    // MAP HANDSHAKE ----------------------
    [Command]
    private void CmdRequestMap(GameObject player)
    {
        NetworkIdentity netIdentity = player.GetComponent<NetworkIdentity>();
        RpcReceiveMap(netIdentity.connectionToClient, GameSettings.Saved.Data.Map);
    }


    [TargetRpc]
    private void RpcReceiveMap(NetworkConnectionToClient _, MapData mapData)
    {
        if (!isOwned)
        {
            Debug.LogError("Client without authority was recipient of Map.");
            return;
        }

        GameObject map = GameObject.Find("Map");
        s_foodSpawnPoints = map.GetComponent<MapLoader>().LoadMap(mapData);

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


    // GENERATE FOOD HANDSHAKE ------------
    [Server]
    private void ServerInitFood()
    {
        s_foods = new GameObject[s_foodSpawnPoints.Count];

        // Unload food items which were removed in settings
        for (int i = 0; i < _foodTemplates.Count; i++)
        {
            GameObject food = _foodTemplates[i];
            if (!GameSettings.Saved.Data.FoodSettingsData.GetBit((int)food.GetComponent<FoodObject>().food))
            {
                _foodTemplates.Remove(food);
                i--;
            }
        }

        // Generate a food for each player
        foreach (var _ in CustomNetworkManager.Instance.Players)
            ServerGenerateFood();
    }
    // ------------------------------------


    // PLACE PLAYERS HANDSHAKE ------------
    [Server]
    public void ServerPlacePlayers()
    {
        if (GameSettings.Saved.Data.GameMode == EGameMode.SnakeRoyale)
        {
            ServerPlacePlayers_SnakeRoyale();
        }

        int length = CustomNetworkManager.Instance.Players.Count;
        PlayerSpawnInfo[] spawnInfo = new PlayerSpawnInfo[length];
        for (int i = 0; i < length; i++)
        {
            PlayerObjectController player = CustomNetworkManager.Instance.Players[i];
            spawnInfo[i] = new(player.playerSteamID, player.transform.position, player.transform.rotation.eulerAngles.z);
        }

        PlacePlayersClientRpc(spawnInfo);
    }


    [Server]
    private void ServerPlacePlayers_SnakeRoyale()
    {
        // ! This can be optimised with a position stored in the MapData struct
        int playerCount = CustomNetworkManager.Instance.Players.Count;
        List<PlayerObjectController> players = CustomNetworkManager.Instance.Players.GetRange(0, playerCount);

        foreach (MapObjectData obj in GameSettings.Saved.Data.Map.objectData)
        {
            // If the spawn point is necessary (i.e. 4 players only necessitates spawn points up to P4)

            if (obj.spawnIndex >= 0 && obj.spawnIndex < players.Count)
            {
                players[obj.spawnIndex].transform.position = new Vector3(obj.x, obj.y) + s_groundTilemap.cellSize / 2;
            }
        }
    }


    [ClientRpc]
    private void PlacePlayersClientRpc(PlayerSpawnInfo[] allSpawnInfo)
    {
        for (int i = 0; i < allSpawnInfo.Length; i++)
        {
            PlayerSpawnInfo spawnInfo = allSpawnInfo[i];
            PlayerObjectController poc = CustomNetworkManager.Instance.Players[i];

            if (poc.playerSteamID != spawnInfo.SteamID)
            {
                Debug.LogError("Failed to place players - The Players list was altered during this!");
            }

            poc.transform.SetPositionAndRotation(spawnInfo.Position, Quaternion.Euler(Vector3.forward * spawnInfo.Rotation_z));
        }
    }
    // ------------------------------------


    // UI HANDSHAKE------------------------
    [Client]
    private void LoadUIElements()
    {
        PlayerHUDElementsHandler hud = GameObject.FindWithTag("HUD").GetComponent<PlayerHUDElementsHandler>();
        hud.LoadHUD();

        CmdOnReady();
    }
    // ------------------------------------


    // Start Game -------------------------
    [Client]
    private void StartGame()
    {
        foreach (var player in CustomNetworkManager.Instance.Players)
        {
            PlayerMovement pm = player.PM;
            pm.BodyPartContainer.SetActive(true);
        }
    }
    // ------------------------------------


    [Server]
    private void ServerGenerateFood()
    {
        // If no foods are enabled, quick exit.
        // This is not necessarily erroneous, as players can disable all foods.
        if (_foodTemplates.Count == 0)
        {
            Debug.LogWarning("No foods");
            return;
        }

        int randomIndex = Random.Range(0, s_foods.Length);

        if (s_foods[randomIndex] != null)
        {
            int freeIndex = -1;
            for (int i = 0; i < s_foods.Length; i++)
            {
                if (s_foods[i] == null)
                    freeIndex = i;
            }

            if (freeIndex == -1)
            {
                Debug.LogWarning("No free slots for food to spawn in!");
                return;
            }

            randomIndex = freeIndex;
        }

        Vector2 objPos = s_foodSpawnPoints[randomIndex];
        objPos.x += 0.5f;
        objPos.y += 0.5f;

        int foodIndex = Random.Range(0, _foodTemplates.Count);

        GameObject obj = Instantiate(_foodTemplates[foodIndex], objPos, Quaternion.Euler(Vector3.forward * 0));
        obj.transform.parent = GameObject.Find("Objects").transform;

        s_foods[randomIndex] = obj;
        NetworkServer.Spawn(obj);
    }


    [Command]
    public void CmdRemoveFood(GameObject food)
    {
        ServerRemoveFood(food);

        if (GameSettings.Saved.Data.GameMode == EGameMode.SnakeRoyale)
        {
            ServerGenerateFood();
        }
    }


    /// <summary>
    /// Removes a given food GameObject from the food list, and despawns it from the server.
    /// </summary>
    /// <param name="food">The food GameObject to remove.</param>
    [Server]
    private void ServerRemoveFood(GameObject food)
    {
        s_foods[Array.IndexOf(s_foods, food)] = null;
        NetworkServer.UnSpawn(food);
        NetworkServer.Destroy(food);
    }


    ///// <summary>
    ///// Finds the first free slot in s_objects(null slot), populates it with obj, and returns the
    ///// index. Linear Search = O(n)
    //[Server]
    //public int AddObjectToGrid(GameObject obj)
    //{
    //    // Linear search for the first empty slot
    //    int objPos = 0;
    //    while (objPos < s_objects.Length)
    //    {
    //        if (s_objects[objPos] == null)
    //        {
    //            s_objects[objPos] = obj;
    //            return objPos;
    //        }
    //        objPos++;
    //    }
    //    return -1;
    //}


    ///// <summary>
    ///// Checks the given position to see if it is free. If not, searches every slot until it finds a
    ///// free slot. Then populates the slot with obj, returns the index of the slot. Recursive.
    ///// </summary>
    //[Server]
    //public int AddObjectToGrid(int objectPos, GameObject obj)
    //{
    //    if (s_objects[objectPos] != null)
    //    {
    //        // If there already is an object at given pos, try to put
    //        // the object on the first different free slot in the array.
    //        for (int i = 0; (i < s_objects.Length) && (i != objectPos); i++)
    //        {
    //            if (s_objects[i] == null)
    //            {
    //                s_objects[i] = obj;
    //                return i;
    //            }
    //        }

    //        Debug.LogError("Grid filled with objects!");
    //        return -1;
    //    }
    //    s_objects[objectPos] = obj;
    //    return objectPos;
    //}


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

        GameObject hud = GameObject.Find("HUD");
        Transform bg = hud.transform.GetChild(0);
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
