using Mirror;
using UnityEngine;

public class GridObject : NetworkBehaviour
{
    private int _gridPos = -1;
    public int GridPos
    {
        get
        {
            if (_gridPos == -1)
            {
                Debug.LogError("GridPos has not been defined!");
            }
            return _gridPos;
        }
        set
        {
            if (_gridPos == -1)
            {
                _gridPos = value;
            }
            else
            {
                Debug.LogWarning("GridPos has already been set. Ignoring.");
            }
        }
    }

    private void Start()
    {
        transform.SetParent(GameObject.Find("Objects").transform);
    }
}