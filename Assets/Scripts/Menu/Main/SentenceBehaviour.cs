using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;

// Turns a series of WordBehaviours into a sentence/logo.
// Aligns the words in a staircase manner.
public class SentenceBehaviour : MonoBehaviour
{
    private const float LETTER_SPACINGS_BETWEEN_WORDS = 4;
    private const float LINE_VERTICAL_SPACING = 0.75f;

    // Spacing added to start of every line is Line No * this
    private const float LINE_STAIR_SPACING = 0.5f;

    [SerializeField]
    private Transform[] m_lineStarters;
    [SerializeField]
    private Transform[] m_disabledWords;

    private void Start()
    {
        UpdateSentence();
    }

    public void UpdateSentence()
    {
        int lineCount = 0;
        Transform prevChild = null;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            if (m_disabledWords.Contains(child)) continue;

            if (m_lineStarters.Contains(child))
            {
                // The first word in a line needs to be spaced staircase-ly (vert & horiz)

                child.localPosition = (LINE_VERTICAL_SPACING * lineCount * Vector3.down)
                    + (LINE_STAIR_SPACING * lineCount * Vector3.right);
                lineCount++;
            }
            else
            {
                // Consecutive words along a line (after the first) need to be spaced accordingly

                if (prevChild == null)
                {
                    Debug.LogError("First element MUST be a line starter!");
                    return;
                }

                Transform prevLetter = prevChild.GetChild(prevChild.childCount - 1);

                Vector3 prevLetterWorldPos = prevLetter.position;
                float prevLetterSpriteWidth = prevLetter.GetComponent<SpriteRenderer>()
                    .sprite.bounds.size.x;

                Transform nextLetter = child.GetChild(0);
                float nextLetterSpriteWidth = nextLetter.GetComponent<SpriteRenderer>()
                    .sprite.bounds.size.x;

                float spacing =
                    LETTER_SPACINGS_BETWEEN_WORDS * WordBehaviour.LETTER_SPACING
                    + prevLetterSpriteWidth / 2 + nextLetterSpriteWidth / 2;

                child.position = prevLetterWorldPos
                    + (spacing * Vector3.right);
            }

            prevChild = child;
        }
    }

    // Swaps out a word in the sentence for a word which is not in the sentence.
    // Note that words must be ordered in ascending sibling index for the sentence to work properly.
    public void SwapOutWord(Transform originalWord, Transform newWord)
    {
        int newIndex = Array.IndexOf(m_disabledWords, newWord);
        m_disabledWords[newIndex] = originalWord;
        
        originalWord.gameObject.SetActive(false);
        newWord.gameObject.SetActive(true);
    }

    public void SwapOutWords(Transform[] originalWords, Transform[] newWords)
    {
        if (originalWords.Length != newWords.Length)
        {
            Debug.LogError("This function only works with two equal-sized arrays!");
            return;
        }
        for (int i = 0; i < originalWords.Length; i++)
        {
            SwapOutWord(originalWords[i], newWords[i]);
        }
    }
}
