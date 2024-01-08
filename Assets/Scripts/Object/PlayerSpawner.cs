using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField]
    private int playerNumber;


    private void OnEnable()
    {
        if (!CustomNetworkManager.Instance)
            return;

        PlayerObjectController player = null;
        if (playerNumber <= CustomNetworkManager.Instance.Players.Count)
            player = CustomNetworkManager.Instance.Players[playerNumber];

        player.transform.position = transform.position;

        StartCoroutine(Wait.WaitForObjectThen(() => player.PM, 0.1f, (PlayerMovement pm) =>
        {
            float rot = transform.rotation.eulerAngles.z;
            Vector3 startDir = Extensions.Vectors.Rotate(Vector3.up, rot);

            for (int i = 0; i < pm.BodyParts.Count; i++)
            {
                pm.BodyParts[i].Position = transform.position - (startDir * i);
                pm.BodyParts[i].Direction = startDir;
                pm.BodyParts[i].RegularAngle = rot;
            }

            pm.startingDirection = startDir;
            pm.FreeMovement = true;
        }));
    }
}