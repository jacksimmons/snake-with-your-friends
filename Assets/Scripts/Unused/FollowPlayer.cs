using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
	public PlayerBehaviour p_Player { get; set; }

	private void Update()
	{
		transform.position = p_Player.transform.position;
		print(p_Player.name);
	}
}
