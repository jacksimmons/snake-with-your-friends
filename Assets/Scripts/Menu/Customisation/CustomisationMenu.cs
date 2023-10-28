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
            { ECustomisationPart.Head, GetPartSprites("RedPurple", "Heads") },
            { ECustomisationPart.Torso, GetPartSprites("RedPurple", "Torsos") },
            { ECustomisationPart.Tail, GetPartSprites("RedPurple", "Tails") },
            { ECustomisationPart.Corner, GetPartSprites("RedPurple", "Corners") }
        };

        spriteDictionary = new()
        {
            { ESnakeColourType.RedPurple, redPurple }
        };
    }

    private Sprite[] GetPartSprites(string colour, string part)
    {
        string filePath = $"Snake/{colour}/{part}";
        return Resources.LoadAll<Sprite>(filePath);
    }
}
