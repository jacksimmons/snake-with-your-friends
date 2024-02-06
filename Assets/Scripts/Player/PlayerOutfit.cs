using System;
using UnityEngine;

public class PlayerOutfit : MonoBehaviour
{
    private PlayerMovement m_pm;


    private void Start()
    {
        m_pm = GetComponent<PlayerObjectController>().PM;
    }


    public static Sprite GetOutfitSprite(ECustomisationPart part, string filename)
    {
        return Resources.Load<Sprite>
            ($"Snake/{OutfitSettings.Saved.Data.ColourName}/{part}/{filename}");
    }


    public void UpdateOutfit(OutfitSettingsData settings)
    {
        foreach (ECustomisationPart part in Enum.GetValues(typeof(ECustomisationPart)))
        {
            UpdatePlayerSprite(part, GetOutfitSprite(part, settings.OutfitSpriteNames[(int)part]), false);
        }
    }


    public void UpdatePlayerSprite(ECustomisationPart part, Sprite sprite, bool saveToSettings)
    {
        if (saveToSettings)
            OutfitSettings.Saved.Data.OutfitSpriteNames[(int)part] = sprite.name;
        m_pm.DefaultSprites[(int)part] = sprite;
    }
}