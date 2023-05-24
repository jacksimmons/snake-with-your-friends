using UnityEngine;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField]
    private Lobby _lobby;

    // Start is called before the first frame update
    void Start()
    {
        _lobby.CreateLobby();
    }
}
