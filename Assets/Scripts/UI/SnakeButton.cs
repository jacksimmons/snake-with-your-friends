using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SnakeButton : MonoBehaviour
{
    [SerializeField]
    private Sprite m_buttonDefault;
    [SerializeField]
    private Sprite m_buttonHover;

    private Image m_image;


    private void Start()
    {
        m_image = GetComponent<Image>();
    }


    public void OnPointerEnter()
    {
        SetSprite(m_buttonHover);
    }


    public void OnPointerExit()
    {
        SetSprite(m_buttonDefault);
    }


    private void SetSprite(Sprite sprite)
    {
        m_image.sprite = sprite;

        // Resizes the sprite rect to fit the new sprite
        m_image.SetNativeSize();
    }
}
