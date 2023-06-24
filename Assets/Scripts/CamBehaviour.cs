using System.Collections;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CamBehaviour : MonoBehaviour
{
    private PlayerMovementController _player = null;
    private float _followSharpness = 0.1f;

    void LateUpdate()
    {
        if (_player != null)
        {
            float blend = 1 - Mathf.Pow(1 - _followSharpness, Time.deltaTime * 30);

            transform.position = Vector3.Lerp(
            transform.position,
            _player.head.Position + Vector3.back,
            blend);
        }
    }

    public void SetupCamera(PlayerMovementController player)
    {
        _player = player;
    }
}
