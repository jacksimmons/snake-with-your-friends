using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System;

public class PlayerMovementController : NetworkBehaviour
{
    [SerializeField]
    private GameBehaviour _gameBehaviour;

    [SerializeField]
    public StatusBehaviour status;

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
    public Sprite m_bpStraight;
    [SerializeField]
    public Sprite m_bpCornerL;

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
    public bool canMoveFreely;
    [SerializeField]
    private float _freeMovementSpeedMod = 1.0f;

    // Forced movement
    // The movement vector to be moved along every frame
    // When the player is in forced movement state
    private Vector2 _forcedMovement = Vector2.zero;
    // Restored after forced movement ends

    private List<Vector2> _storedBodyPartDirections = new List<Vector2>();

    public bool frozen = false;

    public const int LOWEST_COUNTER_MAX = 1;
    public const int DEFAULT_COUNTER_MAX = 20;
    private int _counterMax = DEFAULT_COUNTER_MAX;
    public int CounterMax
    { 
        get
        {
            return _counterMax;
        }
        set
        {
            if (value < LOWEST_COUNTER_MAX) { _counterMax = LOWEST_COUNTER_MAX; }
            else { _counterMax = value; }
        }
    }
    public int counter = 0;

    // Body Parts
    public List<BodyPart> BodyParts { get; set; }

    // All actions are executed after the next movement frame
    private List<Action> _queuedActions;

    // Components
    private Rigidbody2D _rb;

    private void Start()
    {
        bodyPartContainer.SetActive(false);

        // Data structures
        BodyParts = new List<BodyPart>();
        Transform containerTransform = bodyPartContainer.transform;
        for (int i = 0; i < containerTransform.childCount; i++)
        {
            Transform _transform = containerTransform.GetChild(i);
            BodyPart bp;

            // Head and body
            if (i < containerTransform.childCount - 1)
            {
                Sprite _sprite;
                EBodyPartType _bodyPartType;

                // Calculate sprites for head and straights
                if (i == 0)
                {
                    _bodyPartType = EBodyPartType.Head;
                    _sprite = m_bpHead;
                }
                else
                {
                    _bodyPartType = EBodyPartType.Straight;
                    _sprite = m_bpStraight;
                }

                bp = new BodyPart(_transform, _startingDirection, _bodyPartType);
            }

            // Tail - the BodyPart script handles these differently.
            else
            {
                bp = new BodyPart(_transform, _startingDirection, EBodyPartType.Tail);
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
        //if (canMoveFreely)
        //    _moveTime = Mathf.CeilToInt(_moveTime / _freeMovementSpeedMod);
    }

    private void FixedUpdate()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {
            if (!bodyPartContainer.activeSelf)
            {
                bodyPartContainer.SetActive(true);
            }

            // So that we only move a player if we have authority over it
            if (isOwned || _gameBehaviour.ClientMode == GameBehaviour.EClientMode.Offline)
            {
                HandleInput();
                HandleMovementLoop();
            }
        }
    }

    private void HandleInput()
    {
        // Movement
        float x_input = Input.GetAxisRaw("Horizontal");
        float y_input = Input.GetAxisRaw("Vertical");

        // Movement states
        if (!frozen)
        {
            if (canMoveFreely || (direction != Vector2.zero))
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
        // Counter logic
        counter++;
        if (counter <= CounterMax)
            return;
        counter = 0;

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
            // Prevents an extra move occurring before death
            if (CheckForInternalCollisions()) return;

            // Update prevMovement
            PrevMovement = movement;

            // Iterate backwards through the body parts, from tail to head
            // The reason for doing this is so every part inherits its next
            // direction from the part before it.
            
            // Tail first
            BodyPart tailPrev = BodyParts[^2];
            BodyParts[^1].Move(tailPrev.Direction);

            // Then the rest of the body, tail - 1 to head
            for (int i = BodyParts.Count - 2; i >= 0; i--)
            {
                BodyPart next = null;
                Vector2 dir = movement;
                if (i > 0)
                {
                    dir = BodyParts[i - 1].Direction;
                }
                if (i + 1 < BodyParts.Count)
                    next = BodyParts[i + 1];
                BodyParts[i].HandleMovement(dir, next);
            }

            // Update to server
            GetComponent<PlayerObjectController>().TryUpdateBodyParts(BodyParts);
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
    /// </summary>
    private void AddBodyPart()
    {
        GameObject newBodyPartObj = Instantiate(_bodyPartTemplate);
        newBodyPartObj.transform.parent = bodyPartContainer.transform;

        BodyPart tail = BodyParts[^1];
        BodyPart newBodyPart = new(tail, newBodyPartObj.transform, new(EBodyPartType.Tail,
            EBodyPartType.Tail))
        {
            Position = tail.Position - (Vector3)tail.Direction
        };

        tail.BPType = new(EBodyPartType.Straight, EBodyPartType.Straight);

        if (BodyParts[^2].BPType.CurrentType == EBodyPartType.Corner)
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
    /// Bisects the snake at the body part which is removed.
    /// </summary>
    /// <param name="bp">The removed body part.</param>
    private void RemoveBodyPart(BodyPart bp)
    {
        int deadIndex = BodyParts.IndexOf(bp);
        if (deadIndex == 0)
        {
            HandleDeath();
        }

        while (true)
        {
            if (deadIndex >= BodyParts.Count) break;
            SetBodyPartDead(BodyParts[deadIndex], true);
            BodyParts.RemoveAt(deadIndex);

            // Remove the body part from the container
            bodyPartContainer.transform.GetChild(deadIndex).parent = GameObject.Find("Objects").transform;
            continue;
        }

        if (BodyParts.Count > 1)
            BodyParts[^1].BPType = new(EBodyPartType.Tail, EBodyPartType.Tail);
        else
        {
            // Snake must have >1 body part left.
            SetDead(true);
        }
    }

    private void SetBodyPartDead(BodyPart bp, bool dead)
    {
        float timeToDestroy = 5;
        bp.Transform.gameObject.GetComponent<SpriteRenderer>().color = dead ? Color.gray : Color.white;

        if (BodyParts.IndexOf(bp) == 0)
        {
            CamBehaviour cb = GameObject.FindWithTag("MainCamera").GetComponent<CamBehaviour>();
            cb.Player = null;
        }

        Destroy(bp.Transform.gameObject, timeToDestroy);
    }

    public void SetDead(bool dead)
    {
        _rb.simulated = !dead;
        frozen = dead;
        foreach (BodyPart part in BodyParts)
            SetBodyPartDead(part, dead);
        status.gameObject.SetActive(!dead);
    }

    /// <summary>
    /// Handles death.
    /// </summary>
    public void HandleDeath()
    {
        SetDead(true);
        GameBehaviour game = GetComponentInChildren<GameBehaviour>();
        game.OnGameOver(score: BodyParts.Count);
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
            { "CounterMax", CounterMax.ToString() }
        };
        for (int i = 0; i < _queuedActions.Count; i++)
            playerValues.Add("queuedActions [" + i.ToString() + "]", _queuedActions[i].Target.ToString());
        playerValues.Add("forcedMovement", _forcedMovement.ToString());
        for (int i = 0; i < _storedBodyPartDirections.Count; i++)
            playerValues.Add("storedBpDirections [" + i.ToString() + "]", _storedBodyPartDirections[i].ToString());
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

    public void QRemoveBodyPart(BodyPart bp)
    {
        _queuedActions.Add(() => RemoveBodyPart(bp));
    }
    public void QRemoveBodyPart(int index)
    {
        QRemoveBodyPart(BodyParts[index]);
    }

    /// <summary>
    /// Queues the beginning of forced movement.
    /// </summary>
    /// <param name="direction">The direction of the forced movement.</param>
    /// <param name="counterMax">The number of frames between each movement.</param>
    public void QBeginForcedMovement(Vector2 direction, int counterMax)
    {
        _queuedActions.Add(new Action(() =>
        {
            frozen = true;
            CounterMax = counterMax;
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
            frozen = false;

            // ! Limitation - the snake must continue,
            // this means if the snake starts on a scootile,
            // they start without input.
            CounterMax = DEFAULT_COUNTER_MAX;
            _forcedMovement = Vector2.zero;
            for (int i = 0; i < BodyParts.Count; i++)
            {
                BodyParts[i].Direction = _storedBodyPartDirections[i];
            }
            _storedBodyPartDirections.Clear();
        }));
    }
}
