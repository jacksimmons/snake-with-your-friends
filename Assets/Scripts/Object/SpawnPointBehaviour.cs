using UnityEngine;

public class SpawnPointBehaviour : MonoBehaviour
{
    [SerializeField]
    private int m_playerIndex;
    public int PlayerIndex
    {
        get { return m_playerIndex; }
    }

    //private void OnEnable()
    //{
    //    if (!CustomNetworkManager.Instance)
    //        return;

    //    PlayerObjectController player = null;
    //    if (m_playerIndex <= CustomNetworkManager.Instance.Players.Count)
    //        player = CustomNetworkManager.Instance.Players[m_playerIndex];

    //    player.transform.position = transform.position;

    //    StartCoroutine(Wait.WaitForObjectThen(() => player.PM, 0.1f, (PlayerMovement pm) =>
    //    {
    //        float rot = transform.rotation.eulerAngles.z;
    //        Vector3 startDir = Extensions.Vectors.Rotate(Vector3.up, rot);
                
    //        for (int i = 0; i < pm.BodyParts.Count; i++)
    //        {
    //            pm.BodyParts[i].Position = transform.position - (startDir * i);
    //            pm.BodyParts[i].Direction = startDir;
    //            pm.BodyParts[i].RegularAngle = rot;
    //        }

    //        pm.startingDirection = startDir;
    //        pm.FreeMovement = true;
    //    }));
    //}
}