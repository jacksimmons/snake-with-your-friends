using UnityEngine;

public partial class Steam : MonoBehaviour
{
    private static Steam _instance;
    public static Steam Instance
    {
        get
        {
            if (_instance != null) return _instance;

            GameObject go = GameObject.Find("NetworkManager");
            if (go == null) return null;
            return _instance = go.GetComponent<Steam>();
        }
    }
}