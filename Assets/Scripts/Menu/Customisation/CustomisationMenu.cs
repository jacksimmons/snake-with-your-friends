using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomisationMenu : MonoBehaviour
{
    public enum OutfitComponent
    {
        Head,
        Torso,
        Tail,
        Corner,
        Colour,
        Hat
    }

    public enum SnakeColourScheme
    {
        RedPurple,
    }

    public SnakeColourScheme currentColourScheme;
    public Dictionary<SnakeColourScheme, Dictionary<OutfitComponent, Sprite[]>> spriteDictionary;

    private void Awake()
    {
        Dictionary<OutfitComponent, Sprite[]> redPurple = new()
        {
            { OutfitComponent.Head, GetPartSprites("RedPurple", "Heads") },
            { OutfitComponent.Torso, GetPartSprites("RedPurple", "Torsos") },
            { OutfitComponent.Tail, GetPartSprites("RedPurple", "Tails") },
            { OutfitComponent.Corner, GetPartSprites("RedPurple", "Corners") }
        };

        spriteDictionary = new()
        {
            { SnakeColourScheme.RedPurple, redPurple }
        };
    }

    private Sprite[] GetPartSprites(string colour, string part)
    {
        string filePath = $"Snake/{colour}/{part}";
        return Resources.LoadAll<Sprite>(filePath);
    }
}
