using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugLabels : MonoBehaviour
{
    private EFoodType foodType = EFoodType.Apple;

    [SerializeField]
    private TextMeshProUGUI cycledPowerupDisplay;

    private void Start()
    {
        UpdatePowerupDisplay();
    }

    public void Debug_AddBodyPart()
    {
        GameObject localPlayerObj = GameObject.Find("LocalPlayerObject");
        if (localPlayerObj == null) return;
        localPlayerObj.GetComponent<PlayerMovement>().QAddBodyPart();
    }

    public void Debug_RemoveBodyPart()
    {
        GameObject localPlayerObj = GameObject.Find("LocalPlayerObject");
        if (localPlayerObj == null) return;
        localPlayerObj.GetComponent<PlayerMovement>().QRemoveBodyPart();
    }

    public void CyclePowerup(bool right)
    {
        if (right) foodType = foodType.Next();
        else foodType = foodType.Prev();
        UpdatePowerupDisplay();
    }

    public void UpdatePowerupDisplay()
    {
        cycledPowerupDisplay.text = "Eat: " + foodType.ToString();
    }

    public void Debug_Eat()
    {
        GameObject localPlayerObj = GameObject.Find("LocalPlayerObject");
        if (localPlayerObj == null) return;
        
        PlayerStatus playerStatus = localPlayerObj.GetComponentInChildren<PlayerStatus>();
        playerStatus.Eat(foodType);
    }
}
