using UnityEngine;

public enum EBodyPartType
{
    None,
    Straight,
    Head,
    Tail,
    Corner
}

public class BodyPartType
{
    public EBodyPartType DefaultType { get; private set; }
    public EBodyPartType CurrentType { get; set; }

    public BodyPartType(EBodyPartType defaultType, EBodyPartType currentType)
    {
        DefaultType = defaultType;
        CurrentType = currentType;
    }

    public static Sprite TypeToPlayerSprite(EBodyPartType type, PlayerMovementController player)
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