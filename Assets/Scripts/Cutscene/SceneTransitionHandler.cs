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
            LoadSceneInBackground(sceneName);
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