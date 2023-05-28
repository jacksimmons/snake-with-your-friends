using System;
using System.Collections.Generic;
using UnityEngine;

/* Movement:
 * Moves smoothly, can't go back on itself (sign(d0) == sign(d1))
 * Movement is automatic, starting with the first input
 * Changing direction on one axis snaps your position on the other
 */

public class PlayerBehaviour : MonoBehaviour
{
    public Lobby lobby;

    [SerializeField]
    public StatusBehaviour status;

    // Templates and sprites
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

    // Directions and movement

    private Vector2 _startingDirection = Vector2.up;
    // Simple boolean which gets set to false after the starting direction is set
    public Vector2 direction = Vector2.zero;
    // The last valid, non-zero direction vector
    public Vector2 movement = Vector2.zero;
    // The last `movement` which was used
    public Vector2 PrevMovement { get; private set; }

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

    public bool frozen = false;

    public float DefaultMovementSpeed { get; private set; } = 1.0f;
    public float MovementSpeed { get; set; } = 1.0f;

    // Body Parts
    public BodyPart head;
    public BodyPart tail;
    public List<BodyPart> BodyParts { get; private set; }

    // All actions are executed after the next movement frame
    private List<Action> _queuedActions;

    // Components
    private Rigidbody2D _rb;

    /// <summary>
    /// Initialise data structures and objects.
    /// </summary>
    void Awake()
    {
        // Data structures
        BodyParts = new List<BodyPart>();
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
                bp = new BodyPart(_transform, _startingDirection, _sprite, _cornerSprites);
                if (i == 0)
                    head = bp;
            }

            // Tail
            else
            {
                bp = new BodyPart(_transform, _startingDirection, _sprite, null);
                tail = bp;
            }
            BodyParts.Add(bp);
        }
        
        _queuedActions = new List<Action>();
        
        List<BodyPartStatus> bpss = new List<BodyPartStatus>();
        for (int i = 0; i < BodyParts.Count; i++)
        {
            BodyPartStatus bps = new BodyPartStatus(false, false);
            bpss.Add(bps);
        }

        // Initialisation
        _rb = GetComponent<Rigidbody2D>();
        //if (freeMovement)
        //    _moveTime = Mathf.CeilToInt(_moveTime / _freeMovementSpeedMod);
    }

    private void Start()
    {
        lobby = GameObject.FindWithTag("Lobby").GetComponent<Lobby>();
        lobby.PlayerLoaded();
    }

    /// <summary>
    /// Custom reset method; removes all body parts,
    /// and resets head and tail rotation.
    /// </summary>
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
        HandleInput();
    }

    private void HandleInput()
    {
        // Movement
        float x_input = Input.GetAxisRaw("Horizontal");
        float y_input = Input.GetAxisRaw("Vertical");

        // Movement states
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

        // Direction
        direction = Vector2.zero;
        // Raw user input
        if (x_input > 0)
            direction = Vector2.right;
        else if (x_input < 0)
            direction = Vector2.left;

        // If no x input was provided, check for y input
        if (direction == Vector2.zero)
        {
            if (y_input > 0)
                direction = Vector2.up;
            else if (y_input < 0)
                direction = Vector2.down;
        }

        // We can't have the snake going back on itself.
        // So cancel the new input.
        if (direction == -PrevMovement)
            direction = Vector2.zero;

        // Pausing
        if (Input.GetButtonDown("Pause"))
        {
            // 1 -> 0, 0 -> 1
            // ! Steam callbacks will NOT WORK after this as Update is not called.
            Time.timeScale = Mathf.Abs(Time.timeScale - 1f);
        }
    }

    /// <summary>
    /// Handles movement for all body parts, and the frequency of movement ticks.
    /// Called by the lobby every synced moveframe.
    /// </summary>
    public void HandleMovementLoop()
    {
        // Queued actions happen every move frame, before movement occurs.
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
            PrevMovement = movement;

            // Iterate backwards through the body parts, from tail to head
            // The reason for doing this is so every part inherits its next
            // direction from the part before it.
            if (BodyParts.Count > 1)
            {
                // Tail first
                BodyPart tailPrev = BodyParts[^2];
                BodyParts[^1].Move(tailPrev.p_Direction);

                // Then the rest of the body, tail - 1 to head
                for (int i = BodyParts.Count - 2; i >= 0; i--)
                {
                    BodyPart next = null;
                    Vector2 dir = movement;
                    if (i > 0)
                    {
                        dir = BodyParts[i - 1].p_Direction;
                    }
                    if (i + 1 < BodyParts.Count)
                        next = BodyParts[i + 1];
                    BodyParts[i].HandleMovement(dir, next);
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
        BodyPart newBodyPart = new BodyPart(tail, newBodyPartObj.transform)
        {
            p_DefaultSprite = _tailPiece,
            p_Sprite = _tailPiece,
            p_CornerSprites = null
        };

        // The snake will end with ~- (~ is the new tail), as expected
        float angle = Vector2.SignedAngle(tail.p_Direction, BodyParts[^2].p_Direction);
        if (angle != 0)
        {
            newBodyPart.p_Rotation = tail.prevRot;
            tail.MakeCorner(BodyParts[^2].p_Direction);
        }
        BodyParts.Add(newBodyPart);

        // Set the tail to the new tail
        tail = newBodyPart;
    }

    /// <summary>
    /// Removes the i-1th body part from the snake.
    /// ! This needs testing.
    /// </summary>
    /// <returns>
    /// A boolean, `true` meaning the body part was removed and the snake is
    /// still alive.
    /// `false` meaning the body part was not removed, as there is only a head
    /// and a tail left; so the snake should die.
    /// </returns>
    private bool RemoveBodyPart()
    {
        if (transform.childCount > 2)
        {
            Destroy(transform.GetChild(transform.childCount - 1).gameObject);
            BodyParts.RemoveAt(transform.childCount - 1);
            tail = BodyParts[transform.childCount - 1];
            tail.p_DefaultSprite = _straightPiece;
            tail.p_CornerSprites = null;
            tail.MakeNotCorner(tail.prevRot);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Handles death.
    /// </summary>
    public void HandleDeath()
    {
        _rb.simulated = false;
        AddBodyPart();

        // Half move so covered by head, and increase every sorting order other than this new part
        BodyParts[1].Move(PrevMovement * 0.5f);
        head.p_Transform.GetComponent<SpriteRenderer>().sortingOrder += 1;
        for (int i = 2; i < BodyParts.Count; i++)
        {
            BodyParts[i].Move(PrevMovement);
            BodyParts[i].p_Transform.GetComponent<SpriteRenderer>().sortingOrder += 1;
        }
        frozen = true;
    }

    /// <summary>
    /// Outputs debug values for this object.
    /// </summary>
    /// <returns>A dictionary containing variable names and their values.</returns>
    public Dictionary<string, string> GetPlayerDebug()
    {
        Dictionary<string, string> playerValues = new Dictionary<string, string>
        {
            { "direction", direction.ToString() },
            { "movement", movement.ToString() },
            { "PrevMovement", PrevMovement.ToString() },
            { "MovementSpeed", MovementSpeed.ToString() }
        };
        for (int i = 0; i < _queuedActions.Count; i++)
            playerValues.Add("queuedActions [" + i.ToString() + "]", _queuedActions[i].Target.ToString());
        playerValues.Add("forcedMovement", _forcedMovement.ToString());
        for (int i = 0; i < _stored_bp_directions.Count; i++)
            playerValues.Add("storedBpDirections [" + i.ToString() + "]", _stored_bp_directions[i].ToString());
        return playerValues;
    }

    /// <summary>
    /// Queue a new ambiguous action.
    /// </summary>
    /// <param name="action">An action (call).</param>
    public void Q(Action action)
    {
        _queuedActions.Add(action);
    }

    /// <summary>
    /// Queues an AddBodyPart action.
    /// </summary>
    public void QAddBodyPart()
    {
        _queuedActions.Add(new Action(AddBodyPart));
    }

    /// <summary>
    /// Queues the beginning of forced movement.
    /// </summary>
    /// <param name="direction">The direction of the forced movement.</param>
    /// <param name="speed">The number of tiles per tick to move at.</param>
    public void QBeginForcedMovement(Vector2 direction, float speed)
    {
        _queuedActions.Add(new Action(() =>
        {
            frozen = true;
            _forcedMovement = direction * speed;
            movement = _forcedMovement;
            foreach (var part in BodyParts)
            {
                _stored_bp_directions.Add(part.p_Direction);
                part.p_Direction = _forcedMovement;
            }
        }));
    }

    /// <summary>
    /// Queues the ending of forced movement.
    /// </summary>
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
            for (int i = 0; i < BodyParts.Count; i++)
            {
                BodyParts[i].p_Direction = _stored_bp_directions[i];
            }
            _stored_bp_directions.Clear();
        }));
    }
}