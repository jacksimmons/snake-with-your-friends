using UnityEngine;


public enum EBodyPartType
{
    None,
    Straight,
    Head,
    Tail,
    Corner
}


public static class BodyPartType
{
    public static Sprite TypeToPlayerSprite(EBodyPartType type, PlayerMovement player)
    {
        Sprite sprite = null;
        switch (type)
        {
            case EBodyPartType.Head:
                sprite = player.DefaultSprites[0];
                break;
            case EBodyPartType.Straight:
                sprite = player.DefaultSprites[1];
                break;
            case EBodyPartType.Tail:
                sprite = player.DefaultSprites[2];
                break;
            case EBodyPartType.Corner:
                sprite = player.DefaultSprites[3];
                break;
        }
        return sprite;
    }
}