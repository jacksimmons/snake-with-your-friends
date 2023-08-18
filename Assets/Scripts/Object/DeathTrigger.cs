using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collider)
    {
        GameObject obj = collider.gameObject;

        //GameObject maybeProjectile = obj;
        //if (maybeProjectile && maybeProjectile.TryGetComponent(out ProjectileBehaviour pb))
        //{
        //    switch (pb.Type)
        //    {
        //        case EProjectileType.Shit:
        //            pb.HandleCollision(EProjectileCollisionType.Splat, true);
        //            break;
        //        case EProjectileType.HurtOnce:
        //            pb.HandleCollision(EProjectileCollisionType.Bounce, true);
        //            break;
        //    }
        //    return;
        //}

        Transform player = Player.TryGetPlayerTransformFromBodyPart(obj);
        if (player == null) return;

        if (collider.transform.parent)
        {
            Transform maybeParent = collider.transform.parent;
            if (maybeParent.parent)
            {
                GameObject maybePlayer = maybeParent.parent.gameObject;
                if (maybePlayer && maybePlayer.CompareTag("Player"))
                {
                    PlayerMovementController pmc = maybePlayer.GetComponent<PlayerMovementController>();
                    player.position -= (Vector3)pmc.PrevMovement;
                    if (!pmc.canMoveFreely)
                        pmc.HandleDeath();
                    return;
                }
            }
        }
    }
}