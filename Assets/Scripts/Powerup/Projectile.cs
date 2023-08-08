using UnityEngine;

public enum EProjectileType
{
    None,
    Shit,
}

public class Projectile
{
    public float Lifetime { get; private set; }
    public Vector2 Direction { get; private set; }
    public Quaternion Rotation { get; private set; }

    // Uses a Counter approach despite moving continuously,
    // making it easier for it to move with a similar speed
    // to the player.
    public const int LOWEST_COUNTER_MAX = 1;
    public const int DEFAULT_COUNTER_MAX = 20;
    private int _counterMax = DEFAULT_COUNTER_MAX;
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
    public GameObject immune = null;

    public Projectile(float lifetime, Vector2 direction, BodyPartRotation bpRotation, int counterMax, GameObject immune = null)
    {
        Lifetime = lifetime;
        Direction = direction;
        CounterMax = counterMax;
        Rotation = Quaternion.Euler(Vector3.forward * bpRotation.RegularAngle);

        this.immune = immune;
    }
}