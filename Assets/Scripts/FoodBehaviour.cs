using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodBehaviour : MonoBehaviour
{
	public enum Food
	{
		Coffee,
		Booze,
		Apple,
		Orange,
		Banana,
		FireFruit,
		Drumstick,
		Bone,
		Cheese,
		Pizza,
		Pineapple,
		PineapplePizza,
		IceCream
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		GameObject obj = collision.gameObject;
		if (obj.transform.parent.CompareTag("Player"))
		{
			PlayerBehaviour player = obj.transform.GetComponentInParent<PlayerBehaviour>();
			player.AddToBodyPartQueue();
			Destroy(gameObject);
		}
	}
}
