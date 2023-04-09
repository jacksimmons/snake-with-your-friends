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
	private Vector2 _startingDirection = Vector2.up;

	[SerializeField]
	private Sprite _straightPiece;
	[SerializeField]
	private Sprite _cornerPiece;

	// Simple boolean which gets set to false after the starting direction is set
	private bool _firstDirectionNotSet = true;
	public Vector2 direction = Vector2.zero;
	// The last valid, non-zero direction vector
	public Vector2 movement = Vector2.zero;

	private Queue<Vector2> _directionQueue;

	public BodyPart head;
	public BodyPart tail;
	private List<BodyPart> _bodyParts;

	private float _movementSpeed = 1f;
	// Increments to _moveTime * childCount, then resets
	public int timer = 0;
	// Increments to _moveTime, then resets
	public int moveTimer = 0;
	private int _moveTime = 20;

	// Start is called before the first frame update
	void Awake()
	{
		_bodyParts = new List<BodyPart>();
		head = new BodyPart();
		tail = new BodyPart();
		head.transform = transform.GetChild(0);
		tail.transform = transform.GetChild(transform.childCount - 1);
		head.direction = _startingDirection;
		tail.direction = _startingDirection;
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
		// - Handle new body parts in the middle of a _moveTime
		// .Set direction = movement
		// Set sprite rotation = angle between prev. dir and lastTravDir.
		// Calculate what WILL be a corner piece after movement
		// Apply corner sprites
		// Move the entire snake, excluding corner pieces which act as pipes

		// Ensure the first movement has been made
		if (movement != Vector2.zero)
		{
			//if (_firstDirectionNotSet)
			//	SetFirstDirection();

			// If a rotation is required, add it to the end of the queue.
			if (_directionQueue.Count == 0 && movement != bodyPartDirections[0])
			{
				rotationQueue.Enqueue(Vector2.SignedAngle(bodyPartDirections[0], movement));
				_directionQueue.Enqueue(movement);
			}

			if (moveTimer >= _moveTime)
			{
				// Reset the timer(s)
				if (timer >= transform.childCount * _moveTime)
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
				if (_directionQueue.Count > 0)
					bodyPartDirections[0] = _directionQueue.Dequeue();

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
					child.Translate(Quaternion.Inverse(child.rotation) * bodyPartDirections[i] * _movementSpeed);
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
		BodyPart bodyPart = new BodyPart(_straightPiece);
		bodyPart.transform.SetParent(transform);
		// Makes the new body part have the same direction, indexed rotation and raw rotation as the tail
		bodyPart.direction = tail.direction;
		bodyPart.transform.rotation = tail.rotation;
		tail.SetAsLastSibling();
	}

	public class BodyPart
	{
		private Vector2 direction;
		private Transform transform;

		// For a body part with pre-existing transform
		public BodyPart();

		// For a body part without pre-existing transform
		public BodyPart(Sprite sprite)
		{
			GameObject bodyPartGameObject = new GameObject("Body", typeof(SpriteRenderer));
			bodyPartGameObject.GetComponent<SpriteRenderer>().sprite = sprite;
			this.transform = bodyPartGameObject.transform;
		}

		public void Move(Vector2 direction)
		{
			Vector2 prevDirection = this.direction;

			// Move the body part
			this.transform.position += (Vector3)direction;

			float angle;
			angle = Vector2.SignedAngle(prevDirection, direction);

			// If the body part is a corner piece
			if (angle != 0)
			{
				if (Mathf.Approximately(angle, 90))
				{
					// r
				}

				else if (Mathf.Approximately(angle, -90))
				{
					// ¬
				}
			}
		}
	}
}