using System.Collections.Generic;
using UnityEngine;

/* Movement:
 * Moves smoothly, can't go back on itself (sign(d0) == sign(d1))
 * Movement is automatic, starting with the first input
 * Changing direction on one axis snaps your position on the other
 */

public class PlayerBehaviour : MonoBehaviour
{
	private Vector2 _startingDirection = Vector2.up;

	[SerializeField]
	private GameObject _bp_template;

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
	public Vector2 direction = Vector2.zero;
	// The last valid, non-zero direction vector
	public Vector2 movement = Vector2.zero;
	// The last `movement` which was used
	private Vector2 _prevMovement = Vector2.zero;

	public BodyPart head;
	public BodyPart tail;
	private List<BodyPart> _bodyParts;

	private float _movementSpeed = 1f;
	// Increments to _moveTime * childCount, then resets
	public int timer = 0;
	// Increments to _moveTime, then resets
	public int moveTimer = 0;
	private int _moveTime = 20;

	public bool frozen = false;

	// Components
	private Rigidbody2D _rb;

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
				bp = new TailBodyPart(_transform, _direction, _sprite, null);
				tail = bp;
			}
			_bodyParts.Add(bp);
		}
	}

	void Start()
	{
		_rb = GetComponent<Rigidbody2D>();
	}

	// Update is called once per frame
	void Update()
	{
		HandleInputs();
	}

	void FixedUpdate()
	{
		// .Handle first direction
		// .Increment move time, reset counter if it's passed the number of body parts
		// - Handle new body parts in the middle of a _moveTime
		// .Set direction = movement
		// Set sprite rotation = angle between prev. dir and lastTravDir.
		// Calculate what WILL be a corner piece after movement
		// Apply corner sprites
		// Move the entire snake, excluding corner pieces which act as pipes
		HandleMovementLoop();
		HandleInternalCollisions();
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
		// So cancel the new input.
		if (direction == -_prevMovement)
			direction = Vector2.zero;
	}

	void HandleMovementLoop()
	{
		// Increment the timers
		timer++;
		moveTimer++;

		// Ensures the first movement has been made
		if (movement != Vector2.zero && !frozen)
		{
			if (moveTimer >= _moveTime)
			{
				// Reset the timer(s)
				if (timer >= transform.childCount * _moveTime)
				{
					timer = 0;
				}
				moveTimer = 0;

				// Update prevMovement
				_prevMovement = movement;

				// Iterate backwards through the body parts, from tail to head
				// The reason for doing this is so every part inherits its next
				// direction from the part before it.
				if (_bodyParts.Count > 1)
				{
					// Tail first
					BodyPart tailPrev = _bodyParts[_bodyParts.Count - 2];
					_bodyParts[_bodyParts.Count - 1].Move(tailPrev.direction);

					// Then the rest of the body, tail - 1 to head
					for (int i = _bodyParts.Count - 2; i >= 0; i--)
					{
						BodyPart next = null;
						Vector2 dir = movement;
						if (i > 0)
						{
							dir = _bodyParts[i - 1].direction;
						}
						if (i + 1 < _bodyParts.Count)
							next = _bodyParts[i + 1];
						_bodyParts[i].HandleMovement(dir, next);
					}
				}
			}
		}
	}

	/// <summary>
	/// Handles all collisions involving the head and another body part.
	/// </summary>
	void HandleInternalCollisions()
	{
		Vector2 headPos = head.transform.position;
		foreach (BodyPart bp in _bodyParts)
		{
			if (bp != head)
			{
				Collider2D c = bp.transform.GetComponent<Collider2D>();
				if (c != null)
				{
					if (c.OverlapPoint(headPos))
					{
						HandleDeath();
					}
				}
				else
				{
					Debug.LogWarning("No collider for " + _bodyParts.IndexOf(bp) + "th body part.");
				}
			}
		}
	}

	void HandleDeath()
	{
		foreach (BodyPart bp in _bodyParts)
		{
			SpriteRenderer sr = bp.transform.GetComponent<SpriteRenderer>();
			if (sr != null)
			{
				sr.color = Color.grey;
			}
		}
		frozen = true;
		_rb.simulated = false;
	}

	void AddBodyPart()
	{
		// Adds a new body part as a child, then moves the tail after it
		GameObject bp = Instantiate(_bp_template);
		bp.transform.SetParent(transform);
		bp.transform.position = tail.transform.position;
		tail.transform.position -= (Vector3)tail.direction;

		// Makes the new body part have the same direction and rotation as the tail
		BodyPart bodyPart = new BodyPart(bp.transform, tail.direction, _straightPiece,
			_cornerPieces);
		bodyPart.transform.rotation = tail.transform.rotation;

		tail.transform.SetAsLastSibling();
		_bodyParts.RemoveAt(_bodyParts.Count - 1);
		_bodyParts.Add(bodyPart);
		_bodyParts.Add(tail);
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		// Handles all snake collision OTHER than internal collisions
		// (collisions of body parts with each other)
		Transform col = collision.collider.transform;
		Transform other = collision.otherCollider.transform;
		if (other != null)
		{
			// The head has crashed into something (not itself)
			if (other == head.transform)
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

		// Rotation before it became a corner, useful only to parts after this one
		protected Quaternion prevRot = Quaternion.identity;

		private Sprite[] _cornerSprites = null;

		// For a body part that isn't the head or the tail
		public BodyPart(Transform transform, Vector2 direction, Sprite defaultSprite,
			Sprite[] cornerSprites)
		{
			this.transform = transform;
			this.direction = direction;
			this.defaultSprite = defaultSprite;
			isCorner = false;
			_cornerSprites = cornerSprites;
			SetSprite(defaultSprite);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="prevDir">The direction of the previous body part.</param>
		public void MakeCorner(Vector2 prevDir)
		{
			isCorner = true;
			transform.rotation = Quaternion.identity;

			if (direction == Vector2.up)
			{
				if (prevDir == Vector2.left)
					SetSprite(_cornerSprites[3]); // -R
				else if (prevDir == Vector2.right)
					SetSprite(_cornerSprites[2]); // R
			}

			else if (direction == Vector2.left)
			{
				if (prevDir == Vector2.up)
					SetSprite(_cornerSprites[0]); // L
				else if (prevDir == Vector2.down)
					SetSprite(_cornerSprites[2]); // R
			}

			else if (direction == Vector2.down)
			{
				if (prevDir == Vector2.left)
					SetSprite(_cornerSprites[1]); // -L
				else if (prevDir == Vector2.right)
					SetSprite(_cornerSprites[0]); // L
			}

			else if (direction == Vector2.right)
			{
				if (prevDir == Vector2.up)
					SetSprite(_cornerSprites[1]); // -L
				else if (prevDir == Vector2.down)
					SetSprite(_cornerSprites[3]); // -R
			}
		}

		public void MakeNotCorner(Quaternion rot)
		{
			isCorner = false;
			this.transform.rotation = rot;
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
		public virtual void HandleMovement(Vector2 dir, BodyPart next)
		{
			// First handle our own movement and rotation
			Vector2 prevDirection = this.direction;
			Move(dir);
			float angle = Vector2.SignedAngle(prevDirection, direction);
			this.transform.Rotate(Vector3.forward, angle);

			// If the body part is a corner piece
			if (next != null)
			{
				// If the next part isn't a tail, and is an angled body part,
				// make it a corner.
				if (next._cornerSprites != null)
				{
					if (angle != 0)
					{
						if (!next.isCorner)
							next.prevRot = next.transform.rotation;
						next.MakeCorner(dir);
					}
					else
					{
						// When making `next` not a corner, set its rotation
						// to our prevRot (if this is a corner), or our rotation
						// (if this isn't a corner)
						Quaternion rot = this.transform.rotation;
						if (isCorner)
							rot = prevRot;
						next.MakeNotCorner(rot);
					}
				}
				// If it is a tail, rotate alongside this body part
				else
				{
					next.transform.Rotate(Vector3.forward, angle);
				}
			}
		}
	}

	public class TailBodyPart : BodyPart
	{
		public TailBodyPart(Transform transform, Vector2 direction, Sprite defaultSprite, 
			Sprite[] cornerSprites)
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