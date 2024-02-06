using System;


[Serializable]
public struct OutfitSettingsData
{
    public string[] OutfitSpriteNames;
    public string ColourName;


    public OutfitSettingsData(string[] outfitSpriteNames, string colourName)
    {
        OutfitSpriteNames = outfitSpriteNames;
        ColourName = colourName;
    }
}


[Serializable]
public class OutfitSettings : ICached
{
    private static string[] DEFAULT_SPRITE_NAMES { get; } = new string[5]
    {
        "Default", "Default", "Default", "Default", "Default"
    };
    private const string DEFAULT_COLOUR_NAME = "RedPurple";


    public static OutfitSettings Saved;
    public OutfitSettingsData Data;


    public OutfitSettings()
    {
        string[] spriteNames = new string[5];
        DEFAULT_SPRITE_NAMES.CopyTo(spriteNames, 0);

        Data = new OutfitSettingsData(spriteNames, DEFAULT_COLOUR_NAME);
    }


    // Copy constructor
    public OutfitSettings(OutfitSettings other)
    {
        Data = other.Data;
    }

    public void Cache() { Saved = this; }
}