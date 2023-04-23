using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatusBehaviour : MonoBehaviour
{
	[SerializeField]
	private PlayerBehaviour _player;
	[SerializeField]
	private GameObject _fireball;

	private List<BodyPartStatus> _bodyPartStatuses;

	public List<Effect> p_ActiveInputEffects { get; private set; } = new List<Effect>();
	public List<Effect> p_ActivePassiveEffects { get; private set; } = new List<Effect>();

	private float _inputEffectCooldownMax = 0f;
	private float _inputEffectCooldown = 0f;

	// Counters
	private int _shit_o_counter = 0;
	public int p_ShitOCounter
	{
		get { return _shit_o_counter; }
	}

	private int _numPints = 0;
	public int p_NumPints
	{
		get { return _numPints; }
	}

	private float _speedIncrease = 0f;
	public float p_SpeedIncrease
	{
		get { return _speedIncrease; }
	}

	private int _potassiumLevels = 0;
	public int p_PotassiumLevels
	{
		get { return _potassiumLevels; }
	}

	private void Update()
	{
		print("active input: " + p_ActiveInputEffects.Count);
		_inputEffectCooldown -= Time.deltaTime;
		if (p_ActiveInputEffects.Count > 0)
			if (!p_ActiveInputEffects[0].SubtractTime(Time.deltaTime))
				p_ActiveInputEffects.RemoveAt(0);
		foreach (Effect effect in p_ActivePassiveEffects)
			if (!effect.SubtractTime(Time.deltaTime))
				p_ActivePassiveEffects.Remove(effect);
	}

	public void HandleInput()
	{
		if (_inputEffectCooldown < 0f)
		{
			_inputEffectCooldown = _inputEffectCooldownMax;
			if (p_ActiveInputEffects[0].p_EffectName == e_Effect.BreathingFire)
			{
				GameObject fireball = Instantiate(_fireball, GameObject.Find("Projectiles").transform);
				Projectile proj = fireball.GetComponent<Projectile>();
				proj.Create(Mathf.Infinity, _player.head.p_Position + (Vector3)_player.head.p_Direction,
					_player.head.p_Direction, _player.head.p_Rotation, 0.2f);
			}
		}
	}

	public void AddInputEffect(Effect effect, float cooldown)
	{
		// Clear the old effect for the new one
		if (p_ActiveInputEffects.Count > 0)
			ClearInputEffects();
		p_ActiveInputEffects.Add(effect);
		_inputEffectCooldown = 0;
		_inputEffectCooldownMax = cooldown;
	}

	public void AddPassiveEffect(Effect effect)
	{
		p_ActivePassiveEffects.Add(effect);
	}

	/// <summary>
	/// Disables all input status effects.
	/// </summary>
	public void ClearInputEffects()
	{
		p_ActiveInputEffects.Clear();
		_inputEffectCooldown = 0f;
	}

	/// <summary>
	/// Disables all passive status effects, and resets all passive counters.
	/// </summary>
	public void ClearPassiveEffects()
	{
		p_ActivePassiveEffects.Clear();

		_shit_o_counter = 0;
		_numPints = 0;
		_speedIncrease = 0;
		_potassiumLevels = 0;
	}

	public Dictionary<string, string> GetStatusDebug()
	{
		Dictionary<string, string> statuses = new Dictionary<string, string>();
		foreach (Effect effect in p_ActiveInputEffects)
			statuses[Enum.GetName(typeof(e_Effect), effect.p_EffectName)] = "True";
		foreach (Effect effect in p_ActivePassiveEffects)
			statuses[Enum.GetName(typeof(e_Effect), effect.p_EffectName)] = "True";
		foreach (string e_name in Enum.GetNames(typeof(e_Effect)))
		{
			if (!statuses.ContainsKey(e_name))
				statuses[e_name] = "False";
		}

		statuses["shit_o_counter"] = _shit_o_counter.ToString();
		statuses["numPints"] = _numPints.ToString();
		statuses["speedIncrease"] = _speedIncrease.ToString();
		statuses["potassiumLevels"] = _potassiumLevels.ToString();
		statuses["NumPieces"] = _player.BodyParts.Count.ToString();
		return statuses;
	}

	public void Eat(e_Food food)
	{
		switch (food)
		{
			case e_Food.Coffee:
				DrinkCoffee();
				break;
			case e_Food.Booze:
				DrinkBooze();
				break;
			case e_Food.Apple:
				EatApple();
				break;
			case e_Food.Orange:
				EatOrange();
				break;
			case e_Food.Banana:
				EatBanana();
				break;
			case e_Food.Dragonfruit:
				EatDragonfruit();
				break;
			case e_Food.Drumstick:
				EatDrumstick();
				break;
			case e_Food.Bone:
				EatBone();
				break;
			case e_Food.Cheese:
				EatCheese();
				break;
			case e_Food.Pizza:
				EatPizza();
				break;
			case e_Food.Pineapple:
				EatPineapple();
				break;
			case e_Food.PineapplePizza:
				EatPineapplePizza();
				break;
			case e_Food.IceCream:
				EatIceCream();
				break;
			case e_Food.CrapALot:
				EatCrapALot();
				break;
			case e_Food.Balti:
				EatBalti();
				break;
			case e_Food.Brownie:
				EatBrownie();
				break;
		}
	}

	private void DrinkCoffee()
	{
		Effect minor = new Effect(e_Effect.MinorSpeedBoost);
		Effect major = new Effect(e_Effect.MajorSpeedBoost, 10, minor);
		AddPassiveEffect(major);
	}

	private void DrinkBooze()
	{
		Effect drunk = new Effect(e_Effect.Drunk, 100, null);
		Effect pissing = new Effect(e_Effect.Pissing, 10, null);
		AddInputEffect(pissing, 1);
		AddPassiveEffect(drunk);
	}

	private void EatApple()
	{
		ClearPassiveEffects();
	}

	private void EatOrange()
	{
		// Produce projectile
	}

	private void EatBanana()
	{
		// Produce peel
		_potassiumLevels++;
		if (_potassiumLevels >= 3)
		{
			// Die ... ?
		}
	}

	private void EatDragonfruit()
	{
		Effect fireBreath = new Effect(e_Effect.BreathingFire, 5, null);
		AddInputEffect(fireBreath, 0.1f);
	}

	private void EatDrumstick()
	{
		Effect buff = new Effect(e_Effect.Buff, 20, null);
		EatBone();
	}

	private void EatBone()
	{
		// Rupture asshole
	}

	private void EatCheese()
	{
		// Get to eat it again
	}

	private void EatPizza()
	{
		// Get to eat it again twice
	}

	private void EatPineapple()
	{
		// ?
	}

	private void EatPineapplePizza()
	{
		// Die
	}

	private void EatIceCream()
	{
		Effect brainFreeze = new Effect(e_Effect.BrainFreeze, 3, null);
		Effect unicorn = new Effect(e_Effect.Unicorn, 3, brainFreeze);
		AddPassiveEffect(unicorn);
	}

	private void EatCrapALot()
	{
		Effect laxative = new Effect(e_Effect.Laxative, 20, null);
		AddInputEffect(laxative, 1);
	}

	private void EatBalti()
	{
		// Add rocket shit for 1 second after 10 seconds
	}

	private void EatBrownie()
	{
		// Sleep for 5 turns
	}
}