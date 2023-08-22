using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Fontifies a series of sprite letters.
// Supports scaling, spacing
public class WordBehaviour : MonoBehaviour
{
    public const float LETTER_SPACING = 0.05f;

    [SerializeField]
    private Color modulate = Color.white;

    // Start is called before the first frame update
    void Start()
    {
        // Fontifies the word object (each letter is x = LETTER_SPACING after the previous)
        Vector3 prevLocalPos = Vector3.zero;
        float prevSpriteWidth = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
            spriteRenderer.color = modulate;

            Sprite childSprite = spriteRenderer.sprite;

            float spriteWidth = childSprite.bounds.size.x;
            float spacing = 0;
            if (i > 0)
                spacing = LETTER_SPACING + prevSpriteWidth / 2 + spriteWidth / 2;

            child.localPosition = prevLocalPos + spacing * Vector3.right;

            prevLocalPos = child.localPosition;
            prevSpriteWidth = spriteWidth;
        }
    }
}
