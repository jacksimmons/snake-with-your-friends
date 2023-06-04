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
using static UnityEngine.Networking.UnityWebRequest;

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
    private CSteamID _id;

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
    private LobbyState _lobbyState = LobbyState.NotInOne;
    private Dictionary<CSteamID, string> _lobbyNames = new Dictionary<CSteamID, string>();
    private Dictionary<CSteamID, PlayerBehaviour> _lobbyPlayers = new Dictionary<CSteamID, PlayerBehaviour>();

    // Loading data
    private int _playersLoaded = 0;

    // Packet data
    private IntPtr _sendBuf = Marshal.AllocHGlobal(65536);
    private const int _MAX_MESSAGES = 16;
    private IntPtr[] _receiveBufs = new IntPtr[_MAX_MESSAGES];

    // SteamAPI
    protected Callback<LobbyChatUpdate_t> m_LobbyChatUpdate;
    protected Callback<LobbyDataUpdate_t> m_LobbyDataUpdate;
    private CallResult<LobbyEnter_t> m_LobbyEnter;
    private CallResult<LobbyCreated_t> m_LobbyCreated;

    // Start is called before the first frame update
    private void Awake()
    {
        if (SteamManager.Initialized)
        {
            m_LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
            m_LobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
            m_LobbyEnter = CallResult<LobbyEnter_t>.Create(OnLobbyEnter);
            m_LobbyCreated = CallResult<LobbyCreated_t>.Create(OnLobbyCreated);

            _id = SteamUser.GetSteamID();

            DontDestroyOnLoad(this);
        }
    }

    private void Update()
    {
        ReceiveMessages(Channel.Default);
        ReceiveMessages(Channel.Console, true);
    }

    private void FixedUpdate()
    {
        ReceiveMessages(Channel.Physics);

        if (!_isOwner && !_counter.Paused)
            _counter.Paused = true;
    }

    public void PlayerLoaded()
    {
        if (!_isOwner)
        {
            SendMessageTo(SteamMatchmaking.GetLobbyOwner(_lobbyId), ToBytes("player_loaded"), 0);
        }
        else
        {
            _playersLoaded++;
            if (_playersLoaded == _lobbyNames.Keys.Count)
            {
                SendMessageTo(CSteamID.Nil, ToBytes("all_players_loaded"), Channel.Default);
                SendMessageTo(CSteamID.Nil, ToBytes("All players have loaded successfully."), Channel.Console);
            }
        }
    }

    /// <summary>
    /// The global counter threshold handles all players with default move speed.
    /// </summary>
    public void OnCounterThresholdReached()
    {
        // Call all movement loops with default movement speed.
        foreach (var kvp in _lobbyPlayers)
        {
            if (kvp.Value.MovementSpeed != 1.0f)
                SendMessageTo(kvp.Key, ToBytes("move_timer"), Channel.Physics);
        }

        if (Player.MovementSpeed == 1.0f)
        {
            Message_PlayerMovement();
        }
    }

    /// <summary>
    /// Player counter thresholds handle individual players with non-standard move
    /// speed.
    /// </summary>
    /// <param name="mover">The player with a custom threshold that got triggered.</param>
    public void OnCounterThresholdReached(CSteamID mover)
    {
        SendMessageTo(mover, ToBytes("move_timer"), Channel.Physics);
    }

    private byte[] ToBytes(string str)
    {
        return Encoding.ASCII.GetBytes(str.ToString());
    }

    private byte[] ToBytes<T>(T data)
    {
        int size = Marshal.SizeOf(data);
        byte[] arr = new byte[size];

        IntPtr ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(data, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
        }
        catch
        {
            Debug.LogError("Was unable to convert to bytes.");
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        return arr;
    }

    private string FromBytes(byte[] data)
    {
        return Encoding.ASCII.GetString(data);
    }

    private T FromBytes<T>(byte[] data)
    {
        T payload;

        IntPtr ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, ptr, data.Length);
            payload = Marshal.PtrToStructure<T>(ptr);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        return payload;
    }

    /// <summary>
    /// Raw send procedure, used by SendMessageTo.
    /// Sends a message to one user.
    /// </summary>
    private void SendMessageToUser(CSteamID cSteamID, byte[] message, int channel)
    {
        // Don't need to waste time clearing the buffer; only message.Length
        // bytes of it are going to be used.
        Marshal.Copy(message, 0, _sendBuf, message.Length);
        SteamNetworkingIdentity identity = new SteamNetworkingIdentity();
        identity.SetSteamID(cSteamID);
        EResult result = SteamNetworkingMessages.SendMessageToUser(ref identity, _sendBuf, (uint)message.Length, 0, (int)channel);
        switch (result)
        {
            case EResult.k_EResultOK:
                break;
            default:
                Debug.LogError("Message failed to send.");
                break;
        }
    }

    /// <summary>
    /// Sends a message to one or all users.
    /// </summary>
    /// <param name="target">Either a valid CSteamID, or CSteamID.Nil to send to all.</param>
    /// <param name="message">The bytes representation of the message to send.</param>
    /// <param name="channel">The channel to send the message on.</param>
    private void SendMessageTo(CSteamID target, byte[] message, Channel channel)
    {
        try
        {
            if (target == CSteamID.Nil)
            {
                foreach (CSteamID id in _lobbyNames.Keys)
                    if (id != _id)
                        SendMessageToUser(id, message, (int)channel);
            }
            else if (target != _id)
                SendMessageToUser(target, message, (int)channel);
        }
        catch
        {
            Marshal.FreeHGlobal(_sendBuf);
        }
    }

    /// <summary>
    /// Sends one or more messages to one or all users.
    /// </summary>
    /// <param name="target">Either a valid CSteamID, or CSteamID.Nil to send to all.</param>
    /// <param name="title">The first message, i.e. the string title, which infers the type of the remaining contents.</param>
    /// <param name="messages">The remaining messages.</param>
    /// <param name="channel">The channel to send on.</param>
    private void SendFormattedMessageTo(CSteamID target, string title, List<byte[]> messages, Channel channel)
    {
        SendMessageTo(target, ToBytes(title), channel);
        foreach (byte[] msg in messages)
        {
            SendMessageTo(target, msg, channel);
        }
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
            try
            {
                SteamNetworkingMessage_t netMessage = Marshal.PtrToStructure<SteamNetworkingMessage_t>(_receiveBufs[i]);
                byte[] data = new byte[netMessage.m_cbSize];
                Marshal.Copy(netMessage.m_pData, data, 0, data.Length);
                CSteamID sender = netMessage.m_identityPeer.GetSteamID();

                if (i == 0)
                {
                    message = FromBytes(data);
                    if (outputMessage)
                        print(message);
                }

                switch (message)
                {
                    case "move_timer":
                        if (_isOwner)
                            Debug.LogError("Owner should never receive a move_timer packet!");
                        else
                            Message_PlayerMovement();
                        break;
                    case "player_loaded":
                        if (_isOwner)
                            _playersLoaded++;
                        else
                            SendMessageTo(netMessage.m_identityPeer.GetSteamID(), ToBytes("player_loaded sent to non-host."), Channel.Console);
                        break;
                    case "bp_data":
                        if (i > 0)
                        {
                            // Every i is a new BodyPart.
                            PlayerBehaviour player = _lobbyPlayers[sender];
                            BodyPartData bpData = FromBytes<BodyPartData>(data);
                            BodyPart bp = player.BodyParts[i - 1];
                            bp.p_Position = new Vector3(bpData.pos_x, bpData.pos_y, bp.p_Position.z);
                            print(bp.p_Rotation);
                            bp.p_Rotation = Quaternion.Euler(Vector3.forward * bpData.rotation);
                            bp.p_Sprite = bpData.sprite;
                        }
                        break;
                    case "movement_speed_update":
                    default:
                        break;
                }
            }
            finally
            {
                Marshal.DestroyStructure<SteamNetworkingMessage_t>(_receiveBufs[i]);
            }
        }
    }

    private void Message_PlayerMovement()
    {
        Player.HandleMovementLoop();
        List<byte[]> msgs = new List<byte[]>();
        foreach (BodyPart bp in Player.BodyParts)
            msgs.Add(ToBytes(bp.ToData()));
        SendFormattedMessageTo(CSteamID.Nil, "bp_data", msgs, Channel.Physics);
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

    public void CreateLobby()
    {
        SteamAPICall_t handle = SteamMatchmaking.CreateLobby(
            ELobbyType.k_ELobbyTypePublic, cMaxMembers: 4);
        m_LobbyCreated.Set(handle);
        m_LobbyEnter.Set(handle);
        _isOwner = true;

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("PlayerInput"))
        {
            if (go.name == "SpeedSlider")
            {
                Slider slider = go.GetComponent<Slider>();
                _counter.ThresholdSeconds = slider.value;
            }
        }
    }

    public void JoinLobby(CSteamID id)
    {
        SteamAPICall_t handle = SteamMatchmaking.JoinLobby(id);
        m_LobbyEnter.Set(handle);
    }

    // Callbacks
    private void OnLobbyCreated(LobbyCreated_t result, bool bIOFailure)
    {
        switch (result.m_eResult)
        {
            case EResult.k_EResultOK:
                print("Lobby [ID: " + result.m_ulSteamIDLobby + "] created successfully.");
                bool success = SteamMatchmaking.SetLobbyData(
                (CSteamID)result.m_ulSteamIDLobby,
                "name",
                SteamFriends.GetPersonaName() + "'s lobby");
                _lobbyId = (CSteamID)result.m_ulSteamIDLobby;

                if (success)
                {
                    print("Yay set name!");
                }
                else
                    print("Nay didn't set name...");

                break;
            default:
                print("Failed to create lobby.");
                return;
        }
    }

    private void OnLobbyEnter(LobbyEnter_t result, bool bIOFailure)
    {
        if (bIOFailure || result.m_EChatRoomEnterResponse == (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseError)
        {
            print("Failed to enter lobby.");
        }
        else
        {
            // If lobbyId hasn't already been set (i.e. by creating a lobby)
            if (_lobbyId == CSteamID.Nil)
                _lobbyId = (CSteamID)result.m_ulSteamIDLobby;
            StartCoroutine(LoadLobby());
            print("Entered lobby successfully.");
        }
    }

    // A user has joined, left, disconnected, etc. Need to check if we are the new owner.
    private void OnLobbyChatUpdate(LobbyChatUpdate_t pCallback)
    {
        CSteamID affects = (CSteamID)pCallback.m_ulSteamIDUserChanged;
        CSteamID changer = (CSteamID)pCallback.m_ulSteamIDMakingChange;

        string affectsName = SteamFriends.GetFriendPersonaName(
            (CSteamID)pCallback.m_ulSteamIDUserChanged);
        string changerName = SteamFriends.GetFriendPersonaName(
            (CSteamID)pCallback.m_ulSteamIDMakingChange);

        uint stateChange = pCallback.m_rgfChatMemberStateChange;
        switch (stateChange)
        {
            case 1 << 0:
                print(affectsName + " entered.");
                AddLobbyMember(affects);
                break;
            case 1 << 1:
                print(affectsName + " left.");
                RemoveLobbyMember(affects);
                break;
            case 1 << 2:
                print(affectsName + " disconnected.");
                RemoveLobbyMember(affects);
                break;
            case 1 << 3:
                print(changerName + " kicked " + affects);
                RemoveLobbyMember(affects);
                break;
            case 1 << 4:
                print(changer + " banned " + affects);
                RemoveLobbyMember(affects);
                break;
            default:
                print("[OnLobbyChatUpdate] Something...happened?");
                break;
        }

        _isOwner = _id == SteamMatchmaking.GetLobbyOwner(_lobbyId);
    }

    private void OnLobbyDataUpdate(LobbyDataUpdate_t pCallback)
    {
        if (pCallback.m_bSuccess == 1)
        {
        }
        else
        {
            print("Data was unable to be changed for ID " + pCallback.m_ulSteamIDMember + ".");
        }
    }

    private void AddLobbyMember(CSteamID id)
    {
        string name = SteamFriends.GetFriendPersonaName(id);
        _lobbyNames.Add(id, name);
        CreatePlayer(id);

        GameObject content = GameObject.FindWithTag("LobbyPanel");
        GameObject entry = Instantiate(_lobbyEntryTemplate, content.transform);
        TextMeshProUGUI[] tmps = entry.GetComponentsInChildren<TextMeshProUGUI>();

        tmps[0].text = _lobbyNames.Count.ToString();
        tmps[1].text = name;
    }

    private void RemoveLobbyMember(CSteamID id)
    {
        PlayerBehaviour pb = _lobbyPlayers[id];
        Destroy(pb.gameObject);
        _lobbyPlayers.Remove(id);
        _lobbyNames.Remove(id);
    }

    /// <summary>
    /// Should only be used when joining a lobby, to prevent reconstruction on every
    /// chat update event.
    /// </summary>
    private void AddAllLobbyMembers()
    {
        int numPlayers = SteamMatchmaking.GetNumLobbyMembers(_lobbyId);
        for (int i = 0; i < numPlayers; i++)
        {
            CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(_lobbyId, i);
            AddLobbyMember(memberId);
        }
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

        _lobbyPlayers[id] = snake.GetComponentInChildren<PlayerBehaviour>();

        if (id == _id)
        {
            Player = _lobbyPlayers[id];
        }
    }

    private IEnumerator LoadLobby()
    {
        _lobbyState = LobbyState.InLobbyMenu;
        SceneManager.LoadSceneAsync("LobbyMenu");

        while (SceneManager.GetActiveScene().name != "LobbyMenu")
        {
            yield return new WaitForSeconds(1);
        }

        AddAllLobbyMembers();
        GameObject.FindWithTag("MainCamera").GetComponent<CamBehaviour>().SetupCamera(Player);

        _counter.Paused = false;

        yield break;
    }

    public void OnPlayerMovementSpeedUpdate(float movementSpeed)
    {
        if (_isOwner)
        {
            
        }
        List<byte[]> data = new();
        byte[] movement_speed = ToBytes(movementSpeed);
        data.Add(movement_speed);
        SendFormattedMessageTo(SteamMatchmaking.GetLobbyOwner(_lobbyId), "movement_speed_update", data, Channel.Physics);
    }

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