using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ColliderToggleTilemap : MonoBehaviour
{
    /// <summary>
    /// When entered by the player, toggles a given Behaviour
    /// component on or off.
    /// </summary>

    [SerializeField]
    private Collider2D _toToggle;

    private bool _triggerComplete = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_triggerComplete)
        {
            if (!(_toToggle == null))
            {
                Transform parent = other.transform.parent;
                if (parent != null && parent.gameObject.CompareTag("Player"))
                {
                    if (parent.gameObject.CompareTag("Player"))
                    {
                        _toToggle.enabled = !_toToggle.enabled;
                    }
                }
                _triggerComplete = true;
            }

            else
            {
                Debug.LogWarning("No Behaviour was passed to " + name);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        _triggerComplete = false;
    }
}
