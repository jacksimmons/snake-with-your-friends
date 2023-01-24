using Extensions;
using UnityEditor;
using UnityEngine;

public class BodyPartBehaviour : MonoBehaviour
{
	public int index;
	public Vector2 direction = Vector2.zero;
	// Positive Infinity is used as "there is no cutoff point"
	private Vector2 currentCutoffPoint = Vector2.positiveInfinity;

	private GameObject player;
	private PlayerBehaviour playerBehaviour;

	private SpriteRenderer sr;
	private Sprite sprite;

	void Awake()
	{
		player = transform.parent.gameObject;

		// ! Every bp needs a unique name
		player.transform.Find(name);

		playerBehaviour = player.GetComponent<PlayerBehaviour>();

		sr = GetComponent<SpriteRenderer>();
		sprite = sr.sprite;
	}

	public void Move(float speed)
	{
		Vector2 displacement = speed * direction;

		print(name);
		print(direction);

		// Turn position into a component of direction
		if (!Vectors.componentGreaterThanOrEqualTo((Vector2)transform.position * direction + displacement, currentCutoffPoint))
		{
			transform.Translate((Vector3)displacement);
		}
		else
		{
			transform.position = (Vector3)currentCutoffPoint;
			Rotate();
			currentCutoffPoint = Vector2.positiveInfinity;
		}
	}

	public void Rotate()
	{
		Vector2 previousDirection = direction;

		if (index == 0)
		{
			direction = playerBehaviour.travellingDirection;
		}
		else
		{
			direction = playerBehaviour.body[index - 1].direction;
		}

		transform.Rotate(Vector3.forward * Vector2.Angle(previousDirection, direction));
	}

	public void SetCutoffPointIfNone(Vector2 cutoffPoint)
	{
		if (currentCutoffPoint == Vector2.positiveInfinity)
		{
			Debug.DrawLine(cutoffPoint, cutoffPoint + direction * 20);
			currentCutoffPoint = cutoffPoint;
		}
	}
}
