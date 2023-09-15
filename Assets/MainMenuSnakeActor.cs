using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuSnakeActor : MonoBehaviour
{
    private const float MIN_MOVE_TIME = 0.1f;
    private const float MAX_MOVE_TIME = 0.75f;

    private float timeSinceLastMove = 0;
    private float moveTime;

    private void Start()
    {
        moveTime = Random.Range(MIN_MOVE_TIME, MAX_MOVE_TIME);
    }

    // Update is called once per frame
    private void Update()
    {
        timeSinceLastMove += Time.deltaTime;

        if (timeSinceLastMove >= moveTime)
        {
            timeSinceLastMove = 0;
            transform.position += Vector3.right;
        }

        if (transform.position.x > 200)
            Destroy(gameObject);
    }
}
