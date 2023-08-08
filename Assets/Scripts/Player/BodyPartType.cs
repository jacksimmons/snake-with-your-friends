public enum EBodyPartType
{
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

    public void FromData(BodyPartTypeData data)
    {
        DefaultType = data.DefaultType;
        CurrentType = data.CurrentType;
    }

    public BodyPartTypeData ToData()
    {
        return new BodyPartTypeData(DefaultType, CurrentType);
    }
}