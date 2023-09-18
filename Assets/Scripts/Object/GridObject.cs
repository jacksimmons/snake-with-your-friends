using Mirror;
using UnityEngine;

public class GridObject : NetworkBehaviour
{
    [SyncVar]
    public int gridPos;


    private void Start()
    {
        if (gridPos == -1)
            transform.SetParent(GameObject.Find("Objects").transform);

        // We need to add all manually placed objects to the objects array
        // Manually placed objects have a grid pos of -1
        if (gridPos != -1) return;
        gridPos = GameObject.Find("LocalPlayerObject")
            .transform.Find("Game")
            .GetComponent<GameBehaviour>()
            .AddObjectToGrid(gameObject);
    }
}