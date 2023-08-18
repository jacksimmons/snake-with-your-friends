using System.Collections;
using UnityEngine;
using UnityEngine.XR;

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

            _rb = GetComponent<Rigidbody2D>();
            _proj = value;
            _ready = true;
            // v = d / t; a distance of 1 is moved every counter completion.
            m_speed = 1f / _proj.CounterMax;
            transform.rotation = _proj.Rotation;
            Destroy(gameObject, _proj.Lifetime);
        }
    }
    private bool _ready = false;
    private float m_speed = 0f;

    [Range(0, 1)]
    // Bounciness - amount of speed retained after bounce
    private float m_restitution = 0.5f;
    private float m_speedMod = 1f;

    private bool m_playerImmune = false;

    private Rigidbody2D _rb = null; // Assigned to in Proj setter
    private Sprite m_sprite; // Assigned to once in Awake

    public EProjectileType GetProjectileType()
    {
        return m_type;
    }

    private void Awake()
    {
        m_sprite = GetComponent<SpriteRenderer>().sprite;
    }

    private void Start()
    {
        m_explosionEffect = GetComponent<ParticleSystem>();
        m_explosionEffect.Stop();

        StartCoroutine(HandleImmunity(Proj.ImmunityDuration));
    }

    private void FixedUpdate()
    {
        if (_ready)
        {
            _rb.MovePosition(_rb.position + m_speedMod * m_speed * _proj.Direction);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject obj = collision.gameObject;
        // Ignore collision with certain projectiles
        if (obj.TryGetComponent(out ProjectileBehaviour pb))
        {
            if (pb.m_type == EProjectileType.Shit)
                return;
        }

        bool isPlayer = true;
        Transform player = Player.TryGetPlayerTransformFromBodyPart(obj);
        if (player == null) isPlayer = false;
        if (m_playerImmune) isPlayer = false;

        // Handle collision type first so we can quick-exit in the player section
        switch (Proj.CollisionType)
        {
            case ECollisionType.None:
                break;
            case ECollisionType.IfPlayerExplodeElseBounce:
                if (isPlayer)
                    StartCoroutine(Explode());
                else
                {
                    m_speedMod *= m_restitution * -1;
                    transform.Rotate(0, 0, 180);
                }
                break;
            case ECollisionType.Splat:
                Destroy(gameObject);
                break;
            case ECollisionType.Explode:
                StartCoroutine(Explode());
                break;
        }

        if (!isPlayer) return;

        // Confirmed dealing with a Player collision
        switch (m_type)
        {
            case EProjectileType.Shit:
                // Add a shit to the foreground overlay (blooper effect)
                GameObject fg = GameObject.FindWithTag("Foreground");
                fg.GetComponent<ForegroundBehaviour>().AddToForeground(m_sprite);
                break;
            case EProjectileType.InstantDamage: // e.g. fireball
                // Remove the body part
                PlayerMovementController pmc = player.GetComponent<PlayerMovementController>();
                int index = obj.transform.GetSiblingIndex();
                pmc.QRemoveBodyPart(index);
                break;
            default:
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