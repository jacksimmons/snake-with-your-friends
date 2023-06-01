using UnityEngine;

public interface IProjectile
{
    Vector2 Direction { get; }
    float Speed { get; }
}

public class Projectile : MonoBehaviour, IProjectile
{
    private Vector2 _direction;
    public Vector2 Direction { get { return _direction; } }

    private float _speed;
    public float Speed { get { return _speed; } }

    private bool _has_started = false;
    private Rigidbody2D _rb = null;

    public GameObject immune = null;

    // Must be called after this object has a rigidbody
    public void Create(float lifetime, Vector2 direction, Quaternion rotation, float speed, GameObject immune = null)
    {
        _direction = direction;
        _speed = speed;
        _has_started = true;

        transform.rotation = rotation;
        _rb = gameObject.GetComponent<Rigidbody2D>();

        this.immune = immune;

        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (_has_started)
        {
            _rb.MovePosition(_rb.position + (_direction * _speed));
        }
    }
}