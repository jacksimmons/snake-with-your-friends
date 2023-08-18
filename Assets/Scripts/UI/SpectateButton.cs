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

    private Image m_image;
    private SpectateBehaviour m_sb;

    private void Start()
    {
        m_image = GetComponent<Image>();
        m_sb = GetComponentInParent<SpectateBehaviour>();
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
        if (m_isRight)
            m_sb.ChangeTarget(1);
        else
            m_sb.ChangeTarget(-1);
    }
}
