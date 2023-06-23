using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using TMPro;
using UnityEditor.PackageManager.Requests;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Extensions;

// Channels are used for different types:
// 0 - Update
// 1 - FixedUpdate
// 2 - Console Output

/// <summary>
/// Server-side lobby management.
/// </summary>
public class Lobby : MonoBehaviour
{
    [SerializeField]
    private GameObject _lobbyEntryTemplate;
    [SerializeField]
    private GameObject _snakeTemplate;

    [SerializeField]
    private Counter _counter;

    // Other Data
    public PlayerBehaviour Player { get; private set; }

    // User Data
    public CSteamID Id { get; private set; } = CSteamID.Nil;

    // Lobby Data
    private enum Channel : int
    {
        Default,
        Physics,
        Console
    }
	
    private enum LobbyState
    {
        NotInOne,
        InLobbyMenu,
        InGame
    }
	
    private CSteamID _lobbyId = CSteamID.Nil;
	
	private bool _isOwner = false;
    public bool IsOwner 
	{
		get { return _isOwner; }
		set 
		{
			_counter.Paused = !value;
			_isOwner = value;
		}
	}
	
    private LobbyState _lobbyState = LobbyState.NotInOne;
    private Dictionary<CSteamID, string> _lobbyNames = new();
	
	public Dictionary<CSteamID, PlayerBehaviour> LobbyPlayers { get; private set; } = new();

    // Packet data
    private IntPtr _sendBuf = Marshal.AllocHGlobal(65536);
    private const int _MAX_MESSAGES = 16;
    private IntPtr[] _receiveBufs = new IntPtr[_MAX_MESSAGES];

    // SteamAPI

    protected Callback<SteamNetworkingMessagesSessionFailed_t> m_SteamNetworkingMessagesSessionFailed;


    // Start is called before the first frame update
    private void Awake()
    {
        if (SteamManager.Initialized)
        {

            m_SteamNetworkingMessagesSessionFailed = Callback<SteamNetworkingMessagesSessionFailed_t>.Create(OnMessageSessionFailed);

            Id = SteamUser.GetSteamID();
            DontDestroyOnLoad(this);
        }
    }


    private void Update()
    {
		// Handle all messages directed to Update.
        ReceiveMessages(Channel.Default);
        ReceiveMessages(Channel.Console, true);
    }


    private void FixedUpdate()
    {
		// Handle all messages directed to FixedUpdate.
        ReceiveMessages(Channel.Physics);
    }


    public void SetupOffline(PlayerBehaviour player)
    {
        IsOwner = true;
        Player = player;
    }


    public void PlayerLoaded()
    {
        if (!IsOwner)
        {
            SendMessageToUser(SteamMatchmaking.GetLobbyOwner(_lobbyId), Bytes.ToBytes("player_loaded"), 0);
        }
        else
        {
            //if (_playersLoaded == _lobbyNames.Keys.Count)
            //{
            //    SendMessagesTo(CSteamID.Nil, "all_players_loaded", null, Channel.Default);
            //    SendMessagesTo(CSteamID.Nil, "All players have loaded successfully.", null, Channel.Console);
            //}
        }
    }


    /// <summary>
    /// The global counter threshold handles all players with default move speed.
    /// </summary>
    private void OnCounterThresholdReached()
    {
        // Call all other player movement loops with default movement speed.
        foreach (var kvp in LobbyPlayers)
        {
            if (kvp.Key != Id)
                if (Mathf.Approximately(kvp.Value.MovementSpeed, PlayerBehaviour.DEFAULT_MOVEMENT_SPEED))
				{
					print("aaaa");
                    SendMessageToUser(kvp.Key, Bytes.ToBytes("move_timer"), Channel.Physics);
					SendMessageToUser(kvp.Key, Bytes.ToBytes("move_timer"), Channel.Console);
				}
		}

        // Call our own player's movement loop if it has default movement speed
        if (Mathf.Approximately(Player.MovementSpeed, PlayerBehaviour.DEFAULT_MOVEMENT_SPEED))
            OnMoveTimerReceived();
    }


    /// <summary>
    /// Player counter thresholds handle individual players with non-standard move
    /// speed.
    /// </summary>
    /// <param name="mover">The player with a custom threshold that got triggered.</param>
    /// 
    private void OnCustomCounterThresholdReached(CSteamID mover)
    {
        if (mover == Id)
            if (Player.MovementSpeed != PlayerBehaviour.DEFAULT_MOVEMENT_SPEED)
                OnMoveTimerReceived();
        else
            SendMessageToUser(mover, Bytes.ToBytes("move_timer"), Channel.Physics);
    }


    /// <summary>
    /// Raw send procedure, used by SendMessageTo.
    /// Sends a message to one user.
    /// </summary>
    private void SendMessageToUser(CSteamID cSteamID, byte[] message, Channel channel)
    {
        // Don't need to waste time clearing the buffer; only message.Length
        // bytes of it are going to be used.
        string name = SteamFriends.GetFriendPersonaName(cSteamID);
        Marshal.Copy(message, 0, _sendBuf, message.Length);

        SteamNetworkingIdentity identity = new SteamNetworkingIdentity();
        identity.SetSteamID(cSteamID);
        EResult result = SteamNetworkingMessages.SendMessageToUser(ref identity, _sendBuf, (uint)message.Length, 0, (int)channel);

        BodyPartData bp_data = Bytes.FromBytes<BodyPartData>(message);
        string str_data = Bytes.FromBytes(message);

        switch (result)
        {
            case EResult.k_EResultOK:
                break;
            default:
                break;
        }
    }


    /// <summary>
    /// Sends a message to one or all users.
    /// </summary>
    /// <param name="target">Either a valid CSteamID, or CSteamID.Nil to send to all.</param>
    /// <param name="messages">The bytes representation of the messages to send.</param>
    /// <param name="channel">The channel to send the message on.</param>
    private void SendMessagesTo(CSteamID target, string title, List<byte[]> messages, Channel channel)
    {
        try
        {
            if (target == CSteamID.Nil)
            {
                foreach (CSteamID id in _lobbyNames.Keys)
                    if (id != Id)
                    {
                        SendMessageToUser(id, Bytes.ToBytes(title), channel);
                        if (messages != null)
                        {
                            foreach (byte[] message in messages)
                                SendMessageToUser(id, message, channel);
                        }
                    }
            }
            else if (target != Id)
            {
                SendMessageToUser(target, Bytes.ToBytes(title), channel);
                if (messages != null)
                {
                    foreach (byte[] message in messages)
                        SendMessageToUser(target, message, channel);
                }
            }
        }
        catch
        {
            Marshal.FreeHGlobal(_sendBuf);
        }
    }


    private void SendBodyPartData()
    {
        List<byte[]> msgs = new List<byte[]>();
        foreach (BodyPart bp in Player.BodyParts)
            msgs.Add(Bytes.ToBytes(bp.ToData()));
        SendMessagesTo(CSteamID.Nil, "bp_data", msgs, Channel.Physics);
    }


    public void SetPlayerMovementSpeed(CSteamID id, float value)
    {
        if (IsOwner)
        {
            if (value != PlayerBehaviour.DEFAULT_MOVEMENT_SPEED && value != Player.MovementSpeed)
            {
                // Remove existing custom counter if there is one
                // Thus, custom counters are only cleaned up when the next custom counter is requested.
                if (_counter.PlayerCounters.Keys.Contains(id))
                    _counter.RemovePlayerCounter(id);
                _counter.AddPlayerCounter(id, value, _counter.Cnt);
            }
            else if (value == PlayerBehaviour.DEFAULT_MOVEMENT_SPEED)
            {
                if (_counter.PlayerCounters.Keys.Contains(id))
                    _counter.RemovePlayerCounter(id);
            }
        }
        else
        {
            SendMovementSpeedUpdateData(value);
        }
    }


    public void SendMovementSpeedUpdateData(float movementSpeed)
    {
        List<byte[]> data = new();
        byte[] movement_speed = Bytes.ToBytes(movementSpeed);
        data.Add(movement_speed);
        SendMessagesTo(SteamMatchmaking.GetLobbyOwner(_lobbyId), "movement_speed_update", data, Channel.Physics);
    }


    /// <summary>
    /// Receives messages from other users.
    /// The first message is ALWAYS a string with the name of the message.
    /// The other messages following it are optional data, determined by the message name.
    /// </summary>
    /// <param name="channel">The channel to receive messages from.</param>
    /// <param name="outputMessage">Is the message to be outputted to the console?</param>
    private void ReceiveMessages(Channel channel, bool outputMessage = false)
    {
        int messageCount = SteamNetworkingMessages.ReceiveMessagesOnChannel((int)channel, _receiveBufs, _MAX_MESSAGES);
        string message = "none";
        for (int i = 0; i < messageCount; i++)
        {
            print("message" + i);
            try
            {
                SteamNetworkingMessage_t netMessage = Marshal.PtrToStructure<SteamNetworkingMessage_t>(_receiveBufs[i]);
                byte[] data = new byte[netMessage.m_cbSize];
                Marshal.Copy(netMessage.m_pData, data, 0, data.Length);
                CSteamID sender = netMessage.m_identityPeer.GetSteamID();

                if (i == 0)
                {
                    message = Bytes.FromBytes(data);
                    if (outputMessage)
                        print(message);
                }

                switch (message)
                {
                    case "move_timer":
                        if (IsOwner)
                            Debug.LogError("Owner should never receive a move_timer packet!");
                        else
                            OnMoveTimerReceived();
                        break;
                    case "player_loaded":
                        if (IsOwner) { }
                        else
                            Debug.LogError("player_loaded sent to a non-owner!");
                        break;
                    case "bp_data":
                        if (i > 0)
                        {
                            // Every i > 0 is a new BodyPart.
                            print(i);
                            PlayerBehaviour player = LobbyPlayers[sender];
                            BodyPartData bpData = Bytes.FromBytes<BodyPartData>(data);
                            player.BodyParts[i - 1].FromData(bpData);
                        }
                        break;
                    case "movement_speed_update":
                        break;
                    case "none":
                        print("none");
                        break;
                    default:
                        print(message);
                        break;
                }
            }
            finally
            {
                Marshal.DestroyStructure<SteamNetworkingMessage_t>(_receiveBufs[i]);
            }
        }
    }


    private void OnMoveTimerReceived()
    {
        Player.HandleMovementLoop();
        SendBodyPartData();
    }


    // Lobby Menu
    public void OnBackPressed()
    {
        if (_lobbyId != CSteamID.Nil)
        {
            SteamMatchmaking.LeaveLobby(_lobbyId);
            print("Left the lobby.");
        }
        Destroy(gameObject);
        SceneManager.LoadScene("MainMenu");
    }


    //public void JoinLobby(CSteamID id)
    //{
    //    SteamAPICall_t handle = SteamMatchmaking.JoinLobby(id);
    //    m_LobbyEnter.Set(handle);
    //}


    private void OnMessageSessionFailed(SteamNetworkingMessagesSessionFailed_t pCallback)
    {
        SteamNetConnectionInfo_t info = pCallback.m_info;
    }


    private void CreatePlayer(CSteamID id)
    {
        // Need to do all but finding PlayerParent locally, and not with Find,
        // else other already created players may ping up in the search.
        string name = SteamFriends.GetFriendPersonaName(id);
        GameObject playerParent = GameObject.FindWithTag("PlayerParent");
        GameObject snake = Instantiate(_snakeTemplate, playerParent.transform);
        TextMeshProUGUI text = snake.transform.Find("Name").GetComponent<TextMeshProUGUI>();
        text.text = name;

        LobbyPlayers[id] = snake.GetComponentInChildren<PlayerBehaviour>();

        if (id == Id)
        {
            Player = LobbyPlayers[id];
        }
    }


    //private IEnumerator LoadLobby()
    //{
    //    _lobbyState = LobbyState.InLobbyMenu;
    //    AsyncOperation loadLobbyMenuComplete = SceneManager.LoadSceneAsync("LobbyMenu");

    //    while (!loadLobbyMenuComplete.isDone)
    //    {
    //        yield return new WaitForSeconds(1);
    //    }
    //    AddAllLobbyMembers();

    //    foreach (Transform child in GameObject.FindWithTag("PlayerParent").transform)
    //    {
    //        child.Find("Player").GetComponent<PlayerBehaviour>().InLobbyMenu = true;
    //    }

    //    GameObject.FindWithTag("MainCamera").GetComponent<CamBehaviour>().SetupCamera(Player);

    //    yield break;
    //}


    public Dictionary<string, string> GetLobbyDebug()
    {
        Dictionary<string, string> lobbyValues;
        if (SteamManager.Initialized)
        {
            lobbyValues = new Dictionary<string, string>
            {
                { "Steam Name", SteamFriends.GetPersonaName() },
                { "Steam State", SteamFriends.GetPersonaState().ToString().Substring(15) },
                { "Lobby ID", (ulong) _lobbyId == 0 ? "False" : _lobbyId.ToString() },
                { "Lobby Name", (ulong) _lobbyId == 0 ? "-" : SteamMatchmaking.GetLobbyData(_lobbyId, "name") }
            };
        }
        else
            lobbyValues = new Dictionary<string, string>();

        return lobbyValues;
    }
}