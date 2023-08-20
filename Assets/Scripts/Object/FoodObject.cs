using UnityEngine;

public class FoodObject : GridObject
{
    [SerializeField]
    public EFoodType food;

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject obj = other.gameObject;
        Transform player = Player.TryGetPlayerTransformFromBodyPart(obj);

        bool removeAndReplaceFood = false;

        // Collision with player
        if (player != null)
        {
            PlayerMovementController playerMovementController = player.GetComponent<PlayerMovementController>();

            // Not our collision to handle -> return.
            if (!playerMovementController.isOwned) return;

            if (playerMovementController != null)
            {
                playerMovementController.QAddBodyPart();
                playerMovementController.status.Eat(food);

                GameObject.FindWithTag("AudioHandler").GetComponent<AudioHandler>().eatAudioSource.Play();
            }

            removeAndReplaceFood = true;
        }

        if (TryGetComponent(out ProjectileBehaviour pb))
        {
            switch (pb.type)
            {
                case EProjectileType.InstantDamage:
                    removeAndReplaceFood = true;
                    break;
            }
        }

        if (removeAndReplaceFood)
        {
            GameObject playerObj = GameObject.Find("LocalPlayerObject");
            GameBehaviour game = playerObj.GetComponentInChildren<GameBehaviour>();

            game.CmdRemoveAndReplaceFood(gridPos);
        }
    }
}