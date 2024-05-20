using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;

public class PlayerHUDElement : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_nameLabel;
    [SerializeField]
    private TMP_Text m_numPartsLabel;

    [SerializeField]
    private Transform m_statusEffectBoxParent;
    [SerializeField]
    private GameObject m_statusEffectBoxTemplate;

    private List<Sprite> m_statusEffectSprites;
    private List<GameObject> m_statusEffectBoxes;

    // Status effects with custom status images for each level.
    // Indices are shared between the three arrays.
    [SerializeField]
    private EEffect[] m_customEffects;
    [SerializeField]
    private Sprite[] m_customEffectSprites;
    [SerializeField]
    private int[] m_customEffectLevels;


    private void Start()
    {
        m_statusEffectSprites = new();
        m_statusEffectBoxes = new();
    }


    public void SetName(string name)
    {
        m_nameLabel.text = name;
    }


    public void SetNumParts(int numParts)
    {
        m_numPartsLabel.text = numParts.ToString();
    }


    public void AppearDead()
    {
        m_numPartsLabel.text = "";
        transform.Find("DeathIcon").gameObject.SetActive(true);
        Transform box = transform.Find("SnakeBox");
        box.Find("Head").gameObject.SetActive(false);
        box.Find("Torso").gameObject.SetActive(false);
        box.Find("Tail").gameObject.SetActive(false);
    }


    public void AddStatusEffect(Effect effect)
    {
        if (m_customEffects.Contains(effect.EffectName))
        {
            // Match the effect name, level and image before adding.
            for (int j = 0; j < m_customEffects.Length; j++)
            {
                if (m_customEffects[j] == effect.EffectName)
                {
                    // Remove all other level images; add the current level image.
                    if (m_customEffectLevels[j] == effect.EffectLevel)
                    {
                        AddStatusEffectSprite(m_customEffectSprites[j]);
                    }
                    else
                    {
                        RemoveStatusEffectSprite(m_customEffectSprites[j]);
                    }
                }
            }
        }
    }


    /// <summary>
    /// Adds a status effect to the list of boxes/images. If the status effect is already
    /// present, adds it and makes it invisible.
    /// </summary>
    /// <param name="sprite">The sprite to add.</param>
    public void AddStatusEffectSprite(Sprite sprite)
    {
        // Instantiate the box template.
        GameObject go = Instantiate(m_statusEffectBoxTemplate, m_statusEffectBoxParent);

        // If we already have this sprite, disable the new one.
        if (m_statusEffectSprites.Contains(sprite))
        {
            go.SetActive(false);
        }

        Image im = go.transform.GetChild(0).GetComponent<Image>();
        im.sprite = sprite;
        im.SetNativeSize();

        // Add the sprite and GameObject pair.
        m_statusEffectBoxes.Add(go);
        m_statusEffectSprites.Add(sprite);
    }


    /// <summary>
    /// Removes a status effect to the list of boxes/images. If the status effect exists
    /// more than once, makes the remaining one visible.
    /// </summary>
    /// <param name="sprite">The sprite to remove.</param>
    public void RemoveStatusEffectSprite(Sprite sprite)
    {
        // Destroy the box and remove it from the pairs.
        int spriteIndex = m_statusEffectSprites.IndexOf(sprite);
        if (spriteIndex == -1) return;

        Destroy(m_statusEffectBoxes[spriteIndex]);
        m_statusEffectBoxes.RemoveAt(spriteIndex);
        m_statusEffectSprites.RemoveAt(spriteIndex);

        // If we still have another instance of this image, enable it.
        int otherIndex = m_statusEffectSprites.IndexOf(sprite);
        if (otherIndex != -1) m_statusEffectBoxes[otherIndex].SetActive(true);
    }
}