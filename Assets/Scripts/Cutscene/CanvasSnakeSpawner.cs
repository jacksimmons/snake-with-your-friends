using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasSnakeSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject m_snakeActorTemplate;

    private const float SNAKE_SCALE_MOD = 0.25f;

    protected const float MIN_MOVE_TIME = 0.1f;
    protected const float MAX_MOVE_TIME = 0.75f;

    protected float randomHeightAddMax = 0f;
    public GameObject BigSnake { get; protected set; } = null;


    protected virtual void Start()
    {
        randomHeightAddMax = GetComponent<RectTransform>().rect.height / 2;
    }


    public GameObject SpawnSnake(int numParts, float snakeScale, float moveTime, int speed = 1)
    {
        GameObject spawned = Instantiate(m_snakeActorTemplate, transform);

        SnakeActor actor = spawned.GetComponent<SnakeActor>();
        actor.moveTime = moveTime;
        actor.speed = speed;

        spawned.transform.GetChild(0).GetComponent<Image>().sprite =
            GetRandomSprite(ECustomisationPart.Head);
        spawned.transform.GetChild(1).GetComponent<Image>().sprite =
            GetRandomSprite(ECustomisationPart.Tail);

        if (numParts > 2)
        {
            AddMoreBodyParts(spawned, numParts);
        }

        spawned.transform.localScale = Vector2.one * snakeScale * SNAKE_SCALE_MOD;
        spawned.transform.localPosition =
            Vector3.up * Random.Range(-randomHeightAddMax, randomHeightAddMax);

        return spawned;
    }


    private void AddMoreBodyParts(GameObject spawned, int numParts)
    {
        Transform tail = spawned.transform.GetChild(1); // The template starts as a 2 part snake
        float width = ((RectTransform)tail).rect.width;

        Sprite randomTorsoSprite = GetRandomSprite(ECustomisationPart.Torso);

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


    private Sprite GetRandomSprite(ECustomisationPart part)
    {
        Sprite[] torsoSprites = Resources.LoadAll<Sprite>($"Snake/RedPurple/{part}");
        int randIndex = Random.Range(0, torsoSprites.Length);
        return torsoSprites[randIndex];
    }


    protected float GetRandomSpeed()
    {
        return Random.Range(MIN_MOVE_TIME, MAX_MOVE_TIME);
    }
}
