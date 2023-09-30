using UnityEngine;

public class Shooter : MonoBehaviour
{
    [SerializeField]
    private float freqInSeconds;
    private float timeSinceLast = 0;
    [SerializeField]
    private GameObject ammo;

    private EProjectileType projectileType;



    private void Start()
    {
        projectileType = ammo.GetComponent<ProjectileBehaviour>().type;
    }


    private void Update()
    {
        if (timeSinceLast < freqInSeconds)
        {
            timeSinceLast += Time.deltaTime;
            return;
        }

        timeSinceLast = 0;

        GameObject inst = Instantiate(ammo, transform);
        ProjectileBehaviour pb = inst.GetComponent<ProjectileBehaviour>();

        switch (projectileType)
        {
            case EProjectileType.Fireball:
                pb.Proj = Projectiles.ConstructFireball(
                    Extensions.Vectors.Rotate(Vector2.up, transform.rotation.eulerAngles.z),
                    transform.rotation.eulerAngles.z);
                break;
        }
    }
}