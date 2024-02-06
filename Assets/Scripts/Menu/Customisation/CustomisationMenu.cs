using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomisationMenu : MonoBehaviour
{
    public ESnakeColourType currentColourScheme;
    public Dictionary<ESnakeColourType, Dictionary<ECustomisationPart, Sprite[]>> spriteDictionary;

    private void Awake()
    {
        Dictionary<ECustomisationPart, Sprite[]> redPurple = new()
        {
            { ECustomisationPart.Head, GetPartSprites("RedPurple", ECustomisationPart.Head) },
            { ECustomisationPart.Torso, GetPartSprites("RedPurple", ECustomisationPart.Torso) },
            { ECustomisationPart.Tail, GetPartSprites("RedPurple", ECustomisationPart.Tail) },
            { ECustomisationPart.Corner, GetPartSprites("RedPurple", ECustomisationPart.Corner) }
        };

        spriteDictionary = new()
        {
            { ESnakeColourType.RedPurple, redPurple }
        };
    }

    private Sprite[] GetPartSprites(string colour, ECustomisationPart part)
    {
        string filePath = $"Snake/{colour}/{part}";
        return Resources.LoadAll<Sprite>(filePath);
    }
}
