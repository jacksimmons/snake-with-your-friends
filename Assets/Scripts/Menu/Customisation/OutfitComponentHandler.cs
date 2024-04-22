using System;
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
        m_menu = GameObject.FindWithTag("CustomisationMenu").GetComponent<CustomisationMenu>();
        m_outfitComponentImage = transform.Find("Part").GetComponent<Image>();

        string filename = OutfitSettings.Saved.Data.OutfitSpriteNames[(int)m_outfitComponent];
        LoadOutfitComponentSprite(m_outfitComponent, filename);

        // Get current index of the sprite in the appropriate folder in sprite dictionary.
        m_currentIndex = Array.IndexOf(
            m_menu.spriteDictionary[m_menu.currentColourScheme][m_outfitComponent],
            m_outfitComponentImage.sprite);
    }


    private void LoadOutfitComponentSprite(ECustomisationPart part, string filename)
    {
        Sprite sprite = Resources.Load<Sprite>($"Snake/{OutfitSettings.Saved.Data.ColourName}/{part}/{filename}");
        if (sprite == null)
            Debug.LogError($"Null resource: Snake/{OutfitSettings.Saved.Data.ColourName}/{part}/{filename}");
        UpdateAllSpriteInstances(sprite);
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
                (GameObject lpo) => lpo.GetComponent<PlayerOutfit>().UpdatePlayerSprite(m_outfitComponent, sprite, true)));
            return;
        }

        lpo.GetComponent<PlayerOutfit>().UpdatePlayerSprite(m_outfitComponent, sprite, true);
    }


    public void OnOutfitComponentChanged(bool isRight)
    {
        print(m_menu.currentColourScheme);
        print(m_menu.spriteDictionary);
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
        Saving.SaveToFile(OutfitSettings.Saved, "OutfitSettings.json");
    }
}
