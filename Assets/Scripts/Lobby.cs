using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Channels are used for different types:
// 0 - Non-FixedUpdate strings
// 1 - FixedUpdate strings

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
    }

    private void FixedUpdate()
    {
        ReceiveMessages<string>(1);
        if (_lobbyState != LobbyState.NotInOne && _isOwner)
        {
            IncrementGlobalTimer();
        }
    }

    private void IncrementGlobalTimer()
    {
        _moveTimer++;
        if (_moveTimer % _frequency == 0)
        {
            // Call all player movement loops, including our own.
            string message = "move_timer";
            byte[] data = ToBytes(message);
            SendMessageTo("all", data, 1);
            _player.HandleMovementLoop();
        }
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

    private void SendMessageTo(string target, byte[] message, int channel)
    {
        print("Message to " + target + ", length: " + message.Length + " bytes");
        Marshal.Copy(message, 0, _sendBuf, message.Length);
        try
        {
            if (target == "all")
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
                ulong.TryParse(target, out ulong id);
                CSteamID steamId = new CSteamID(id);
                SteamNetworkingIdentity identity = new SteamNetworkingIdentity();
                identity.SetSteamID(steamId);
                SteamNetworkingMessages.SendMessageToUser(ref identity, _sendBuf, (uint)message.Length, 0, channel);
            }
        }
        catch
        {
            Marshal.FreeHGlobal(_sendBuf);
        }
    }

    private void ReceiveMessages<T>(int channel)
    {
        int messageCount = SteamNetworkingMessages.ReceiveMessagesOnChannel(channel, _receiveBufs, _MAX_MESSAGES);
        for (int i = 0; i < messageCount; i++)
        {
            try
            {
                SteamNetworkingMessage_t netMessage = Marshal.PtrToStructure<SteamNetworkingMessage_t>(_receiveBufs[i]);
                byte[] message = new byte[netMessage.m_cbSize];
                Marshal.Copy(netMessage.m_pData, message, 0, message.Length);
                ProcessMessage(FromBytes<T>(message));
            }
            finally
            {
                Marshal.DestroyStructure<SteamNetworkingMessage_t>(_receiveBufs[i]);
            }
        }
    }

    private void ProcessMessage<T>(T data)
    {
        if (typeof(T) == typeof(string))
        {
            string message = data.ToString();
            if (message == "move_timer")
            {
                _player.HandleMovementLoop();
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
        _isOwner = true;
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