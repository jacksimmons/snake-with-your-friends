using UnityEngine;

// Fontifies a series of sprite letters from a string.
// Supports scaling, spacing, colouring
// Supports null terminating ("-" to create an empty word)
public class WordBehaviour : MonoBehaviour
{
    public const float LETTER_SPACING = 0.05f;

    [SerializeField]
    private string m_word;
    [SerializeField]
    private Color m_modulate = Color.white;

    [SerializeField]
    private GameObject m_letterTemplate;

    // Initialisation must be before SentenceBehaviour's initialisation (in Start)
    void Awake()
    {
        UpdateWord(m_word, m_modulate, false);
    }

    public void UpdateWord(string word, Color colour, bool clearWord = true)
    {
        if (clearWord)
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        // Fontifies the word object (each letter is x = LETTER_SPACING after the previous)

        Vector3 prevLocalPos = Vector3.zero;
        float prevSpriteWidth = 0;

        for (int i = 0; i < word.Length; i++)
        {
            char c = word[i];

            // Empty word
            if (c == '-') return;

            char newChar = char.ToUpper(c);
            GameObject newLetter = Instantiate(m_letterTemplate, transform);

            SpriteRenderer ren = newLetter.GetComponent<SpriteRenderer>();
            ren.sprite = Resources.Load<Sprite>("Font/" + newChar); // Extensions omitted
            ren.color = colour;

            float spriteWidth = ren.sprite.bounds.size.x;
            float spacing = 0;
            if (i > 0)
                spacing = LETTER_SPACING + prevSpriteWidth / 2 + spriteWidth / 2;

            newLetter.transform.localPosition = prevLocalPos + spacing * Vector3.right;

            prevLocalPos = newLetter.transform.localPosition;
            prevSpriteWidth = spriteWidth;
        }
    }
}
