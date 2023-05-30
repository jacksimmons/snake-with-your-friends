using System.Collections;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CamBehaviour : MonoBehaviour
{
    private PlayerBehaviour _player = null;
    private float _followSharpness = 0.1f;

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

    public void SetupCamera(PlayerBehaviour player)
    {
        _player = player;
    }
}
