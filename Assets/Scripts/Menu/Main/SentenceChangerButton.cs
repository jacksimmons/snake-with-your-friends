using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SentenceChangerButton : MonoBehaviour
{
    [SerializeField]
    private SentenceBehaviour m_sentenceBehaviour;

    [SerializeField]
    private Transform[] m_onHoverDisabledWords;
    [SerializeField]
    private Transform[] m_onHoverEnabledWords;

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
        print("enter");
        m_sentenceBehaviour.SwapOutWords(m_onHoverDisabledWords, m_onHoverEnabledWords);
        m_sentenceBehaviour.UpdateSentence();
    }

    void OnPointerExit()
    {
        print("exit");
        m_sentenceBehaviour.SwapOutWords(m_onHoverEnabledWords, m_onHoverDisabledWords);
        m_sentenceBehaviour.UpdateSentence();
    }
}
