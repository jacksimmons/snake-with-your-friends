using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamBehaviour : MonoBehaviour
{
	[SerializeField]
	private PlayerBehaviour playerBehaviour;
	private GameObject player;
	private Camera cam;

	private float followSharpness = 0.1f;
	private Vector3 offset;

	void Awake()
	{
		cam = gameObject.GetComponent<Camera>();
		player = playerBehaviour.gameObject;
		offset = transform.position - player.transform.position;
	}

	// Start is called before the first frame update

	void Start()
    {
    }

	void LateUpdate()
	{
		float blend = 1 - Mathf.Pow(1 - followSharpness, Time.deltaTime * 30);

		transform.position = Vector3.Lerp(
			transform.position,
			player.transform.position + offset,
			blend);
	}
}
