using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpectateButton : MonoBehaviour
{
    [SerializeField]
    private bool m_isRight;

    [SerializeField]
    private Sprite m_buttonDefault;
    [SerializeField]
    private Sprite m_buttonHover;

    [SerializeField]
    private SpectateNameLabel m_nameLabel;

    private Image m_image;

    private void Start()
    {
        m_image = GetComponent<Image>();
    }

    public void OnPointerEnter()
    {
        m_image.sprite = m_buttonHover;
    }

    public void OnPointerExit()
    {
        m_image.sprite = m_buttonDefault;
    }

    public void OnPointerClick()
    {
        m_nameLabel.UpdateName();
    }
}
