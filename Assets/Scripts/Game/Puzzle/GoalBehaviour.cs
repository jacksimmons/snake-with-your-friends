    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalBehaviour : ObjectBehaviour
{
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // ! Note: goals can be destroyed by projectiles (is this intended?)
        base.OnTriggerEnter2D(other);

        Transform player = PlayerStatic.TryGetOwnedPlayerTransformFromBodyPart(other.gameObject);
        if (player == null) return;


        // Prevents infinite loading of puzzles below
        GetComponent<Collider2D>().enabled = false;

        GameBehaviour.Instance.OnPuzzleComplete();
    }
}
