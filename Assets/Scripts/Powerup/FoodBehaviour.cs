using UnityEngine;

public class FoodBehaviour : MonoBehaviour
{
    [SerializeField]
    private e_Food food;
    public int gridPos;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject obj = collision.gameObject;
        if (obj.transform.parent != null && obj.transform.parent.CompareTag("Player"))
        {
            GameBehaviour game = GameObject.FindWithTag("GameHandler").GetComponent<GameBehaviour>();
            PlayerBehaviour player = obj.transform.GetComponentInParent<PlayerBehaviour>();
            if (player != null)
            {
                player.QAddBodyPart();
                player.status.Eat(food);

                GameObject.FindWithTag("AudioHandler").GetComponent<AudioHandler>().eatAudioSource.Play();
            }

            game.RemoveFromGrid(gridPos);
            game.GenerateFood();
            Destroy(gameObject);
        }
    }
}
