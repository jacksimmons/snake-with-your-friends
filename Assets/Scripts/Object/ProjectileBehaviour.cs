using System.Collections;
using TMPro;
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
            Destroy(gameObject, _proj.Lifetime);
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

        if (GameSettings.Saved.FriendlyFire)
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

        // --- Other Collisions
        bool isPlayer = true;
        Transform player = Player.TryGetPlayerTransformFromBodyPart(other.gameObject);
        if (player == null) isPlayer = false;
        if (PlayerImmune) isPlayer = false;

        // Visual Effects section, enabled on all clients
        switch (type)
        {
            case EProjectileType.InstantDamage:
                if (isPlayer)
                    StartCoroutine(Explode());
                else
                {
                    m_speedMod *= m_restitution * -1;
                    transform.Rotate(Vector3.forward * 180);
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
        PlayerImmune = true;

        yield return new WaitForSeconds(seconds);

        PlayerImmune = false;
    }
}