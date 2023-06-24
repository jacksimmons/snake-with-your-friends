using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Struct to represent data of a BodyPart needed for networking.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct BodyPartData
{
    public float pos_x, pos_y;
    public float rotation;
    public int e_bodyPartSprite;
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
    private BodyPartSprite _sprite;
    public BodyPartSprite Sprite
    {
        get { return _sprite; }
        set
        {
            if (value == BodyPartSprite.None)
                return;

            if (SpriteSheet != null)
            {
                _sprite = value;
                Transform.gameObject.GetComponent<SpriteRenderer>().sprite = SpriteSheet[(int)_sprite];
            }
            else
            {
                Debug.LogError("Cannot allocate sprite when there is no sprite sheet.");
            }
        }
    }
    public BodyPartSprite DefaultSprite { get; set; }
    public Sprite[] SpriteSheet { get; set; }
    public Vector2 Direction { get; set; }
    public int TeleportCounter { get; set; }

    // Read-only outside of this class
    private bool _isCorner;
    public bool IsCorner
    {
        get { return _isCorner; }
    }

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
        SpriteSheet = old.SpriteSheet;
        DefaultSprite = old.DefaultSprite;
        _isCorner = old.IsCorner;
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
    public BodyPart(Transform transform, Vector2 direction, BodyPartSprite defaultSprite,
        Sprite[] spriteSheet)
    {
        Transform = transform;
        Direction = direction;
        DefaultSprite = defaultSprite;
        SpriteSheet = spriteSheet;

        _isCorner = false;
        TeleportCounter = 0;

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
        _isCorner = true;
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
        _isCorner = false;
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
            if (next.SpriteSheet != null)
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
    /// Updates object data by a given BodyPartData struct.
    /// </summary>
    /// <param name="data">The struct with updated data.</param>
    public void FromData(BodyPartData data)
    {
        Position = new Vector2(data.pos_x, data.pos_y);
        Rotation = Quaternion.Euler(Vector3.forward * data.rotation);
        Sprite = (BodyPartSprite)data.e_bodyPartSprite;
    }

    /// <summary>
    /// Exports the object as a BodyPartData struct, ready for marshalling.
    /// </summary>
    /// <returns>The exported struct.</returns>
    public BodyPartData ToData()
    {
        BodyPartData data;
        data.pos_x = Position.x;
        data.pos_y = Position.y;
        data.rotation = Rotation.eulerAngles.z;
        data.e_bodyPartSprite = (int)Sprite;
        return data;
    }
}