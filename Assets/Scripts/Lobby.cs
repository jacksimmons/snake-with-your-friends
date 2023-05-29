using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using TMPro;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    // Other Data
    public PlayerBehaviour Player { get; private set; }

    // User Data
    private CSteamID _id;

    // Lobby Data
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
    private int _moveTimer = 0;
    private int _frequency = 60;
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
        ReceiveMessages(0);
        ReceiveMessages(2, true);
    }

    private void FixedUpdate()
    {
        ReceiveMessages(1);

        if ((_lobbyState != LobbyState.NotInOne) && _isOwner && Player != null)
        {
            IncrementGlobalTimer();
        }
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
                SendMessageTo(CSteamID.Nil, ToBytes("all_players_loaded"), 0);
                SendMessageTo(CSteamID.Nil, ToBytes("All players have loaded successfully."), 2);
            }
        }
    }

    private void IncrementGlobalTimer()
    {
        _moveTimer++;
        if (_moveTimer % _frequency == 0)
        {
            // Call all player movement loops, including our own.
            SendMessageTo(CSteamID.Nil, ToBytes("move_timer"), 1);
            SendMessageTo(CSteamID.Nil, ToBytes("Move timer."), 2);

            Player.HandleMovementLoop();
            SendBodyPartPackets(CSteamID.Nil);
        }
    }

    private byte[] ToBytes(string str)
    {
        return Encoding.ASCII.GetBytes(str.ToString());
    }

    private byte[] ToBytes<T>(T[] data)
    {
        int size = Marshal.SizeOf(data.Length);
        byte[] arr = new byte[size];

        IntPtr ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(data, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
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

    private T[] FromBytes<T>(byte[] data)
    {
        T[] payload;

        IntPtr ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, ptr, data.Length);
            payload = Marshal.PtrToStructure<T[]>(ptr);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        return payload;
    }

    private void SendMessageTo(CSteamID target, byte[] message, int channel)
    {
        try
        {
            Marshal.Copy(message, 0, _sendBuf, message.Length);
            if (target == CSteamID.Nil)
            {
                foreach (CSteamID id in _lobbyNames.Keys)
                {
                    if (id != _id)
                    {
                        SteamNetworkingIdentity identity = new SteamNetworkingIdentity();
                        identity.SetSteamID(id);
                        print(SteamFriends.GetFriendPersonaName(id));
                        EResult result = SteamNetworkingMessages.SendMessageToUser(ref identity, _sendBuf, (uint)message.Length, 0, channel);
                        if (result != EResult.k_EResultOK)
                        {
                            print(":( all");
                        }
                    }
                }
            }
            else
            {
                SteamNetworkingIdentity identity = new SteamNetworkingIdentity();
                identity.SetSteamID(target);
                EResult result = SteamNetworkingMessages.SendMessageToUser(ref identity, _sendBuf, (uint)message.Length, 0, channel);
            }
        }
        catch
        {
            Marshal.FreeHGlobal(_sendBuf);
        }
    }

    private void ReceiveMessages(int channel, bool outputMessage=false)
    {
        int messageCount = SteamNetworkingMessages.ReceiveMessagesOnChannel(channel, _receiveBufs, _MAX_MESSAGES);
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
                        Player.HandleMovementLoop();
                        SendBodyPartPackets(CSteamID.Nil);
                        break;
                    case "player_loaded":
                        if (_isOwner)
                            _playersLoaded++;
                        else
                            SendMessageTo(netMessage.m_identityPeer.GetSteamID(), ToBytes("player_loaded sent to non-host."), 2);
                        break;
                    case "body_parts":
                        // "body_parts" packet:
                        // Payload 1 - position_xs
                        // Payload 2 - position_ys
                        // Payload 3 - rotations
                        // Payload 4 - sprites
                        PlayerBehaviour player = _lobbyPlayers[sender];
                        if (i == 1)
                        {
                            float[] position_xs = FromBytes<float>(data);
                            // The loop ends if body parts were modified in transit
                            for (int j = 0; (j < position_xs.Length && j < player.BodyParts.Count); j++)
                            {
                                BodyPart bp = player.BodyParts[j];
                                bp.p_Position = new Vector3(position_xs[j], bp.p_Position.y, bp.p_Position.z);
                            }
                        }
                        else if (i == 2)
                        {
                            float[] position_ys = FromBytes<float>(data);
                            // The loop ends if body parts were modified in transit
                            for (int j = 0; (j < position_ys.Length && j < player.BodyParts.Count); j++)
                            {
                                BodyPart bp = player.BodyParts[j];
                                bp.p_Position = new Vector3(bp.p_Position.x, position_ys[j], bp.p_Position.z);
                            }
                        }
                        else if (i == 3)
                        {
                            float[] rotations = FromBytes<float>(data);
                            // The loop ends if body parts were modified in transit
                            for (int j = 0; (j < rotations.Length && j < player.BodyParts.Count); j++)
                            {
                                BodyPart bp = player.BodyParts[j];
                                bp.p_Rotation = Quaternion.Euler(Vector3.forward * rotations[j]);
                            }
                        }
                        else if (i == 4)
                        {
                            BodyPartSprite[] sprites = FromBytes<BodyPartSprite>(data);
                            // The loop ends if body parts were modified in transit
                            for (int j = 0; (j < sprites.Length && j < player.BodyParts.Count); j++)
                            {
                                BodyPart bp = player.BodyParts[j];
                                bp.p_Sprite = sprites[j];
                            }
                        }
                        break;
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
            if (go.name == "SpeedValue")
            {
                int.TryParse(go.GetComponent<TextMeshProUGUI>().text, out _frequency);
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
        string affects = SteamFriends.GetFriendPersonaName(
            (CSteamID)pCallback.m_ulSteamIDUserChanged);
        string changer = SteamFriends.GetFriendPersonaName(
            (CSteamID)pCallback.m_ulSteamIDMakingChange);

        uint stateChange = pCallback.m_rgfChatMemberStateChange;
        switch (stateChange)
        {
            case 1 << 0:
                print(affects + " entered.");
                break;
            case 1 << 1:
                print(affects + " left.");
                break;
            case 1 << 2:
                print(affects + " disconnected.");
                break;
            case 1 << 3:
                print(changer + " kicked " + affects);
                break;
            case 1 << 4:
                print(changer + " banned " + affects);
                break;
            default:
                print("[OnLobbyChatUpdate] Something...happened?");
                break;
        }

        _isOwner = _id == SteamMatchmaking.GetLobbyOwner(_lobbyId);

        UpdatePlayerList();
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

    private void UpdatePlayerList()
    {
        int numPlayers = SteamMatchmaking.GetNumLobbyMembers(_lobbyId);
        print(_lobbyId);
        print(SteamMatchmaking.GetLobbyData(_lobbyId, "name"));
        for (int i = 0; i < numPlayers; i++)
        {
            CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(_lobbyId, i);
            string name = SteamFriends.GetFriendPersonaName(memberId);

            _lobbyNames.Add(memberId, name);
            CreatePlayer(memberId);
        }

        UpdatePlayerPanel();
    }

    public void UpdatePlayerPanel()
    {
        // The "Content" child has this tag.
        GameObject content = GameObject.FindWithTag("LobbyPanel");
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
        int numPlayers = SteamMatchmaking.GetNumLobbyMembers(_lobbyId);
        for (int i = 0; i < numPlayers; i++)
        {
            GameObject entry = Instantiate(_lobbyEntryTemplate, content.transform);
            TextMeshProUGUI[] tmps = entry.GetComponentsInChildren<TextMeshProUGUI>();

            CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(_lobbyId, i);
            string name = SteamFriends.GetFriendPersonaName(memberId);

            tmps[0].text = i.ToString();
            tmps[1].text = name;
        }
    }

    private void CreatePlayer(CSteamID id)
    {
        // Need to do all but finding PlayerParent locally, and not with Find,
        // else other already created players may ping up in the search.
        GameObject playerParent = GameObject.FindWithTag("PlayerParent");
        GameObject snake = Instantiate(_snakeTemplate, playerParent.transform);
        GameObject nameLabel = new GameObject("Name");
        TextMeshProUGUI tmp = nameLabel.AddComponent<TextMeshProUGUI>();
        tmp.text = _lobbyNames[id];
        nameLabel.transform.SetParent(snake.transform.Find("Player").Find("Head"));

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

        UpdatePlayerList();

        yield break;
    }

    private void SendBodyPartPackets(CSteamID to)
    {
        float[] position_xs = new float[Player.BodyParts.Count];
        float[] position_ys = new float[Player.BodyParts.Count];
        float[] rotations = new float[Player.BodyParts.Count];
        BodyPartSprite[] sprites = new BodyPartSprite[Player.BodyParts.Count];
        for (int j = 0; j < Player.BodyParts.Count; j++)
        {
            BodyPart bp = Player.BodyParts[j];
            position_xs[j] = bp.p_Position.x;
            position_ys[j] = bp.p_Position.y;
            rotations[j] = bp.p_Rotation.z;
            sprites[j] = bp.p_Sprite;
        }
        SendMessageTo(to, ToBytes("body_parts"), 1);
        SendMessageTo(to, ToBytes(position_xs), 1);
        SendMessageTo(to, ToBytes(position_ys), 1);
        SendMessageTo(to, ToBytes(rotations), 1);
        SendMessageTo(to, ToBytes(sprites), 1);
        SendMessageTo(to, ToBytes("Sent body part data"), 2);
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