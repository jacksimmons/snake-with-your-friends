using System.Collections;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CamBehaviour : MonoBehaviour
{
    [SerializeField]
    private Lobby _lobby;
    public PlayerBehaviour _player;

    private float _followSharpness = 0.1f;

    private void Awake()
    {
        StartCoroutine(WaitForPlayer());
    }

    void LateUpdate()
    {
        if (_player != null)
        {
            float blend = 1 - Mathf.Pow(1 - _followSharpness, Time.deltaTime * 30);

            transform.position = Vector3.Lerp(
            transform.position,
            _player.head.p_Position + Vector3.back,
            blend);
        }
    }

    private IEnumerator WaitForPlayer()
    {
        while (_lobby == null || _player == null)
        {
            try
            {
                _lobby = GameObject.FindWithTag("Lobby").GetComponent<Lobby>();
                _player = _lobby.Player;
            }
            catch { }
            yield return new WaitForSeconds(1);
        }
        yield break;
    }
}
