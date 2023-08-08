using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    [SerializeField]
    private EProjectileType m_type;
    private Projectile _proj;
    public Projectile Proj
    {
        get
        {
            return _proj;
        }
        set
        {
            // A mini ready-function.

            _rb = gameObject.GetComponent<Rigidbody2D>();
            _proj = value;
            _ready = true;
            transform.rotation = _proj.Rotation;
            Destroy(gameObject, _proj.Lifetime);
        }
    }
    private bool _ready = false;
    private Rigidbody2D _rb = null;

    private void FixedUpdate()
    {
        if (_ready)
        {
            // v = d / t, remember a distance of 1 is moved every counter completion.
            float speed = 1f / _proj.CounterMax;
            _rb.MovePosition(_rb.position + _proj.Direction * speed);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject obj = collision.gameObject;
        if (obj == _proj.immune)
            return;

        Transform player = obj.transform.parent.parent;
        if (player != null && player.CompareTag("Player"))
        {
            if (m_type == EProjectileType.Shit)
            {
                GameObject.FindWithTag("Foreground").GetComponent<ForegroundBehaviour>().AddToForeground(GetComponent<SpriteRenderer>().sprite);
                Destroy(gameObject);
            }
        }
    }
}