using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField]
    private int playerNumber;


    private void Awake()
    {
        PlayerObjectController player = null;
        if (playerNumber <= CustomNetworkManager.Instance.Players.Count)
            player = CustomNetworkManager.Instance.Players[playerNumber];

        player.transform.position = transform.position;

        for (int i = 0; i < player.PM.BodyParts.Count; i++)
        {
            float rot = transform.rotation.eulerAngles.z;
            player.PM.BodyParts[i].Position = transform.position;
            player.PM.BodyParts[i].Direction = Extensions.Vectors.Rotate(Vector2.up, rot);
            player.PM.BodyParts[i].RegularAngle = rot;
        }

        player.PM.startingDirection = player.PM.BodyParts[0].Position - player.PM.BodyParts[1].Position;
    }
}