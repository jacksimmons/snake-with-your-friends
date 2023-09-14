using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutfitComponentHandler : MonoBehaviour
{
    private int m_currentIndex;

    [SerializeField]
    private CustomisationMenu.OutfitComponent m_outfitComponent;
    private CustomisationMenu m_menu;
    private Sprite m_outfitComponentSprite;
    [SerializeField]
    private GameObject[] m_previewObjectsToUpdate;

    private void Start()
    {
        m_menu = GameObject.FindWithTag("CustomisationMenu").GetComponent<CustomisationMenu>();
        m_outfitComponentSprite = transform.Find("Part").GetComponent<Image>().sprite;

        // Get current index of the sprite in the appropriate folder in sprite dictionary.
        m_currentIndex = Array.IndexOf(
            m_menu.spriteDictionary[m_menu.currentColourScheme][m_outfitComponent],
            m_outfitComponentSprite);
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

        transform.Find("Part").GetComponent<Image>().sprite = sprites[m_currentIndex];

        foreach (GameObject go in m_previewObjectsToUpdate)
        {
            go.GetComponent<Image>().sprite = sprites[m_currentIndex];
        }

        PlayerMovement pm = 
            GameObject.Find("LocalPlayerObject").GetComponent<PlayerMovement>();

        if (m_outfitComponent == CustomisationMenu.OutfitComponent.Head)
        {
            pm.m_bpHead = sprites[m_currentIndex];
            PlayerPrefs.SetString("Head", pm.m_bpHead.name);
            PlayerPrefs.Save();
            print(pm.m_bpHead.name);
        }
        else if (m_outfitComponent == CustomisationMenu.OutfitComponent.Torso)
            pm.m_bpTorso = sprites[m_currentIndex];
        else if (m_outfitComponent == CustomisationMenu.OutfitComponent.Tail)
            pm.m_bpTail = sprites[m_currentIndex];
        else if (m_outfitComponent == CustomisationMenu.OutfitComponent.Corner)
            pm.m_bpCornerL = sprites[m_currentIndex];
    }
}
