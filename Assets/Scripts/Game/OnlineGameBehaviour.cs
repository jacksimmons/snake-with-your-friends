using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class OnlineGameBehaviour : NetworkBehaviour
{
    public GameBehaviour m_gameBehaviour;
    private bool _alreadyReady = false;
    private int _numPlayersReadyToLoad = 0;

    private CustomNetworkManager _manager;
    private CustomNetworkManager Manager
    {
        get
        {
            if (_manager != null) { return _manager; }
            return _manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }

    public void OnServerChangeScene(string name)
    {
        if (name == "Game" && !_alreadyReady && isOwned)
        {
            OnPlayerReady();
            _alreadyReady = true;
        }
    }

    public void OnPlayerReady()
    {
        _numPlayersReadyToLoad++;
        if (_numPlayersReadyToLoad >= Manager.players.Count)
        {
            CmdLoadGame();
        }
    }

    [Command]
    private void CmdLoadGame()
    {
        ClientLoadGame();
        m_gameBehaviour.SetupObjects();
        CmdGenerateStartingFood();
    }

    [ClientRpc]
    private void ClientLoadGame()
    {
        PlayerMovementController player = GameObject.Find("LocalGamePlayer").GetComponent<PlayerMovementController>();
        GameObject cam = GameObject.FindWithTag("MainCamera");
        cam.GetComponent<CamBehaviour>().Player = player;

        m_gameBehaviour.CreateTilemaps();

        if (isServer)
        {
            List<GameObject> players = new List<GameObject>();
            foreach (PlayerObjectController playerObjectController in Manager.players)
            {
                players.Add(playerObjectController.gameObject);
            }
            m_gameBehaviour.PlacePlayers(depth: 1, playersStartIndex: 0, players);

            List<Vector2> positions = new(Manager.players.Count);
            List<float> rotation_zs = new(Manager.players.Count);
            for (int i = 0; i < Manager.players.Count; i++)
            {
                positions.Add(Manager.players[i].transform.position);
                rotation_zs.Add(Manager.players[i].transform.rotation.eulerAngles.z);
            }
            ClientPlacePlayers(positions, rotation_zs);
        }
    }

    [Command]
    private void CmdGenerateStartingFood()
    {
        for (int i = 0; i < Manager.players.Count; i++)
        {
            this.CmdGenerateFood();
        }
    }

    [Command]
    private void CmdGenerateFood()
    {
        int objectPos = Random.Range(0, m_gameBehaviour.Objects.Length);
        GameObject obj = m_gameBehaviour.GenerateFood(objectPos);

        if (obj == null)
            return;

        NetworkServer.Spawn(obj);
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
            player.transform.SetPositionAndRotation(positions[i], Quaternion.Euler(Vector3.forward * rotation_zs[i]));
        }
    }

    /// <summary>
    /// First gets every client to delete the object, THEN removes it from
    /// the objects array. Because the array is a SyncVar, clients would lose
    /// reference to it otherwise.
    /// </summary>
    [Command]
    public void CmdRemoveObjectFromGrid(int objectPos)
    {
        GameObject go = m_gameBehaviour.Objects[objectPos];

        if (m_gameBehaviour.RemoveObjectFromGrid(objectPos))
        {
            NetworkServer.UnSpawn(go);
            NetworkServer.Destroy(go);
            CmdGenerateFood();
        }
    }
}
