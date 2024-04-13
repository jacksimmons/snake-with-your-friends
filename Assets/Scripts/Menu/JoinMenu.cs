using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Client-side lobby joining.
/// </summary>
public class JoinMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject _contentOutput;

    [SerializeField]
    private GameObject _joinEntryTemplate;

    protected Callback<LobbyDataUpdate_t> m_LobbyDataUpdate;

    private CallResult<LobbyMatchList_t> m_LobbyMatchList;
    private CallResult<LobbyEnter_t> m_LobbyEnter;


    private void Start()
    {
        if (SteamManager.Initialized)
        {
            string name = SteamFriends.GetPersonaName();
            Debug.Log(name);

            m_LobbyMatchList = CallResult<LobbyMatchList_t>.Create(OnLobbyMatchList);

            RefreshLobbyList();
        }
    }

    public void RefreshLobbyList()
    {
        SteamAPICall_t handle = SteamMatchmaking.RequestLobbyList();
        m_LobbyMatchList.Set(handle);
    }

    public void OnLobbyJoinPressed(GameObject lobbyEntry)
    {
        ulong.TryParse(lobbyEntry.name, out ulong id);

        Steam.Instance.JoinLobby((CSteamID)id);
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
        _contentOutput.transform.DestroyAllChildren();

        uint count = result.m_nLobbiesMatching;
        for (int i = 0; i < count; i++)
        {
            CSteamID lobby_id = SteamMatchmaking.GetLobbyByIndex(i);
            GameObject newElement = Instantiate(_joinEntryTemplate, _contentOutput.transform);
            TextMeshProUGUI[] tmps = newElement.GetComponentsInChildren<TextMeshProUGUI>();

            newElement.name = lobby_id.ToString();
            string name = SteamMatchmaking.GetLobbyData(lobby_id, "name");
            int players = SteamMatchmaking.GetNumLobbyMembers(lobby_id);
            int player_limit = SteamMatchmaking.GetLobbyMemberLimit(lobby_id);

            // Even indices are labels
            tmps[1].text = name;
            tmps[3].text = players.ToString() + "/" + player_limit.ToString();

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
