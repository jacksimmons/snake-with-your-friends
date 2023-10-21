using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalBehaviour : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GameBehaviour.Instance != null)
        {
            // Prevents infinite loading of puzzles below
            GetComponent<Collider2D>().enabled = false;

            // Convert prefab level name "PuzzleX" to X
            //byte level = byte.Parse(transform.parent.name["Puzzle".Length..]);

            // Finished all puzzles, so early exit
            if (SaveData.Saved.PuzzleLevel == SaveData.MaxPuzzleLevel) return;

            // Increment highest puzzle
            SaveData.Saved.PuzzleLevel++;
            Saving.SaveToFile(SaveData.Saved, "SaveData.dat");

            // Load the next puzzle
            GameBehaviour.Instance.OnGameSceneLoaded();
            Destroy(transform.parent.gameObject);
            return;
        }
        else
        {
            Debug.LogError("GameBehaviour was null!");
        }

        // --- Everything else disappears
        Destroy(collision.gameObject);
    }
}
