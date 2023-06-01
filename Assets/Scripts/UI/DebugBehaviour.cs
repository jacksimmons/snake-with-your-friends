using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugBehaviour : MonoBehaviour
{
    [SerializeField]
    private Lobby _lobby;

    [SerializeField]
    private PlayerBehaviour _player;

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
        try
        {
            _lobby = GameObject.FindWithTag("Lobby").GetComponent<Lobby>();
        }
        catch { }

        try
        {
            _player = _lobby.Player;
        }
        catch
        {
            StartCoroutine(WaitForPlayer());
        }
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
                if (_player)
                {
                    Dictionary<string, string> stringStatuses = _player.status.GetStatusDebug();
                    UpdateDisplay(stringStatuses);
                }
                break;
            case e_Display.Player:
                if (_player)
                {
                    Dictionary<string, string> stringPlayerValues = _player.GetPlayerDebug();
                    UpdateDisplay(stringPlayerValues);
                }
                break;
            case e_Display.Lobby:
                if (_lobby)
                {
                    Dictionary<string, string> stringLobbyValues = _lobby.GetLobbyDebug();
                    UpdateDisplay(stringLobbyValues);
                }
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

    private IEnumerator WaitForPlayer()
    {
        GameObject player = null;
        while (player == null)
        {
            player = GameObject.FindWithTag("Player");
            yield return new WaitForSeconds(1);
        }
        _player = player.GetComponent<PlayerBehaviour>();
        yield break;
    }
}
