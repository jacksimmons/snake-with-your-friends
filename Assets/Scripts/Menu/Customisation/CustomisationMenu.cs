using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomisationMenu : MonoBehaviour
{
    public string currentColourScheme;
    public Dictionary<string, Dictionary<ECustomisationPart, Sprite[]>> spriteDictionary;

    private void Awake()
    {
        if (OutfitSettings.Saved.Data.ColourName == string.Empty)
            OutfitSettings.Saved.Data.ColourName = "RedPurple";

        currentColourScheme = OutfitSettings.Saved.Data.ColourName;

        Dictionary<ECustomisationPart, Sprite[]> redPurple = new()
        {
            { ECustomisationPart.Head, GetPartSprites("RedPurple", ECustomisationPart.Head) },
            { ECustomisationPart.Torso, GetPartSprites("RedPurple", ECustomisationPart.Torso) },
            { ECustomisationPart.Tail, GetPartSprites("RedPurple", ECustomisationPart.Tail) },
            { ECustomisationPart.Corner, GetPartSprites("RedPurple", ECustomisationPart.Corner) }
        };

        spriteDictionary = new()
        {
            { "RedPurple", redPurple }
        };
    }


    private Sprite[] GetPartSprites(string colour, ECustomisationPart part)
    {
        string filePath = $"Snake/{colour}/{part}";
        return Resources.LoadAll<Sprite>(filePath);
    }
}
