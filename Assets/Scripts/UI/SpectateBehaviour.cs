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

    public int spectateIndex;
    private bool m_bother = true;

    [SerializeField]
    private TextMeshProUGUI m_nameLabel;

    public void UpdateNameLabel(PlayerObjectController target)
    {
        m_nameLabel.text = string.Format($"Spectating: {target.playerName}");
    }

    // Changes target to spectateIndex + diff, unless that player is dead,
    // in which case the function is recursively called until it wraps back
    // to the original spectateIndex.
    // Once this happens, the script won't bother with spectating any longer
    // to not waste resources. (m_bother = true)
    public void ChangeTarget(int diff, int firstTryIndex = -1)
    {
        // If there no point in running this function
        if (!m_bother) return;

        spectateIndex = spectateIndex + diff;
        if (spectateIndex == firstTryIndex)
        {
            m_bother = false;
            return;
        }
        if (firstTryIndex == -1)
            firstTryIndex = spectateIndex;

        // Index wrapping
        if (spectateIndex >= Manager.Players.Count)
        {
            spectateIndex = 0;
        }
        else if (spectateIndex < 0)
        {
            spectateIndex = Manager.Players.Count - 1;
        }

        if (Manager.Players.Count == 0)
        {
            Debug.LogError("Somehow, no players to spectate!");
            m_nameLabel.text = "Noone to spectate (error)";
            m_bother = false;
            return;
        }

        PlayerObjectController target = Manager.Players[spectateIndex];
        if (target.GetComponent<PlayerMovementController>().dead)
        {
            ChangeTarget(diff, firstTryIndex);
        }

        CamBehaviour cam = GameObject.FindWithTag("MainCamera").GetComponent<CamBehaviour>();
        cam.Player = target.GetComponent<PlayerMovementController>();

        UpdateNameLabel(target);
    }
}
