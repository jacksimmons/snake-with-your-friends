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
    private bool m_allPlayersDead = false;

    [SerializeField]
    private TextMeshProUGUI m_nameLabel;

    public void UpdateNameLabel(PlayerObjectController target)
    {
        if (target)
            m_nameLabel.text = string.Format($"Spectating: [{spectateIndex}] {target.playerName}");
        else
        {
            m_nameLabel.text = string.Format($"[Noone to spectate]");
            m_allPlayersDead = true;
        }
    }

    // Gets the first spectate target that is defaulted to after you die (0th index)
    public void GetFirstTarget()
    {
        PlayerObjectController firstTarget = null;
        if (Manager.AlivePlayers.Count == 0)
            m_allPlayersDead = true;
        else
            firstTarget = Manager.AlivePlayers[0];

        UpdateNameLabel(firstTarget);
    }

    // Changes spectate target (by +1 or -1, determined by diff)
    public void ChangeTarget(int diff)
    {
        if (m_allPlayersDead) return;

        if (Manager.AlivePlayers.Count == 0)
        {
            m_allPlayersDead = true;
            UpdateNameLabel(null);
            return;
        }

        int nextIndex = spectateIndex + diff;
        spectateIndex =
            nextIndex >= Manager.AlivePlayers.Count ? 0
            : 
            nextIndex < 0 ? Manager.AlivePlayers.Count - 1
            :
            nextIndex;

        SpectateTarget();
    }

    private void SpectateTarget()
    {
        CamBehaviour cam = GameObject.FindWithTag("MainCamera").GetComponent<CamBehaviour>();
        PlayerObjectController poc = Manager.AlivePlayers[spectateIndex];
        cam.Player = poc.GetComponent<PlayerMovement>();

        UpdateNameLabel(poc);
    }
}
