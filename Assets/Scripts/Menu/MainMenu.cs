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
    private GameObject _lobbyTemplate;

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
        // ..!

        AsyncOperation loadGame = SceneManager.LoadSceneAsync("Game");
        while (!loadGame.isDone)
            yield return new WaitForSeconds(0.1f);

        GameBehaviour gameBehaviour = GameObject.FindWithTag("GameHandler").GetComponent<GameBehaviour>();
        // Create Snake from template, under the Players object (which has tag PlayerParent)
        GameObject snake = Instantiate(_snakeTemplate, GameObject.FindWithTag("PlayerParent").transform);
        gameBehaviour.WorldMode = GameBehaviour.EWorldMode.Offline;
        PlayerMovementController player = snake.GetComponentInChildren<PlayerMovementController>();
        gameBehaviour.SetupGame(player, new GameObject[] { snake });

        // Cleanup
        Destroy(gameObject);
        yield break;
    }
}
