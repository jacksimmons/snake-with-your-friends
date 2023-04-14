using Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
	private Sprite _headPiece;
	[SerializeField]
	private Sprite _tailPiece;
	[SerializeField]
	private Sprite _straightPiece;

	// Corner pieces
	[SerializeField]
	private Sprite[] _cornerPieces = new Sprite[4];

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
		// Add the BodyParts
		_bodyParts = new List<BodyPart>();
		for (int i = 0; i < transform.childCount; i++)
		{
			Sprite _sprite;
			Sprite[] _cornerSprites = null;

			if (i == 0)
				_sprite = _headPiece;
			else if (i == transform.childCount - 1)
				_sprite = _tailPiece;
			else
			{
				_sprite = _straightPiece;
				_cornerSprites = _cornerPieces;
			}

			Transform _transform = transform.GetChild(i);
			Vector2 _direction = _startingDirection;

			BodyPart bp;

			// Head and body
			if (i < transform.childCount - 1)
			{
				bp = new BodyPart(_transform, _direction, _sprite, _cornerSprites);
				if (i == 0)
					head = bp;
			}
			
			// Tail
			else
			{
				bp = new TailBodyPart(_transform, _direction, _sprite);
				tail = bp;
			}
			_bodyParts.Add(bp);
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
		// - Handle new body parts in the middle of a _moveTime
		// .Set direction = movement
		// Set sprite rotation = angle between prev. dir and lastTravDir.
		// Calculate what WILL be a corner piece after movement
		// Apply corner sprites
		// Move the entire snake, excluding corner pieces which act as pipes

		// Ensure the first movement has been made
		if (movement != Vector2.zero)
		{
			if (moveTimer >= _moveTime)
			{
				// Reset the timer(s)
				if (timer >= transform.childCount * _moveTime)
				{
					timer = 0;
				}
				moveTimer = 0;

				// Iterate backwards through the body parts, from tail to head
				// The reason for doing this is so every part inherits its next
				// direction from the part before it.

				if (_bodyParts.Count > 1)
				{
					// Tail first
					BodyPart prev = _bodyParts[_bodyParts.Count - 2];
					_bodyParts[_bodyParts.Count - 1].Move(prev.direction);

					// Then the rest of the body, tail - 1 to head
					for (int i = _bodyParts.Count - 2; i >= 0; i--)
					{
						BodyPart next = null;
						Vector2 dir = movement;
						if (i + 1 < _bodyParts.Count)
							next = _bodyParts[i + 1];
						if (i > 0)
							dir = _bodyParts[i - 1].direction;
						_bodyParts[i].HandleMovement(dir, next);
					}

					// Update tail rotation
					if (!prev.isCorner)
						_bodyParts[_bodyParts.Count - 1].transform.rotation = prev.transform.rotation;
				}
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
		GameObject part = new GameObject();
		part.transform.SetParent(transform);

		// Makes the new body part have the same direction and rotation as the tail
		BodyPart bodyPart = new BodyPart(part.transform, tail.direction, _straightPiece, _cornerPieces);
		bodyPart.transform.rotation = tail.transform.rotation;

		tail.transform.SetAsLastSibling();
	}

	void HandleDeath()
	{
		Destroy(gameObject);
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		Transform t = collision.otherCollider.transform;
		if (t != null)
		{
			if (t == head.transform)
			{
				HandleDeath();
			}
		}
	}

	public class BodyPart
	{
		public Vector2 direction;
		public Transform transform;
		public Sprite defaultSprite;
		public bool isCorner;
		private Sprite[] _cornerSprites = null;

		// For a body part that isn't the head or the tail
		public BodyPart(Transform transform, Vector2 direction, Sprite defaultSprite, Sprite[] cornerSprites)
		{
			this.transform = transform;
			this.direction = direction;
			this.defaultSprite = defaultSprite;
			isCorner = false;
			_cornerSprites = cornerSprites;
			SetSprite(defaultSprite);
		}

		public void SetSprite(Sprite sprite)
		{
			this.transform.gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
		}

		public void Move(Vector2 direction)
		{
			this.direction = direction;
			this.transform.position += (Vector3)direction;
		}

		/// <summary>
		/// Complex movement handling with corner piece handling.
		/// </summary>
		/// <param name="dir">The new direction to move along.</param>
		/// <param name="next">The "next" body part, the body part
		/// which appears next in the child hierarchy (towards tail).</param>
		public virtual void HandleMovement(Vector2 dir, BodyPart next)
		{
			Vector2 prevDirection = this.direction;
			Move(dir);
			float angle = Vector2.SignedAngle(prevDirection, direction);
			this.transform.Rotate(Vector3.forward, angle);

			// If the body part is a corner piece
			if (next != null)
			{
				if (angle != 0)
				{
					// If the next part isn't a tail, make it a corner
					if (next._cornerSprites != null)
					{
						next.transform.rotation = Quaternion.identity;
						next.isCorner = true;

						if (next.direction == Vector2.up)
						{
							if (dir == Vector2.left)
								next.SetSprite(next._cornerSprites[3]); // -R
							else if (dir == Vector2.right)
								next.SetSprite(next._cornerSprites[2]); // R
						}

						else if (next.direction == Vector2.left)
						{
							if (dir == Vector2.up)
								next.SetSprite(next._cornerSprites[0]); // L
							else if (dir == Vector2.down)
								next.SetSprite(next._cornerSprites[2]); // R
						}

						else if (next.direction == Vector2.down)
						{
							if (dir == Vector2.left)
								next.SetSprite(next._cornerSprites[1]); // -L
							else if (dir == Vector2.right)
								next.SetSprite(next._cornerSprites[0]); // L
						}

						else if (next.direction == Vector2.right)
						{
							if (dir == Vector2.up)
								next.SetSprite(next._cornerSprites[1]); // -L
							else if (dir == Vector2.down)
								next.SetSprite(next._cornerSprites[3]); // -R
						}
					}
				}
				else
				{
					// In case it is currently a corner, set the sprite to default
					// Also set the rotation to the body part in front
					next.isCorner = false;
					next.SetSprite(next.defaultSprite);
					next.transform.rotation = this.transform.rotation;
				}
			}
		}
	}

	public class TailBodyPart : BodyPart
	{
		public TailBodyPart(Transform transform, Vector2 direction, Sprite defaultSprite, Sprite[] cornerSprites = null)
			: base(transform, direction, defaultSprite, cornerSprites) { }

		/// <summary>
		/// Movement handling without handling corner pieces.
		/// Moves in direction `dir`.
		/// </summary>
		/// <param name="dir">New direction.</param>
		public void HandleMovement(Vector2 dir)
		{
			Move(dir);
		}
	}
}