using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.UI;
using TMPro;

public class LobbyController : MonoBehaviour
{
    public static LobbyController instance;

    // UI Elements
    public TextMeshProUGUI lobbyNameLabel;

    // Player Data
    public GameObject playerListViewContent;
    public GameObject playerListItemPrefab;
    public GameObject localPlayerObject;

    // Other Data
    public ulong lobbyID;
    public bool playerItemCreated = false;
    private List<PlayerListItem> _playerListItems = new();
    public PlayerObjectController localPlayerController;

    // Manager
    private CustomNetworkManager _manager;
    private CustomNetworkManager Manager
    {
        get
        {
            if (_manager != null) { return _manager; }
            return _manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }


    private void Awake()
    {
        if (instance == null) { instance = this; }
    }


    public void UpdateLobbyName()
    {
        lobbyID = Manager.GetComponent<SteamLobby>().LobbyID;
        lobbyNameLabel.text = SteamMatchmaking.GetLobbyData(new CSteamID(lobbyID), "name");
    }

    public void UpdatePlayerList()
    {
        // Host
        if (!playerItemCreated) { CreateHostPlayerItem(); }

        if (_playerListItems.Count < Manager.players.Count) { CreateClientPlayerItem(); }

        if (_playerListItems.Count > Manager.players.Count) { RemovePlayerItem(); }

        if (_playerListItems.Count == Manager.players.Count) { UpdatePlayerItem(); }
    }

    public void FindLocalPlayer()
    {
        localPlayerObject = GameObject.Find("LocalGamePlayer");
        localPlayerController = localPlayerObject.GetComponent<PlayerObjectController>();
    }

    public void CreatePlayerItem(PlayerObjectController player)
    {
        print("members: " + SteamMatchmaking.GetNumLobbyMembers(new CSteamID(lobbyID)));

        GameObject newPlayerItem = Instantiate(playerListItemPrefab);
        PlayerListItem newPlayerListItemScript = newPlayerItem.GetComponent<PlayerListItem>();

        newPlayerListItemScript.playerName = player.playerName;
        newPlayerListItemScript.connectionID = player.connectionID;
        newPlayerListItemScript.steamID = player.playerSteamID;
        newPlayerListItemScript.SetPlayerValues();

        newPlayerItem.transform.SetParent(playerListViewContent.transform);
        newPlayerItem.transform.localScale = Vector3.one;

        _playerListItems.Add(newPlayerListItemScript);
    }

    public void CreateHostPlayerItem()
    {
        foreach (PlayerObjectController player in Manager.players)
        {
            CreatePlayerItem(player);
        }

        playerItemCreated = true;
    }

    public void CreateClientPlayerItem()
    {
        foreach (PlayerObjectController player in Manager.players)
        {
            // If any items in the list have the same connection ID
            if (!_playerListItems.Any(b => b.connectionID == player.connectionID))
            {
                CreatePlayerItem(player);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdatePlayerItem()
    {
        foreach (PlayerObjectController player in Manager.players)
        {
            foreach (PlayerListItem playerListItemScript in _playerListItems)
            {
                if (playerListItemScript.connectionID == player.connectionID)
                {
                    playerListItemScript.playerName = player.playerName;
                    playerListItemScript.SetPlayerValues();
                }
            }
        }
    }

    public void RemovePlayerItem()
    {
        List<PlayerListItem> playerListItemsToRemove = new List<PlayerListItem>();

        foreach (PlayerListItem playerListItem in _playerListItems)
        {
            if (!Manager.players.Any(b => b.connectionID == playerListItem.connectionID))
            {
                playerListItemsToRemove.Add(playerListItem);
            }
        }

        foreach (PlayerListItem playerListItemToRemove in playerListItemsToRemove)
        {
            if (playerListItemToRemove != null)
            {
                GameObject objToRemove = playerListItemToRemove.gameObject;
                _playerListItems.Remove(playerListItemToRemove);
                Destroy(objToRemove);
                objToRemove = null;
            }
        }
    }
}