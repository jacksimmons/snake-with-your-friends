using TMPro;
using UnityEngine;

public class MapEditorPopupPanel : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_message;


    public void Setup(string text)
    {
        m_message.text = text;
    }


    public void OnOKPressed()
    {
        gameObject.SetActive(false);
    }
}
