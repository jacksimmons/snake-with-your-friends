using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField]
    private int playerNumber;
    private GameObject player = null;
    private bool stopTeleporting = false;


    private void Awake()
    {
        if (playerNumber <= CustomNetworkManager.Instance.Players.Count)
            player = CustomNetworkManager.Instance.Players[playerNumber].gameObject;

        player.transform.position = transform.position;
    }
}