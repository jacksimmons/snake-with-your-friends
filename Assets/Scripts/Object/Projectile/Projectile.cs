using UnityEngine;


public enum EProjectileType
{
    Shit,
    Fireball,
    Orange,

    None
}


public struct Projectile
{
    public readonly float LifetimeMax;
    public readonly Vector2 Velocity;
    public readonly Quaternion Rotation;
    public readonly float ImmunityDuration;


    public Projectile(float lifetime, Vector2 velocity, float rotation, float immunityDuration = 0)
    {
        LifetimeMax = lifetime;
        Velocity = velocity;
        Rotation = Quaternion.Euler(Vector3.forward * rotation);
        ImmunityDuration = immunityDuration;
    }
}


public static class Projectiles
{
    public static Projectile ConstructShit(Vector2 velocity_vec, float rotation_z)
    {
        return new Projectile(
            lifetime: 5,
            velocity: velocity_vec,
            rotation: rotation_z,
            immunityDuration: 0.2f
        );
    }

    public static Projectile ConstructFireball(Vector2 velocity_vec, float rotation_z)
    {
        return new Projectile(
            lifetime: 5,
            velocity: velocity_vec,
            rotation: rotation_z,
            immunityDuration: 0.2f
        );
    }
}