using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassiveScrollingSnake : MonoBehaviour
{
    private Vector3 m_startingPos;
    [SerializeField]
    private float m_offscreenX;

    private float m_timer = 0;
    private bool m_isMoving = false;
    private float m_speed = 0.1f;

    private void Start()
    {
        m_startingPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_isMoving)
        {
            m_timer += Time.deltaTime;
            if (m_timer > 180)
            {
                m_timer = 0;
                transform.position = m_startingPos;
                m_isMoving = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if (m_isMoving)
        {
            if (transform.position.x <= m_offscreenX)
            {
                transform.position += (Vector3)(Vector2.right * m_speed);
            }
            else
                m_isMoving = false;
        }
    }
}
