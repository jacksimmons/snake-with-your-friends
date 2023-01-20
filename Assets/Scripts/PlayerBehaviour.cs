using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Movement:
 * Moves smoothly, can't go back on itself (sign(d0) == sign(d1))
 * Movement is automatic, starting with the first input
 * Changing direction on one axis snaps your position on the other
 */

public class PlayerBehaviour : MonoBehaviour
{
	private Rigidbody2D rb;

	private Vector2 travellingDirection;
	private Vector2 inputDirection;

	private float movementSpeed = 0.1f;

	// Start is called before the first frame update
	void Start()
    {
		rb = gameObject.GetComponent<Rigidbody2D>();
	}

    // Update is called once per frame
    void Update()
    {
		HandleInputs();
    }

	void FixedUpdate()
	{
		rb.MovePosition(rb.position + travellingDirection * movementSpeed);
	}

	void HandleInputs()
	{
		// Movement
		float x_input = Input.GetAxisRaw("Horizontal");
		float y_input = Input.GetAxisRaw("Vertical");

		inputDirection = new Vector2(x_input, y_input);

		if (inputDirection != Vector2.zero)
		{
			// Ensure we don't assign a zero vector to the travelling direction; the snake can never stop!
			if (inputDirection.x != 0 && inputDirection.y != 0)
			{
				// If multiple axis inputs are present, prefers an axis if it is equal to that of the travellingDirection
				if (inputDirection.y == travellingDirection.y)
				{
					inputDirection = Vector2.up * inputDirection.y;
				}
				else
				{
					// We use x even if neither axes are equal; this is the default case.
					inputDirection = Vector2.right * inputDirection.x;
				}
			}

			if (inputDirection.x == 0 && Math.Sign(inputDirection.y) != -Math.Sign(travellingDirection.y)
				|| inputDirection.y == 0 && Math.Sign(inputDirection.x) != -Math.Sign(travellingDirection.x))
			{
				// If inputDirection doesn't go back on itself
				travellingDirection = inputDirection;
			}
		}
	}
}
