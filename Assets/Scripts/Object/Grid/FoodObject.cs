using UnityEngine;


public class FoodObject : MonoBehaviour
{
    [SerializeField]
    public EFoodType food;


    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject obj = other.gameObject;
        Transform player = PlayerStatic.TryGetPlayerTransformFromBodyPart(obj);

        bool removeAndReplaceFood = false;

        // Collision with player
        if (player != null)
        {
            PlayerMovement pm = player.GetComponent<PlayerMovement>();

            // Not our collision to handle -> return.
            if (!pm.isOwned) return;

            pm.QAddBodyPart();
            pm.GetComponent<PlayerStatus>().Eat(food, GetComponent<SpriteRenderer>().sprite);
            GameObject.FindWithTag("AudioHandler").GetComponent<AudioHandler>().eatAudioSource.Play();

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
            game.CmdRemoveFood(gameObject);
        }
    }
}