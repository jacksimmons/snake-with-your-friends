using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CamBehaviour : MonoBehaviour
{
    public PlayerMovement Player { get; set; } = null;
    private const float FOLLOW_SHARPNESS = 0.1f;

    private void LateUpdate()
    {
        if (Player != null)
        {
            float blend = 1 - Mathf.Pow(1 - FOLLOW_SHARPNESS, Time.deltaTime * 30);

            transform.position = Vector3.Lerp(
            transform.position,
            Player.BodyParts[0].Position + Vector3.back,
            blend);
        }
    }
}
