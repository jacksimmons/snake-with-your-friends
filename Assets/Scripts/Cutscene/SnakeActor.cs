using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SnakeActor : MonoBehaviour
{
    private float timeSinceLastMove = 0;
    private float bpWidth;
    public float moveTime;
    public float speed;

    private void Start()
    {
        RectTransform head = (RectTransform)transform.GetChild(0);
        bpWidth = head.rect.width * transform.localScale.x;
    }

    // Update is called once per frame
    private void Update()
    {
        Move();
    }

    private void Move()
    {
        timeSinceLastMove += Time.fixedDeltaTime;

        if (timeSinceLastMove >= moveTime)
        {
            timeSinceLastMove = 0;
            transform.localPosition += Vector3.right * speed;
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
