using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SentenceChangerButton : MonoBehaviour
{
    [SerializeField]
    private SentenceBehaviour m_sentenceBehaviour;

    // Changes from the original words.
    // "" = No change
    // "-" = Disable word
    [SerializeField]
    private string[] m_wordChanges;
    [SerializeField]
    private Color m_firstWordColour;

    // Start is called before the first frame update
    void Start()
    {
        EventTrigger eventTrigger = gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry();
        pointerEnterEntry.eventID = EventTriggerType.PointerEnter;
        pointerEnterEntry.callback.AddListener((data) => OnPointerEnter());
        eventTrigger.triggers.Add(pointerEnterEntry);

        EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry();
        pointerExitEntry.eventID = EventTriggerType.PointerExit;
        pointerExitEntry.callback.AddListener((data) => OnPointerExit());
        eventTrigger.triggers.Add(pointerExitEntry);
    }

    void OnPointerEnter()
    {
        m_sentenceBehaviour.SwapOutWords(m_wordChanges, m_firstWordColour);
        m_sentenceBehaviour.UpdateSentence();
    }

    void OnPointerExit()
    {
        m_sentenceBehaviour.SwapOutWords(SentenceBehaviour.originalWords, Color.white);
        m_sentenceBehaviour.UpdateSentence();
    }
}
