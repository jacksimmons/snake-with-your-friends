using System.Collections;
using UnityEngine;
using UnityEngine.XR;

public class ProjectileBehaviour : MonoBehaviour
{
    [SerializeField]
    public EProjectileType type;

    private ParticleSystem m_explosionEffect;

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
            transform.rotation = _proj.Rotation;
            Destroy(gameObject, _proj.Lifetime);
        }
    }
    private bool _ready = false;

    [Range(0, 1)]
    // Bounciness - amount of speed retained after bounce
    private float m_restitution = 0.5f;
    private float m_speedMod = 1f;

    private bool m_playerImmune = false;

    private Rigidbody2D _rb = null; // Assigned to in Proj setter
    private Sprite m_sprite; // Assigned to once in Awake

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
            _rb.MovePosition(_rb.position + _proj.Velocity * m_speedMod);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Projectile Collision section
        if (other.TryGetComponent(out ProjectileBehaviour otherPb))
        {
            StartCoroutine(Explode());
            otherPb.StartCoroutine(Explode());
        }

        bool isPlayer = true;
        Transform player = Player.TryGetPlayerTransformFromBodyPart(other.gameObject);
        if (player == null) isPlayer = false;
        if (m_playerImmune) isPlayer = false;

        // Other Collisions section
        // Visual Effects section, enabled on all clients
        switch (type)
        {
            case EProjectileType.InstantDamage:
                if (isPlayer)
                    StartCoroutine(Explode());
                else
                {
                    m_speedMod *= m_restitution * -1;
                    transform.Rotate(0, 0, 180);
                }
                break;
            default:
                StartCoroutine(Explode());
                break;
        }

        // Player callbacks section, enabled only on the owning client
        if (!isPlayer) return;
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (!pm.isOwned) return; // Not our collision to handle

        // Confirmed dealing with a Player collision
        switch (type)
        {
            case EProjectileType.Shit:
                // Add a shit to the foreground overlay (blooper effect)
                GameObject fg = GameObject.FindWithTag("Foreground");
                fg.GetComponent<ForegroundBehaviour>().AddToForeground(m_sprite);
                break;
            case EProjectileType.InstantDamage: // e.g. fireball
                // Remove the body part
                int index = other.transform.GetSiblingIndex();
                pm.QRemoveBodyPart(index);
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