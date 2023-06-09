using UnityEngine;

public class WallTilemap : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject col = collision.GetComponent<Collider2D>().gameObject;
        if (col != null)
        {
            if (col.transform.parent != null)
            {
                if (col.transform.parent.CompareTag("Player"))
                {
                    PlayerBehaviour pb = col.transform.parent.GetComponent<PlayerBehaviour>();
                    pb.transform.position -= (Vector3)pb.PrevMovement;
                    if (!pb.canMoveFreely)
                        pb.HandleDeath();
                }
                else if (col.transform.parent.CompareTag("Projectile"))
                {
                    Destroy(col);
                }
            }
        }
    }
}
