using Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using TreeEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

/* Movement:
 * Moves smoothly, can't go back on itself (sign(d0) == sign(d1))
 * Movement is automatic, starting with the first input
 * Changing direction on one axis snaps your position on the other
 */

public class PlayerBehaviour : MonoBehaviour
{
	private Vector2 startingDirection = Vector2.up;
	private float startingRotation = 0f;

	[SerializeField]
	private GameBehaviour game;

	[SerializeField]
	private Sprite straightPiece;
	[SerializeField]
	private Sprite cornerPiece;

	// Simple boolean which gets set to false after the starting direction is set
	private bool firstDirectionNotSet = true;
	public Vector2 direction = Vector2.zero;
	// The last valid, non-zero direction vector
	public Vector2 movement = Vector2.zero;

	// These points are used with the current direction to determine whether the
	// body part should turn or not.
	[SerializeField]
	private GameObject bodyPartTemplate;
	private List<bool> bodyPartIsCorner;

	private List<Vector2> bodyPartDirections;
	private Queue<Vector2> directionQueue;

	// On next tile events
	private List<float> bodyPartRotations;
	// For when the player inputs a turn, but we don't want to change the nextRotation until the move cycle is complete
	// We want every body part to experience the same rotation in each cycle
	private Queue<float> rotationQueue;

	public Transform head;
	public Transform tail;

	private float movementSpeed = 1f;
	// Increments to moveTime * childCount, then resets
	public int timer = 0;
	// Increments to moveTime, then resets
	public int moveTimer = 0;
	private int moveTime = 20;

	// Start is called before the first frame update
	void Awake()
	{
		head = transform.GetChild(0);
		tail = transform.GetChild(transform.childCount - 1);

		bodyPartDirections = new List<Vector2>();

		directionQueue = new Queue<Vector2>();
		rotationQueue = new Queue<float>();
		bodyPartRotations = new List<float>();
		bodyPartIsCorner = new List<bool>();

		for (int i = 0; i < transform.childCount; i++)
		{
			bodyPartDirections.Add(startingDirection);
			bodyPartIsCorner.Add(false);
			bodyPartRotations.Add(startingRotation);
		}
	}

	// Update is called once per frame
	void Update()
	{
		HandleInputs();
	}

	void FixedUpdate()
	{
		// Increment the timers every FixedUpdate
		timer++;
		moveTimer++;

		// .Handle first direction
		// .Increment move time, reset counter if it's passed the number of body parts
		// - Handle new body parts in the middle of a movetime
		// .Set direction = movement
		// Set sprite rotation = angle between prev. dir and lastTravDir.
		// Calculate what WILL be a corner piece after movement
		// Apply corner sprites
		// Move the entire snake, excluding corner pieces which act as pipes

		// Ensure the first movement has been made
		if (movement != Vector2.zero)
		{
			//if (firstDirectionNotSet)
			//	SetFirstDirection();

			// If a rotation is required, add it to the end of the queue.
			if (directionQueue.Count == 0 && movement != bodyPartDirections[0])
			{
				rotationQueue.Enqueue(Vector2.SignedAngle(bodyPartDirections[0], movement));
				directionQueue.Enqueue(movement);
			}

			if (moveTimer >= moveTime)
			{
				// Reset the timer(s)
				if (timer >= transform.childCount * moveTime)
				{
					timer = 0;
				}
				moveTimer = 0;

				// Update rotations
				// We need to allow rotations to occur every tile jump, so a nextRotation property for every child is needed
				// The first direction needs to be able to bypass this to set the rotation value
				if (rotationQueue.Count > 0)
					bodyPartRotations[0] = rotationQueue.Dequeue();
				else
					bodyPartRotations[0] = 0f;

				// Update direction (only if there is one)
				if (directionQueue.Count > 0)
					bodyPartDirections[0] = directionQueue.Dequeue();

				// Rotate all non-corner children
				for (int i = 0; i < transform.childCount; i++)
				{
					//if (!bodyPartIsCorner[i])
					transform.GetChild(i).Rotate(Vector3.forward * bodyPartRotations[i]);
				}

				// Move children
				for (int i = 0; i < transform.childCount; i++)
				{
					Transform child = transform.GetChild(i);
					child.Translate(Quaternion.Inverse(child.rotation) * bodyPartDirections[i] * movementSpeed);
				}

				// Assign the next direction for every child
				// Negative iteration is used so dn -> dn-1, ..., d3 -> d2, d2 -> d1, so dn != d1
				// Positive iteration would mean d2 -> d1, d3 -> d2, ..., dn -> dn-1 so dn = dn-1 = ... = d3 = d2 = d1
				for (int i = transform.childCount - 1; i > 0; i--)
				{
					bodyPartDirections[i] = bodyPartDirections[i - 1];
					bodyPartRotations[i] = bodyPartRotations[i - 1];
				}

				// Corners

				string output_B = "";
				string output_R = "";
				string output_C = "";
				for (int i = 0; i < transform.childCount; i++)
				{
					output_B += bodyPartDirections[i].ToString() + ",";
					output_R += bodyPartRotations[i].ToString() + ",";
					output_C += bodyPartIsCorner[i].ToString() + ",";
				}

				//print("B: " + output_B);
				//print("R: " + output_R);
				print("C: " + output_C);
			}
		}
	}

	void HandleInputs()
	{
		// Movement
		float x_input = Input.GetAxisRaw("Horizontal");
		float y_input = Input.GetAxisRaw("Vertical");

		if (direction != Vector2.zero)
			movement = direction;

		if (x_input > 0)
			direction = Vector2.right;
		else if (x_input < 0)
			direction = Vector2.left;
		else
			direction = Vector2.zero;

		if (direction == Vector2.zero)
		{
			if (y_input > 0)
				direction = Vector2.up;
			else if (y_input < 0)
				direction = Vector2.down;
			else
				direction = Vector2.zero;
		}

		// We can't have the snake going back on itself.
		// This will make the movement not update next Update
		if (direction == -movement)
			direction = Vector2.zero;
	}

	void AddBodyPart()
	{
		// Adds a new body part as a child, then moves the tail after it
		GameObject bodyPart = Instantiate(bodyPartTemplate, gameObject.transform);
		// Makes the new body part have the same direction, indexed rotation and raw rotation as the tail
		bodyPart.transform.rotation = tail.rotation;
		tail.SetAsLastSibling();
	}

	void TrimAtIndex(int index)
	{

		/*childDirections.RemoveAt(childDirections.Count - 1);
		childIndexedRotations.RemoveAt(childDirections.Count - 1);
		Destroy(bodyPart);*/
	}
}
