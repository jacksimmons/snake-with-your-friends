using UnityEngine;

public partial class PlayerBehaviour
{
    /// <summary>
    /// Only collisions that are possible without invincibility are head and other parts.
    /// Therefore, check if the head's position matches any of the others.
    /// </summary>
    /// <returns>Whether a collision (and subsequent death) occurred.</returns>
    private bool InternalCollisions()
    {
        BoxCollider2D bcHead = BodyParts[0].Transform.GetComponent<BoxCollider2D>();
        Collider2D[] result = new Collider2D[1];
        if (bcHead.OverlapCollider(new ContactFilter2D(), result) > 0)
        {
            if (result[0].gameObject.CompareTag("BodyPart"))
            {
                if (!FreeMovement) HandleDeath();
                print("Internal collision.");
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// Collisions with walls and static objects should be evaluated here.
    /// This function is used for detection of collisions along a given
    /// direction directly before the movement occurs.
    /// This allows the death to be identified and the movement that would've
    /// occurred can be changed accordingly.
    /// </summary>
    /// <param name="direction">The direction to test for collisions on.</param>
    /// <returns>Whether a collision (and subsequent death) occurred.</returns>
    private bool ExternalCollisions(Vector2 direction)
    {
        // Player layer is layer 6, want any collision other than Players
        int excludeMask = 1 << 6;
        // NOT the exclude mask (to include everything but the exclude mask layers)
        int layerMask = ~excludeMask;

        BodyPart head = BodyParts[0];
        RaycastHit2D hit;
        if (hit = Physics2D.Raycast(head.Position, direction, 1, layerMask))
        {
            if (hit.collider.gameObject.TryGetComponent<DeathTrigger>(out _))
            {
                if (!FreeMovement) HandleDeath();
                print("External collision.");
                return true;
            }
        }
        return false;
    }
}