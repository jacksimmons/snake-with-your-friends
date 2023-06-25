using System;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Struct to represent data of a BodyPart needed for networking.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct BodyPartData
{
    public Vector2 position;
    public Vector2 direction;
    public float rotation_z;
    public BodyPartSprite currentSprite;
    public BodyPartSprite defaultSprite;
    public int teleportCounter;
    public bool isCorner;
    public BodyPartData(Vector2 position, Vector2 direction, float rotation_z, BodyPartSprite currentSprite,
    BodyPartSprite defaultSprite, int teleportCounter, bool isCorner)
    {
        this.position = position;
        this.direction = direction;
        this.rotation_z = rotation_z;
        this.currentSprite = currentSprite;
        this.defaultSprite = defaultSprite;
        this.teleportCounter = teleportCounter;
        this.isCorner = isCorner;
    }
}

/// <summary>
/// Enum used to represent which sprite is being used.
/// Passed as a message to the other users.
/// For other users, every body part has their full sprite sheet,
/// and sprites are selected by these messages.
/// For local user, only body parts with changing sprites need the
/// full sprite sheet, or a BodyPartSprite property at all.
/// </summary>
public enum BodyPartSprite
{
    Head,
    Tail,
    Straight,
    CornerTopLeft, // r
    CornerTopRight, // 7
    CornerBottomLeft, // l
    CornerBottomRight, // _|
    None
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
    public Quaternion Rotation
    {
        get { return Transform.rotation; }
        set { Transform.rotation = value; }
    }

    // Get components
    private PlayerMovementController Player
    {
        get { return Transform.parent.GetComponentInParent<PlayerMovementController>(); }
    }
    private SpriteRenderer SpriteRen
    {
        get { return Transform.GetComponent<SpriteRenderer>(); }
    }

    private BodyPartSprite _sprite;
    public BodyPartSprite Sprite
    {
        get { return _sprite; }
        set
        {
            if (value == BodyPartSprite.None)
                return;

            if (Player.spriteSheet != null)
            {
                _sprite = value;
                SpriteRen.sprite = Player.spriteSheet[(int)_sprite];
            }
            else
            {
                Debug.LogError("No sprite sheet provided in PlayerMovementController!");
            }
        }
    }
    public BodyPartSprite DefaultSprite { get; set; }
    public Vector2 Direction { get; set; }
    public int TeleportCounter { get; set; } = 0;
    public bool IsCorner { get; set; } = false;

    // Rotation before it became a corner, useful only to parts after this one
    public Quaternion prevRot = Quaternion.identity;


    /// <summary>
    /// Copy body part constructor.
    /// </summary>
    /// <param name="old"></param>
    /// <param name="transform"></param>
    public BodyPart(BodyPart old, Transform transform)
    {
        Transform = transform;
        Direction = old.Direction;
        DefaultSprite = old.DefaultSprite;
        IsCorner = old.IsCorner;
        // Will not affect teleporting UNLESS necessary
        TeleportCounter = old.TeleportCounter + 1;
        prevRot = old.prevRot;
    }

    /// <summary>
    /// Standard body part constructor.
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="direction"></param>
    /// <param name="defaultSprite"></param>
    /// <param name="cornerSprites"></param>
    public BodyPart(Transform transform, Vector2 direction, BodyPartSprite defaultSprite)
    {
        Transform = transform;
        Direction = direction;
        DefaultSprite = defaultSprite;

        Sprite = DefaultSprite;
    }

    /// <summary>
    /// Turns a body part into a corner piece.
    /// Uses its current direction and the previous part's direction to calculate
    /// what type of corner sprite is required.
    /// </summary>
    /// <param name="prevDir">The direction of the previous body part.</param>
    public void MakeCorner(Vector2 prevDir)
    {
        IsCorner = true;
        Rotation = Quaternion.identity;

        if (Direction == Vector2.up)
        {
            if (prevDir == Vector2.left)
                Sprite = BodyPartSprite.CornerTopRight;
            //Sprite = CornerSprites[3]; // -R
            else if (prevDir == Vector2.right)
                Sprite = BodyPartSprite.CornerTopLeft;
            //Sprite = CornerSprites[2]; // R
        }

        else if (Direction == Vector2.left)
        {
            if (prevDir == Vector2.up)
                Sprite = BodyPartSprite.CornerBottomLeft;
            //Sprite = CornerSprites[0]; // L
            else if (prevDir == Vector2.down)
                Sprite = BodyPartSprite.CornerTopLeft;
            //Sprite = CornerSprites[2]; // R
        }

        else if (Direction == Vector2.down)
        {
            if (prevDir == Vector2.left)
                Sprite = BodyPartSprite.CornerBottomRight;
            //Sprite = CornerSprites[1]; // -L
            else if (prevDir == Vector2.right)
                Sprite = BodyPartSprite.CornerBottomLeft;

            //Sprite = CornerSprites[0]; // L
        }

        else if (Direction == Vector2.right)
        {
            if (prevDir == Vector2.up)
                Sprite = BodyPartSprite.CornerBottomRight;

            //Sprite = CornerSprites[1]; // -L
            else if (prevDir == Vector2.down)
                Sprite = BodyPartSprite.CornerTopRight;

            //Sprite = CornerSprites[3]; // -R
        }
    }

    /// <summary>
    /// Reverts a body part into its default form.
    /// </summary>
    /// <param name="rotation">The rotation to assign it, as pieces obtain
    /// a rotation of Quat.Identity after becoming corners.</param>
    public void MakeNotCorner(Quaternion rotation)
    {
        IsCorner = false;
        Rotation = rotation;
        Sprite = DefaultSprite;
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
        Transform.Rotate(Vector3.forward, angle);

        // If the body part is a corner piece
        if (next != null)
        {
            // If the next part isn't a tail, and is an angled body part,
            // make it a corner.
            if (next.DefaultSprite != BodyPartSprite.Tail)
            {
                if (angle != 0)
                {
                    if (!next.IsCorner)
                        next.prevRot = next.Rotation;
                    next.MakeCorner(newDirection);
                }
                else
                {
                    // When making `next` not a corner, set its rotation
                    // to our prevRot (if this is a corner), or our rotation
                    // (if this isn't a corner)
                    Quaternion rot = Rotation;
                    if (IsCorner)
                        rot = prevRot;
                    next.MakeNotCorner(rot);
                }
            }
            // If it is a tail, rotate alongside this body part
            else
            {
                // Store the tail's previous rotation in prevRot
                // This is needed in some situations in HandleAddBodyPart
                next.prevRot = next.Rotation;
                next.Transform.Rotate(Vector3.forward, angle);
            }
        }
    }

    /// <summary>
    /// Returns a BodyPart from a transform and a BodyPartData struct.
    /// </summary>
    /// <param name="data">The struct with updated data.</param>
    /// <param name="transform">The transform of the physical body part.</param>
    public static BodyPart FromData(BodyPartData data, Transform transform)
    {
        BodyPart bp = new(transform, data.direction, data.defaultSprite)
        {
            Position = data.position,
            Rotation = Quaternion.Euler(Vector3.forward * data.rotation_z),
            Sprite = data.currentSprite,
            TeleportCounter = data.teleportCounter,
            IsCorner = data.isCorner,
        };
        return bp;
    }

    /// <summary>
    /// Exports an object as a BodyPartData struct.
    /// </summary>
    /// <returns>The exported struct.</returns>
    public static BodyPartData ToData(BodyPart bp)
    {
        BodyPartData data = new(bp.Position, bp.Direction, bp.Rotation.eulerAngles.z, bp.Sprite, bp.DefaultSprite,
            bp.TeleportCounter, bp.IsCorner);
        return data;
    }
}