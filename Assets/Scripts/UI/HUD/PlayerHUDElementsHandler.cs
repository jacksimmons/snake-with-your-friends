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
                Wait.WaitForObjectThen(
                    () => poc.PM,
                    0.1f,
                    (PlayerMovement pm) =>
                    {
                        item.SetNumParts(pm.BodyParts.Count);
                        item.transform.Find("Tail").GetComponent<Image>().sprite = pm.m_bpTail;
                        item.transform.Find("Torso").GetComponent<Image>().sprite = pm.m_bpTorso;
                        item.transform.Find("Head").GetComponent<Image>().sprite = pm.m_bpHead;
                    }
                )
            );

            m_mapSteamIDToHUDElement[poc.playerSteamID] = item;
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
