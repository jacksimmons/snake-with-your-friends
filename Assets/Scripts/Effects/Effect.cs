public enum EFoodType
{
    Apple,
    Balti,
    Banana,
    Bone,
    Booze,
    Cheese,
    Coffee,
    Doughnut,
    Dragonfruit,
    Drumstick,
    IceCream,
    Orange,
    Pineapple,
    PineapplePizza,
    Pizza,
}

public enum EEffect
{
    None,

    // Apple
    CureAll,

    // Dragonfruit
    BreathingFire,

    // Booze
    Drunk,
    Pissing,

    // Balti
    SpeedBoost,
    RocketShitting,

    // Doughnut
    Sleeping,

    SoberUp,
    Hallucination,
    Unicorn,
    BrainFreeze,
    Buff
}

public class Effect
{
    public EEffect EffectName { get; private set; }
    public int EffectLevel { get; private set; }

    public float LifetimeMax { get; private set; }
    public float LifetimeRemaining { get; set; }
    // The current value of the cooldown counter, which goes from CooldownMax to 0.
    public float Cooldown { get; private set; } = 0f;
    // The max value of the Cooldown, set once it gets used to restart the cooldown.
    public float CooldownMax { get; private set; }
    public Effect[] Causes { get; private set; }

    private BitField bf = new();
    public bool IsInputEffect
    {
        get { return bf.GetBit(0); }
        set { bf.SetBit(0, value); }
    }
    public bool IsOneOff
    {
        get { return bf.GetBit(1); }
        set { bf.SetBit(1, value); }
    }

    // An effect which may cause another effect.
    public Effect(EEffect effectName, float lifetime=0f, float cooldown=0f, bool isInputEffect=false,
        int level=0, Effect[] causes = null, bool isOneOff=false)
    {
        EffectName = effectName;
        LifetimeMax = lifetime;
        LifetimeRemaining = LifetimeMax;
        IsInputEffect = isInputEffect;
        EffectLevel = level;
        CooldownMax = cooldown;
        Causes = causes;
        IsOneOff = isOneOff;
    }

    /// <summary>
    /// Subtracts time from the remaining time, and returns
    /// if the effect is still in place.
    /// </summary>
    /// <param name="seconds">Number of seconds to subtract.
    /// </param>
    /// <returns>`true` if the effect should stay, `false`
    /// if the effect needs to be removed.</returns>
    public bool SubtractTime(float seconds)
    {
        if (IsOneOff) return true;

        LifetimeRemaining -= seconds;
        if (LifetimeRemaining <= 0)
            return false;
        return true;
    }

    /// <summary>
    /// Subtracts time from the cooldown.
    /// </summary>
    /// <param name="seconds">Number of seconds to subtract.</param>
    public void SubtractCooldown(float seconds)
    {
        Cooldown -= seconds;
    }

    public void ResetCooldown()
    {
        Cooldown = CooldownMax;
    }
}