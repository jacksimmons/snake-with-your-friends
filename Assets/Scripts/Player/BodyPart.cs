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
    public BodyPartRotationData bpRotationData;
    public Sprite currentSprite;
    public Sprite defaultSprite;
    public int teleportCounter;
    public BodyPartTypeData bpTypeData;
    public BodyPartData(Vector2 position, Vector2 direction, BodyPartRotationData bpRotationData, Sprite currentSprite,
    Sprite defaultSprite, int teleportCounter, BodyPartTypeData bpTypeData)
    {
        this.position = position;
        this.direction = direction;
        this.bpRotationData = bpRotationData;
        this.currentSprite = currentSprite;
        this.defaultSprite = defaultSprite;
        this.teleportCounter = teleportCounter;
        this.bpTypeData = bpTypeData;
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

    public BodyPartRotation Rotation { get; private set; }

    // Get components
    private PlayerMovementController Player
    {
        get { return Transform.parent.parent.GetComponent<PlayerMovementController>(); }
    }

    public Sprite DefaultSprite { get; set; }
    public Vector2 Direction { get; set; }
    public int TeleportCounter { get; set; }
    public BodyPartType BodyPartType { get; set; }

    /// <summary>
    /// Copy body part constructor.
    /// </summary>
    public BodyPart(BodyPart old, Transform transform, BodyPartType type)
    {
        Transform = transform;
        BodyPartType = type;

        Direction = old.Direction;
        DefaultSprite = old.DefaultSprite;
        SetSprite(old.GetSprite());
        Rotation = new BodyPartRotation(transform);
        Rotation.RegularAngle = old.Rotation.RegularAngle;

        // Will not affect teleporting UNLESS necessary
        TeleportCounter = old.TeleportCounter + 1;
    }

    /// <summary>
    /// Standard body part constructor.
    /// </summary>
    public BodyPart(Transform transform, Vector2 direction, Sprite defaultSprite,
        EBodyPartType type)
    {
        Transform = transform;
        Direction = direction;
        DefaultSprite = defaultSprite;
        SetSprite(defaultSprite);
        Rotation = new(transform);
        BodyPartType = new(type, type);
        TeleportCounter = 0;
    }

    /// <summary>
    /// Complete body part constructor.
    /// </summary>
    public BodyPart(Transform transform, Vector2 position, BodyPartRotation rotation,
        Sprite defaultSprite, Sprite currentSprite, Vector2 direction, int teleportCounter,
        BodyPartType bodyPartType)
    {
        Transform = transform;
        Position = position;
        Rotation = rotation;
        DefaultSprite = defaultSprite;
        SetSprite(currentSprite);
        Direction = direction;
        TeleportCounter = teleportCounter;
        BodyPartType = bodyPartType;
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
        if (BodyPartType.DefaultType == EBodyPartType.Tail
            || BodyPartType.DefaultType == EBodyPartType.Head)
            return;

        BodyPartType.CurrentType = EBodyPartType.Corner;

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

        Rotation.CornerAngle = cornerRot;
        SetSprite(Player.m_bpCornerL);
    }

    /// <summary>
    /// Reverts a body part into its default form.
    /// </summary>
    public void MakeNotCorner()
    {
        BodyPartType.CurrentType = BodyPartType.DefaultType;

        Rotation.RegularAngle = Rotation.RegularAngle; // => transform.rotation = Rotation.RegularAngle
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
        Rotation.RegularAngle += angle;

        if (next != null)
        {
            // If the next part isn't a tail, and is an angled body part,
            // make it a corner.
            if (!(next.BodyPartType.CurrentType == EBodyPartType.Tail))
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
                next.Rotation.RegularAngle += angle;
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
        BodyPart bp = new(transform, data.direction, new(transform, data.bpRotationData),
            data.defaultSprite, data.currentSprite, data.direction, data.teleportCounter,
            new(data.bpTypeData));
        return bp;
    }

    /// <summary>
    /// Exports an object as a BodyPartData struct.
    /// </summary>
    /// <returns>The exported struct.</returns>
    public static BodyPartData ToData(BodyPart bp)
    {
        BodyPartData data = new(bp.Position, bp.Direction, bp.Rotation.ToData(), bp.GetSprite(),
            bp.DefaultSprite, bp.TeleportCounter, bp.BodyPartType.ToData());
        return data;
    }
}