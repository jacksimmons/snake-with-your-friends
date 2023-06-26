using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using UnityEditor.Build.Content;
using System.Linq;
using System;

public class CustomNetworkManager : NetworkManager
{
    private readonly string[] gameBehaviourScenes = { "Game" };

    [SerializeField]
    private PlayerObjectController _playerPrefab;
    public List<PlayerObjectController> players { get; } = new List<PlayerObjectController>();

    private string _activeSceneName;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (SceneManager.GetActiveScene().name == "LobbyMenu")
        {
            PlayerObjectController playerInstance = Instantiate(_playerPrefab);
            playerInstance.connectionID = conn.connectionId;
            playerInstance.playerNo = players.Count + 1;
            playerInstance.playerSteamID = (ulong)SteamMatchmaking.GetLobbyMemberByIndex(new CSteamID(SteamLobby.instance.LobbyID), players.Count);

            NetworkServer.AddPlayerForConnection(conn, playerInstance.gameObject);
        }
    }

    public void StartGame(string sceneName)
    {
        ServerChangeScene(sceneName);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _activeSceneName = scene.name;
    }

    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        if (gameBehaviourScenes.Contains(newSceneName))
        {
            StartCoroutine(
                WhenSceneNameIs(
                    newSceneName,
                    () => GameObject.Find("LocalGamePlayer").GetComponentInChildren<GameBehaviour>().OnServerChangeScene(newSceneName)
                )
            );
        }
    }

    /// <summary>
    /// Invokes an Action when scene name is equal to the provided scene name.
    /// </summary>
    private IEnumerator WhenSceneNameIs(string sceneName, Action action)
    {
        while (_activeSceneName != sceneName || !NetworkClient.ready)
        {
            yield return new WaitForEndOfFrame();
        }

        action.Invoke();
        yield return null;
    }
}