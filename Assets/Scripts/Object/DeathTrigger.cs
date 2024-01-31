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

        Transform player = PlayerStatic.TryGetPlayerTransformFromBodyPart(obj);
        if (player == null) return;

        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (!pm.FreeMovement)
            pm.HandleDeath();
        return;
    }
}