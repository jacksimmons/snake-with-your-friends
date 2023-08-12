using UnityEngine;

public enum EBodyPartType
{
    None,
    Straight,
    Head,
    Tail,
    Corner
}

public struct BodyPartTypeData
{
    public EBodyPartType DefaultType { get; private set; }
    public EBodyPartType CurrentType { get; private set; }

    public BodyPartTypeData(EBodyPartType defaultType, EBodyPartType currentType)
    {
        DefaultType = defaultType;
        CurrentType = currentType;
    }
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

    public BodyPartType(BodyPartTypeData data)
    {
        FromData(data);
    }

    public static BodyPartType FromData(BodyPartTypeData data)
    {
        return new BodyPartType(data.DefaultType, data.CurrentType);
    }

    public static BodyPartTypeData ToData(BodyPartType type)
    {
        return new BodyPartTypeData(type.DefaultType, type.CurrentType);
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