using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDElementsHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject m_playerElementTemplate;


    private void Start()
    {
        foreach (PlayerObjectController poc in CustomNetworkManager.Instance.Players)
        {
            GameObject newElement = Instantiate(m_playerElementTemplate, transform);
            PlayerHUDElement item = newElement.GetComponent<PlayerHUDElement>();

            poc.PlayerOnHUD = item;

            PlayerMovement pm = poc.GetComponent<PlayerMovement>();
            item.SetName(poc.playerName);
            StartCoroutine(
                Wait.WaitForObjectThen<PlayerMovement>(
                    () => poc.GetComponent<PlayerMovement>(),
                    new WaitForSeconds(0.1f),
                    (PlayerMovement pm) =>
                    {
                        item.SetNumParts(pm.BodyParts.Count);
                        item.transform.Find("Tail").GetComponent<Image>().sprite = pm.m_bpTail;
                        item.transform.Find("Torso").GetComponent<Image>().sprite = pm.m_bpTorso;
                        item.transform.Find("Head").GetComponent<Image>().sprite = pm.m_bpHead;
                    }
                )
            );
        }
    }
}