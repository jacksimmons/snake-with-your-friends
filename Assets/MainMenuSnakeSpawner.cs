using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuSnakeSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject m_snakeActorTemplate;

    private const float MIN_SPAWN_TIME = 0.1f;
    private const float MAX_SPAWN_TIME = 10f;

    private const int MIN_PARTS = 2;
    private const int MAX_PARTS = 10;

    private const float SNAKE_SCALE_MOD = 0.25f;

    private const float BIG_MIN_SPAWN_TIME = 0.1f;
    private const float BIG_MAX_SPAWN_TIME = 120f;

    private const int BIG_MIN_PARTS = 4;
    private const int BIG_MAX_PARTS = 8;

    private const float BIG_SNAKE_SCALE = 50f;

    private float timeSinceLastSpawn = 0;

    private float spawnTime;
    private float bigSpawnTime;
    private GameObject bigSnake = null;

    private float timeSinceLastBigSpawn = 0;

    private float randomHeightAddMax;


    private void Start()
    {
        randomHeightAddMax = GetComponent<RectTransform>().rect.height / 2;
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

            SpawnSnake(numParts, snakeScale);
        }

        // Spawn a BIG snake
        if (timeSinceLastBigSpawn >= bigSpawnTime && !bigSnake)
        {
            timeSinceLastBigSpawn = 0;

            GetRandomBigSpawnTime();

            // Exclusive (MIN_PARTS to MAX_PARTS - 1)
            int numParts = Random.Range(BIG_MIN_PARTS, BIG_MAX_PARTS);

            bigSnake = SpawnSnake(numParts, BIG_SNAKE_SCALE);
        }
    }

    private GameObject SpawnSnake(int numParts, float snakeScale)
    {
        GameObject spawned = Instantiate(m_snakeActorTemplate, transform);

        spawned.transform.GetChild(0).GetComponent<Image>().sprite =
            GetRandomSprite("Heads");
        spawned.transform.GetChild(1).GetComponent<Image>().sprite =
            GetRandomSprite("Tails");

        if (numParts > 2)
        {
            AddMoreBodyParts(spawned, numParts);
        }

        spawned.transform.localScale = Vector2.one * snakeScale * SNAKE_SCALE_MOD;
        spawned.transform.localPosition =
            Vector3.up * Random.Range(-randomHeightAddMax, randomHeightAddMax);

        return spawned;
    }

    private void GetRandomSpawnTime()
    {
        spawnTime = RandomDistribution.RandomGaussian(MIN_SPAWN_TIME, MAX_SPAWN_TIME);
    }

    private void GetRandomBigSpawnTime()
    {
        bigSpawnTime = RandomDistribution.RandomGaussian(BIG_MIN_SPAWN_TIME, BIG_MAX_SPAWN_TIME);
    }

    private void AddMoreBodyParts(GameObject spawned, int numParts)
    {
        Transform tail = spawned.transform.GetChild(1); // The template starts as a 2 part snake
        float width = ((RectTransform)tail).rect.width;

        Sprite randomTorsoSprite = GetRandomSprite("Torsos");

        // Generate, and place correctly the right number of parts.
        for (int i = 0; i < numParts - 2; i++)
        {
            GameObject newPart = Instantiate(tail.gameObject, tail.parent);

            // Using Vector2.down, since every bp is rotated 90 deg to face right.
            newPart.GetComponent<RectTransform>().anchoredPosition
                += i * width * Vector2.down;

            newPart.GetComponent<Image>().sprite = randomTorsoSprite;
        }

        // Using Vector2.down, since every bp is rotated 90 deg to face right.
        ((RectTransform)tail).anchoredPosition += (numParts - 2) * width * Vector2.down;
    }

    private Sprite GetRandomSprite(string outfitComponent)
    {
        Sprite[] torsoSprites = Resources.LoadAll<Sprite>($"Snake/RedPurple/{outfitComponent}");
        int randIndex = Random.Range(0, torsoSprites.Length);
        return torsoSprites[randIndex];
    }
}
