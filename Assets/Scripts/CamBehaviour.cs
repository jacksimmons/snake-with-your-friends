using UnityEngine;

public class CamBehaviour : MonoBehaviour
{
	[SerializeField]
	private PlayerBehaviour _playerBehaviour;
	private Transform _playerHead;

	private float _followSharpness = 0.1f;
	private Vector3 _offset;

	void Awake()
	{
		_playerHead = _playerBehaviour.transform.GetChild(0);
		_offset = transform.position - _playerHead.position;
	}

	void LateUpdate()
	{
		float blend = 1 - Mathf.Pow(1 - _followSharpness, Time.deltaTime * 30);

		if (_playerHead != null)
			transform.position = Vector3.Lerp(
			transform.position,
			_playerHead.position + _offset,
			blend);
	}
}
