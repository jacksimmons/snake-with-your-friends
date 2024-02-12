using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class ObjectBehaviour : MonoBehaviour
{
    private bool m_dontDestroyOnExplosion = false;

    private byte m_objId;
    public byte ObjId
    {
        get { return m_objId; }
        set 
        {
            if (m_objId == 0)
                m_objId = value;
            else
                Debug.LogWarning("Type already set.");
        }
    }

    [SerializeField]
    private int m_hardToMoveness;
    public int HardToMoveness
    {
        get { return m_hardToMoveness; }
    }

    private ParticleSystem m_explosionEffect;
    protected Sprite m_sprite; // Assigned to once in Awake


    private BitField bf = new(1);
    protected bool Ready
    {
        get { return bf.GetBit(0); }
        set { bf.SetBit(0, value); }
    }
    protected bool PlayerImmune
    {
        get { return bf.GetBit(1); }
        set { bf.SetBit(1, value); }
    }


    protected virtual void Start()
    {
        SpriteRenderer sr = gameObject.GetComponentInChildren<SpriteRenderer>();
        if (sr)
            m_sprite = sr.sprite;
        else
            Debug.LogError("No sprite attached to this GameObject or any of its children.");

        m_explosionEffect = GetComponent<ParticleSystem>();
        m_explosionEffect.Stop();
    }


    /// <summary>
    /// Handles collisions of ObjectBehaviour objects.
    /// </summary>
    /// <param name="other">The other collider.</param>
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // --- Portal Collisions
        if (other.TryGetComponent(out Teleporter _)) { return; }


        // --- "Projectile hit me" Collisions
        // When a projectile and object (incl. another projectile) collide, the one
        // which is harder to move wins (the other explodes). If they have equal hard to moveness,
        // they pass over each other.
        if (other.TryGetComponent(out ProjectileBehaviour pbOther))
        {
            if (pbOther.HardToMoveness > HardToMoveness)
                StartCoroutine(Explode());
            return;
        }


        // --- Player collisions
        Transform player = PlayerStatic.TryGetPlayerTransformFromBodyPart(other.gameObject);
        if (player == null) return;

        PlayerMovement pm = player.GetComponent<PlayerMovement>();

        // If the player can move freely, they don't die when "hitting" stuff
        if (!pm.FreeMovement)
        {
            if (pm.GetComponent<PlayerStatus>().IsBuff)
                StartCoroutine(Explode());
            else
                pm.HandleDeath();
        }
    }


    protected IEnumerator Explode()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        m_explosionEffect.Play();
        while (!m_explosionEffect.isStopped)
        {
            yield return null;
        }

        if (!m_dontDestroyOnExplosion)
            Destroy(gameObject);
    }


    /// <summary>
    /// To be used in the editor - prevents objects getting removed from the map by projectiles.
    /// </summary>
    public void DontDestroyOnExplosion()
    {
        m_dontDestroyOnExplosion = true;
    }
}
