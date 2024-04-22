using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

// Class which controls the Body Parts of a Player object.
// Is Destroyed when the player dies, but other components are kept.
public class PlayerMovement : NetworkBehaviour
{
    /// <summary>
    /// Body Parts
    /// </summary>
    public List<BodyPart> BodyParts;
    [SerializeField]
    private GameObject m_bodyPartContainer;
    public GameObject BodyPartContainer
    {
        get { return m_bodyPartContainer; }
    }
    [SerializeField]
    private GameObject m_bodyPartTemplate;


    /// <summary>
    /// Components
    /// </summary>
    private PlayerControls m_pc;
    private PlayerObjectController m_poc;
    [SerializeField]
    private GameBehaviour m_gameBehaviour;
    [SerializeField]
    public Sprite[] DefaultSprites; // Head, Torso, Tail, Corner, Hat


    /// <summary>
    /// Player settings
    /// </summary>
    private BitField bf = new(1);
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

    // Shorthand for m_direction != Vector2.zero
    public bool IsMoving
    {
        get
        {
            if (!bf.GetBit(2) && m_direction != Vector2.zero)
                bf.SetBit(2, true);

            return bf.GetBit(2);
        }
        set { bf.SetBit(2, value); }
    }


    /// <summary>
    /// Movement variables
    /// </summary>
    private Vector2 m_direction;
    private Vector2 m_prevDirection;
    private Vector2 m_forcedDirection; // Where player cannot stop/change dir


    /// <summary>
    /// Queued actions
    /// </summary>
    private List<Action> m_queuedActions; // Executed after the next movement frame
    private List<Vector2> m_storedBodyPartDirections;


    private void Awake()
    {
        // Input setup
        m_pc = new();
        m_pc.Gameplay.Move.performed += ctx => HandleDirectionInput(Extensions.Vectors.StickToDPad(ctx.ReadValue<Vector2>()));
        m_pc.Gameplay.Move.canceled += ctx => HandleDirectionInput(Vector2.zero);

        //m_pn = GetComponent<PlayerNetwork>();
        m_poc = GetComponent<PlayerObjectController>();
        BodyParts = new();
    }


    private void OnEnable()
    {
        m_pc.Gameplay.Enable();
        Init();
    }


    private void OnDisable()
    {
        m_pc.Gameplay.Disable();
    }


    private void Init()
    {
        // Initialise movement variables
        m_direction = Vector2.zero;
        m_prevDirection = Vector2.zero;
        m_forcedDirection = Vector2.zero;

        SetupBodyParts();

        // Generate QueuedActions structure
        m_queuedActions = new List<Action>();

        // First update, necessary for second, third etc games
        if (isOwned)
            m_poc.UpdateBodyParts();
    }


    private void SetupBodyParts()
    {
        RecreateStartingParts(2);
        BodyPartContainer.SetActive(false);
        BodyParts = PlayerStatic.SetupBodyParts(BodyPartContainer.transform, 0, DefaultSprites);
        m_poc.UpdateBodyParts();
        BodyPartContainer.SetActive(true);
    }


    private void HandleDirectionInput(Vector2 dir)
    {
        if (!isOwned)
            return;

        // Prevent snakes going back on themselves
        bool canGoBackOnItself = FreeMovement && BodyParts.Count <= 2;
        if (!canGoBackOnItself)
        {
            if (dir == -m_prevDirection)
            {
                dir = Vector2.zero;
            }
        }

        // Free movement
        if (FreeMovement)
        {
            if (dir != m_direction)
            {
                m_direction = dir;
            }
        }
        else if (dir != Vector2.zero)
        {
            m_direction = dir;
        }
    }


    public void RecreateStartingParts(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject bp = Instantiate(m_bodyPartTemplate, m_bodyPartContainer.transform);
            bp.transform.position += Vector3.up * (count - i - 1);
        }
    }


    private void FixedUpdate()
    {
        if (!isOwned) return;

        if (Frozen)
        {
            m_direction = Vector2.zero;
            return;
        }
    }


    /// <summary>
    /// Handles movement for all body parts, and the frequency of movement ticks.
    /// Called by the lobby every synced moveframe.
    /// </summary>
    public void HandleMovementLoop()
    {
        // Queued actions wait until the next move frame before being called.
        for (int i = 0; i < m_queuedActions.Count; i++)
        {
            m_queuedActions[0]();
            m_queuedActions.RemoveAt(0);
        }

        // Prevents an extra move occurring before death
        if (CheckForInternalCollisions()) return;

        if (m_direction != Vector2.zero)
        {
            if (CheckForExternalCollisions(m_direction)) return;

            // `PrevMovement` should only be updated if `movement` causes no external collisions.
            m_prevDirection = m_direction;

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
            BodyParts[0].HandleMovement(m_direction, BodyParts[1]);

            // Update to server
            m_poc.UpdateBodyParts();
        }
    }

    /// <summary>
    /// Only collisions that are possible without invincibility are head and other parts.
    /// Therefore, check if the head's position matches any of the others.
    /// </summary>
    /// <returns>Whether a collision (and subsequent death) occurred.</returns>
    private bool CheckForInternalCollisions()
    {
        BoxCollider2D bcHead = BodyParts[0].Transform.GetComponent<BoxCollider2D>();
        Collider2D[] result = new Collider2D[1];
        if (bcHead.OverlapCollider(new ContactFilter2D(), result) > 0)
        {
            if (result[0].gameObject.CompareTag("BodyPart"))
            {
                if (!FreeMovement) HandleDeath();
                print("Internal collision.");
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// Collisions with walls and static objects should be evaluated here.
    /// This function is used for detection of collisions along a given
    /// direction directly before the movement occurs.
    /// This allows the death to be identified and the movement that would've
    /// occurred can be changed accordingly.
    /// </summary>
    /// <param name="direction">The direction to test for collisions on.</param>
    /// <returns>Whether a collision (and subsequent death) occurred.</returns>
    private bool CheckForExternalCollisions(Vector2 direction)
    {
        // Player layer is layer 6, want any collision other than Players
        int excludeMask = 1 << 6;
        // NOT the exclude mask (to include everything but the exclude mask layers)
        int layerMask = ~excludeMask;

        BodyPart head = BodyParts[0];
        RaycastHit2D hit;
        if (hit = Physics2D.Raycast(head.Position, direction, 1, layerMask))
        {
            if (hit.collider.gameObject.TryGetComponent<DeathTrigger>(out _))
            {
                if (!FreeMovement) HandleDeath();
                print("External collision.");
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
        if (!IsMoving)
            return;

        GameObject newBodyPartObj = Instantiate(m_bodyPartTemplate);
        newBodyPartObj.transform.parent = BodyPartContainer.transform;

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
        if (!IsMoving)
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
    /// When the player dies. (Loses a body part when only has 2, or got hit in the head.)
    /// </summary>
    public void HandleDeath()
    {
        m_poc.LogDeath();
        GetComponent<PlayerStatus>().ClearAll();

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
        m_queuedActions.Add(action);
    }

    /// <summary>
    /// Queues an AddBodyPart action.
    /// </summary>
    public void QAddBodyPart()
    {
        m_queuedActions.Add(new Action(AddBodyPart));
    }

    public void QRemoveBodyPart(int collisionIndex = -1)
    {
        if (collisionIndex == 0)
            HandleDeath();
        else
            m_queuedActions.Add(() => RemoveBodyPart());
    }

    /// <summary>
    /// Queues the beginning of forced movement.
    /// </summary>
    /// <param name="direction">The direction of the forced movement.</param>
    /// <param name="timeToMove">The number of seconds between each movement.</param>
    public void QBeginForcedMovement(Vector2 direction, float timeToMove)
    {
        m_queuedActions.Add(new Action(() =>
        {
            Frozen = true;
            m_forcedDirection = direction;
            foreach (var part in BodyParts)
            {
                m_storedBodyPartDirections.Add(part.Direction);
                part.Direction = m_forcedDirection;
            }
        }));
    }


    /// <summary>
    /// Queues the ending of forced movement.
    /// </summary>
    public void QEndForcedMovement()
    {
        m_queuedActions.Add(new Action(() =>
        {
            Frozen = false;

            // ! Limitation - the snake must continue,
            // this means if the snake starts on a scootile,
            // they start without input.
            // It also removes all speed modifiers
            m_forcedDirection = Vector2.zero;
            for (int i = 0; i < BodyParts.Count; i++)
            {
                BodyParts[i].Direction = m_storedBodyPartDirections[i];
            }
            m_storedBodyPartDirections.Clear();
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
            { "direction", m_direction.ToString() },
            { "prevDirection", m_prevDirection.ToString() }
        };
        for (int i = 0; i < m_queuedActions.Count; i++)
            playerValues.Add("queuedActions [" + i.ToString() + "]", m_queuedActions[i].Target.ToString());
        playerValues.Add("forcedMovement", m_forcedDirection.ToString());
        for (int i = 0; i < m_storedBodyPartDirections.Count; i++)
            playerValues.Add("storedBpDirections [" + i.ToString() + "]", m_storedBodyPartDirections[i].ToString());
        return playerValues;
    }
}
