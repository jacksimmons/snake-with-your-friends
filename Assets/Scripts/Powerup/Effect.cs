using UnityEngine;

public enum e_Food
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
	RocketShitting,

	MajorSpeedBoost,
	MinorSpeedBoost,
	Drunk,
	Hallucination,
	Unicorn,
	BrainFreeze,
	Laxative,
	Buff
}

public class Effect
{
	public e_Effect p_EffectName { get; private set; }
	public float p_Lifetime { get; private set; }
	public float p_TimeRemaining { get; set; }
	public Effect p_Causes { get; private set; } = null;
	public bool p_CausesInputEffect { get; private set; } = false;
	public float p_CausesCooldown { get; private set; } = 0f;

	public Effect(e_Effect effectName, float lifetime)
	{
		p_EffectName = effectName;
		p_Lifetime = lifetime;
		p_TimeRemaining = p_Lifetime;
	}

	public Effect(e_Effect effectName, float lifetime, Effect causes, bool causesInputEffect, float causesCooldown)
	{
		p_EffectName = effectName;
		p_Lifetime = lifetime;
		p_Causes = causes;
		p_TimeRemaining = p_Lifetime;
		p_CausesInputEffect = causesInputEffect;
		p_CausesCooldown = causesCooldown;
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
		p_TimeRemaining -= seconds;
		if (p_TimeRemaining <= 0)
			return false;
		return true;
	}
}