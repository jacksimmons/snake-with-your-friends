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
    public bool IsOwner { get; private set; } = false;
    private LobbyState _lobbyState = LobbyState.NotInOne;
    private Dictionary<CSteamID, string> _lobbyNames = new();
    public Dictionary<CSteamID, PlayerBehaviour> LobbyPlayers { get; private set; } = new();

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

    protected Callback<SteamNetworkingMessagesSessionFailed_t> m_SteamNetworkingMessagesSessionFailed;

    // Start is called before the first frame update
    private void Awake()
    {
        if (SteamManager.Initialized)
        {
            m_LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
            m_LobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
            m_LobbyEnter = CallResult<LobbyEnter_t>.Create(OnLobbyEnter);
            m_LobbyCreated = CallResult<LobbyCreated_t>.Create(OnLobbyCreated);

            m_SteamNetworkingMessagesSessionFailed = Callback<SteamNetworkingMessagesSessionFailed_t>.Create(OnMessageSessionFailed);

            Id = SteamUser.GetSteamID();
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

        if (!IsOwner && !_counter.Paused)
            _counter.Paused = true;
    }

    public void SetupOffline(PlayerBehaviour player)
    {
        IsOwner = true;
        _counter.Paused = false;
        Player = player;
    }

    public void PlayerLoaded()
    {
        if (!IsOwner)
        {
            SendMessageToUser(SteamMatchmaking.GetLobbyOwner(_lobbyId), ToBytes("player_loaded"), 0);
        }
        else
        {
            _playersLoaded++;
            if (_playersLoaded == _lobbyNames.Keys.Count)
            {
                SendMessagesTo(CSteamID.Nil, "all_players_loaded", null, Channel.Default);
                SendMessagesTo(CSteamID.Nil, "All players have loaded successfully.", null, Channel.Console);
            }
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
                if (kvp.Value.MovementSpeed == PlayerBehaviour.DEFAULT_MOVEMENT_SPEED)
                    SendMessageToUser(kvp.Key, ToBytes("move_timer"), Channel.Physics);
        }

        // Call our own player's movement loop if it has default movement speed
        if (Player.MovementSpeed == PlayerBehaviour.DEFAULT_MOVEMENT_SPEED)
            Message_MoveTimer();
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
                Message_MoveTimer();
        else
            SendMessageToUser(mover, ToBytes("move_timer"), Channel.Physics);
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
    private void SendMessageToUser(CSteamID cSteamID, byte[] message, Channel channel)
    {
        // Don't need to waste time clearing the buffer; only message.Length
        // bytes of it are going to be used.
        string name = SteamFriends.GetFriendPersonaName(cSteamID);
        Marshal.Copy(message, 0, _sendBuf, message.Length);

        SteamNetworkingIdentity identity = new SteamNetworkingIdentity();
        identity.SetSteamID(cSteamID);
        EResult result = SteamNetworkingMessages.SendMessageToUser(ref identity, _sendBuf, (uint)message.Length, 0, (int)channel);

        BodyPartData bp_data = FromBytes<BodyPartData>(message);
        string str_data = FromBytes(message);

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
                        SendMessageToUser(id, ToBytes(title), channel);
                        if (messages != null)
                        {
                            foreach (byte[] message in messages)
                                SendMessageToUser(id, message, channel);
                        }
                    }
            }
            else if (target != Id)
            {
                SendMessageToUser(target, ToBytes(title), channel);
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
            msgs.Add(ToBytes(bp.ToData()));
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
        byte[] movement_speed = ToBytes(movementSpeed);
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
                        if (IsOwner)
                            Debug.LogError("Owner should never receive a move_timer packet!");
                        else
                            Message_MoveTimer();
                        break;
                    case "player_loaded":
                        if (IsOwner)
                            _playersLoaded++;
                        else
                            SendMessageToUser(netMessage.m_identityPeer.GetSteamID(), ToBytes("player_loaded sent to non-host."), Channel.Console);
                        break;
                    case "bp_data":
                        if (i > 0)
                        {
                            // Every i is a new BodyPart.
                            PlayerBehaviour player = LobbyPlayers[sender];
                            BodyPartData bpData = FromBytes<BodyPartData>(data);
                            BodyPart bp = player.BodyParts[i - 1];
                            bp.p_Position = new Vector3(bpData.pos_x, bpData.pos_y, bp.p_Position.z);
                            bp.p_Rotation = Quaternion.Euler(Vector3.forward * bpData.rotation);
                            bp.p_Sprite = bpData.sprite;
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


    private void Message_MoveTimer()
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

    public void CreateLobby()
    {
        SteamAPICall_t handle = SteamMatchmaking.CreateLobby(
            ELobbyType.k_ELobbyTypePublic, cMaxMembers: 4);
        m_LobbyCreated.Set(handle);
        m_LobbyEnter.Set(handle);
        IsOwner = true;

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("PlayerInput"))
        {
            if (go.name == "SpeedSlider")
            {
                Slider slider = go.GetComponent<Slider>();
                _counter.thresholdSeconds = slider.value;
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

        IsOwner = Id == SteamMatchmaking.GetLobbyOwner(_lobbyId);
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

    private void OnMessageSessionFailed(SteamNetworkingMessagesSessionFailed_t pCallback)
    {
        SteamNetConnectionInfo_t info = pCallback.m_info;
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
        PlayerBehaviour pb = LobbyPlayers[id];
        Destroy(pb.gameObject);
        LobbyPlayers.Remove(id);
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
        print("hi");
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

    private IEnumerator LoadLobby()
    {
        _lobbyState = LobbyState.InLobbyMenu;
        AsyncOperation loadLobbyMenuComplete = SceneManager.LoadSceneAsync("LobbyMenu");

        while (!loadLobbyMenuComplete.isDone)
        {
            yield return new WaitForSeconds(1);
        }
        AddAllLobbyMembers();

        foreach (Transform child in GameObject.FindWithTag("PlayerParent").transform)
        {
            child.Find("Player").GetComponent<PlayerBehaviour>().InLobbyMenu = true;
        }

        _counter.Paused = false;

        GameObject.FindWithTag("MainCamera").GetComponent<CamBehaviour>().SetupCamera(Player);

        yield break;
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