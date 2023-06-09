using Steamworks;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject _snakeTemplate;
    [SerializeField]
    private GameObject _counterTemplate;
    [SerializeField]
    private GameObject _localLobbyTemplate;

    public void OnNoFriendsButtonPressed()
    {
        // Allows this to persist (briefly)
        DontDestroyOnLoad(this);
        StartCoroutine(LoadGame());
    }

    public void OnCreateServerButtonPressed()
    {
        SceneManager.LoadScene("CreateMenu");
    }

    public void OnJoinServerButtonPressed()
    {
        SceneManager.LoadScene("JoinMenu");
    }

    public void OnSettingsButtonPressed()
    {
        SceneManager.LoadScene("SettingsMenu");
    }

    private IEnumerator LoadGame()
    {
        SceneManager.LoadScene("Game");
        while (SceneManager.GetActiveScene().name != "Game")
            yield return new WaitForSeconds(1);
        GameBehaviour gameBehaviour = GameObject.FindWithTag("GameHandler").GetComponent<GameBehaviour>();
        // Create Snake from template, under the Players object (which has tag PlayerParent)
        GameObject snake = Instantiate(_snakeTemplate, GameObject.FindWithTag("PlayerParent").transform);
        gameBehaviour.WorldMode = GameBehaviour.EWorldMode.Offline;
        gameBehaviour.SetupGame(snake.GetComponentInChildren<PlayerBehaviour>(), new GameObject[] { snake });
        // Create Counter from template, set LocalLobby as parent and listener
        Instantiate(_localLobbyTemplate, gameBehaviour.gameObject.transform.parent);
        // Cleanup
        Destroy(gameObject);
        yield break;
    }
}
