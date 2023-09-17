using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System;

// Class which controls the Body Parts of a Player object.
// Is Destroyed when the player dies, but other components are kept.
public class PlayerMovement : NetworkBehaviour
{
    private BitField bf = new();
    public bool Frozen
    {
        get { return bf.GetBit(0); }
        set { bf.SetBit(0, value); }
    }
    public bool FreeMovement
    {
        get { return bf.GetBit(1); }
        set { bf.SetBit(1, value); }
    }
    public bool HasMoved
    {
        get
        {
            if (!bf.GetBit(2) && (movement != Vector2.zero))
                return true;
            return false;
        }
        set { bf.SetBit(2, value); }
    }

    [SerializeField]
    private GameBehaviour _gameBehaviour;

    [SerializeField]
    public PlayerStatus status;

    [SerializeField]
    private PlayerObjectController m_poc;

    [SerializeField]
    public GameObject bodyPartContainer;

    // Templates and sprites
    [SerializeField]
    private GameObject _bodyPartTemplate;

    [SerializeField]
    public Sprite m_bpHead;
    [SerializeField]
    public Sprite m_bpTail;
    [SerializeField]
    public Sprite m_bpTorso;
    [SerializeField]
    public Sprite m_bpCornerL;

    // Directions and movement
    public Vector2 startingDirection = Vector2.up;
    // Simple boolean which gets set to false after the starting direction is set
    public Vector2 direction = Vector2.zero;
    // The last valid, non-zero direction vector
    public Vector2 movement = Vector2.zero;
    // The last `movement` which was used
    public Vector2 PrevMovement { get; private set; }

    [SerializeField]
    private float _freeMovementSpeedMod = 1.0f;

    // Forced movement
    // The movement vector to be moved along every frame
    // When the player is in forced movement state
    private Vector2 _forcedMovement = Vector2.zero;
    // Restored after forced movement ends

    private List<Vector2> _storedBodyPartDirections = new List<Vector2>();

    public float TimeToMove { get; set; }
    private float m_timeSinceLastMove = 0;

    // Body Parts
    public List<BodyPart> BodyParts { get; set; }

    // All actions are executed after the next movement frame
    private List<Action> _queuedActions;


    public void RecreateStartingParts(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject bp = Instantiate(_bodyPartTemplate, bodyPartContainer.transform);
            bp.transform.position += Vector3.up * (count - i);
        }
    }

    private void OnEnable()
    {
        TimeToMove = GameSettings.Saved.TimeToMove;

        bodyPartContainer.SetActive(false);

        // Generate BodyParts structure & starting body parts
        BodyParts = new List<BodyPart>();
        Transform containerTransform = bodyPartContainer.transform;

        int bodyPartCount = containerTransform.childCount;
        if (bodyPartCount == 0)
        {
            RecreateStartingParts(2);
            bodyPartCount = containerTransform.childCount;
        }

        for (int i = 0; i < bodyPartCount; i++)
        {
            Transform _transform = containerTransform.GetChild(i);
            BodyPart bp = new
            (
                _transform,
                startingDirection,

                // if (i == 0) => Head
                i == 0 ? 
                EBodyPartType.Head :
                // else if (i is not the final index) => Straight
                // else => Tail
                i < containerTransform.childCount - 1 ?
                EBodyPartType.Straight :
                EBodyPartType.Tail
            );
            BodyParts.Add(bp);

            _transform.GetComponent<SpriteRenderer>().sprite =
                bp.CurrentType == EBodyPartType.Head ? m_bpHead :
                bp.CurrentType == EBodyPartType.Straight ? m_bpTorso :
                bp.CurrentType == EBodyPartType.Tail ? m_bpTail :
                bp.CurrentType == EBodyPartType.Corner ? m_bpCornerL :
                null;
        }

        // Generate QueuedActions structure
        _queuedActions = new List<Action>();

        //List<BodyPartStatus> bpss = new List<BodyPartStatus>();
        //for (int i = 0; i < BodyParts.Count; i++)
        //{
        //    BodyPartStatus bps = new BodyPartStatus(false, false);
        //    bpss.Add(bps);
        //}

        //if (CanMoveFreely)
        //    _moveTime = Mathf.CeilToInt(_moveTime / _freeMovementSpeedMod);

        // First update, necessary for second, third etc games
        m_poc.UpdateBodyParts();
    }

    private void FixedUpdate()
    {
        if (Array.IndexOf(GameBehaviour.GAME_SCENES, SceneManager.GetActiveScene().name) == -1)
            return;

        if (!bodyPartContainer.activeSelf)
        {
            bodyPartContainer.SetActive(true);
        }

        // So that we only move a player if we have authority over it
        if (isOwned)
        {
            HandleInput();
            HandleMovementLoop();
        }
    }

    private void HandleInput()
    {
        // Movement
        float x_input = Input.GetAxisRaw("Horizontal");
        float y_input = Input.GetAxisRaw("Vertical");

        // Forced movement
        if (Frozen)
        {
            movement = _forcedMovement;
            return;
        }

        // Free Movement update
        // - Instant if changing direction
        // - Time delay if not
        if (FreeMovement)
        {
            if (direction != movement)
            {
                movement = direction;
                m_timeSinceLastMove = TimeToMove;
            }
        }
        // Movement update
        else if (direction != Vector2.zero && direction != movement)
        {
            movement = direction;
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
        // Counter logic
        m_timeSinceLastMove += Time.fixedDeltaTime;
        if (m_timeSinceLastMove < TimeToMove)
            return;

        m_timeSinceLastMove = 0;

        // Queued actions wait until the next move frame before being called.
        for (int i = 0; i < _queuedActions.Count; i++)
        {
            _queuedActions[0]();
            _queuedActions.RemoveAt(0);
        }

        // Prevents an extra move occurring before death
        if (CheckForInternalCollisions()) return;

        if (HasMoved && movement != Vector2.zero)
        {
            RaycastHit2D raycast = Physics2D.Raycast(
                BodyParts[0].Position,
                movement,
                1);

            Debug.DrawLine(BodyParts[0].Position, movement * 1f, Color.red);

            if (raycast)
                print("Would hit a wall");

            PrevMovement = movement;

            // Iterate backwards through the body parts, from tail to head
            // so every part inherits its direction from the part before it.
            
            // Tail first - move in the same direction as the bp in front
            BodyPart bpBeforeTail = BodyParts[^2];
            BodyParts[^1].Move(bpBeforeTail.Direction);

            // Tail - 1 to Head + 1
            for (int i = BodyParts.Count - 2; i > 0; i--)
            {
                // Inherit direction from in front.
                Vector2 bpPrevDir = BodyParts[i - 1].Direction;
                BodyPart bpNext = BodyParts[i + 1];

                BodyParts[i].HandleMovement(bpPrevDir, bpNext);
            }

            // Head
            BodyParts[0].HandleMovement(movement, BodyParts[1]);

            // Update to server
            m_poc.UpdateBodyParts();
        }
    }

    /// <summary>
    /// Only collisions that are possible without invincibility are head and other parts.
    /// Therefore, check if the head's position matches any of the others.
    /// </summary>
    private bool CheckForInternalCollisions()
    {
        BoxCollider2D bcHead = BodyParts[0].Transform.GetComponent<BoxCollider2D>();
        Collider2D[] result = new Collider2D[1];
        if (bcHead.OverlapCollider(new ContactFilter2D(), result) > 0)
        {
            if (result[0].gameObject.CompareTag("BodyPart"))
            {
                HandleDeath();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Adds a new body part onto the end of the snake, then makes it the new tail.
    /// Then turns the tail into a regular straight piece.
    /// Not having moved yet grants immunity.
    /// </summary>
    private void AddBodyPart()
    {
        if (!HasMoved)
            return;

        GameObject newBodyPartObj = Instantiate(_bodyPartTemplate);
        newBodyPartObj.transform.parent = bodyPartContainer.transform;

        BodyPart tail = BodyParts[^1];
        BodyPart newBodyPart = new(tail, newBodyPartObj.transform)
        {
            Position = tail.Position - (Vector3)tail.Direction
        };

        tail.DefaultType = EBodyPartType.Straight;
        tail.CurrentType = EBodyPartType.Straight;

        if (BodyParts[^2].CurrentType == EBodyPartType.Corner)
        {
            tail.MakeCorner(BodyParts[^2].Direction);
        }
        else
        {
            tail.MakeNotCorner();
        }
        BodyParts.Add(newBodyPart);

        // Set the tail to the new tail and update names
        tail.Transform.name = "BodyPart(" + tail.Transform.GetSiblingIndex() + ")";
        tail = newBodyPart;
        tail.Transform.name = "Tail";
    }

    /// <summary>
    /// Removes one body part from the snake, from the tail side.
    /// Not having moved yet grants immunity.
    /// </summary>
    private void RemoveBodyPart()
    {
        if (!HasMoved)
            return;

        if (BodyParts.Count >= 3)
        {
            BodyPart partToRemove = BodyParts[^2];
            BodyPart tail = BodyParts[^1];

            tail.Position = partToRemove.Position;

            if (partToRemove.CurrentType == EBodyPartType.Corner)
                tail.RegularAngle = BodyParts[^3].RegularAngle;
            else
                tail.RegularAngle = partToRemove.RegularAngle;

            Destroy(partToRemove.Transform.gameObject);
            BodyParts.Remove(partToRemove);
        }
        else
        {
            // Snake must have >= 2 body parts left.
            HandleDeath();
        }
    }

    /// <summary>
    /// Not having moved yet grants immunity.
    /// </summary>
    public void HandleDeath()
    {
        if (!HasMoved)
            return;

        m_poc.LogDeath();

        GameBehaviour game = GetComponentInChildren<GameBehaviour>();
        game.OnGameOver(score: BodyParts.Count);

        enabled = false;
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

    public void QRemoveBodyPart(int collisionIndex = -1)
    {
        if (collisionIndex == 0)
            HandleDeath();
        else
            _queuedActions.Add(() => RemoveBodyPart());
    }

    /// <summary>
    /// Queues the beginning of forced movement.
    /// </summary>
    /// <param name="direction">The direction of the forced movement.</param>
    /// <param name="timeToMove">The number of seconds between each movement.</param>
    public void QBeginForcedMovement(Vector2 direction, float timeToMove)
    {
        _queuedActions.Add(new Action(() =>
        {
            Frozen = true;
            TimeToMove = timeToMove;
            _forcedMovement = direction;
            foreach (var part in BodyParts)
            {
                _storedBodyPartDirections.Add(part.Direction);
                part.Direction = _forcedMovement;
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
            Frozen = false;

            // ! Limitation - the snake must continue,
            // this means if the snake starts on a scootile,
            // they start without input.
            TimeToMove = GameSettings.Saved.TimeToMove;
            _forcedMovement = Vector2.zero;
            for (int i = 0; i < BodyParts.Count; i++)
            {
                BodyParts[i].Direction = _storedBodyPartDirections[i];
            }
            _storedBodyPartDirections.Clear();
        }));
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
            { "CounterMax", TimeToMove.ToString() }
        };
        for (int i = 0; i < _queuedActions.Count; i++)
            playerValues.Add("queuedActions [" + i.ToString() + "]", _queuedActions[i].Target.ToString());
        playerValues.Add("forcedMovement", _forcedMovement.ToString());
        for (int i = 0; i < _storedBodyPartDirections.Count; i++)
            playerValues.Add("storedBpDirections [" + i.ToString() + "]", _storedBodyPartDirections[i].ToString());
        return playerValues;
    }
}
