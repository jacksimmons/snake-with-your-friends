using TMPro;
using UnityEngine;

public class PlayerHUDElement : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_nameLabel;
    [SerializeField]
    private TMP_Text m_numPartsLabel;


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
        m_numPartsLabel.text = "";
        transform.Find("DeathIcon").gameObject.SetActive(true);
        Transform box = transform.Find("SnakeBox");
        box.Find("Head").gameObject.SetActive(false);
        box.Find("Torso").gameObject.SetActive(false);
        box.Find("Tail").gameObject.SetActive(false);
    }
}