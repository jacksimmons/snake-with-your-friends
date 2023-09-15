using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuSnakeSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject m_snakeActorTemplate;

    private const float MIN_SPAWN_TIME = 0.1f;
    private const float MAX_SPAWN_TIME = 20f;

    private const float SNAKE_SCALE_MOD = 0.25f;

    private float timeSinceLastSpawn = 0;
    private float spawnTime;

    private float randomHeightAddMax;


    private void Start()
    {
        randomHeightAddMax = GetComponent<RectTransform>().rect.height / 2;
        print(randomHeightAddMax);
        GetRandomSpawnTime();
    }


    private void Update()
    {
        timeSinceLastSpawn += Time.deltaTime;

        if (timeSinceLastSpawn >= spawnTime)
        {
            timeSinceLastSpawn = 0;

            float snakeScale = spawnTime;
            GetRandomSpawnTime();

            GameObject spawned = Instantiate(m_snakeActorTemplate, transform);
            spawned.transform.localScale = Vector2.one * snakeScale * SNAKE_SCALE_MOD;
            spawned.transform.localPosition =
                Vector3.up * Random.Range(-randomHeightAddMax, randomHeightAddMax);
        }
    }

    private void GetRandomSpawnTime()
    {
        spawnTime = RandomDistribution.RandomGaussian(MIN_SPAWN_TIME, MAX_SPAWN_TIME);
    }
}
