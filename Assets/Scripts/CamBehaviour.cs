using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamBehaviour : MonoBehaviour
{
	[SerializeField]
	private PlayerBehaviour playerBehaviour;
	private Transform playerHead;
	private Camera cam;

	private float followSharpness = 0.1f;
	private Vector3 offset;

	void Awake()
	{
		cam = gameObject.GetComponent<Camera>();
		playerHead = playerBehaviour.gameObject.transform.GetChild(0);
		offset = transform.position - playerHead.position;
	}

	void LateUpdate()
	{
		float blend = 1 - Mathf.Pow(1 - followSharpness, Time.deltaTime * 30);

		transform.position = Vector3.Lerp(
			transform.position,
			playerHead.position + offset,
			blend);
	}
}
