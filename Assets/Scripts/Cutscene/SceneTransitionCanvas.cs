using UnityEngine;

public class SceneTransitionCanvas : MonoBehaviour
{
    private Canvas canvas;
    [SerializeField]
    private SceneTransitionSnakeSpawner[] spawners;
    [SerializeField]
    private Vector2Comparison deathComparison;


    private void Start()
    {
        canvas = GetComponent<Canvas>();
    }


    // Update is called once per frame
    private void LateUpdate()
    {
        if (canvas.worldCamera == null)
            canvas.worldCamera = Camera.main;
        else
        {
            foreach (var spawner in spawners)
            {
                if (spawner.BigSnake)
                {
                    if (deathComparison.CompareVector2(spawner.BigSnake.transform.position))
                        Destroy(gameObject);
                }
            }
        }
    }
}
