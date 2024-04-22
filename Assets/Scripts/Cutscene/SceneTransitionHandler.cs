using UnityEngine;

public class SceneTransitionHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject m_transitionSnakeSpawner;

    [SerializeField]
    protected string sceneName;
    protected bool isReady = false;
    protected SceneTransitionSnakeSpawner[] snakeSpawners;

    protected virtual void Start()
    {
        if (sceneName != "")
        {
            LoadSceneInBackground(sceneName);
        }

        GameObject tsp = GameObject.Find("TransitionSnakeSpawner");
        if (!tsp)
        {
            tsp = Instantiate(m_transitionSnakeSpawner);
            tsp.name = "TransitionSnakeSpawner";
        }

        // Get all the snake spawners in the scene, and store them in the snakeSpawners array.
        snakeSpawners = new SceneTransitionSnakeSpawner[tsp.transform.childCount];
        for (int i = 0; i < snakeSpawners.Length; i++)
        {
            snakeSpawners[i] = tsp.transform.GetChild(i).GetComponent<SceneTransitionSnakeSpawner>();
            snakeSpawners[i].transitionHandler = this;
        }
    }

    protected void LoadSceneInBackground(string name)
    {
        StartCoroutine(
            Wait.LoadSceneThenWait(
                name,
                () =>
                {
                    bool ready = GetReady();
                    SetReady(false); // E.g. if retry is used, ready will be used again later.
                    return ready;
                },
                0.1f
            )
        );
    }

    // Loads a scene with a loading symbol, and chungusnake anim.
    protected void LoadScene(string name)
    {
        LoadingIcon.Instance.Toggle(true);

        LoadSceneInBackground(name);

        foreach (var spawner in snakeSpawners)
            spawner.SpawnChungusnake();
    }

    protected void ReloadScene(string name)
    {
        LoadingIcon.Instance.Toggle(true);

        LoadSceneInBackground(name);
        SetReady(true);
    }

    public void SetReady(bool ready) { isReady = ready; }
    private bool GetReady() { return isReady; }
}