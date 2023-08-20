using UnityEngine;

public enum EProjectileType
{
    Shit,
    InstantDamage,
    Orange
}

public struct Projectile
{
    public float Lifetime { get; private set; }
    public Vector2 Velocity { get; private set; }
    public Quaternion Rotation { get; private set; }

    public float ImmunityDuration { get; private set; }

    public Projectile(float lifetime, Vector2 velocity, float rotation, float immunityDuration = 0)
    {
        Lifetime = lifetime;
        Velocity = velocity;
        Rotation = Quaternion.Euler(Vector3.forward * rotation);
        ImmunityDuration = immunityDuration;
    }
}