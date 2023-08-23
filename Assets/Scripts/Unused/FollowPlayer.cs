using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public PlayerMovement Player { get; set; }

    private void Update()
    {
        transform.position = Player.transform.position;
    }
}
