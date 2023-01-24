using Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Tilemaps;

/* Movement:
 * Moves smoothly, can't go back on itself (sign(d0) == sign(d1))
 * Movement is automatic, starting with the first input
 * Changing direction on one axis snaps your position on the other
 */

public class PlayerBehaviour : MonoBehaviour
{
	[SerializeField]
	private GameObject groundTilemapObject;
	public Tilemap groundTilemap;

	public List<BodyPartBehaviour> body;

	private Rigidbody2D rb;

	// Simple boolean which gets set to false after the starting direction is set
	private bool firstDirection = true;
	private Vector2 inputDirection;
	private Vector2 lastTravellingDirection;
	private Vector2 lastDifferentTravellingDirection;
	public Vector2 travellingDirection;

	// These points are used with the current direction to determine whether the
	// body part should turn or not.
	public List<Vector2> cutoffPoints;

	private float movementSpeed = 0.1f;
	private bool moving = false;

	// Start is called before the first frame update
	void Awake()
	{
		body = new List<BodyPartBehaviour>();
		BodyPartBehaviour head = transform.GetChild(0).GetComponent<BodyPartBehaviour>();
		BodyPartBehaviour tail = transform.GetChild(transform.childCount - 1).GetComponent<BodyPartBehaviour>();

		body.Add(head);
		body.Add(tail);

		groundTilemap = groundTilemapObject.GetComponent<Tilemap>();
		rb = gameObject.GetComponent<Rigidbody2D>();
	}

	// Update is called once per frame
	void Update()
	{
		HandleInputs();
	}

	void FixedUpdate()
	{
		if (travellingDirection != lastTravellingDirection)
		{
			lastDifferentTravellingDirection = lastTravellingDirection;
			// We have rotated. Get position of head; this is the turn cutoff point
			// THEN, rotate the head.
			Vector2 cutoffPoint = body[0].transform.position;
			cutoffPoints.Add(cutoffPoint);

			print("HI");
			body[0].Rotate();

			foreach (BodyPartBehaviour bph in body)
			{
				bph.SetCutoffPointIfNone(cutoffPoint);
			}
		}

		if (moving)
		{
			foreach (BodyPartBehaviour bph in body)
			{
				if (firstDirection)
				{
					bph.direction = travellingDirection;
				}
				bph.Move(movementSpeed);
			}
			firstDirection = false;
		}
	}

	void HandleInputs()
	{
		// Movement
		float x_input = Input.GetAxisRaw("Horizontal");
		float y_input = Input.GetAxisRaw("Vertical");

		lastTravellingDirection = travellingDirection;
		inputDirection = new Vector2(x_input, y_input);

		if (inputDirection != Vector2.zero)
		{
			// Ensure we don't assign a zero vector to the travelling direction; the snake can never stop!
			if (inputDirection.x != 0 && inputDirection.y != 0)
			{
				// If multiple axis inputs are present, prefers an axis if it is equal to that of the travellingDirection
				if (inputDirection.y == lastTravellingDirection.y)
				{
					inputDirection = Vector2.up * inputDirection.y;
				}
				else
				{
					// We use x even if neither axes are equal; this is the default case.
					inputDirection = Vector2.right * inputDirection.x;
				}
			}

			if (inputDirection.x == 0 && Math.Sign(inputDirection.y) != -Math.Sign(lastTravellingDirection.y)
				|| inputDirection.y == 0 && Math.Sign(inputDirection.x) != -Math.Sign(lastTravellingDirection.x))
			{
				// If inputDirection doesn't go back on itself
				travellingDirection = inputDirection;
			}

			moving = true;
		}
		else
			moving = false;
	}
}
