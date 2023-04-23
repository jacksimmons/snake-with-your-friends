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
	public Effect p_Causes { get; private set; }

	public Effect(e_Effect effectName)
	{
		p_EffectName = effectName;
		p_Lifetime = Mathf.Infinity;
		p_Causes = null;
		p_TimeRemaining = p_Lifetime;
	}

	public Effect(e_Effect effectName, float lifetime, Effect causes)
	{
		p_EffectName = effectName;
		p_Lifetime = lifetime;
		p_Causes = causes;
		p_TimeRemaining = p_Lifetime;
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