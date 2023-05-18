using UnityEngine;

public class Teleporter : MonoBehaviour
{
    // The two teleporter objects
    public GameObject A;
    public GameObject B;

    [SerializeField]
    private float rotationSpeed;

    private Rigidbody2D _rb;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject obj = collision.gameObject;
        if (obj.transform.parent.CompareTag("Player"))
        {
            PlayerBehaviour player = obj.transform.GetComponentInParent<PlayerBehaviour>();
            BodyPart bp = player.BodyParts[obj.transform.GetSiblingIndex()];
            if (bp.p_TeleportCounter == 0)
            {
                float dA = (obj.transform.position - A.transform.position).magnitude;
                float dB = (obj.transform.position - B.transform.position).magnitude;
                GameObject teleportTo;

                // Find the shorter distance, and confirm the player is touching that end
                if (dA < dB)
                {
                    teleportTo = B;
                }
                else if (dB < dA)
                {
                    teleportTo = A;
                }
                else
                {
                    // teleportTo is null by default
                    throw new System.Exception("Player is colliding with teleporter " + name + " illegally.");
                }

                // Teleport to the other teleporter
                obj.transform.position = teleportTo.transform.position;

                // Add 2 to the counter (very safe)
                bp.p_TeleportCounter += 2;
            }
        }
    }

    private void FixedUpdate()
    {
        A.transform.Rotate(Vector3.forward * rotationSpeed
        * Time.fixedDeltaTime);
        B.transform.Rotate(Vector3.forward * rotationSpeed
        * Time.fixedDeltaTime);
    }
}
