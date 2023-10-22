using UnityEngine;


public enum EFoodType
{
    None, // 0

    Apple,
    Balti,
    Banana,
    Bone,
    Booze,
    Cheese,
    Coffee,
    Doughnut,
    Dragonfruit,
    Drumstick,
    IceCream,
    Orange,
    Pineapple,
    PineapplePizza,
    Pizza, // 15

    // !Final index must be less than 32 (for FoodSettings bitfield)
}


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
            PlayerMovement playerMovementController = player.GetComponent<PlayerMovement>();

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

        // Collision with fireball, etc.
        if (other.TryGetComponent(out ProjectileBehaviour pb))
        {
            switch (pb.type)
            {
                case EProjectileType.Fireball:
                    removeAndReplaceFood = true;
                    break;
            }
        }

        if (removeAndReplaceFood)
        {
            GameObject playerObj = GameObject.Find("LocalPlayerObject");
            GameBehaviour game = playerObj.GetComponentInChildren<GameBehaviour>();

            game.CmdRemoveFood(gridPos);
        }
    }
}