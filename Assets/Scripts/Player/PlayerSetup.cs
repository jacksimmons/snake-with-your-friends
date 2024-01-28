using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

public static class PlayerSetup
{
    public static List<BodyPart> SetupBodyParts(Transform container, float startingRotation, List<Sprite> defaultSprites)
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
}