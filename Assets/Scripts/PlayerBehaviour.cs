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
	[SerializeField]
	private GameBehaviour game;

	[SerializeField]
	private Sprite straightPiece;
	[SerializeField]
	private Sprite cornerPiece;

	// Simple boolean which gets set to false after the starting direction is set
	private bool firstDirectionNotSet = true;
	private bool firstDirectionNotApplied = true;
	public Vector2 direction = Vector2.zero;
	public Vector2 lastTravellingDirection = Vector2.zero;

	// These points are used with the current direction to determine whether the
	// body part should turn or not.
	[SerializeField]
	private GameObject bodyPartTemplate;
	private List<Vector2> bodyPartDirections;
	private List<bool> bodyPartIsCorner;

	// On next tile events
	private List<float> nextRotations;
	// For when the player inputs a turn, but we don't want to change the nextRotation until the move cycle is complete
	// We want every body part to experience the same rotation in each cycle
	private List<float> delayedNextRotations;

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

		delayedNextRotations = new List<float>();
		nextRotations = new List<float>();
		bodyPartIsCorner = new List<bool>(); 

		for (int i = 0; i < transform.childCount; i++)
		{
			bodyPartDirections.Add(Vector2.zero);
			bodyPartIsCorner.Add(false);
			nextRotations.Add(0f);
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

		if (lastTravellingDirection != Vector2.zero)
		{
			// Above ensures that the first direction has been selected
			if (firstDirectionNotSet)
			{
				bodyPartDirections[0] = lastTravellingDirection;
				nextRotations[0] = Vector2.SignedAngle(Vector2.up, lastTravellingDirection);

				firstDirectionNotSet = false;
				// firstDirectionNotApplied set to false in the moveTimer >= moveTime if statement
				// This is to prevent starting gap between the body parts (more precision on when each part starts moving)
			}

			if (lastTravellingDirection != bodyPartDirections[0])
			{
				// Sets the timer back to the first moveTime seamlessly (if it's part way through a moveTime it retains this progression)
				delayedNextRotations.Add(Vector2.SignedAngle(bodyPartDirections[0], lastTravellingDirection));
				bodyPartDirections[0] = lastTravellingDirection;
			}

			if (moveTimer >= moveTime)
			{
				// Reset the timer(s)
				if (timer >= transform.childCount * moveTime)
				{
					timer = 0;
				}
				moveTimer = 0;

				// Apply first direction if not done yet
				if (firstDirectionNotApplied)
				{
					for (int i = 1; i < transform.childCount; i++)
					{
						// Set the direction AFTER the previous part (i) has moved, so that the next move includes this part
						// The direction is Vector2.up, ASSUMING every snake always starts facing up
						// This is so the body parts behind the head go towards the head, then turn when the head first turns
						bodyPartDirections[i] = Vector2.up;
					}
					firstDirectionNotApplied = false;
				}

				// Update rotations
				// We need to allow rotations to occur every tile jump, so a nextRotation property for every child is needed
				// The first direction needs to be able to bypass this to set the rotation value
				if (!firstDirectionNotApplied)
				{
					if (delayedNextRotations.Count > 0)
					{
						nextRotations[0] = delayedNextRotations[0];
						delayedNextRotations.RemoveAt(0);
					}
					else
					{
						nextRotations[0] = 0f;
					}
				}

				// Rotate all non-corner children
				for (int i = 0; i < transform.childCount; i++)
				{
					if (!bodyPartIsCorner[i])
						transform.GetChild(i).Rotate(Vector3.forward * nextRotations[i]);
				}

				// Move children
				for (int i = 0; i < transform.childCount; i++)
				{
					Transform child = transform.GetChild(i);
					child.Translate(Quaternion.Inverse(child.rotation) * bodyPartDirections[i] * movementSpeed);
				}

				// Handle assignment of corners after movement, as it just became visibly necessary
				// Doing this before would make this a straight when it should be a corner
				HandleCorners();
				HandleNotCorners();

				// Assign the next direction for every child
				// Negative iteration is used so dn -> dn-1, ..., d3 -> d2, d2 -> d1, so dn != d1
				// Positive iteration would mean d2 -> d1, d3 -> d2, ..., dn -> dn-1 so dn = dn-1 = ... = d3 = d2 = d1
				for (int i = transform.childCount - 1; i > 0; i--)
				{
					bodyPartDirections[i] = bodyPartDirections[i - 1];
					nextRotations[i] = nextRotations[i - 1];
				}

				string output_B = "";
				string output_R = "";
				for (int i = 0; i < transform.childCount; i++)
				{
					output_B += bodyPartDirections[i].ToString() + ",";
					output_R += nextRotations[i].ToString() + ",";
				}

				print("B: " + output_B);
				print("R: " + output_R);
			}
		}
	}

	void HandleInputs()
	{
		// Movement
		float x_input = Input.GetAxisRaw("Horizontal");
		float y_input = Input.GetAxisRaw("Vertical");

		if (direction != Vector2.zero)
			lastTravellingDirection = direction;

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
		// This will make the lastTravellingDirection not update next Update
		if (direction == -lastTravellingDirection)
			direction = Vector2.zero;
	}

	void HandleCorners()
	{
		for (int i = 0; i < transform.childCount - 2; i++)
		{
			if (bodyPartDirections[i] != bodyPartDirections[i + 2])
			{
				if (!bodyPartIsCorner[i + 1])
				{
					MakePartCorner(i + 1);
					bodyPartIsCorner[i + 1] = true;
				}
			}
		}
	}

	void HandleNotCorners()
	{
		for (int i = 0; i < transform.childCount - 2; i++)
		{
			if (bodyPartDirections[i] == bodyPartDirections[i + 2])
			{
				if (bodyPartIsCorner[i + 1])
				{
					MakePartNotCorner(i + 1);
					bodyPartIsCorner[i + 1] = false;
				}
			}
		}
	}

	void MakePartCorner(int i)
	{
		Vector2 before = bodyPartDirections[i - 1];
		Vector2 after = bodyPartDirections[i + 1];

		Transform child = transform.GetChild(i);
		SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
		sr.sprite = cornerPiece;

		// L corner (|_)
		if (before == Vector2.left && after == Vector2.up
		|| before == Vector2.down && after == Vector2.right)
		{
			child.rotation = Quaternion.Euler(Vector3.forward * 180);
		}

		// r corner
		else if (before == Vector2.left && after == Vector2.down
		|| before == Vector2.up && after == Vector2.right)
		{
			child.rotation = Quaternion.Euler(Vector3.forward * 90);
		}

		// Flipped r (¬) corner
		else if (before == Vector2.right && after == Vector2.down
		|| before == Vector2.up && after == Vector2.left)
		{
			child.rotation = Quaternion.Euler(Vector3.forward * 0);
		}

		// Flipped L (_|) corner
		else if (before == Vector2.right && after == Vector2.up
		|| before == Vector2.down && after == Vector2.left)
		{
			child.rotation = Quaternion.Euler(Vector3.forward * 270);
		}
	}

	void MakePartNotCorner(int i)
	{
		Transform child = transform.GetChild(i);
		SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
		sr.sprite = straightPiece;
		child.rotation = transform.GetChild(i - 1).rotation;
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
