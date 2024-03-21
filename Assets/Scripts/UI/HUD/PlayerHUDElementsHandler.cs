using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDElementsHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject m_playerElementTemplate;
    private Dictionary<ulong, PlayerHUDElement> m_mapSteamIDToHUDElement = new();


    public void LoadHUD()
    {
        foreach (PlayerObjectController poc in CustomNetworkManager.Instance.Players)
        {
            GameObject newElement = Instantiate(m_playerElementTemplate, transform);
            PlayerHUDElement item = newElement.GetComponent<PlayerHUDElement>();

            item.SetName(poc.playerName);
            StartCoroutine(
                Wait.WaitForConditionThen(
                    () => poc.PM.enabled,
                    0.1f,
                    () =>
                    {
                        item.SetNumParts(poc.PM.BodyParts.Count);
                        Transform box = item.transform.Find("SnakeBox");
                        box.Find("Head").GetComponent<Image>().sprite = poc.PM.DefaultSprites[0];
                        box.Find("Torso").GetComponent<Image>().sprite = poc.PM.DefaultSprites[1];
                        box.Find("Tail").GetComponent<Image>().sprite = poc.PM.DefaultSprites[2];

                        m_mapSteamIDToHUDElement[poc.playerSteamID] = item;
                    }
                )
            );
        }
    }


    /// <summary>
    /// Returns the corresponding HUD element, if it exists, or null.
    /// </summary>
    public PlayerHUDElement GetHUDElementOrNull(ulong steamID)
    {
        if (!m_mapSteamIDToHUDElement.ContainsKey(steamID)) return null;
        return m_mapSteamIDToHUDElement[steamID];
    }
}
