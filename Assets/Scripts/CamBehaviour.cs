using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CamBehaviour : MonoBehaviour
{
    [SerializeField]
    private Lobby _lobby;
    private PlayerBehaviour _player;
    private Transform _playerHead;

    private float _followSharpness = 0.1f;

    private void Awake()
    {
        // i.e. Lobby "DontDestroyOnLoad"s into this scene
        if (_lobby == null)
        {
            _lobby = GameObject.FindWithTag("Lobby").GetComponent<Lobby>();
        }

        _player = _lobby.Player;

        if (_player == null)
            StartCoroutine(WaitForPlayer());
    }

    void LateUpdate()
    {
        if (_player != null)
        {
            float blend = 1 - Mathf.Pow(1 - _followSharpness, Time.deltaTime * 30);

            if (_playerHead != null)
                transform.position = Vector3.Lerp(
                transform.position,
                _playerHead.position + Vector3.back,
                blend);
        }
    }

    private IEnumerator WaitForPlayer()
    {
        _player = _lobby.Player;
        while (_player == null)
        {
            yield return new WaitForSeconds(1);
        }
        _playerHead = _player.transform.GetChild(0);
        yield break;
    }
}
