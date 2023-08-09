using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collider)
    {
        GameObject obj = collider.gameObject;        

        GameObject maybeProjectile = obj;
        if (maybeProjectile && maybeProjectile.TryGetComponent(out ProjectileBehaviour _))
        {
            Destroy(maybeProjectile);
            return;
        }

        if (collider.transform.parent)
        {
            Transform maybeParent = collider.transform.parent;
            if (maybeParent.parent)
            {
                GameObject maybePlayer = maybeParent.parent.gameObject;
                if (maybePlayer && maybePlayer.CompareTag("Player"))
                {
                    PlayerMovementController player = maybePlayer.GetComponent<PlayerMovementController>();
                    player.transform.position -= (Vector3)player.PrevMovement;
                    if (!player.canMoveFreely)
                        player.HandleDeath();
                    return;
                }
            }
        }
    }
}