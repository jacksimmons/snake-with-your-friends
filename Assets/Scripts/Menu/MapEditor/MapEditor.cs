using UnityEngine;

public enum ECreatorTool
{
    None,
    Brush,
    Fill,
    SelectObject,
}
public enum ECreatorLayer
{
    None,
    Ground,
    Wall,
    Object
}


public static class MapEditor
{
    public static GridObjectDictionary GridObjDict = new();
}