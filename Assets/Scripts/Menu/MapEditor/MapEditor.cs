using UnityEngine;

public enum ECreatorTool
{
    Brush,
    Fill,
    Pick,
}
public enum ECreatorLayer
{
    Ground,
    Wall,
    Object,
    Food
}


public static class MapEditor
{
    public static GridObjectDictionary GridObjDict = new();
}