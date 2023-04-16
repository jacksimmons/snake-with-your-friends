using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UIElements;

public class BodyPartBehaviour : MonoBehaviour
{
	PlayerBehaviour playerBehaviour;

	private void Awake()
	{
		playerBehaviour = GetComponentInParent<PlayerBehaviour>();
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		print("HI");
		// Handles all snake collision OTHER than internal collisions
		// (collisions of body parts with each other)
		Transform col = collision.collider.transform;
		Transform other = collision.otherCollider.transform;
		if (other != null)
		{
			if (col.parent.CompareTag("Teleporter"))
			{
				//col.GetComponentInParent<Teleporter>().TeleportObj(gameObject);
			}
		}
	}
}
