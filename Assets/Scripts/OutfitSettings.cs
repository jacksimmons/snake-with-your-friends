using System;

[Serializable]
public class OutfitSettings
{
    public static OutfitSettings Saved = new();

    public string HeadSpriteName { get; set; }
    public string TorsoSpriteName { get; set; }
    public string TailSpriteName { get; set; }
    public string CornerSpriteName { get; set; }

    public string ColourName { get; set; }

    public OutfitSettings()
    {
        HeadSpriteName = string.Empty;
        TorsoSpriteName = string.Empty;
        TailSpriteName = string.Empty;
        CornerSpriteName = string.Empty;

        ColourName = string.Empty;
    }

    // Copy constructor
    public OutfitSettings(OutfitSettings other)
    {
        HeadSpriteName = other.HeadSpriteName;
        TorsoSpriteName = other.TorsoSpriteName;
        TailSpriteName = other.TailSpriteName;
        CornerSpriteName = other.CornerSpriteName;

        ColourName = other.ColourName;
    }

}