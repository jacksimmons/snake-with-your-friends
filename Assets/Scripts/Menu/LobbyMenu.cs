using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.UI;
using TMPro;

public class LobbyMenu : MonoBehaviour
{
    public static LobbyMenu instance;

    // UI Elements
    public TextMeshProUGUI lobbyNameLabel;

    // GameObjects & Scripts
    public GameObject playerListViewContent;
    public GameObject playerListItemPrefab;

    private PlayerObjectController m_localPlayerController = null;
    public PlayerObjectController LocalPlayerController
    {
        get
        {
            if (m_localPlayerController == null)
            {
                GameObject localPlayerObj = GameObject.Find("LocalPlayerObject");
                if (!localPlayerObj) return null;

                m_localPlayerController = localPlayerObj.GetComponent<PlayerObjectController>();
            }
            return m_localPlayerController;
        }
    }

    // Other Data
    public ulong lobbyID;
    public bool playerItemCreated = false;
    private List<PlayerListItem> _playerListItems = new();

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

    // Ready
    [SerializeField]
    private Button startGameButton;
    [SerializeField]
    private TextMeshProUGUI readyButtonText;

    // Host Settings
    [SerializeField]
    private GameObject m_hostSettingsButton;
    [SerializeField]
    private GameObject m_hostSettingsPanel;


    private void Awake()
    {
        if (instance == null) { instance = this; }

        // Determine if we are the host
        if (NetworkServer.active)
        {
            m_hostSettingsButton.SetActive(true);

            // Load any previous host settings (if there are any)
            GameSettings gameSettings = Saving.LoadFromFile<GameSettings>("GameSettings.dat");
            if (gameSettings != null)
                GameSettings.Saved = gameSettings;

            OutfitSettings outfitSettings = Saving.LoadFromFile<OutfitSettings>("OutfitSettings.dat");
            if (outfitSettings != null)
                OutfitSettings.Saved = outfitSettings;
        }
    }

    public void OnHostSettingsButtonPressed()
    {
        m_hostSettingsPanel.SetActive(true);
    }

    public void OnHostSettingsCloseButtonPressed()
    {
        m_hostSettingsPanel.SetActive(false);
    }

    public void UpdateLobbyName()
    {
        lobbyID = Manager.GetComponent<SteamLobby>().LobbyID;
        lobbyNameLabel.text = SteamMatchmaking.GetLobbyData(new CSteamID(lobbyID), "name");
    }

    public void UpdatePlayerList()
    {
        if (!playerItemCreated) { CreateHostPlayerItems(); }

        if (_playerListItems.Count < Manager.Players.Count) { CreateClientPlayerItems(); }

        if (_playerListItems.Count > Manager.Players.Count) { RemovePlayerItems(); }

        if (_playerListItems.Count == Manager.Players.Count) { UpdatePlayerItems(); }
    }

    public void CreatePlayerItem(PlayerObjectController player)
    {
        print("Members: " + SteamMatchmaking.GetNumLobbyMembers(new CSteamID(lobbyID)));

        GameObject newPlayerItem = Instantiate(playerListItemPrefab);
        PlayerListItem newPlayerListItemScript = newPlayerItem.GetComponent<PlayerListItem>();

        // Host Crown (& other stuff in the future)
        if (player.isHost)
            newPlayerListItemScript.hostCrown.SetActive(true);

        newPlayerListItemScript.playerName = player.playerName;
        newPlayerListItemScript.connectionID = player.connectionID;
        newPlayerListItemScript.steamID = player.playerSteamID;
        newPlayerListItemScript.ready = player.ready;
        newPlayerListItemScript.SetPlayerValues();

        newPlayerItem.transform.SetParent(playerListViewContent.transform);
        newPlayerItem.transform.localScale = Vector3.one;

        _playerListItems.Add(newPlayerListItemScript);
    }

    public void CreateHostPlayerItems()
    {
        foreach (PlayerObjectController player in Manager.Players)
        {
            CreatePlayerItem(player);
        }

        playerItemCreated = true;
    }

    public void CreateClientPlayerItems()
    {
        foreach (PlayerObjectController player in Manager.Players)
        {
            // If any items in the list have the same connection ID
            if (!_playerListItems.Any(b => b.connectionID == player.connectionID))
            {
                CreatePlayerItem(player);
            }
        }
    }

    public void UpdatePlayerItems()
    {
        foreach (PlayerObjectController player in Manager.Players)
        {
            foreach (PlayerListItem playerListItemScript in _playerListItems)
            {
                if (playerListItemScript.connectionID == player.connectionID)
                {
                    playerListItemScript.playerName = player.playerName;
                    playerListItemScript.ready = player.ready;
                    playerListItemScript.SetPlayerValues();

                    // We only want to update buttons individually
                    if (player == LocalPlayerController)
                    {
                        UpdateButton();
                    }
                }
            }
        }

        CheckIfAllReady();
    }

    public void RemovePlayerItems()
    {
        List<PlayerListItem> playerListItemsToRemove = new List<PlayerListItem>();

        foreach (PlayerListItem playerListItem in _playerListItems)
        {
            if (!Manager.Players.Any(b => b.connectionID == playerListItem.connectionID))
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

    public void TogglePlayerReady()
    {
        LocalPlayerController.CmdSetPlayerReady();
    }

    public void UpdateButton()
    {
        if (LocalPlayerController.ready)
        {
            readyButtonText.text = "Not Ready";
        }
        else
        {
            readyButtonText.text = "Ready";
        }
    }

    public void CheckIfAllReady()
    {
        bool allReady = true;
        foreach (PlayerObjectController player in Manager.Players)
        {
            if (!player.ready)
            {
                allReady = false;
                break;
            }
        }

        startGameButton.interactable = false;

        // If everyone is ready and we are the host...
        if (allReady)
        {
            if (LocalPlayerController && LocalPlayerController.playerNo == 1)
            {
                if (Manager.Players.Count > 1 || Manager.singleplayer)
                    startGameButton.interactable = true;
            }
        }
    }

    public void OnStartGamePressed()
    {
        GameObject lpo = GameObject.Find("LocalPlayerObject");
        GameObject game = lpo.transform.Find("Game").gameObject;
        Destroy(game.GetComponent<GameBehaviour>());

        switch (GameSettings.Saved.GameMode)
        {
            case "SnakeRoyale":
                game.AddComponent<SnakeRoyaleBehaviour>();
                break;
            case "Puzzle":
                game.AddComponent<PuzzleBehaviour>();
                break;
        }

        LocalPlayerController.CmdStartGame();
    }
}