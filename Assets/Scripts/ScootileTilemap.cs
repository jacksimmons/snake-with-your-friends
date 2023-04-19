using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScootileTilemap : MonoBehaviour
{
	[SerializeField]
	private Vector2 direction;
	private const int scootSpeed = 4;

	private int contacts = 0;

	private void OnTriggerEnter2D(Collider2D other)
	{
		GameObject col = other.gameObject;
		Transform parent = col.transform.parent;
		if (parent != null)
		{
			if (parent.gameObject.CompareTag("Player"))
			{
				parent.GetComponent<PlayerBehaviour>().
				QBeginForcedMovement(direction, scootSpeed);
			}
		}
		contacts++;
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		contacts--;
		GameObject col = other.gameObject;
		Transform parent = col.transform.parent;
		if (parent != null && contacts == 0)
		{
			if (parent.gameObject.CompareTag("Player"))
			{
				parent.GetComponent<PlayerBehaviour>().
				QEndForcedMovement();
			}
		}
	}
}
