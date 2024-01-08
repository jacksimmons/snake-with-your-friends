using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmOverwriteMapPanel : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI titleLabel;
    [SerializeField]
    private Button confirmButton;
    [SerializeField]
    private EditorMenu menu;

    public void Setup(string mapName)
    {
        titleLabel.text = $"Are you sure? ({mapName})";
        confirmButton.interactable = false;

        confirmButton.onClick.AddListener(() => { 
            menu.SaveMapToFile(mapName);
            gameObject.SetActive(false);
        });

        StartCoroutine(Wait.WaitThen(3f, () => confirmButton.interactable = true));
    }
}
