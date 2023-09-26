using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField]
    private int playerNumber;
    private GameObject player = null;


    private void Start()
    {
        if (playerNumber <= CustomNetworkManager.Instance.Players.Count)
            player = CustomNetworkManager.Instance.Players[playerNumber].gameObject;
    }
}