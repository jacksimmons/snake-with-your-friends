using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDElement : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_nameLabel;
    [SerializeField]
    private TextMeshProUGUI m_numPartsLabel;


    public void SetName(string name)
    {
        m_nameLabel.text = name;
    }

    public void SetNumParts(int numParts)
    {
        m_numPartsLabel.text = numParts.ToString();
    }

    public void AppearDead()
    {
        transform.Find("NumPartsLabel").GetComponent<TextMeshProUGUI>().text = "RIP";
        transform.Find("DeathIcon").gameObject.SetActive(true);
        transform.Find("Head").gameObject.SetActive(false);
        transform.Find("Torso").gameObject.SetActive(false);
        transform.Find("Tail").gameObject.SetActive(false);
    }
}