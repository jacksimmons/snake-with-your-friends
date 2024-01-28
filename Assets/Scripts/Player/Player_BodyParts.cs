using System.Collections.Generic;
using System.Globalization;
using System;
using UnityEditor;
using UnityEngine;

public partial class PlayerBehaviour
{
    // Settings
    private float m_startingRotation; // In degrees, z-rotation applied to (0, 1, 0)

    // Body Parts
    [SerializeField]
    private GameObject m_bodyPartTemplate;
    [SerializeField]
    private GameObject m_bodyPartContainer;
    public GameObject BodyPartContainer { get { return m_bodyPartContainer; } }
    public List<BodyPart> BodyParts { get; private set; }
    private List<Vector2> m_storedBodyPartDirections = new List<Vector2>();

    // Queued actions
    private List<Action> m_queuedActions;


    private void SetupBodyParts(Sprite head, Sprite torso, Sprite tail, Sprite corner)
    {
        Vector2 startingDirection = Extensions.Vectors.Rotate(Vector2.up, m_startingRotation);

        BodyPartContainer.SetActive(false);

        // Generate BodyParts structure & starting body parts
        BodyParts = new List<BodyPart>();
        Transform containerTransform = BodyPartContainer.transform;

        int bodyPartCount = containerTransform.childCount;
        if (bodyPartCount == 0)
        {
            RecreateStartingParts(2);
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

            Sprite sprite;
            switch (bp.CurrentType)
            {
                case EBodyPartType.Head:
                    sprite = head;
                    break;
                case EBodyPartType.Straight:
                    sprite = torso;
                    break;
                case EBodyPartType.Tail:
                    sprite = tail;
                    break;
                case EBodyPartType.Corner:
                    sprite = corner;
                    break;
                default:
                    sprite = null;
                    break;
            }

            _transform.GetComponent<SpriteRenderer>().sprite = sprite;
        }

        // Generate QueuedActions structure
        m_queuedActions = new List<Action>();

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


    private void RecreateStartingParts(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject bp = Instantiate(m_bodyPartTemplate, BodyPartContainer.transform);
            bp.transform.position += Vector3.up * (count - i - 1);
        }
    }


    /// <summary>
    /// Handles movement for all body parts, and the frequency of movement ticks.
    /// Called by the lobby every synced moveframe.
    /// </summary>
    public void HandleMovementLoop()
    {
        // Counter logic
        m_timeTillMove -= Time.fixedDeltaTime;
        if (m_timeTillMove > 0)
            return;

        m_timeTillMove = m_timeBetweenMoves;

        // Queued actions wait until the next move frame before being called.
        for (int i = 0; i < m_queuedActions.Count; i++)
        {
            m_queuedActions[0]();
            m_queuedActions.RemoveAt(0);
        }

        // Prevents an extra move occurring before death
        if (InternalCollisions()) return;

        if (HasMoved && m_direction != Vector2.zero)
        {
            if (ExternalCollisions(m_direction)) return;

            // Previous direction should only be updated if direction causes no external collisions.
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
}