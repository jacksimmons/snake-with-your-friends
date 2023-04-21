using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

/* Movement:
 * Moves smoothly, can't go back on itself (sign(d0) == sign(d1))
 * Movement is automatic, starting with the first input
 * Changing direction on one axis snaps your position on the other
 */

public class PlayerBehaviour : MonoBehaviour
{
	public Status status;

	// Templates and sprites
	[SerializeField]
	private GameObject _bp_template;
	[SerializeField]
	private GameObject _ef_too_many_pints_template;
	private TooManyPints _ef_too_many_pints_script = null;

	[SerializeField]
	private Sprite _headPiece;
	[SerializeField]
	private Sprite _tailPiece;
	[SerializeField]
	private Sprite _straightPiece;

	// Corner pieces
	[SerializeField]
	private Sprite[] _cornerPieces = new Sprite[4];

	// Directions and movement

	private Vector2 _startingDirection = Vector2.up;
	// Simple boolean which gets set to false after the starting direction is set
	public Vector2 direction = Vector2.zero;
	// The last valid, non-zero direction vector
	public Vector2 movement = Vector2.zero;
	// The last `movement` which was used
	public Vector2 p_PrevMovement { get; private set; }

	// Free movement
	[SerializeField]
	public bool freeMovement;
	[SerializeField]
	private float _freeMovementSpeedMod = 1.0f;

	// Forced movement

	// The movement vector to be moved along every frame
	// When the player is in forced movement state
	private Vector2 _forcedMovement = Vector2.zero;
	// Restored after forced movement ends
	private List<Vector2> _stored_bp_directions = new List<Vector2>();

	// Movement values

	[SerializeField]
	private float _defaultMovementSpeed;
	public float p_MovementSpeed { get; private set; }
	// Increments to _moveTime * childCount, then resets
	public int timer = 0;
	// Increments to _moveTime, then resets
	public int moveTimer = 0;
	private int _moveTime = 20;
	public bool frozen = false;

	// Body Parts
	public BodyPart head;
	public BodyPart tail;
	private List<BodyPart> _bodyParts;
	public List<BodyPart> BodyParts
	{
		get { return _bodyParts; }
	}

	// All actions are executed after the next movement frame
	private List<Action> _queuedActions;

	// Components
	private Rigidbody2D _rb;

	void Awake()
	{
		// Set movement speed
		p_MovementSpeed = _defaultMovementSpeed;

		// Create BodyParts
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

			BodyPart bp;

			// Head and body
			if (i < transform.childCount - 1)
			{
				bp = new BodyPart(_transform, _startingDirection, _sprite, _cornerSprites, p_MovementSpeed);
				if (i == 0)
					head = bp;
			}
			
			// Tail
			else
			{
				bp = new BodyPart(_transform, _startingDirection, _sprite, null, p_MovementSpeed);
				tail = bp;
			}
			_bodyParts.Add(bp);
		}

		// Create QueuedActions
		_queuedActions = new List<Action>();
	}

	void Start()
	{
		// Initialise components
		_rb = GetComponent<Rigidbody2D>();

		// Create Status
		List<BodyPartStatus> bpss = new List<BodyPartStatus>();
		for (int i = 0; i < _bodyParts.Count; i++)
		{
			BodyPartStatus bps = new BodyPartStatus(false, false);
			bpss.Add(bps);
		}
		status = new Status(bpss);

		// Handle free movement
		if (freeMovement)
			_moveTime = Mathf.CeilToInt(_moveTime / _freeMovementSpeedMod);
	}

	public void Reset()
	{
		while (RemoveBodyPart()) { }

		head.p_Position = Vector3.zero;
		head.p_Rotation = Quaternion.identity;
		tail.p_Position = Vector3.down;
		tail.p_Rotation = Quaternion.identity;
	}

	private void Update()
	{
		HandleInputs();
		HandleStatus();
	}

	void FixedUpdate()
	{
		// Increment the timers
		timer++;
		moveTimer++;

		HandleMovementLoop();
	}

	void HandleInputs()
	{
		// Movement
		float x_input = Input.GetAxisRaw("Horizontal");
		float y_input = Input.GetAxisRaw("Vertical");

		if (!frozen)
		{
			if (freeMovement || (direction != Vector2.zero))
			{
				movement = direction;
			}
		}
		else
		{
			movement = _forcedMovement;
		}

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
		if (direction == -p_PrevMovement)
			direction = Vector2.zero;
	}

	void HandleStatus()
	{
		if (status.p_NumPints > 0 && _ef_too_many_pints_script == null)
		{
			GameObject go = new GameObject("Effect:TooManyPints");
			go.layer = LayerMask.NameToLayer("Effects");
			go.AddComponent<TooManyPints>();
			_ef_too_many_pints_script = go.GetComponent<TooManyPints>();
		}

		if (_ef_too_many_pints_script != null)
		{
			if (status.p_NumPints > 0)
			{
				_ef_too_many_pints_script.UpdatePints(status.p_NumPints);
			}
			else
			{
				Destroy(_ef_too_many_pints_script.transform.gameObject);
			}
		}
	}

	void HandleMovementLoop()
	{
		if (moveTimer >= _moveTime)
		{
			// Reset the timer(s)
			if (timer >= transform.childCount * _moveTime)
			{
				timer = 0;
			}
			moveTimer = 0;

			// Handle queued actions (other than any that
			// get added by said actions)
			for (int i = 0; i < _queuedActions.Count; i++)
			{
				_queuedActions[0]();
				_queuedActions.RemoveAt(0);
			}

			// Ensures the first movement has been made
			if (movement != Vector2.zero)
			{
				// Update prevMovement
				p_PrevMovement = movement;

				// Iterate backwards through the body parts, from tail to head
				// The reason for doing this is so every part inherits its next
				// direction from the part before it.
				if (_bodyParts.Count > 1)
				{
					// Tail first
					BodyPart tailPrev = _bodyParts[_bodyParts.Count - 2];
					_bodyParts[_bodyParts.Count - 1].Move(tailPrev.p_Direction);

					// Then the rest of the body, tail - 1 to head
					for (int i = _bodyParts.Count - 2; i >= 0; i--)
					{
						BodyPart next = null;
						Vector2 dir = movement;
						if (i > 0)
						{
							dir = _bodyParts[i - 1].p_Direction;
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
	/// Adds a new body part onto the end of the snake, then makes it the new tail.
	/// Then turns the tail into a regular straight piece.
	/// </summary>
	private void AddBodyPart()
	{
		Vector2 position = tail.p_Position - (Vector3)tail.p_Direction;

		// Update the (previously) tail into a normal body part
		tail.p_DefaultSprite = _straightPiece;
		tail.p_Sprite = _straightPiece;
		tail.p_CornerSprites = _cornerPieces;

		// Instantiate the new body part
		GameObject newBodyPartObj = Instantiate(_bp_template, position, tail.p_Rotation, transform);

		// Create the new BodyPart object, and turn it into the tail
		BodyPart newBodyPart = new BodyPart(tail, newBodyPartObj.transform);
		newBodyPart.p_DefaultSprite = _tailPiece;
		newBodyPart.p_Sprite = _tailPiece;
		newBodyPart.p_CornerSprites = null;

		// The snake will end with ~- (~ is the new tail), as expected
		float angle = Vector2.SignedAngle(tail.p_Direction, _bodyParts[_bodyParts.Count - 2].p_Direction);
		if (angle != 0)
		{
			newBodyPart.p_Rotation = tail.prevRot;
			tail.MakeCorner(_bodyParts[_bodyParts.Count - 2].p_Direction);
		}
		_bodyParts.Add(newBodyPart);

		// Set the tail to the new tail
		tail = newBodyPart;

		// Increase the number of pieces
		status.p_NumPieces++;
	}

	public void Q(Action action)
	{
		_queuedActions.Add(action);
	}

	private bool RemoveBodyPart()
	{
		if (transform.childCount > 2)
		{
			Destroy(transform.GetChild(transform.childCount - 2));
			_bodyParts.RemoveAt(transform.childCount - 2);
			return true;
		}
		return false;
	}

	public void QAddBodyPart()
	{
		_queuedActions.Add(new Action(AddBodyPart));
	}

	public void QBeginForcedMovement(Vector2 direction, float speed)
	{
		_queuedActions.Add(new Action(() =>
		{
			frozen = true;
			_forcedMovement = direction * speed;
			movement = _forcedMovement;
			foreach (var part in _bodyParts)
			{
				_stored_bp_directions.Add(part.p_Direction);
				part.p_Direction = _forcedMovement;
			}
		}));
	}

	public void QEndForcedMovement()
	{
		_queuedActions.Add(new Action(() =>
		{
			frozen = false;

			// ! Limitation - the snake must continue,
			// this means if the snake starts on a scootile,
			// they start without input.
			movement = _forcedMovement.normalized;
			_forcedMovement = Vector2.zero;
			for (int i = 0; i < _bodyParts.Count; i++)
			{
				_bodyParts[i].p_Direction = _stored_bp_directions[i];
			}
			_stored_bp_directions.Clear();
		}));
	}

	public void HandleDeath()
	{
		_rb.simulated = false;
		AddBodyPart();

		// Half move so covered by head, and increase every sorting order other than this new part
		_bodyParts[1].Move(p_PrevMovement * 0.5f);
		head.p_Transform.GetComponent<SpriteRenderer>().sortingOrder += 1;
		for (int i = 2; i < _bodyParts.Count; i++)
		{
			_bodyParts[i].Move(p_PrevMovement);
			_bodyParts[i].p_Transform.GetComponent<SpriteRenderer>().sortingOrder += 1;
		}
		frozen = true;
	}

	public Dictionary<string, string> GetPlayerDebug()
	{
		Dictionary<string, string> playerValues = new Dictionary<string, string>
		{
			{ "direction", direction.ToString() },
			{ "movement", movement.ToString() },
			{ "prevMovement", p_PrevMovement.ToString() }
		};
		for (int i = 0; i < _queuedActions.Count; i++)
			playerValues.Add("queuedActions [" + i.ToString() + "]" , _queuedActions[i].Target.ToString());
		playerValues.Add("forcedMovement", _forcedMovement.ToString());
		for (int i = 0; i < _stored_bp_directions.Count; i++)
			playerValues.Add("storedBpDirections [" + i.ToString() + "]" , _stored_bp_directions[i].ToString());
		return playerValues;
	}
}