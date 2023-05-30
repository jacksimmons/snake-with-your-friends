using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Struct to represent data of a BodyPart needed for networking.
/// </summary>
public struct BodyPartData
{
    public float pos_x, pos_y;
    public float rotation;
    public BodyPartSprite sprite;
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
    private Transform _transform;
    public Transform p_Transform
    {
        get { return _transform; }
    }
    public Vector3 p_Position
    {
        get { return _transform.position; }
        set { _transform.position = value; }
    }
    public Quaternion p_Rotation
    {
        get { return _transform.rotation; }
        set { _transform.rotation = value; }
    }
    private BodyPartSprite _sprite;
    public BodyPartSprite p_Sprite
    {
        get { return _sprite; }
        set
        {
            if (value == BodyPartSprite.None)
                return;

            if (p_SpriteSheet != null)
            {
                _sprite = value;
                _transform.gameObject.GetComponent<SpriteRenderer>().sprite = p_SpriteSheet[(int)_sprite];
            }
            else
            {
                Debug.LogError("Cannot allocate sprite when there is no sprite sheet.");
            }
        }
    }
    public BodyPartSprite p_DefaultSprite { get; set; }
    public Sprite[] p_SpriteSheet { get; set; }
    public Vector2 p_Direction { get; set; }
    public int p_TeleportCounter { get; set; }

    // Read-only outside of this class
    private bool _isCorner;
    public bool p_IsCorner
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
        _transform = transform;
        p_Direction = old.p_Direction;
        p_SpriteSheet = old.p_SpriteSheet;
        p_DefaultSprite = old.p_DefaultSprite;
        _isCorner = old.p_IsCorner;
        // Will not affect teleporting UNLESS necessary
        p_TeleportCounter = old.p_TeleportCounter + 1;
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
        _transform = transform;
        p_Direction = direction;
        p_DefaultSprite = defaultSprite;
        p_SpriteSheet = spriteSheet;

        _isCorner = false;
        p_TeleportCounter = 0;

        p_Sprite = p_DefaultSprite;
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
        p_Rotation = Quaternion.identity;

        if (p_Direction == Vector2.up)
        {
            if (prevDir == Vector2.left)
                p_Sprite = BodyPartSprite.CornerTopRight;
            //p_Sprite = p_CornerSprites[3]; // -R
            else if (prevDir == Vector2.right)
                p_Sprite = BodyPartSprite.CornerTopLeft;
            //p_Sprite = p_CornerSprites[2]; // R
        }

        else if (p_Direction == Vector2.left)
        {
            if (prevDir == Vector2.up)
                p_Sprite = BodyPartSprite.CornerBottomLeft;
            //p_Sprite = p_CornerSprites[0]; // L
            else if (prevDir == Vector2.down)
                p_Sprite = BodyPartSprite.CornerTopLeft;
            //p_Sprite = p_CornerSprites[2]; // R
        }

        else if (p_Direction == Vector2.down)
        {
            if (prevDir == Vector2.left)
                p_Sprite = BodyPartSprite.CornerBottomRight;
            //p_Sprite = p_CornerSprites[1]; // -L
            else if (prevDir == Vector2.right)
                p_Sprite = BodyPartSprite.CornerBottomLeft;

            //p_Sprite = p_CornerSprites[0]; // L
        }

        else if (p_Direction == Vector2.right)
        {
            if (prevDir == Vector2.up)
                p_Sprite = BodyPartSprite.CornerBottomRight;

            //p_Sprite = p_CornerSprites[1]; // -L
            else if (prevDir == Vector2.down)
                p_Sprite = BodyPartSprite.CornerTopRight;

            //p_Sprite = p_CornerSprites[3]; // -R
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
        p_Rotation = rotation;
        p_Sprite = p_DefaultSprite;
    }

    /// <summary>
    /// Core movement method, adds to the position and reduces the teleport
    /// counter.
    /// </summary>
    /// <param name="direction">The direction to move in next</param>
    public void Move(Vector2 direction)
    {
        p_Direction = direction;
        p_Position += (Vector3)p_Direction;

        if (p_TeleportCounter > 0)
            p_TeleportCounter--;
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
        Vector2 prevDirection = p_Direction;

        // Move the body part
        Move(newDirection);

        // Rotate the body part
        float angle = Vector2.SignedAngle(prevDirection, p_Direction);
        _transform.Rotate(Vector3.forward, angle);

        // If the body part is a corner piece
        if (next != null)
        {
            // If the next part isn't a tail, and is an angled body part,
            // make it a corner.
            if (next.p_SpriteSheet != null)
            {
                if (angle != 0)
                {
                    if (!next.p_IsCorner)
                        next.prevRot = next.p_Rotation;
                    next.MakeCorner(newDirection);
                }
                else
                {
                    // When making `next` not a corner, set its rotation
                    // to our prevRot (if this is a corner), or our rotation
                    // (if this isn't a corner)
                    Quaternion rot = p_Rotation;
                    if (p_IsCorner)
                        rot = prevRot;
                    next.MakeNotCorner(rot);
                }
            }
            // If it is a tail, rotate alongside this body part
            else
            {
                // Store the tail's previous rotation in prevRot
                // This is needed in some situations in HandleAddBodyPart
                next.prevRot = next.p_Rotation;
                next.p_Transform.Rotate(Vector3.forward, angle);
            }
        }
    }

    public BodyPartData ToData()
    {
        BodyPartData data;
        data.pos_x = p_Position.x;
        data.pos_y = p_Position.y;
        data.rotation = p_Rotation.z;
        data.sprite = p_Sprite;
        return data;
    }
}