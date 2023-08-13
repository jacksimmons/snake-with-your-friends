using Mirror;
using UnityEngine;

public class GridObject : NetworkBehaviour
{
    [SyncVar]
    public int gridPos;

    private void Start()
    {
        transform.SetParent(GameObject.Find("Objects").transform);
    }
}