using Steamworks;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject _snakeTemplate;
    [SerializeField]
    private GameObject _gameTemplate;
    [SerializeField]
    private GameObject _counterTemplate;

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

    private IEnumerator LoadGame()
    {
        SceneManager.LoadScene("Game");
        while (SceneManager.GetActiveScene().name != "Game")
            yield return new WaitForSeconds(1);
        // Create Game from template
        GameBehaviour gameBehaviour = Instantiate(_gameTemplate).GetComponent<GameBehaviour>();
        // Create Snake from template, under the Players object (which has tag PlayerParent)
        GameObject snake = Instantiate(_snakeTemplate, GameObject.FindWithTag("PlayerParent").transform);
        gameBehaviour.SetupGame(new GameObject[] { snake }, snake.GetComponentInChildren<PlayerBehaviour>());
        // Create Counter from template, set snake as parent, set player as listener
        GameObject counter = Instantiate(_counterTemplate, snake.transform);
        Counter counterScript = counter.GetComponent<Counter>();
        counterScript.SetListener(snake.transform.Find("Player").gameObject);
        counterScript.ThresholdSeconds = 0.2f;
        counterScript.Paused = false;
        // Cleanup
        Destroy(gameObject);
        yield break;
    }
}
