using UnityEngine;

public class FoodObject : GridObject
{
    [SerializeField]
    public EFoodType food;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject obj = collision.gameObject;
        Transform player = obj.transform.parent.parent;
        if (player != null && player.CompareTag("Player"))
        {
            GameBehaviour game = obj.transform.parent.parent.GetComponentInChildren<GameBehaviour>();
            PlayerMovementController playerMovementController = obj.transform.GetComponentInParent<PlayerMovementController>();

            // Not our collision to handle -> return.
            if (!playerMovementController.isOwned) return;

            if (playerMovementController != null)
            {
                playerMovementController.QAddBodyPart();
                playerMovementController.status.Eat(food);

                GameObject.FindWithTag("AudioHandler").GetComponent<AudioHandler>().eatAudioSource.Play();
            }

            game.CmdRemoveObjectFromGrid(gridPos);
        }
    }
}