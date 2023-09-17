using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ObjectBehaviour : MonoBehaviour
{
    [SerializeField]
    public int HardToMoveness { get; private set; }

    private ParticleSystem m_explosionEffect;
    protected Sprite m_sprite; // Assigned to once in Awake

    private BitField bf = new();
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
        m_sprite = GetComponent<SpriteRenderer>().sprite;

        m_explosionEffect = GetComponent<ParticleSystem>();
        m_explosionEffect.Stop();
    }


    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // --- Object Collisions
        if (other.TryGetComponent(out ObjectBehaviour otherOb))
        {
            if (otherOb.HardToMoveness >= HardToMoveness)
            {
                StartCoroutine(Explode());
            }

            return;
        }


        // --- Portal Collisions (ignore)
        if (other.TryGetComponent(out Teleporter _)) { return; }
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
        Destroy(gameObject);
    }
}
