using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MainMenuSnakeActor : MonoBehaviour
{
    private const float MIN_MOVE_TIME = 0.1f;
    private const float MAX_MOVE_TIME = 0.75f;

    private float timeSinceLastMove = 0;
    private float moveTime;

    private float bpWidth;

    private void Start()
    {
        moveTime = Random.Range(MIN_MOVE_TIME, MAX_MOVE_TIME);
        RectTransform head = (RectTransform)transform.GetChild(0);
        bpWidth = head.rect.width * transform.localScale.x;
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

        if (CalculateTailEndPos() > 250)
            Destroy(gameObject);
    }

    private float CalculateTailEndPos()
    {
        return
            ((RectTransform)transform).anchoredPosition.x -
            bpWidth * transform.childCount;
    }
}
