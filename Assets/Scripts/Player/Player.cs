using UnityEngine;

public class Player : MonoBehaviour
{
    // Attempts to return the Player transform of a supposed Body Part.
    // Will return null if unsuccessful, indicating that a non-Body Part was provided.
    public static Transform TryGetPlayerTransformFromBodyPart(GameObject obj)
    {
        Transform player;

        // A body part has a parent, so return if our GameObject does not.
        if (obj.transform.parent == null) return null;

        // A body part has a grandparent, and it is the player object.
        player = obj.transform.parent.parent;
        if (player == null) return null;

        // Players must have the Player tag, and immunity must be off.
        if (!player.CompareTag("Player")) return null;

        // All checks passed
        return player;
    }
}