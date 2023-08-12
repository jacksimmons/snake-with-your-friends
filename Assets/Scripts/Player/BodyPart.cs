using System;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Struct to represent data of a BodyPart needed for networking.
/// </summary>
public struct BodyPartData
{
    public Vector2 position;
    public Vector2 direction;
    public float CornerAngle;
    public float RegularAngle;
    public EBodyPartType DefaultType;
    public EBodyPartType CurrentType;
    public int teleportCounter;
    public BodyPartData(Vector2 position, Vector2 direction, float cornerAngle, float regularAngle,
        EBodyPartType defaultType, EBodyPartType currentType, int teleportCounter)
    {
        this.position = position;
        this.direction = direction;
        this.CornerAngle = cornerAngle;
        this.RegularAngle = regularAngle;
        this.DefaultType = defaultType;
        this.CurrentType = currentType;
        this.teleportCounter = teleportCounter;
    }
}

public class BodyPart
{
    public Transform Transform { get; private set; }

    // Get transform properties
    public Vector3 Position
    {
        get { return Transform.position; }
        set { Transform.position = value; }
    }

    private float _corAngle;
    public float CornerAngle
    {
        get { return _corAngle; }
        set
        {
            // Assign corner angle to transform (implicit state change)
            _corAngle = value % 360;
            if (BPType?.CurrentType != EBodyPartType.Corner) Transform.rotation = Quaternion.Euler(Vector3.forward * _corAngle);
        }
    }

    private float _regAngle;
    public float RegularAngle
    {
        get { return _regAngle; }
        set
        {
            // Assign regular angle to transform (implicit state change)
            _regAngle = value % 360;
            if (BPType?.CurrentType != EBodyPartType.Corner) Transform.rotation = Quaternion.Euler(Vector3.forward * _regAngle);
        }
    }

    private PlayerMovementController Player;

    public Vector2 Direction { get; set; }
    public int TeleportCounter { get; set; }

    private BodyPartType _bpType = null;
    public BodyPartType BPType
    {
        get { return _bpType; }
        set
        {
            if (_bpType != null)
            {
                if (value.DefaultType != EBodyPartType.None && value.DefaultType != _bpType.DefaultType)
                    DefaultSprite = BodyPartType.TypeToPlayerSprite(value.DefaultType, Player);
                if (value.CurrentType != EBodyPartType.None && value.CurrentType != _bpType.CurrentType)
                    SetSprite(BodyPartType.TypeToPlayerSprite(value.CurrentType, Player));
            }
            else if (value != null)
            {
                DefaultSprite = BodyPartType.TypeToPlayerSprite(value.DefaultType, Player);
                SetSprite(BodyPartType.TypeToPlayerSprite(value.CurrentType, Player));
            }
            _bpType = value;
        }
    }

    public Sprite DefaultSprite { get; private set; }

    /// <summary>
    /// Copy body part constructor.
    /// </summary>
    public BodyPart(BodyPart old, Transform transform, BodyPartType type)
    {
        Transform = transform;
        Init();

        BPType = type;

        Direction = old.Direction;
        RegularAngle = old.RegularAngle;
        CornerAngle = old.CornerAngle;

        // Will not affect teleporting UNLESS necessary
        TeleportCounter = old.TeleportCounter + 1;
    }

    // Constructor Notes:
    // Transform MUST be initialised first due to dependency.
    // BPType should be initialised before Regular/CornerAngle, due to dependency.

    /// <summary>
    /// Standard body part constructor.
    /// </summary>
    public BodyPart(Transform transform, Vector2 direction, EBodyPartType type)
    {
        Transform = transform;
        Init();

        BPType = new(type, type);

        Direction = direction;
        RegularAngle = 0;
        CornerAngle = 0;

        TeleportCounter = 0;
    }

    /// <summary>
    /// Complete body part constructor.
    /// </summary>
    public BodyPart(Transform transform, Vector2 position, Vector2 direction, float regAngle,
        float corAngle, BodyPartType type, int teleportCounter)
    {
        Transform = transform;
        Init();

        BPType = type;

        Position = position;
        Direction = direction;
        RegularAngle = regAngle;
        CornerAngle = corAngle;

        TeleportCounter = teleportCounter;
    }

    private void Init()
    {
        Player = Transform.parent.parent.GetComponent<PlayerMovementController>();
    }

    private void SetSprite(Sprite sprite)
    {
        Transform.GetComponent<SpriteRenderer>().sprite = sprite;
    }

    private Sprite GetSprite()
    {
        return Transform.GetComponent<SpriteRenderer>().sprite;
    }

    /// <summary>
    /// Turns a body part into a corner piece.
    /// Uses its current direction and the previous part's direction to calculate
    /// what type of corner sprite is required.
    /// </summary>
    /// <param name="prevDir">The direction of the previous body part.</param>
    public void MakeCorner(Vector2 prevDir)
    {
        if (BPType.DefaultType == EBodyPartType.Tail
            || BPType.DefaultType == EBodyPartType.Head)
            return;

        int cornerRot = 0;

        if (Direction == Vector2.up)
        {
            if (prevDir == Vector2.left)
                cornerRot = -180;
            //Sprite = CornerSprites[3]; // -R
            else if (prevDir == Vector2.right)
                cornerRot = -90;
            //Sprite = CornerSprites[2]; // R
        }

        else if (Direction == Vector2.left)
        {
            if (prevDir == Vector2.up)
                cornerRot = -0;
            //Sprite = CornerSprites[0]; // L
            else if (prevDir == Vector2.down)
                cornerRot = -90;
            //Sprite = CornerSprites[2]; // R
        }

        else if (Direction == Vector2.down)
        {
            if (prevDir == Vector2.left)
                cornerRot = -270;
            //Sprite = CornerSprites[1]; // -L
            else if (prevDir == Vector2.right)
                cornerRot = -0;
            //Sprite = CornerSprites[0]; // L
        }

        else if (Direction == Vector2.right)
        {
            if (prevDir == Vector2.up)
                cornerRot = -270;
            //Sprite = CornerSprites[1]; // -L
            else if (prevDir == Vector2.down)
                cornerRot = -180;
            //Sprite = CornerSprites[3]; // -R
        }

        BPType.CurrentType = EBodyPartType.Corner;
        CornerAngle = cornerRot;
        SetSprite(Player.m_bpCornerL);
    }

    /// <summary>
    /// Reverts a body part into its default form.
    /// </summary>
    public void MakeNotCorner()
    {
        BPType.CurrentType = BPType.DefaultType;
        SetSprite(DefaultSprite);
    }

    /// <summary>
    /// Core movement method, adds to the position and reduces the teleport
    /// counter.
    /// </summary>
    /// <param name="direction">The direction to move in next</param>
    public void Move(Vector2 direction)
    {
        Direction = direction;
        Position += (Vector3)Direction;

        if (TeleportCounter > 0)
            TeleportCounter--;
    }

    /// <summary>
    /// Complex movement handling with corner piece handling.
    /// </summary>
    /// <param name="newDirection">The new direction to move along.</param>
    /// <param name="next">The "next" body part - directly after this one
    /// in the child ordering</param>
    public void HandleMovement(Vector2 newDirection, BodyPart next)
    {
        // Store the previous direction for use in angles
        Vector2 prevDirection = Direction;

        // Move the body part
        Move(newDirection);

        // Rotate the body part
        float angle = Vector2.SignedAngle(prevDirection, Direction);
        RegularAngle += angle;

        if (next != null)
        {
            // If the next part isn't a tail, and is an angled body part,
            // make it a corner.
            if (!(next.BPType.CurrentType == EBodyPartType.Tail))
            {
                if (!Mathf.Approximately(angle, 0))
                {
                    next.MakeCorner(newDirection);
                }
                else
                {
                    next.MakeNotCorner();
                }
            }
            // If it is a tail, move in the same way as this body part
            else
            {
                next.Direction = Direction;
                next.RegularAngle += angle;
            }
        }
        else
        {
            Debug.LogWarning("Next body part is null!");
        }
    }

    /// <summary>
    /// Returns a BodyPart from a transform and a BodyPartData struct.
    /// </summary>
    /// <param name="data">The struct with updated data.</param>
    /// <param name="transform">The transform of the physical body part.</param>
    public static BodyPart FromData(BodyPartData data, Transform transform)
    {
        BodyPart bp = new(transform, data.position, data.direction, data.RegularAngle,
            data.CornerAngle, new(data.DefaultType, data.CurrentType), data.teleportCounter);
        return bp;
    }

    /// <summary>
    /// Exports an object as a BodyPartData struct.
    /// </summary>
    /// <returns>The exported struct.</returns>
    public static BodyPartData ToData(BodyPart bp)
    {
        BodyPartData data = new(bp.Position, bp.Direction, bp.CornerAngle, bp.RegularAngle,
            bp.BPType.DefaultType, bp.BPType.CurrentType, bp.TeleportCounter);
        return data;
    }
}