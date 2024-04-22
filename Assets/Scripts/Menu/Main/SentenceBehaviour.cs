using System;
using System.Linq;
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

    public static string[] originalWords = { "Snake", "With", "Your", "Friends" };

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

    public void SwapOutWords(string[] newWords, Color firstWordColour)
    {
        if (newWords.Length != transform.childCount)
        {
            Debug.LogError("Incorrect newWords length.");
            return;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            if (newWords[i] == "")
                continue;
            Transform child = transform.GetChild(i);

            Color colour;
            if (i == 0)
                colour = firstWordColour;
            else
                colour = Color.white;
            child.GetComponent<WordBehaviour>().UpdateWord(newWords[i], colour);
        }
    }
}
