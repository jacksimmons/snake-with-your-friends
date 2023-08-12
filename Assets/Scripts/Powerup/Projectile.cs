using UnityEngine;

public enum EProjectileType
{
    None,
    Blooper,
    HurtOnce
}

public struct Projectile
{
    public float Lifetime { get; private set; }
    public Vector2 Direction { get; private set; }
    public Quaternion Rotation { get; private set; }

    // Uses a Counter approach despite moving continuously,
    // making it easier for it to move with a similar speed
    // to the player.
    public const int LOWEST_COUNTER_MAX = 1;
    public const int DEFAULT_COUNTER_MAX = 20;
    private int _counterMax;
    public int CounterMax
    {
        get
        {
            return _counterMax;
        }
        set
        {
            if (value < LOWEST_COUNTER_MAX) { _counterMax = LOWEST_COUNTER_MAX; }
            else { _counterMax = value; }
        }
    }
    public float immunityDuration;

    public Projectile(float lifetime, Vector2 direction, float rotation, int counterMax, float immunityDuration = 0)
    {
        Lifetime = lifetime;
        Direction = direction;
        _counterMax = counterMax;
        Rotation = Quaternion.Euler(Vector3.forward * rotation);

        this.immunityDuration = immunityDuration;
    }
}