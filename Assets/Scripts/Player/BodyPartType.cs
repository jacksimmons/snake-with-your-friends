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
            case EBodyPartType.Straight:
                sprite = player.m_bpStraight;
                break;
            case EBodyPartType.Head:
                sprite = player.m_bpHead;
                break;
            case EBodyPartType.Tail:
                sprite = player.m_bpTail;
                break;
            case EBodyPartType.Corner:
                sprite = player.m_bpCornerL;
                break;
        }
        return sprite;
    }
}