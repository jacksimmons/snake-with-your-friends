using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpectateBehaviour : MonoBehaviour
{
    private CustomNetworkManager _manager;
    public CustomNetworkManager Manager
    {
        get
        {
            if (_manager != null) { return _manager; }
            return _manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }

    private PlayerObjectController m_currentTarget;
    public int spectateIndex;

    [SerializeField]
    private TextMeshProUGUI m_nameLabel;

    public void UpdateNameLabel()
    {
        m_nameLabel.text = string.Format($"Spectating: {m_currentTarget.playerName}");
    }

    public void ChangeTarget(int diff)
    {
        spectateIndex = spectateIndex + diff;

        // Index wrapping
        if (spectateIndex >= Manager.Players.Count)
        {
            spectateIndex = 0;
        }
        else if (spectateIndex < 0)
        {
            spectateIndex = Manager.Players.Count - 1;
        }

        print(spectateIndex);

        CamBehaviour cam = GameObject.FindWithTag("MainCamera").GetComponent<CamBehaviour>();
        m_currentTarget = Manager.Players[spectateIndex];
        cam.Player = m_currentTarget.GetComponent<PlayerMovementController>();
    }
}
