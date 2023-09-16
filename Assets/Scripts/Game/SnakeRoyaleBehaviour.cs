using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class SnakeRoyaleBehaviour : GameBehaviour
{
    // Soft limit is preferred, but if it is too small, the hard limit is used (1 tile).
    // The minimum ratio between the distance between two snakes, and the WORLD_SIZE, before an inner square must be established.
    private const float SOFT_MIN_DIST_WORLD_SIZE_RATIO = 0.2f;
    private const float HARD_MIN_DIST = 2f;

    [SerializeField]
    private List<GameObject> _foodTemplates = new();


    [Client]
    protected override void ClientLoadGame()
    {
        base.ClientLoadGame();

        PlayerMovement player = GameObject.Find("LocalPlayerObject").GetComponent<PlayerMovement>();
        player.enabled = true;

        GameObject cam = GameObject.FindWithTag("MainCamera");
        cam.GetComponent<CamBehaviour>().Player = player;
    }


    [Server]
    protected override void ServerLoadGame()
    {
        base.ServerLoadGame();

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

        PlacePlayers(depth: 1, playersStartIndex: 0, Vector2Int.zero);

        List<Vector2> positions = new(CustomNetworkManager.Instance.Players.Count);
        List<float> rotation_zs = new(CustomNetworkManager.Instance.Players.Count);
        for (int i = 0; i < CustomNetworkManager.Instance.Players.Count; i++)
        {
            positions.Add(CustomNetworkManager.Instance.Players[i].transform.position);
            rotation_zs.Add(CustomNetworkManager.Instance.Players[i].transform.rotation.eulerAngles.z);
        }
        PlacePlayersClientRpc(positions, rotation_zs);
        ActivateLocalPlayerClientRpc();

        s_objects = new GameObject[(int)GroundSize * (int)GroundSize];
        GenerateStartingFood();
    }


    [Server]
    public override void PlacePlayers(int depth, int playersStartIndex, Vector2Int bl)
    {
        // Outer snakes (along the walls)
        // Calculate the maximum distance between snakes.
        // If this distance is too small, spawn inner snakes.

        int playersCount = 0;
        if (CustomNetworkManager.Instance.Players.Count - playersStartIndex > 0)
        {
            playersCount = CustomNetworkManager.Instance.Players.Count - playersStartIndex;
        }
        List<PlayerObjectController> players = CustomNetworkManager.Instance.Players.GetRange(playersStartIndex, playersCount);

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
        Vector2 foodPos = new((objectPos % (int)GroundSize) + (1.5f), (objectPos / (int)GroundSize) + (1.5f));

        GameObject obj = Instantiate(_foodTemplates[foodIndex], foodPos, Quaternion.Euler(Vector3.forward * 0));
        obj.GetComponent<GridObject>().gridPos = objectPos;

        if (AddObjectToGrid(objectPos, obj) != -1)
        {
            NetworkServer.Spawn(obj);
        }
    }


    [Command]
    public override void CmdRemoveFood(int objPos)
    {
        base.CmdRemoveFood(objPos);
        GenerateFood();
    }
}