using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class PlayerHUDElement : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_nameLabel;
    [SerializeField]
    private TextMeshProUGUI m_numPartsLabel;
    [SerializeField]
    private Sprite m_deadSprite;


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
        transform.Find("NumPartsLabel").GetComponent<TextMeshProUGUI>().text = "Dead";
        transform.Find("Head").GetComponent<Image>().sprite = m_deadSprite;
        transform.Find("Torso").gameObject.SetActive(false);
        transform.Find("Tail").gameObject.SetActive(false);
    }
}