using System.Collections;
using UnityEngine;


public class ProjectileBehaviour : ObjectBehaviour
{
    [SerializeField]
    public EProjectileType type;

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
            Ready = true;
            transform.rotation = _proj.Rotation;
            Destroy(gameObject, _proj.LifetimeMax);
        }
    }

    [Range(0, 1)]
    // Bounciness - amount of speed retained after bounce
    private float m_restitution = 0.5f;
    private float m_speedMod = 1f;

    private Rigidbody2D _rb = null; // Assigned to in Proj setter


    protected override void Start()
    {
        base.Start();

        if (GameSettings.Saved == null)
        {
            Debug.LogWarning("No GameSettings applied. Ensure you are in editor.");
            return;
        }

        if (GameSettings.Saved.Data.FriendlyFire)
            StartCoroutine(HandleImmunity(Proj.ImmunityDuration));
        else
            PlayerImmune = true;
    }

    private void FixedUpdate()
    {
        if (Ready)
        {
            _rb.MovePosition(_rb.position + _proj.Velocity * m_speedMod);
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);


        // --- Wall Collisions
        if (other.TryGetComponent(out DeathTrigger _))
        {
            StartCoroutine(Explode());
            return;
        }


        // --- Player Collisions
        Transform player = PlayerStatic.TryGetOwnedPlayerTransformFromBodyPart(other.gameObject);
        if (player == null) return;
        if (PlayerImmune) return;


        // Core effects
        switch (type)
        {
            case EProjectileType.Shit:
                // Add a shit to the foreground overlay (blooper effect)
                GameObject fg = GameObject.FindWithTag("Foreground");
                fg.GetComponent<ForegroundBehaviour>().AddToForeground(m_sprite);
                break;
            case EProjectileType.Fireball: // e.g. fireball
                // Remove the body part
                int index = other.transform.GetSiblingIndex();
                PlayerMovement pm = player.GetComponent<PlayerMovement>();
                pm.QRemoveBodyPart(index);
                StartCoroutine(Explode());
                break;
            default:
                break;
        }


        // Bounce effects
        //m_speedMod *= m_restitution * -1;
        //transform.Rotate(Vector3.forward * 180);
    }

    private IEnumerator HandleImmunity(float seconds)
    {
        PlayerImmune = true;

        yield return new WaitForSeconds(seconds);

        PlayerImmune = false;
    }
}