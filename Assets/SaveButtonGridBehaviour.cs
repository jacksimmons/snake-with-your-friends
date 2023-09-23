using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveButtonGridBehaviour : MonoBehaviour
{
    [SerializeField]
    private Button backButton;
    [SerializeField]
    private Button saveButton;
    [SerializeField]
    private Button noSaveButton;


    // Toggles which buttons are visible (Back, or the two save buttons)
    private void ToggleExitButtons(bool toggle)
    {
        saveButton.gameObject.SetActive(toggle);
        noSaveButton.gameObject.SetActive(toggle);
        backButton.gameObject.SetActive(!toggle);
    }


    public void OnBackPressed()
    {
        ToggleExitButtons(true);
        StartCoroutine(
            Wait.WaitThen(1,
            () =>
            {
                noSaveButton.interactable = true;
                StartCoroutine(
                    Wait.WaitThen(3,
                    () =>
                    {
                        ToggleExitButtons(false);
                        noSaveButton.interactable = false;
                    })
                );
            })
        );
    }
}
