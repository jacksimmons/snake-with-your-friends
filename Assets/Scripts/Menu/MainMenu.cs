using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnCreateServerButtonPressed()
    {
        SceneManager.LoadScene("LobbyMenu");
    }

    public void OnJoinServerButtonPressed()
    {
        SceneManager.LoadScene("JoinMenu");
    }
}
