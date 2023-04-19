using UnityEngine;

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
	
	public Sprite p_Sprite
	{
		get { return _transform.gameObject.GetComponent<SpriteRenderer>().sprite; }
		set { _transform.gameObject.GetComponent<SpriteRenderer>().sprite = value; }
	}
	public Sprite p_DefaultSprite { get; set; }
	public Sprite[] p_CornerSprites { get; set; }

	public float p_Speed { get; set; }
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

	// For copying a body part (except for transform)
	public BodyPart(BodyPart old, Transform transform)
	{
		_transform = transform;
		p_Direction = old.p_Direction;
		p_Speed = old.p_Speed;
		p_Sprite = old.p_Sprite;
		p_DefaultSprite = old.p_DefaultSprite;
		p_CornerSprites = old.p_CornerSprites;
		_isCorner = old.p_IsCorner;
		// Will not affect teleporting UNLESS necessary
		p_TeleportCounter = old.p_TeleportCounter + 1;
		prevRot = old.prevRot;
	}

	// For a body part that isn't the tail
	public BodyPart(Transform transform, Vector2 direction, Sprite defaultSprite,
		Sprite[] cornerSprites, float movementSpeed)
	{
		_transform = transform;
		p_Direction = direction;
		p_DefaultSprite = defaultSprite;
		p_CornerSprites = cornerSprites;

		_isCorner = false;
		p_TeleportCounter = 0;
		p_Sprite = p_DefaultSprite;
		p_Speed = movementSpeed;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="prevDir">The direction of the previous body part.</param>
	public void MakeCorner(Vector2 prevDir)
	{
		_isCorner = true;
		p_Rotation = Quaternion.identity;

		if (p_Direction == Vector2.up)
		{
			if (prevDir == Vector2.left)
				p_Sprite = p_CornerSprites[3]; // -R
			else if (prevDir == Vector2.right)
				p_Sprite = p_CornerSprites[2]; // R
		}

		else if (p_Direction == Vector2.left)
		{
			if (prevDir == Vector2.up)
				p_Sprite = p_CornerSprites[0]; // L
			else if (prevDir == Vector2.down)
				p_Sprite = p_CornerSprites[2]; // R
		}

		else if (p_Direction == Vector2.down)
		{
			if (prevDir == Vector2.left)
				p_Sprite = p_CornerSprites[1]; // -L
			else if (prevDir == Vector2.right)
				p_Sprite = p_CornerSprites[0]; // L
		}

		else if (p_Direction == Vector2.right)
		{
			if (prevDir == Vector2.up)
				p_Sprite = p_CornerSprites[1]; // -L
			else if (prevDir == Vector2.down)
				p_Sprite = p_CornerSprites[3]; // -R
		}
	}

	public void MakeNotCorner(Quaternion rotation)
	{
		_isCorner = false;
		p_Rotation = rotation;
		p_Sprite = p_DefaultSprite;
	}

	public void Move(Vector2 direction)
	{
		p_Direction = direction;
		p_Position += (Vector3)(p_Direction * p_Speed);

		if (p_TeleportCounter > 0)
			p_TeleportCounter--;
	}

	/// <summary>
	/// Complex movement handling with corner piece handling.
	/// </summary>
	/// <param name="newDirection">The new direction to move along.</param>
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
			if (next.p_CornerSprites != null)
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
}