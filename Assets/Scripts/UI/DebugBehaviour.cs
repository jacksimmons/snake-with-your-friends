using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugBehaviour : MonoBehaviour
{
    private PlayerMovement _player;

    [SerializeField]
    private GameObject _contentOutput;
    [SerializeField]
    private GameObject _statusBlock;
    [SerializeField]
    private GameObject _debugTextField;

    private enum e_Display
    {
        None = 0,
        Status = 1,
        Player = 2,
        Lobby = 3
    }
    private e_Display Display { get; set; }


    private void Start()
    {
        _player = GameObject.Find("LocalPlayerObject").GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKey(KeyCode.S))
                Display = e_Display.Status;
            else if (Input.GetKey(KeyCode.P))
                Display = e_Display.Player;
            else if (Input.GetKey(KeyCode.L))
                Display = e_Display.Lobby;
        }

        switch (Display)
        {
            case e_Display.Status:
                Dictionary<string, string> stringStatuses = _player.GetComponent<PlayerStatus>().GetStatusDebug();
                UpdateDisplay(stringStatuses);
                break;
            case e_Display.Player:
                Dictionary<string, string> stringPlayerValues = _player.GetPlayerDebug();
                UpdateDisplay(stringPlayerValues);
                break;
            default:
                break;
        }
    }

    private void UpdateDisplay(Dictionary<string, string> dict)
    {
        ClearDisplay();
        foreach (var key in dict.Keys)
        {
            GameObject tf = Instantiate(_debugTextField, _statusBlock.transform);
            TextMeshProUGUI tmp = tf.GetComponent<TextMeshProUGUI>();
            tmp.text += key;
            if (dict[key] == "False")
                tmp.color = Color.red;
            else if (dict[key] == "True")
                tmp.color = Color.green;
            else
                tmp.text += ": " + dict[key];
            tmp.text += "\n";
        }
    }

    /// <summary>
    /// Clears the StatusBlock UI display.
    /// </summary>
    private void ClearDisplay()
    {
        foreach (Transform child in _statusBlock.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
