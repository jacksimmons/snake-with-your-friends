using TMPro;
using UnityEngine;

public class SpectateNameLabel : MonoBehaviour
{
    [SerializeField]
    private CamBehaviour m_cam;
    [SerializeField]
    private TextMeshProUGUI m_textMeshPro;

    public void UpdateName()
    {
        if (m_cam.Player == null) return;
        PlayerObjectController poc = m_cam.Player.GetComponent<PlayerObjectController>();

        m_textMeshPro.text = string.Format($"Spectating [{poc.playerNo}] {poc.playerName}");
    }
}
