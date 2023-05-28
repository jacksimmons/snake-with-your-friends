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
using UnityEngine;
using UnityEngine.SceneManagement;

// Channels are used for different types:
// 0 - Update
// 1 - FixedUpdate
// 2 - Console Output
// 3 - Body Parts (List<BodyPart>)

/// <summary>
/// Server-side lobby management.
/// </summary>
public class Lobby : MonoBehaviour
{
    [SerializeField]
    private GameObject _lobbyEntryTemplate;

    // Other Data
    private PlayerBehaviour _player;

    // User Data
    private CSteamID _id;

    // Lobby Data
    private enum LobbyState
    {
        NotInOne,
        InLobbyMenu,
        InGame
    }
    private CSteamID _lobbyId = new CSteamID();
    private bool _isOwner = false;
    private LobbyState _lobbyState = LobbyState.NotInOne;
    private Dictionary<CSteamID, Dictionary<string, string>> _lobbyPlayerList
        = new Dictionary<CSteamID, Dictionary<string, string>>();

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
        ReceiveMessages<string>(0);
        ReceiveMessages<string>(2, true);
        ReceiveMessages<List<BodyPart>>(3);
    }

    private void FixedUpdate()
    {
        ReceiveMessages<string>(1);
        ReceiveMessages<float>(3);

        if ((_lobbyState != LobbyState.NotInOne) && _isOwner)
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
            if (_playersLoaded == _lobbyPlayerList.Keys.Count)
            {
                SendMessageTo((CSteamID)0, ToBytes("all_players_loaded"), 0);
                SendMessageTo((CSteamID)0, ToBytes("All players have loaded successfully."), 2);
            }
        }
    }

    private void IncrementGlobalTimer()
    {
        _moveTimer++;
        if (_moveTimer % _frequency == 0)
        {
            // Call all player movement loops, including our own.
            SendMessageTo((CSteamID)0, ToBytes("move_timer"), 1);
            SendMessageTo((CSteamID)0, ToBytes("Move timer."), 2);
            _player.HandleMovementLoop();
            SendMessageTo((CSteamID)0, ToBytes(_player.BodyParts), 3);
        }
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
        T message;

        IntPtr ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, ptr, data.Length);
            message = Marshal.PtrToStructure<T>(ptr);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        return message;
    }

    private void SendMessageTo(CSteamID target, byte[] message, int channel)
    {
        Marshal.Copy(message, 0, _sendBuf, message.Length);
        try
        {
            if (target.m_SteamID == 0)
            {
                foreach (CSteamID id in _lobbyPlayerList.Keys)
                {
                    SteamNetworkingIdentity identity = new SteamNetworkingIdentity();
                    identity.SetSteamID(id);
                    SteamNetworkingMessages.SendMessageToUser(ref identity, _sendBuf, (uint)message.Length, 0, channel);
                }
            }
            else
            {
                SteamNetworkingIdentity identity = new SteamNetworkingIdentity();
                identity.SetSteamID(target);
                SteamNetworkingMessages.SendMessageToUser(ref identity, _sendBuf, (uint)message.Length, 0, channel);
            }
        }
        catch
        {
            Marshal.FreeHGlobal(_sendBuf);
        }
    }

    /// <typeparam name="T">The type of the return value in the dictionary.
    /// If `string`, then it is not a dictionary, but simply a string message.</typeparam>
    private void ReceiveMessages<T>(int channel, bool outputMessage=false)
    {
        int messageCount = SteamNetworkingMessages.ReceiveMessagesOnChannel(channel, _receiveBufs, _MAX_MESSAGES);
        for (int i = 0; i < messageCount; i++)
        {
            try
            {
                SteamNetworkingMessage_t netMessage = Marshal.PtrToStructure<SteamNetworkingMessage_t>(_receiveBufs[i]);
                byte[] data = new byte[netMessage.m_cbSize];
                Marshal.Copy(netMessage.m_pData, data, 0, data.Length);

                if (typeof(T) == typeof(string))
                {
                    string message = FromBytes(data);

                    if (outputMessage)
                        print(message);

                    switch (message)
                    {
                        case "move_timer":
                            _player.HandleMovementLoop();
                            SendMessageTo((CSteamID)0, ToBytes(_player.BodyParts), 3);
                            break;
                        case "player_loaded":
                            if (_isOwner)
                            {
                                _playersLoaded++;
                            }
                            else
                            {
                                SendMessageTo(netMessage.m_identityPeer.GetSteamID(), ToBytes("player_loaded sent to non-host."), 2);
                            }
                            break;
                    }
                }

                else if (typeof(T) == typeof(List<BodyPart>))
                {
                    // ... Needs player object implementation.
                    List<BodyPart> bps = FromBytes<List<BodyPart>>(data);
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
        if ((ulong)_lobbyId != 0)
        {
            SteamMatchmaking.LeaveLobby(_lobbyId);
            print("Left the lobby.");
        }
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
                print("Lobby created successfully.");
                break;
            default:
                print("Failed to create lobby.");
                return;
        }

        bool success = SteamMatchmaking.SetLobbyData(
            (CSteamID)result.m_ulSteamIDLobby,
            "name",
            SteamFriends.GetPersonaName() + "'s lobby");
        if (success)
        {
            print("Yay set name!");
        }
        else
            print("Nay didn't set name...");
        StartCoroutine(LoadLobby());
    }

    private void OnLobbyEnter(LobbyEnter_t result, bool bIOFailure)
    {
        switch (result.m_EChatRoomEnterResponse)
        {
            case (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess:
                print("Joined lobby successfully.");
                _lobbyId = (CSteamID)result.m_ulSteamIDLobby;
                StartCoroutine(LoadLobby());
                break;
            case (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseNotAllowed:
                print("Not allowed to join lobby.");
                break;
            case (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseError:
                print("An error occurred.");
                break;
            case (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseDoesntExist:
                print("This lobby no longer exists.");
                break;
        }
    }

    // A user has joined, left, disconnected, etc. Need to check if we are the new owner.
    private void OnLobbyChatUpdate(LobbyChatUpdate_t pCallback)
    {
        string affects = SteamFriends.GetFriendPersonaName(
            (CSteamID)(pCallback.m_ulSteamIDUserChanged));
        string changer = SteamFriends.GetFriendPersonaName(
            (CSteamID)(pCallback.m_ulSteamIDMakingChange));

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
        for (int i = 0; i < numPlayers; i++)
        {
            CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(_lobbyId, i);
            string name = SteamFriends.GetFriendPersonaName(memberId);

            _lobbyPlayerList.Add(memberId,
                new Dictionary<string, string> { { "name", name } });
        }
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
            print(i);
            GameObject entry = Instantiate(_lobbyEntryTemplate, content.transform);
            TextMeshProUGUI[] tmps = entry.GetComponentsInChildren<TextMeshProUGUI>();

            CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(_lobbyId, i);
            string name = SteamFriends.GetFriendPersonaName(memberId);

            tmps[0].text = i.ToString();
            tmps[1].text = name;
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

        _player = GameObject.FindWithTag("Player").GetComponent<PlayerBehaviour>();

        UpdatePlayerList();
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