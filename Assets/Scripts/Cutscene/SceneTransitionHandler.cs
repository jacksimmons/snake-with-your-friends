using UnityEngine;

public class SceneTransitionHandler : MonoBehaviour
{
    [SerializeField]
    protected string sceneName;
    protected bool isReady = false;
    [SerializeField]
    protected SceneTransitionSnakeSpawner[] snakeSpawners;

    protected virtual void Start()
    {
        if (sceneName != "")
        {
            LoadSceneInBackground();
        }
    }

    protected void LoadSceneInBackground()
    {
        StartCoroutine(
            Wait.LoadSceneThenWait(
                sceneName,
                GetReady,
                new WaitForSeconds(0.1f)
            )
        );
    }

    protected void LoadScene(string name)
    {
        sceneName = name;
        LoadSceneInBackground();
        LoadingIcon.Instance.Toggle(true);
        
        foreach (var spawner in snakeSpawners)
            spawner.SpawnChungusnake();
    }

    public void SetReady(bool ready) { isReady = ready; }
    private bool GetReady() { return isReady; }
}