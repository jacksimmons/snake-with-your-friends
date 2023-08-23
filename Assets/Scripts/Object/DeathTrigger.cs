using UnityEngine;

// Script which should be attached to an object with a trigger-based collider which
// kills any player instantly. E.g. a wall, or a rock.
public class DeathTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collider)
    {
        GameObject obj = collider.gameObject;

        //GameObject maybeProjectile = obj;
        //if (maybeProjectile && maybeProjectile.TryGetComponent(out ProjectileBehaviour pb))
        //{
        //    switch (pb.type)
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
                    PlayerMovement pm = maybePlayer.GetComponent<PlayerMovement>();
                    player.position -= (Vector3)pm.PrevMovement;
                    if (!pm.canMoveFreely)
                        pm.HandleDeath();
                    return;
                }
            }
        }
    }
}