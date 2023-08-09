using System.Collections;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CamBehaviour : MonoBehaviour
{
    public PlayerMovementController Player { get; set; } = null;
    private float _followSharpness = 0.1f;

    void LateUpdate()
    {
        if (Player != null)
        {
            float blend = 1 - Mathf.Pow(1 - _followSharpness, Time.deltaTime * 30);

            transform.position = Vector3.Lerp(
            transform.position,
            Player.BodyParts[0].Position + Vector3.back,
            blend);
        }
    }
}
