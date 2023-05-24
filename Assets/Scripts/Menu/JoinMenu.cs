using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JoinMenu : MonoBehaviour
{
    [SerializeField]
    private Lobby _lobby;

    [SerializeField]
    private GameObject _contentOutput;

    [SerializeField]
    private GameObject _lobbyButtonTemplate;

    protected Callback<LobbyDataUpdate_t> m_LobbyDataUpdate;

    private CallResult<LobbyMatchList_t> m_LobbyMatchList;
    private CallResult<LobbyEnter_t> m_LobbyEnter;

    public enum Security
    {
        Private,
        Friends,
        Public,
        Invisible
    }

    public enum Distance
    {
        Worldwide,
        Far,
        Regional,
        Close
    }

    public enum Slots
    {
        Available,
        Any
    }

    private void Start()
    {
        if (SteamManager.Initialized)
        {
            string name = SteamFriends.GetPersonaName();
            Debug.Log(name);

            RefreshLobbyList();
        }
    }

    public void RefreshLobbyList()
    {
        m_LobbyMatchList = CallResult<LobbyMatchList_t>.Create(OnLobbyMatchList);
        SteamAPICall_t handle = SteamMatchmaking.RequestLobbyList();
        m_LobbyMatchList.Set(handle);
    }

    public void OnBackPressed()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnLobbyJoinPressed(TextMeshProUGUI idField)
    {
        uint id;
        uint.TryParse(idField.text, out id);
        id--;

        print(id);

        _lobby.JoinLobby((CSteamID)id);
    }

    public void OnDistanceDropdownUpdated(int distance)
    {
        ELobbyDistanceFilter filter;
        switch (distance)
        {
            case 0:
                filter = ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide;
                break;
            case 1:
                filter = ELobbyDistanceFilter.k_ELobbyDistanceFilterFar;
                break;
            case 2:
                filter = ELobbyDistanceFilter.k_ELobbyDistanceFilterDefault;
                break;
            case 3:
                filter = ELobbyDistanceFilter.k_ELobbyDistanceFilterClose;
                break;
            default:
                filter = ELobbyDistanceFilter.k_ELobbyDistanceFilterDefault;
                break;
        }
        SteamMatchmaking.AddRequestLobbyListDistanceFilter(filter);
        RefreshLobbyList();
    }

    public void OnFullnessDropdownUpdated(int fullness)
    {
        int slots;
        switch (fullness)
        {
            case 0:
                slots = 1;
                break;
            default:
                slots = 0;
                break;
        }
        SteamMatchmaking.AddRequestLobbyListFilterSlotsAvailable(slots);
        RefreshLobbyList();
    }

    private void OnLobbyMatchList(LobbyMatchList_t result, bool bIOFailure)
    {
        // Remove pre-existing lobbies
        foreach (Transform child in _contentOutput.transform)
        {
            Destroy(child.gameObject);
        }

        uint count = result.m_nLobbiesMatching;
        for (int i = 0; i < count; i++)
        {
            CSteamID lobby_id = SteamMatchmaking.GetLobbyByIndex(i);
            GameObject newElement = Instantiate(_lobbyButtonTemplate, _contentOutput.transform);
            TextMeshProUGUI[] tmps = newElement.GetComponentsInChildren<TextMeshProUGUI>();

            string name = SteamMatchmaking.GetLobbyData(lobby_id, "name");
            int players = SteamMatchmaking.GetNumLobbyMembers(lobby_id);
            int player_limit = SteamMatchmaking.GetLobbyMemberLimit(lobby_id);

            // Odd indices are labels
            tmps[0].text = (i + 1).ToString();
            tmps[2].text = name;
            tmps[4].text = players.ToString() + "/" + player_limit.ToString();
            tmps[6].text = lobby_id.ToString();

            //string lobbyTypeName;
            //switch (lobbyType)
            //{
            //	case 0:
            //		lobbyTypeName = "Private";
            //		break;
            //	case 1:
            //		lobbyTypeName = "Friends";
            //		break;
            //	case 2:
            //		lobbyTypeName = "Public";
            //		break;
            //	case 3:
            //		lobbyTypeName = "Invisible";
            //		break;
            //	default:
            //		lobbyTypeName = "Unknown";
            //		break;
            //}
        }
    }
}
