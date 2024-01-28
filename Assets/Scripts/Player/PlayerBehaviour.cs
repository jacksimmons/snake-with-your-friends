using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Path.GUIFramework;
using UnityEngine;

public partial class PlayerBehaviour : MonoBehaviour
{
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
    public bool HasMoved
    {
        get
        {
            if (!bf.GetBit(2) && m_direction != Vector2.zero)
                bf.SetBit(2, true);

            return bf.GetBit(2);
        }
        set { bf.SetBit(2, value); }
    }

    // Movement variables
    private Vector2 m_direction;
    private Vector2 m_prevDirection;
    private Vector2 m_forcedDirection;

    private float m_timeBetweenMoves;
    private float m_timeTillMove;

    // Other player components
    private PlayerObjectController m_poc;
    private PlayerNetwork m_pn;
    private PlayerControls m_pc;

    [SerializeField]
    private Sprite[] m_defaultSprites;


    private void Awake()
    {
        // Input setup
        m_pc = new();
        m_pc.Gameplay.Move.performed += ctx => HandleDirectionInput(Extensions.Vectors.StickToDPad(ctx.ReadValue<Vector2>()));
        m_pc.Gameplay.Move.canceled += ctx => HandleDirectionInput(Vector2.zero);

        m_pn = GetComponent<PlayerNetwork>();
        m_poc = GetComponent<PlayerObjectController>();

        Init();
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
        // Initialise game settings
        if (GameSettings.Saved != null)
            m_timeBetweenMoves = GameSettings.Saved.TimeToMove;

        // Initialise other variables
        m_direction = Vector2.zero;
        m_prevDirection = Vector2.zero;
        m_timeTillMove = m_timeBetweenMoves;

        m_pc.Gameplay.Enable();

        SetupBodyParts(m_defaultSprites[0], m_defaultSprites[1], m_defaultSprites[2], m_defaultSprites[3]);
        print(BodyParts[0].Position);
    }


    private void Start()
    {
        Start_Status();
    }


    private void HandleDirectionInput(Vector2 dir)
    {
        // Forced movement
        //if (Frozen)
        //{
        //    m_direction = m_forcedDirection;
        //    return;
        //}

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
                m_timeTillMove = m_timeBetweenMoves;
            }
        }
        else if (dir != Vector2.zero)
        {
            m_direction = dir;
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
    /// Outputs debug values for this object.
    /// </summary>
    /// <returns>A dictionary containing variable names and their values.</returns>
    public Dictionary<string, string> GetPlayerDebug()
    {
        Dictionary<string, string> playerValues = new Dictionary<string, string>
        {
            { "direction", m_direction.ToString() },
            { "prevDirection", m_prevDirection.ToString() },
            { "timeBetweenMoves", m_timeBetweenMoves.ToString() }
        };
        for (int i = 0; i < m_queuedActions.Count; i++)
            playerValues.Add("queuedActions [" + i.ToString() + "]", m_queuedActions[i].Target.ToString());
        playerValues.Add("forcedDirection", m_forcedDirection.ToString());
        for (int i = 0; i < m_storedBodyPartDirections.Count; i++)
            playerValues.Add("storedBpDirections [" + i.ToString() + "]", m_storedBodyPartDirections[i].ToString());
        return playerValues;
    }
}