using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MainMenuSnakeActor : MonoBehaviour
{
    private float timeSinceLastMove = 0;
    private float bpWidth;
    public float moveTime;
    public int continuousSpeed;

    private void Start()
    {
        RectTransform head = (RectTransform)transform.GetChild(0);
        bpWidth = head.rect.width * transform.localScale.x;
    }

    // Update is called once per frame
    private void Update()
    {
        if (continuousSpeed != 0)
        {
            MoveContinuous();
        }
        else
        {
            MoveDiscrete();
        }
    }

    private void MoveContinuous()
    {
        transform.localPosition += Vector3.right * continuousSpeed * Time.fixedDeltaTime;
    }

    private void MoveDiscrete()
    {
        timeSinceLastMove += Time.deltaTime;

        if (timeSinceLastMove >= moveTime)
        {
            timeSinceLastMove = 0;
            transform.localPosition += Vector3.right;
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
