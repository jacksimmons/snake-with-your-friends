using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class ObjectBehaviour : MonoBehaviour
{
    private bool m_dontDestroyOnExplosion = false;

    private byte m_type;
    public byte Type
    {
        get { return m_type; }
        set 
        {
            if (m_type == 0)
                m_type = value;
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


    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // --- Portal Collisions
        if (other.TryGetComponent(out Teleporter _)) { return; }


        // --- Projectile Collisions
        // When a projectile and object collide, the one which is harder to move wins (the other
        // explodes). If they have equal hard to moveness, they both explode.

        // All objects (projectiles too) can be "exploded" by projectiles (but objects cannot be
        // exploded by all objects)
        // Determines if object explosion is necessary when hit by a projectile.
        if (other.TryGetComponent(out ProjectileBehaviour pb))
        {
            if (pb.HardToMoveness >= HardToMoveness)
            {
                StartCoroutine(Explode());
            }

            return;
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
    /// To be used in the editor - prevents objects getting removed from the map.
    /// </summary>
    public void DontDestroyOnExplosion()
    {
        m_dontDestroyOnExplosion = true;
    }
}
