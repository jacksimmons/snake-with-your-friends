using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public PlayerMovementController Player { get; set; }

    private void Update()
    {
        transform.position = Player.transform.position;
    }
}
