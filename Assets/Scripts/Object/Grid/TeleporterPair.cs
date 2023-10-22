using UnityEngine;

public class TeleporterPair : MonoBehaviour
{
    // The two teleporter objects
    public GameObject A;
    public GameObject B;

    [SerializeField]
    private float rotationSpeed;


    // Teleports the first object, from the given portal to the other portal.
    public void TeleportObject(GameObject toTeleport, GameObject fromPortal)
    {
        GameObject toPortal;
        if (fromPortal == A)
            toPortal = B;
        else if (fromPortal == B)
            toPortal = A;
        else
            throw new System.Exception("Given portal is not valid.");

        toPortal.GetComponent<Teleporter>().collidingObjects.Add(toTeleport);

        // Teleport to the other teleporter
        toTeleport.transform.position = toPortal.transform.position;
    }

    private void FixedUpdate()
    {
        A.transform.Rotate(Vector3.forward * rotationSpeed
        * Time.fixedDeltaTime);
        B.transform.Rotate(Vector3.forward * rotationSpeed
        * Time.fixedDeltaTime);
    }
}
