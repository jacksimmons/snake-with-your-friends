using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveMapPanel : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField m_input;
    [SerializeField]
    private EditorMenu m_menu;
    [SerializeField]
    private Button m_okButton;
    [SerializeField]
    private MainMenuButton m_mmButton;
    [SerializeField]
    private GameObject m_confirmPanel;


    public void LoadPanel(bool quitAfter)
    {
        m_okButton.onClick.RemoveAllListeners();
        m_okButton.onClick.AddListener(() => OnOKPressed(quitAfter));
    }


    public void OnOKPressed(bool quitAfter)
    {
        if (m_input.text == "")
            m_input.text = "Unnamed";

        if (File.Exists(Application.persistentDataPath + $"/Maps/{m_input.text}.map.json"))
        {
            m_confirmPanel.SetActive(true);
            m_confirmPanel.GetComponent<ConfirmOverwriteMapPanel>().LoadPanel(quitAfter, m_input.text);
        }
        else
        {
            m_menu.SaveMapToFile(m_input.text);
            if (quitAfter)
                m_mmButton.OnMainMenuButtonPressed();
        }

        gameObject.SetActive(false);
    }
}
