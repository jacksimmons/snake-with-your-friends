using Mirror;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
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

    private enum EGameLoadStage
    {
        Unloaded,
        SceneLoaded,
        GameSettingsSynced,
        OutfitSettingsSynced,
        MapLoaded,
        PlayerScriptsEnabled,
        UIElementsEnabled,
        Loaded,
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


    // SERVER VARIABLES --------------------
    // These are only accurate on the server; hence do NOT use them on a client.
    // They are static because they belong to no particular GameBehaviour.

    // How "loaded" the game currently is for the furthest behind player.
    private static EGameLoadStage s_serverPlayersLoadingStage;
    private static int s_serverNumPlayersReady;

    // An array of food GameObjects (all foods in this go under the Objects game object parent)
    private static GameObject[] s_foods;
    // A list containing every food spawn point location (these are defined by the position of FoodSpawner objects)
    private static List<Vector2> s_foodSpawnPoints;

    // Tilemaps representing the tiles of the current map.
    protected static Tilemap s_groundTilemap;
    protected static Tilemap s_wallTilemap;

    // Global clock system so players move at the same time
    private static float s_timeSinceLastTick;

    private static float s_playerSpeedMultiplier = 1;
    private static float s_defaultTicksBetweenMoves;

    public static float TicksBetweenMoves
    {
        get
        {
            if (s_playerSpeedMultiplier == 0) return 0;
            return s_defaultTicksBetweenMoves / s_playerSpeedMultiplier;
        }
    }


    // CLIENT VARIABLES --------------------
    [SerializeField]
    private List<GameObject> _foodTemplates = new();

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
            s_serverNumPlayersReady = 0;
            s_serverPlayersLoadingStage = EGameLoadStage.Unloaded;
            m_numOutfitSettingsReceived = 0;
            s_timeSinceLastTick = 0;
            s_defaultTicksBetweenMoves = GameSettings.Saved.Data.TimeToMove;
        }
    }


    private void FixedUpdate()
    {
        if (!isOwned) return;

        if (s_serverPlayersLoadingStage == EGameLoadStage.Loaded)
            ServerFixedUpdate();
    }


    [Server]
    private void ServerFixedUpdate()
    {
        s_timeSinceLastTick += Time.fixedDeltaTime;

        if (s_timeSinceLastTick < TicksBetweenMoves)
            return;

        // Perform tick
        s_timeSinceLastTick = 0;

        RpcExecuteTick();
    }


    [ClientRpc]
    private void RpcExecuteTick()
    {
        GetComponentInParent<PlayerMovement>().HandleMovementLoop();
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
        s_serverNumPlayersReady++;

        //print($"Stage: {serverPlayersLoadingStage}");
        //print($"Ready: {serverNumPlayersReady}/{CustomNetworkManager.Instance.numPlayers}");
        if (s_serverNumPlayersReady >= CustomNetworkManager.Instance.numPlayers)
            ServerOnAllReady();

        if (s_serverNumPlayersReady != 0) return;
    }


    [Server]
    private void ServerLoadingStageUpdate()
    {
        // Server-side loading stages (these will invoke client-side loading stages)
        switch (s_serverPlayersLoadingStage)
        {
            case EGameLoadStage.SceneLoaded:
                Instance.ServerBroadcastGameSettings(Instance.transform.parent.gameObject);
                break;
            case EGameLoadStage.GameSettingsSynced:
                Instance.ServerBroadcastMap(Instance.transform.parent.gameObject);
                break;
            case EGameLoadStage.OutfitSettingsSynced:
                Instance.ServerAllBroadcastOutfit(Instance.transform.parent.gameObject);
                break;
            case EGameLoadStage.PlayerScriptsEnabled:
                ServerInitFood();
                ServerPlacePlayers();
                break;
        }
    }


    [Server]
    private void ServerOnAllReady()
    {
        s_serverNumPlayersReady = 0;

        s_serverPlayersLoadingStage++;
        ServerLoadingStageUpdate();
        RpcLoadingStageUpdate(s_serverPlayersLoadingStage);
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

        // Client-side loading stages
        switch (newValue)
        {
            case EGameLoadStage.Unloaded:
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
    [Server]
    private void ServerBroadcastGameSettings(GameObject player)
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


    // MAP HANDSHAKE ----------------------
    [Server]
    private void ServerBroadcastMap(GameObject player)
    {
        NetworkIdentity netIdentity = player.GetComponent<NetworkIdentity>();

        MapData map;
        if (GameSettings.Saved.Data.GameMode == EGameMode.Puzzle)
            map = Saving.LoadFromResources<MapData>($"Maps/Puzzle{SaveData.Saved.PuzzleLevel}");
        else
            map = GameSettings.Saved.Data.Map;


        if (map.groundData.Length == 0 && map.wallData.Length == 0 && map.objectData.Length == 0)
        {
            Debug.LogWarning("No map selected, or map was corrupted. Loading default map.");
            map = Saving.LoadFromResources<MapData>("Maps/DefaultMap");

            // Update the saved map
            GameSettings.Saved.Data.Map = map;
        }

        RpcReceiveMap(netIdentity.connectionToClient, map);
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


    // CUSTOMISATION HANDSHAKE ------------
    [Server]
    private void ServerAllBroadcastOutfit(GameObject player)
    {
        NetworkIdentity netIdentity = player.GetComponent<NetworkIdentity>();

        foreach (PlayerObjectController poc in CustomNetworkManager.Instance.Players)
        {
        }

        int playerNo = player.GetComponent<PlayerObjectController>().playerNo;
        RpcReceiveOutfitSettings(OutfitSettings.Saved.Data, playerNo);

        RpcBroadcastOutfit(netIdentity.connectionToClient); 
    }


    [TargetRpc]
    private void RpcBroadcastOutfit(NetworkConnectionToClient _)
    {
    }


    [ClientRpc]
    private void RpcReceiveOutfitSettings(OutfitSettingsData data, int playerNo)
    {
        m_numOutfitSettingsReceived++;

        // Assign the settings to the player
        CustomNetworkManager.Instance.Players[playerNo - 1].GetComponent<PlayerOutfit>().UpdateOutfit(data);

        if (m_numOutfitSettingsReceived == CustomNetworkManager.Instance.Players.Count)
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
            if (!GameSettings.Saved.FoodSettings.GetBit((int)food.GetComponent<FoodObject>().food))
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
        // Place each player on their corresponding spawn point
        List<PlayerObjectController> players = CustomNetworkManager.Instance.Players;

        foreach (MapObjectData obj in GameSettings.Saved.Data.Map.objectData)
        {
            // If the spawn point is necessary (i.e. 4 players only necessitates spawn points up to P4)
            if (obj.spawnIndex >= 0 && obj.spawnIndex < players.Count)
            {
                players[obj.spawnIndex].transform.position = new Vector3(obj.x, obj.y) + s_groundTilemap.cellSize / 2;
            }
        }

        // Send the spawn data to clients
        int length = CustomNetworkManager.Instance.Players.Count;
        PlayerSpawnInfo[] spawnInfo = new PlayerSpawnInfo[length];
        for (int i = 0; i < length; i++)
        {
            PlayerObjectController player = CustomNetworkManager.Instance.Players[i];
            spawnInfo[i] = new(player.playerSteamID, player.transform.position, player.transform.rotation.eulerAngles.z);
        }

        PlacePlayersClientRpc(spawnInfo);
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

            // Enable puzzle movement if needed
            if (GameSettings.Saved.Data.GameMode == EGameMode.Puzzle)
            {
                pm.FreeMovement = true;
            }
        }

        CmdOnReady();
    }
    // ------------------------------------


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


    [Server]
    private void ServerGenerateFood()
    {
        // If no foods are enabled, quick exit.
        // This is not necessarily erroneous, as players can disable all foods.
        if (_foodTemplates.Count == 0)
        {
            Debug.LogWarning("No foods enabled in GameSettings.");
            return;
        }


        // If no food spawn points exist, also quick exit.
        // This allows maps to be created with no food spawn points.
        if (s_foods.Length == 0)
        {
            Debug.LogWarning("No food spawn points.");
            return;
        }


        int randomIndex = PickRandomFoodSpawnIndex();
        bool enterStalemate = true;
        for (int i = 0; i < s_foodSpawnPoints.Count; i++)
        {
            int nextSnakeFreeIndex = (randomIndex + i) % s_foodSpawnPoints.Count;

            Vector2 objPos = s_foodSpawnPoints[nextSnakeFreeIndex];
            objPos.x += 0.5f;
            objPos.y += 0.5f;

            // Go to next index (if snake is on the spawn point)
            // If this happens for every index, `enterStalemate` is true
            if (IsSnakeOnFoodSpawn(objPos)) continue;

            int foodIndex = Random.Range(0, _foodTemplates.Count);

            GameObject obj = Instantiate(_foodTemplates[foodIndex], objPos, Quaternion.Euler(Vector3.forward * 0));
            s_foods[nextSnakeFreeIndex] = obj;
            obj.transform.parent = GameObject.Find("Objects").transform;

            NetworkServer.Spawn(obj);
            enterStalemate = false;
            break;
        }

        if (enterStalemate)
            print("STALEMATE");
    }


    /// <summary>
    /// Attempts to pick a random food spawn point index to spawn food in.
    /// If the spawn point is already occupied by a food, return the next available slot.
    /// If there are no available slots, then STALEMATE mode begins.
    /// </summary>
    /// <returns></returns>
    private int PickRandomFoodSpawnIndex()
    {
        int randomIndex = Random.Range(0, s_foods.Length);

        // If the food spawn point is already occupied by a food...
        if (s_foods[randomIndex] != null)
        {
            int nextFreeIndex = -1;
            for (int i = 0; i < s_foods.Length; i++)
            {
                if (s_foods[i] == null)
                    nextFreeIndex = i;
            }

            if (nextFreeIndex == -1)
            {
                // ! This shouldn't happen unless I have fucked up
                Debug.LogWarning("No free slots for food to spawn in!");
                return -1;
            }

            randomIndex = nextFreeIndex;
        }

        return randomIndex;
    }


    private bool IsSnakeOnFoodSpawn(Vector2 spawnPos)
    {
        foreach (PlayerObjectController poc in CustomNetworkManager.Instance.Players)
        {
            foreach (BodyPart bp in poc.PM.BodyParts)
            {
                if (Extensions.Vectors.Approximately(bp.Position, spawnPos))
                {
                    return true;
                }
            }
        }
        return false;
    }


    [Command]
    public void CmdSetTimeToMove(float timeToMove)
    {
        // We need TimeBetweenMoves = timeToMove
        // We have TimeBetweenMoves = baseTimeToMove/{multiplier}
        // So multiplier = timeToMove/baseTimeToMove
        ServerSetSpeedMultiplier(timeToMove / s_defaultTicksBetweenMoves);
    }


    [Command]
    public void CmdSetSpeedMultiplier(float multiplier) { ServerSetSpeedMultiplier(multiplier); }


    [Command]
    public void CmdResetSpeedMultiplier() { ServerSetSpeedMultiplier(1); }


    [Server]
    private void ServerSetSpeedMultiplier(float multiplier)
    {
        s_playerSpeedMultiplier = multiplier;
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


    public void OnPuzzleComplete()
    {
        SetPuzzleCompleteActivity(true);
    }


    private void SetPuzzleCompleteActivity(bool active)
    {
        if (!isOwned)
            return;

        GameObject hud = GameObject.Find("HUD");
        Transform bg = hud.transform.GetChild(0);
        Transform puzzleComplete = bg.Find("PuzzleComplete");
        puzzleComplete.gameObject.SetActive(active);

        if (active)
        {
            puzzleComplete.Find("Level").GetComponent<TMP_Text>().text = $"Level: {SaveData.Saved.PuzzleLevel+1}/{SaveData.MaxPuzzleLevel}";
            Button btn = puzzleComplete.Find("NextButton").GetComponent<Button>();
            btn.onClick.RemoveAllListeners();

            if (SaveData.Saved.PuzzleLevel + 1 == SaveData.MaxPuzzleLevel)
            {
                btn.onClick.AddListener(() => GetComponentInParent<PlayerMovement>().HandleDeath());
                btn.GetComponentInChildren<TMP_Text>().text = "Click here for a pat on the back!";
            }
            else
            {
                // Increment highest puzzle and save
                SaveData.Saved.PuzzleLevel++;
                Saving.SaveToFile(SaveData.Saved, "SaveData.json");

                // When the button is clicked, load the next puzzle
                btn.onClick.AddListener(() => GetComponentInParent<PlayerMovement>().HandleDeath());
                btn.GetComponentInChildren<TMP_Text>().text = "Back to lobby";
            }
        }
    }


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
}
