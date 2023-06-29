using UnityEditorInternal;

public enum EFoodType
{
    Coffee,
    Booze,
    Apple,
    Orange,
    Banana,
    Dragonfruit,
    Drumstick,
    Bone,
    Cheese,
    Pizza,
    Pineapple,
    PineapplePizza,
    IceCream,
    CrapALot,
    Balti,
    Brownie,
}

public enum e_Effect
{
    None,

    BreathingFire,
    Pissing,

    NoSpeedBoost,
    MinorSpeedBoost,
    MajorSpeedBoost,
    CriticalSpeedBoost,

    RocketShitting,
    Laxative,

    SoberUp,
    Hallucination,
    Unicorn,
    BrainFreeze,
    Buff
}

public class Effect
{
    public e_Effect EffectName { get; private set; }
    public float Lifetime { get; private set; }
    public float TimeRemaining { get; set; }
    // The current value of the cooldown counter, which goes from CooldownMax to 0.
    public float Cooldown { get; private set; } = 0f;
    // The max value of the Cooldown, set once it gets used to restart the cooldown.
    public float CooldownMax { get; private set; } = 0f;
    public Effect[] Causes { get; private set; } = null;
    public bool IsInputEffect { get; private set; } = false;
    public bool IsOneOff { get; set; } = false;

    // An effect which lasts for one frame, i.e. an action
    public Effect(e_Effect effectName)
    {
        EffectName = effectName;
        IsOneOff = true;
    }

    // An effect which causes no other effects
    public Effect(e_Effect effectName, float lifetime, float cooldown=0f, bool isInputEffect=false)
    {
        EffectName = effectName;
        Lifetime = lifetime;
        TimeRemaining = Lifetime;
        Cooldown = cooldown;
        CooldownMax = cooldown;
        IsInputEffect = isInputEffect;
    }

    // An effect which may cause another effect.
    public Effect(e_Effect effectName, float lifetime, Effect[] causes, float cooldown=0f, bool isInputEffect=false)
    {
        EffectName = effectName;
        Lifetime = lifetime;
        Causes = causes;
        TimeRemaining = Lifetime;
        IsInputEffect = isInputEffect;
        Cooldown = cooldown;
        CooldownMax = cooldown;
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

        TimeRemaining -= seconds;
        if (TimeRemaining <= 0)
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