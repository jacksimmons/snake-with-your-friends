using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayers : MonoBehaviour
{
    [SerializeField]
    private GameObject m_gamePlayerTemplate;

    void Awake()
    {
        if (!GameObject.Find("LocalGamePlayer"))
        {
            GameObject go = Instantiate(m_gamePlayerTemplate);
            go.name = "LocalGamePlayer";
        }
    }
}
