using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuSnakeSpawner : CanvasSnakeSpawner
{
    private const float MIN_SPAWN_TIME = 0.1f;
    private const float MAX_SPAWN_TIME = 10f;

    private const int MIN_PARTS = 2;
    private const int MAX_PARTS = 10;

    private const float BIG_MIN_SPAWN_TIME = 0.1f;
    private const float BIG_MAX_SPAWN_TIME = 120f;

    private const int BIG_MIN_PARTS = 4;
    private const int BIG_MAX_PARTS = 8;

    private const float BIG_SNAKE_SCALE = 50f;

    private float timeSinceLastSpawn = 0;

    private float spawnTime;
    private float bigSpawnTime;

    private float timeSinceLastBigSpawn = 0;


    protected override void Start()
    {
        base.Start();

        GetRandomSpawnTime();
        GetRandomBigSpawnTime();
    }


    private void Update()
    {
        timeSinceLastSpawn += Time.deltaTime;
        timeSinceLastBigSpawn += Time.deltaTime;

        // Spawn a normal snake
        if (timeSinceLastSpawn >= spawnTime)
        {
            timeSinceLastSpawn = 0;

            float snakeScale = spawnTime;
            GetRandomSpawnTime();

            // Exclusive (MIN_PARTS to MAX_PARTS - 1)
            int numParts = Random.Range(MIN_PARTS, MAX_PARTS);

            SpawnSnake(numParts, snakeScale, Random.Range(MIN_MOVE_TIME, MAX_MOVE_TIME));
        }

        // Spawn a BIG snake
        if (timeSinceLastBigSpawn >= bigSpawnTime && !BigSnake)
        {
            timeSinceLastBigSpawn = 0;

            GetRandomBigSpawnTime();

            // Exclusive (MIN_PARTS to MAX_PARTS - 1)
            int numParts = Random.Range(BIG_MIN_PARTS, BIG_MAX_PARTS);

            BigSnake = SpawnSnake(numParts, BIG_SNAKE_SCALE, Random.Range(MIN_MOVE_TIME, MAX_MOVE_TIME));
        }
    }

    private void GetRandomSpawnTime()
    {
        spawnTime = RandomDistribution.RandomGaussian(MIN_SPAWN_TIME, MAX_SPAWN_TIME);
    }

    private void GetRandomBigSpawnTime()
    {
        bigSpawnTime = RandomDistribution.RandomGaussian(BIG_MIN_SPAWN_TIME, BIG_MAX_SPAWN_TIME);
    }
}
