using UnityEngine;

public class SceneTransitionSnakeSpawner : CanvasSnakeSpawner
{
    public SceneTransitionHandler transitionHandler;
    [SerializeField]
    private Vector2Comparison comparison;


    protected override void Start()
    {
        base.Start();

        DontDestroyOnLoad(transform.parent);
    }


    public void SpawnChungusnake()
    {
        BigSnake = SpawnSnake(
            2,
            125f,
            0f,
            5
        );
    }


    private void Update()
    {
        if (BigSnake)
        {
            Transform head = BigSnake.transform.GetChild(0);
            if (comparison.CompareVector2((Vector2)head.position))
            {
                transitionHandler.SetReady(true);
            }
        }
    }
}
