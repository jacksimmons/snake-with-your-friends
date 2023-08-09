using System.Collections;
using UnityEngine;

public enum EProjectileCollisionType
{
    Bounce,
    Splat
}

public class ProjectileBehaviour : MonoBehaviour
{
    private ParticleSystem m_explosionEffect;

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

    [Range(0, 1)]
    // Bounciness - amount of speed retained after bounce
    private float m_restitution = 0.5f;
    private float m_speedMod = 1f;

    private bool m_playerImmune = false;

    public EProjectileType GetProjectileType()
    {
        return m_type;
    }

    private void Start()
    {
        m_explosionEffect = GetComponent<ParticleSystem>();
        m_explosionEffect.Stop();

        StartCoroutine(HandleImmunity(Proj.immunityDuration));
    }

    private void FixedUpdate()
    {
        if (_ready)
        {
            // v = d / t, remember a distance of 1 is moved every counter completion.
            float speed = 1f / _proj.CounterMax;
            _rb.MovePosition(_rb.position + _proj.Direction * speed * m_speedMod);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject obj = collision.gameObject;
        Transform player = obj.transform.parent.parent;
        if (player != null && player.CompareTag("Player"))
        {
            if (m_playerImmune)
                return;

            switch (m_type)
            {
                case EProjectileType.Blooper:
                    GameObject.FindWithTag("Foreground").GetComponent<ForegroundBehaviour>().AddToForeground(GetComponent<SpriteRenderer>().sprite);
                    Destroy(gameObject);
                    break;
                case EProjectileType.HurtOnce:
                    PlayerMovementController pmc = player.GetComponent<PlayerMovementController>();
                    int index = obj.transform.GetSiblingIndex();
                    pmc.QRemoveBodyPart(index);
                    HandleCollision(EProjectileCollisionType.Splat, true);
                    break;
            }
        }
    }

    public void HandleCollision(EProjectileCollisionType colType, bool explode)
    {
        switch (colType)
        {
            case EProjectileCollisionType.Bounce:
                // Make particles bounce off the surface collider
                m_speedMod *= m_restitution * -1;
                transform.Rotate(0, 0, 180);
                if (explode)
                    m_explosionEffect.Play();
                break;
            case EProjectileCollisionType.Splat:
                if (explode)
                    StartCoroutine(Explode());
                else
                    Destroy(gameObject);
                break;
        }
    }

    private IEnumerator HandleImmunity(float seconds)
    {
        m_playerImmune = true;

        yield return new WaitForSeconds(seconds);

        m_playerImmune = false;
    }

    private IEnumerator Explode()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        m_explosionEffect.Play();
        while (!m_explosionEffect.isStopped)
        {
            yield return null;
        }
        Destroy(gameObject);
    }
}