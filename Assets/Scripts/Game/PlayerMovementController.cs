using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class PlayerMovementController : NetworkBehaviour
{
    public float speed = 0.1f;

    [SerializeField]
    public GameObject playerSprites;


    private void Start()
    {
        playerSprites.SetActive(false);
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {
            if (!playerSprites.activeSelf)
            {
                SetPosition();
                playerSprites.SetActive(true);
            }

            // So that we only move a player if we have authority over it
            if (isOwned)
            {
                Movement();
            }
        }
    }

    public void SetPosition()
    {
        transform.position = new Vector3(Random.Range(-5, 5), 0f, 0f);
    }


    public void Movement()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        Vector2 moveDirection = new Vector2(x, y);
        Rigidbody2D rb = GetComponentInChildren<Rigidbody2D>();
        rb.MovePosition(rb.position + moveDirection * speed);
    }
}
