using UnityEngine;

public interface IProjectile
{
    Vector2 Start { get; }
    Vector2 Direction { get; }
    float Speed { get; }
}

public class Projectile : MonoBehaviour, IProjectile
{
    private Vector2 _start;
    public Vector2 Start { get { return _start; } }

    private Vector2 _direction;
    public Vector2 Direction { get { return _direction; } }

    private float _speed;
    public float Speed { get { return _speed; } }

    private bool _has_started = false;
    private Rigidbody2D _rb = null;

    // Must be called after this object has a rigidbody
    public void Create(float lifetime, Vector2 start, Vector2 direction, Quaternion rotation, float speed)
    {
        _start = start;
        _direction = direction;
        _speed = speed;
        _has_started = true;

        _rb = GetComponent<Rigidbody2D>();
        _rb.position = start;
        transform.rotation = rotation;

        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (_has_started)
        {
            _rb.MovePosition(_rb.position + (_direction * _speed));
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Destroy(gameObject);
    }
}