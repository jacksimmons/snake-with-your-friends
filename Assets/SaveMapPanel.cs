using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class SaveMapPanel : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField m_input;
    [SerializeField]
    private EditorMenu m_menu;
    [SerializeField]
    private GameObject m_confirmPanel;

    public void OnOKPressed()
    {
        if (m_input.text == "")
            m_input.text = "Unnamed";

        if (File.Exists(Application.persistentDataPath + $"/Maps/{m_input.text}.map"))
        {
            m_confirmPanel.SetActive(true);
            m_confirmPanel.GetComponent<ConfirmOverwriteMapPanel>().Setup(m_input.text);
            gameObject.SetActive(false);
        }
        else
        {
            m_menu.SaveMapToFile(m_input.text);
            gameObject.SetActive(false);
        }
    }
}
