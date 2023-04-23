using UnityEngine;

public class FoodBehaviour : MonoBehaviour
{
	[SerializeField]
	private e_Food food;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		GameObject obj = collision.gameObject;
		if (obj.transform.parent != null && obj.transform.parent.CompareTag("Player"))
		{
			PlayerBehaviour player = obj.transform.GetComponentInParent<PlayerBehaviour>();
			if (player != null)
			{
				player.QAddBodyPart();
				player.status.Eat(food);
			}

			Destroy(gameObject);
		}
	}
}
