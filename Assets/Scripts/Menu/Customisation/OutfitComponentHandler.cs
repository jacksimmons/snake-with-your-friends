using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutfitComponentHandler : MonoBehaviour
{
    private int m_currentIndex;

    [SerializeField]
    private ECustomisationPart m_outfitComponent;
    private CustomisationMenu m_menu;
    private Image m_outfitComponentImage;
    [SerializeField]
    private GameObject[] m_previewObjectsToUpdate;

    private void Start()
    {
        m_menu = transform.parent.parent.GetComponent<CustomisationMenu>();
        m_outfitComponentImage = transform.Find("Part").GetComponent<Image>();

        if (OutfitSettings.Saved.ColourName == string.Empty)
            OutfitSettings.Saved.ColourName = "RedPurple";

        switch (m_outfitComponent)
        {
            case ECustomisationPart.Head:
                LoadOutfitComponentSprite("Heads", OutfitSettings.Saved.HeadSpriteName);
                break;
            case ECustomisationPart.Torso:
                LoadOutfitComponentSprite("Torsos", OutfitSettings.Saved.TorsoSpriteName);
                break;
            case ECustomisationPart.Tail:
                LoadOutfitComponentSprite("Tails", OutfitSettings.Saved.TailSpriteName);
                break;
            case ECustomisationPart.Corner:
                LoadOutfitComponentSprite("Corners", OutfitSettings.Saved.CornerSpriteName);
                break;
        }

        // Get current index of the sprite in the appropriate folder in sprite dictionary.
        m_currentIndex = Array.IndexOf(
            m_menu.spriteDictionary[m_menu.currentColourScheme][m_outfitComponent],
            m_outfitComponentImage.sprite);
    }

    private void LoadOutfitComponentSprite(string outfitComponentName, string filename)
    {
        if (filename == "") filename = m_outfitComponentImage.sprite.name;

        UpdateAllSpriteInstances(Resources.Load<Sprite>
            ($"Snake/{OutfitSettings.Saved.ColourName}/{outfitComponentName}/{filename}"));
    }

    private void UpdateAllSpriteInstances(Sprite sprite)
    {
        m_outfitComponentImage.sprite = sprite;

        foreach (GameObject go in m_previewObjectsToUpdate)
        {
            go.GetComponent<Image>().sprite = sprite;
        }

        GameObject lpo = GameObject.Find("LocalPlayerObject");
        if (!lpo)
        {
            StartCoroutine(Wait.WaitForObjectThen(
                () => GameObject.Find("LocalPlayerObject"),
                0.1f,
                (GameObject lpo) => UpdatePlayerSpriteInstance(sprite, lpo)));
            return;
        }

        UpdatePlayerSpriteInstance(sprite);
    }

    private void UpdatePlayerSpriteInstance(Sprite sprite, GameObject lpo = null)
    {
        if (!lpo)
        {
            lpo = GameObject.Find("LocalPlayerObject");
        }

        PlayerMovement pm = lpo.GetComponent<PlayerMovement>();

        switch (m_outfitComponent)
        {
            case ECustomisationPart.Head:
                OutfitSettings.Saved.HeadSpriteName = sprite.name;
                pm.DefaultSprites[0] = sprite;
                break;
            case ECustomisationPart.Torso:
                OutfitSettings.Saved.TorsoSpriteName = sprite.name;
                pm.DefaultSprites[1] = sprite;
                break;
            case ECustomisationPart.Tail:
                OutfitSettings.Saved.TailSpriteName = sprite.name;
                pm.DefaultSprites[2] = sprite;
                break;
            case ECustomisationPart.Corner:
                OutfitSettings.Saved.CornerSpriteName = sprite.name;
                pm.DefaultSprites[3] = sprite;
                break;
        }
    }

    public void OnOutfitComponentChanged(bool isRight)
    {
        Sprite[] sprites = m_menu.spriteDictionary[m_menu.currentColourScheme][m_outfitComponent];

        if (sprites.Length == 0)
        {
            Debug.LogWarning("No sprites to cycle through.");
            return;
        }

        if (isRight)
        {
            if (m_currentIndex + 1 < sprites.Length)
            {
                m_currentIndex++;
            }
            else
            {
                m_currentIndex = 0;
            }
        }
        else
        {
            if (m_currentIndex - 1 >= 0)
            {
                m_currentIndex--;
            }
            else
            {
                m_currentIndex = sprites.Length - 1;
            }
        }

        UpdateAllSpriteInstances(sprites[m_currentIndex]);

        Saving.SaveToFile(OutfitSettings.Saved, "OutfitSettings.dat");
    }
}
