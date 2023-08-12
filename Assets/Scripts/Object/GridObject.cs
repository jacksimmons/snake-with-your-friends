using Mirror;
using UnityEngine;

public class GridObject : NetworkBehaviour
{
    public SetOnce<int> gridPos = new();

    private void Start()
    {
        transform.SetParent(GameObject.Find("Objects").transform);
    }
}