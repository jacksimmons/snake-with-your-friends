using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnCreateServerButtonPressed()
    {
        SceneManager.LoadScene("CreateMenu");
    }

    public void OnJoinServerButtonPressed()
    {
        SceneManager.LoadScene("JoinMenu");
    }
}
