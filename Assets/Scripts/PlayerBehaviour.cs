using Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
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
	private Tilemap groundTilemap;

	private List<Transform> body;
	private Dictionary<Transform, Vector2> bodyDirections;
	private bool rotating;
	private Dictionary<Vector2, int> rotationsToTileCounters;
	private Vector2 targetTileCentre;
	private bool reachedTargetTileCentre;

	private Rigidbody2D rb;

	private Vector2 lastTravellingDirection;
	private Vector2 travellingDirection;
	private Vector2 inputDirection;

	private float movementSpeed = 0.1f;

	// Start is called before the first frame update
	void Awake()
	{
		body = new List<Transform>();
		Transform head = transform.GetChild(0);
		Transform tail = transform.GetChild(transform.childCount - 1);

		body.Add(head);
		body.Add(tail);

		rotating = false;
		rotationsToTileCounters = new Dictionary<Vector2, int>();
		reachedTargetTileCentre = true;

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
		MovePlayer();

		if (reachedTargetTileCentre)
		{
			targetTileCentre = groundTilemap.CellToWorld(groundTilemap.WorldToCell(body[0].position)) + groundTilemap.cellSize / 2;
		}
		else if (Vectors.componentGreaterThanOrEqualTo(travellingDirection, targetTileCentre))
		{
			foreach (Vector2 rotation in rotationsToTileCounters.Keys)
			{
				int tileCounter = rotationsToTileCounters[rotation];
				rotationsToTileCounters[rotation] = tileCounter + 1;
				bodyDirections[body[tileCounter + 1]] = rotation;
			}
			reachedTargetTileCentre = true;
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

				if (travellingDirection != lastTravellingDirection)
				{
					//MovePlayer(-body[0].position + groundTilemap.CellToWorld(groundTilemap.WorldToCell(body[0].position)) + (groundTilemap.cellSize / 2));
					rotating = true;
					rotationsToTileCounters[travellingDirection] = 0;
					reachedTargetTileCentre = true;
				}
			}
		}
	}

	void MovePlayer()
	{
		foreach (Transform bodyPart in body)
		{
			bodyPart.transform.Translate(bodyDirections[bodyPart] * movementSpeed);
		}
	}
}
