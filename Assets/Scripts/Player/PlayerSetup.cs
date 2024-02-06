using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

public static class PlayerStatic
{
    public static List<BodyPart> SetupBodyParts(Transform container, float startingRotation, Sprite[] defaultSprites)
    {
        Vector2 startingDirection = Extensions.Vectors.Rotate(Vector2.up, startingRotation);

        // Generate BodyParts structure & starting body parts
        List<BodyPart> parts = new();

        int bodyPartCount = container.childCount;
        for (int i = 0; i < bodyPartCount; i++)
        {
            Transform _transform = container.GetChild(i);
            BodyPart bp = new
            (
                _transform,
                startingDirection,

                // if (i == 0) => Head
                i == 0 ?
                EBodyPartType.Head :
                // else if (i is not the final index) => Straight
                // else => Tail
                i < container.childCount - 1 ?
                EBodyPartType.Straight :
                EBodyPartType.Tail
            );
            parts.Add(bp);

            Sprite sprite;
            switch (bp.CurrentType)
            {
                case EBodyPartType.Head:
                    sprite = defaultSprites[0];
                    break;
                case EBodyPartType.Straight:
                    sprite = defaultSprites[1];
                    break;
                case EBodyPartType.Tail:
                    sprite = defaultSprites[2];
                    break;
                case EBodyPartType.Corner:
                    sprite = defaultSprites[3];
                    break;
                default:
                    sprite = null;
                    break;
            }

            _transform.GetComponent<SpriteRenderer>().sprite = sprite;
        }

        return parts;
    }


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


    public static Transform TryGetOwnedPlayerTransformFromBodyPart(GameObject obj)
    {
        Transform player;

        // Ensure the original player conditions stand
        if (!(player = TryGetPlayerTransformFromBodyPart(obj))) return null;

        // An owned player has isOwned == true
        if (!player.GetComponent<PlayerMovement>().isOwned) return null;

        return player;
    }
}