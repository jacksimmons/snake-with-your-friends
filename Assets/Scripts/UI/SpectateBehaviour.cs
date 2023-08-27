using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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

    public int spectateIndex = 0;
    private bool m_bother = true;

    [SerializeField]
    private TextMeshProUGUI m_nameLabel;

    public void UpdateNameLabel(PlayerObjectController target)
    {
        if (target)
            m_nameLabel.text = string.Format($"Spectating: [{spectateIndex}] {target.playerName}");
        else
        {
            m_nameLabel.text = string.Format($"[Noone to spectate]");
            m_bother = false;
        }
    }

    // Gets the first spectate target that is defaulted to after you die (0th index)
    public void GetFirstTarget()
    {
        PlayerObjectController firstTarget = null;
        if (Manager.Players.Count == 0)
            m_bother = false;
        else
            firstTarget = Manager.Players[0];

        UpdateNameLabel(firstTarget);
    }

    // Changes spectate target (by +1 or -1, determined by diff)
    public void ChangeTarget(int diff, int firstTryIndex = -1)
    {
        int nextIndex = spectateIndex + diff;
        spectateIndex =
            nextIndex == Manager.AlivePlayers.Count
            ? (diff > 0 ? 0 : Manager.AlivePlayers.Count - 1)
            : nextIndex;

        // Check if the search has failed
        if (spectateIndex == firstTryIndex)
        {
            UpdateNameLabel(null);
            return;
        }
        if (firstTryIndex == -1)
            firstTryIndex = spectateIndex;

        PlayerObjectController poc = Manager.Players[spectateIndex];
        PlayerMovement pm = poc.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            ChangeTarget(diff, firstTryIndex);
            return;
        }

        SpectateTarget(pm);
    }

    private void SpectateTarget(PlayerMovement target)
    {
        CamBehaviour cam = GameObject.FindWithTag("MainCamera").GetComponent<CamBehaviour>();
        cam.Player = target;

        UpdateNameLabel(target.GetComponent<PlayerObjectController>());
    }
}
