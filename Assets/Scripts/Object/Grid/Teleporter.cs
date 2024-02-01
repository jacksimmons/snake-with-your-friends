using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public List<GameObject> collidingObjects = new();
    private TeleporterPair m_pair;

    private void Awake()
    {
        m_pair = transform.parent.GetComponent<TeleporterPair>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject obj = collision.gameObject;
        Transform playerTransform;

        if ((playerTransform = PlayerStatic.TryGetPlayerTransformFromBodyPart(obj)) != null)
        {
            PlayerMovement player = playerTransform.GetComponentInParent<PlayerMovement>();
            BodyPart bp = player.BodyParts[obj.transform.GetSiblingIndex()];

            TryTeleportObject(bp.Transform.gameObject);
        }
        else
        {
            TryTeleportObject(obj);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collidingObjects.Contains(collision.gameObject))
            collidingObjects.Remove(collision.gameObject);
    }

    private void TryTeleportObject(GameObject obj)
    {
        if (collidingObjects.Contains(obj)) return;

        m_pair.TeleportObject(obj, gameObject);
    }
}
